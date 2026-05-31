// execute.ts - ReactionGraph executor. The dumb runtime.
// Dispatches on reaction.kind. Uses shared resolver for ALL component access.
// Component references resolve through the currently active browser plan.

import type {
  PlanDocument, ReactionGraph, SequenceReaction, ParallelReaction, BranchReaction,
  BranchCase, SetReaction, CallReaction, DispatchReaction,
  InjectReaction, ShowValidationErrorsReaction,
  ExecContext, PayloadSource,
} from "../types";
import { RuntimePlan } from "../domain/runtime-plan";
import { evaluateValue } from "../core/evaluate";
export { evaluateValue };
import { evaluateConditionInCurrentLane } from "../conditions/conditions";
import { validateContainer, showServerErrors } from "../validation";
import { executeRequest } from "./http";
import { injectPartial } from "./inject";
import { assertNever } from "../core/assert-never";
import { scope } from "../core/trace";
import { isMissingRuntimeValue } from "../domain/runtime-value";
import { ExecutionContext, type ServerValidationPayload } from "../domain/execution-context";
import { plainObjectRecordFrom } from "../domain/object-record";

const log = scope("execute");

export type ReactionCompletion = void | Promise<void>;

let activeRuntimePlan: RuntimePlan | undefined;

export function setActivePlan(plan: PlanDocument): void {
  activeRuntimePlan = RuntimePlan.from(plan);
}

export function resetActivePlanForTests(): void {
  activeRuntimePlan = undefined;
}

function runtimePlanFor(plan: PlanDocument | undefined): RuntimePlan {
  if (plan) return RuntimePlan.from(plan);
  if (activeRuntimePlan) return activeRuntimePlan;
  throw new Error("[alis] no active plan");
}

// Returns void for immediate-lane reaction kinds (set, call, dispatch, inject,
// show-validation-errors, branch with non-confirm conditions).
// Returns Promise<void> only when execution reaches the async lane:
// request, parallel, confirm, or a sequence/branch step that reaches one.
//
// This two-lane design keeps SF event arg mutations (args.cancel,
// args.preventDefaultAction) in the same browser tick as the event callback.

export function executeReaction(
  reaction: ReactionGraph,
  plan?: PlanDocument,
  ctx?: ExecContext,
): ReactionCompletion {
  return executeReactionWith(reaction, runtimePlanFor(plan), ExecutionContext.from(ctx));
}

function executeReactionWith(
  reaction: ReactionGraph,
  plan: RuntimePlan,
  context: ExecutionContext,
): ReactionCompletion {
  switch (reaction.kind) {
    case "set":
      executeSet(reaction, plan, context);
      return;

    case "call":
      executeCall(reaction, plan, context);
      return;

    case "dispatch":
      executeDispatch(reaction, plan, context);
      return;

    case "inject":
      executeInject(reaction, plan, context);
      return;

    case "show-validation-errors":
      executeShowValidationErrors(reaction, plan, context);
      return;

    case "sequence":
      return executeSequence(reaction, plan, context);

    case "branch":
      return executeBranch(reaction, plan, context);

    case "request":
      return executeRequest(reaction.request, plan.document, context.raw);

    case "parallel":
      return executeParallel(reaction, plan, context);

    default:
      assertNever(reaction, "reaction kind");
  }
}

function executeSequence(
  reaction: SequenceReaction,
  plan: RuntimePlan,
  context: ExecutionContext,
): ReactionCompletion {
  for (const [index, step] of reaction.steps.entries()) {
    const result = executeReactionWith(step, plan, context);
    if (crossedAsyncBoundary(result)) {
      const remaining = reaction.steps.slice(index + 1);
      return result.then(() => executeRemainingSequence(remaining, plan, context));
    }
  }
}

async function executeRemainingSequence(
  steps: readonly ReactionGraph[],
  plan: RuntimePlan,
  context: ExecutionContext,
): Promise<void> {
  for (const step of steps) {
    const result = executeReactionWith(step, plan, context);
    if (crossedAsyncBoundary(result)) await result;
  }
}

function executeBranch(
  reaction: BranchReaction,
  plan: RuntimePlan,
  context: ExecutionContext,
): ReactionCompletion {
  return executeBranchFrom(reaction.cases, plan, context, 0);
}

async function executeParallel(
  reaction: ParallelReaction,
  plan: RuntimePlan,
  context: ExecutionContext,
): Promise<void> {
  const settledSteps = await Promise.allSettled(
    reaction.steps.map(step => reactionCompletion(executeReactionWith(step, plan, context))),
  );
  reportParallelStepFailures(settledSteps);

  switch (reaction.completion.kind) {
    case "none":
      return;

    case "on-settled":
      await waitForAsyncBoundary(executeReactionWith(reaction.completion.reaction, plan, context));
      return;

    default:
      assertNever(reaction.completion, "parallel completion");
  }
}

function executeSet(reaction: SetReaction, plan: RuntimePlan, context: ExecutionContext): void {
  const value = evaluateValue(reaction.value, plan.document, context.raw);

  switch (reaction.on.kind) {
    case "component":
      log.trace("set", { target: reaction.on.component, property: reaction.property, value });
      plan.objectForSource(reaction.on).set(reaction.property, value);
      return;

    case "payload":
      log.trace("set", { target: reaction.on.scope, property: reaction.property, value });
      requireMutablePayload(reaction.on, context, "set property")[reaction.property] = value;
      return;
  }
}

