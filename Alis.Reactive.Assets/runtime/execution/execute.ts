// execute.ts - Reaction executor. The dumb runtime.
// Dispatches on reaction.kind. Uses shared resolver for ALL component access.
// No fallbacks. Every component reference must be in plan.components.

import type {
  PlanDocument, Reaction, SequenceReaction, ParallelReaction, BranchReaction,
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
import { injectHtml } from "./inject";
import { assertNever } from "../core/assert-never";
import { scope } from "../core/trace";
import { isMissingRuntimeValue } from "../domain/runtime-value";
import { ExecutionContext, type ServerValidationPayload } from "../domain/execution-context";
import { plainObjectRecordFrom } from "../domain/object-record";

const log = scope("execute");

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
  reaction: Reaction,
  plan?: PlanDocument,
  ctx?: ExecContext,
): void | Promise<void> {
  return executeReactionWith(reaction, runtimePlanFor(plan), ExecutionContext.from(ctx));
}

function executeReactionWith(
  reaction: Reaction,
  plan: RuntimePlan,
  context: ExecutionContext,
): void | Promise<void> {
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

type ReactionRunner = (reaction: Reaction) => void | Promise<void>;

function executeSequence(
  reaction: SequenceReaction,
  plan: RuntimePlan,
  context: ExecutionContext,
): void | Promise<void> {
  for (const [index, step] of reaction.steps.entries()) {
    const result = executeReactionWith(step, plan, context);
    if (reactionContinuesAsync(result)) {
      const remaining = reaction.steps.slice(index + 1);
      return result.then(() => executeRemainingSequence(remaining, plan, context));
    }
  }
}

async function executeRemainingSequence(
  steps: readonly Reaction[],
  plan: RuntimePlan,
  context: ExecutionContext,
): Promise<void> {
  for (const step of steps) {
    const result = executeReactionWith(step, plan, context);
    if (reactionContinuesAsync(result)) await result;
  }
}

function executeBranch(
  reaction: BranchReaction,
  plan: RuntimePlan,
  context: ExecutionContext,
): void | Promise<void> {
  return executeBranchReaction({
    cases: reaction.cases,
    plan: plan.document,
    context,
    runReaction: child => executeReactionWith(child, plan, context),
  });
}

async function executeParallel(
  reaction: ParallelReaction,
  plan: RuntimePlan,
  context: ExecutionContext,
): Promise<void> {
  const settledSteps = await Promise.allSettled(
    reaction.steps.map(step => reactionAsPromise(executeReactionWith(step, plan, context))),
  );
  reportParallelStepFailures(settledSteps);

  switch (reaction.completion.kind) {
    case "none":
      return;

    case "on-settled":
      await waitForReaction(executeReactionWith(reaction.completion.reaction, plan, context));
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
      log.trace("call", { target: reaction.on.component, method: reaction.method, args });
      plan.objectForSource(reaction.on).call(reaction.method, args);
      return;

    case "plugin":
      log.trace("call", { target: reaction.on.name, method: reaction.method, args });
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
  const container = plan.components.element(reaction.target.component);
  const value = evaluateValue(reaction.value, plan.document, context.raw);
  if (typeof value === "string") {
    injectHtml(container, value, reaction.target);
    log.trace("inject.applied", { component: reaction.target.component, target: reaction.target.kind, size: value.length });
    return;
  }

  log.error("inject.wrong-type", { component: reaction.target.component, type: typeof value });
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
  result: void | Promise<void>,
  onRejected: (error: unknown) => void,
): void {
  if (reactionContinuesAsync(result)) result.catch(onRejected);
}

function reactionContinuesAsync(result: void | Promise<void>): result is Promise<void> {
  return result instanceof Promise;
}

function reactionAsPromise(result: void | Promise<void>): Promise<void> {
  return reactionContinuesAsync(result) ? result : Promise.resolve();
}

async function waitForReaction(result: void | Promise<void>): Promise<void> {
  if (reactionContinuesAsync(result)) await result;
}

function dispatchPayload(reaction: DispatchReaction, plan: PlanDocument, context: ExecutionContext): unknown {
  if (reaction.payload.kind === "none") return {};

  return evaluateValue(reaction.payload.data, plan, context.raw);
}

interface BranchExecutionContext {
  readonly cases: readonly BranchCase[];
  readonly plan: PlanDocument;
  readonly context: ExecutionContext;
  readonly runReaction: ReactionRunner;
}

function executeBranchReaction(branch: BranchExecutionContext): void | Promise<void> {
  return executeBranchFrom(branch, 0);
}

function executeBranchFrom(
  branch: BranchExecutionContext,
  startIndex: number,
): void | Promise<void> {
  for (let index = startIndex; index < branch.cases.length; index++) {
    const branchCase = branch.cases[index]!;

    const guardMatches = branchGuardMatches(branchCase, branch.plan, branch.context);
    if (guardMatches instanceof Promise) {
      return executeAfterAsyncBranchGuard(branch, index, branchCase, guardMatches);
    }

    if (guardMatches) return branch.runReaction(branchCase.reaction);
  }

  log.trace("branch.no-match", { caseCount: branch.cases.length });
}

async function executeAfterAsyncBranchGuard(
  branch: BranchExecutionContext,
  index: number,
  branchCase: BranchCase,
  guardMatches: Promise<boolean>,
): Promise<void> {
  if (await guardMatches) {
    await waitForReaction(branch.runReaction(branchCase.reaction));
    return;
  }

  await waitForReaction(executeBranchFrom(branch, index + 1));
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