function executeCall(reaction: CallReaction, plan: RuntimePlan, context: ExecutionContext): void {
  const args = reaction.args.map(arg => evaluateValue(arg, plan.document, context.raw));

  switch (reaction.on.kind) {
    case "component":
    case "plugin":
      log.trace("call", {
        target: reaction.on.kind === "component" ? reaction.on.component : reaction.on.name,
        method: reaction.method,
        args,
      });
      plan.objectForSource(reaction.on).call(reaction.method, args);
      return;

    case "payload":
      log.trace("call", { target: reaction.on.scope, method: reaction.method, args });
      callPayloadMethod(
        requireMutablePayload(reaction.on, context, "call method"),
        reaction.on.scope,
        reaction.method,
        args,
      );
      return;
  }
}

function executeDispatch(reaction: DispatchReaction, plan: RuntimePlan, context: ExecutionContext): void {
  const detail = dispatchPayload(reaction, plan.document, context);
  log.trace("dispatch", { event: reaction.event, detail });
  document.dispatchEvent(new CustomEvent(reaction.event, { detail }));
}

function executeInject(reaction: InjectReaction, plan: RuntimePlan, context: ExecutionContext): void {
  const container = plan.components.element(reaction.slot);
  const value = evaluateValue(reaction.value, plan.document, context.raw);
  if (typeof value === "string") {
    injectPartial(container, value, reaction.slot);
    log.trace("inject.applied", { slot: reaction.slot, size: value.length });
    return;
  }

  log.error("inject.wrong-type", { slot: reaction.slot, type: typeof value });
  throw new Error(`[alis] inject expects string HTML, got ${typeof value}`);
}

function executeShowValidationErrors(
  reaction: ShowValidationErrorsReaction,
  plan: RuntimePlan,
  context: ExecutionContext,
): void {
  const payload: ServerValidationPayload = context.serverValidationPayload();
  if (payload.kind === "available") {
    log.debug("show-validation.server", { id: reaction.container });
    showServerErrors(plan.document, reaction.container, payload.response);
    return;
  }

  log.debug("show-validation.client", { id: reaction.container });
  validateContainer(plan.document, reaction.container, context.raw);
}

function reportParallelStepFailures(results: readonly PromiseSettledResult<void>[]): void {
  for (const result of results) {
    if (result.status === "rejected") {
      log.error("parallel.step-failed", { error: String(result.reason) });
    }
  }
}

function requireMutablePayload(
  source: PayloadSource,
  context: ExecutionContext,
  operation: string,
): Record<string, unknown> {
  const root = context.resolvePayload(source);
  const payloadWasProvided = !isMissingRuntimeValue(root);
  if (!payloadWasProvided) {
    throw new Error(`[alis] cannot ${operation} on null payload (scope: ${source.scope})`);
  }

  const payload = plainObjectRecordFrom(root);
  const payloadCanHoldMembers = payload !== undefined;
  if (!payloadCanHoldMembers) {
    throw new Error(`[alis] cannot ${operation} on ${typeof root} payload (scope: ${source.scope})`);
  }

  return payload;
}

function callPayloadMethod(
  payload: Record<string, unknown>,
  scope: string,
  method: string,
  args: unknown[],
): void {
  const member = payload[method];
  const memberIsCallable = typeof member === "function";
  if (!memberIsCallable) {
    throw new Error(`[alis] "${method}" is not a function on payload (scope: ${scope})`);
  }

  member.apply(payload, args);
}

export function catchAsyncReactionFailure(
  result: ReactionCompletion,
  onRejected: (error: unknown) => void,
): void {
  if (crossedAsyncBoundary(result)) result.catch(onRejected);
}

function crossedAsyncBoundary(result: ReactionCompletion): result is Promise<void> {
  return result instanceof Promise;
}

function reactionCompletion(result: ReactionCompletion): Promise<void> {
  return crossedAsyncBoundary(result) ? result : Promise.resolve();
}

async function waitForAsyncBoundary(result: ReactionCompletion): Promise<void> {
  if (crossedAsyncBoundary(result)) await result;
}

function dispatchPayload(reaction: DispatchReaction, plan: PlanDocument, context: ExecutionContext): unknown {
  if (reaction.payload.kind === "none") return {};

  return evaluateValue(reaction.payload.data, plan, context.raw);
}

function executeBranchFrom(
  cases: readonly BranchCase[],
  plan: RuntimePlan,
  context: ExecutionContext,
  startIndex: number,
): ReactionCompletion {
  for (let index = startIndex; index < cases.length; index++) {
    const branchCase = cases[index]!;

    const guardMatches = branchGuardMatches(branchCase, plan.document, context);
    if (guardMatches instanceof Promise) {
      return executeAfterAsyncBranchGuard(cases, plan, context, index, branchCase, guardMatches);
    }

    if (guardMatches) return executeReactionWith(branchCase.reaction, plan, context);
  }

  log.trace("branch.no-match", { caseCount: cases.length });
}

async function executeAfterAsyncBranchGuard(
  cases: readonly BranchCase[],
  plan: RuntimePlan,
  context: ExecutionContext,
  index: number,
  branchCase: BranchCase,
  guardMatches: Promise<boolean>,
): Promise<void> {
  if (await guardMatches) {
    await waitForAsyncBoundary(executeReactionWith(branchCase.reaction, plan, context));
    return;
  }

  await waitForAsyncBoundary(executeBranchFrom(cases, plan, context, index + 1));
}

function branchGuardMatches(
  branchCase: BranchCase,
  plan: PlanDocument,
  context: ExecutionContext,
): boolean | Promise<boolean> {
  switch (branchCase.guard.kind) {
    case "default":
      return true;
    case "when":
      return evaluateConditionInCurrentLane(branchCase.guard.condition, plan, context.raw);
    default:
      return assertNever(branchCase.guard, "branch guard");
  }
}
