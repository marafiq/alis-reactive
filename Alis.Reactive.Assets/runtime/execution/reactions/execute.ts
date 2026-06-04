// ReactionGraph execution dispatches generated reactions through the Active Plan.

import type {
  PlanDocument, ReactionGraph, SequenceReaction, ParallelReaction, BranchReaction,
  BranchCase, SetReaction, CallReaction, DispatchReaction,
  InjectReaction, ShowValidationErrorsReaction,
  ExecContext, PayloadSource,
} from "../../types/index";
import { RuntimePlan } from "../../browser-objects/runtime-plan";
import { evaluateValue } from "../../values/evaluate";
export { evaluateValue };
import { evaluateConditionInCurrentLane } from "../../conditions/conditions";
import { validateContainer, showServerErrors } from "../../validation/index";
import { executeRequest } from "../requests/http";
import { injectPartial } from "../partials/inject";
import { assertNever } from "../../shared/assert-never";
import { scope } from "../../diagnostics/trace";
import { isMissingRuntimeValue } from "../../browser-objects/runtime-value";
import { ExecutionContext, type ServerValidationPayload } from "../../browser-objects/execution-context";
import { plainObjectRecordFrom } from "../../browser-objects/object-record";
import { toJavaScriptString } from "../../shared/javascript-string";

const log = scope("execute");

export type ReactionCompletion = void | Promise<void>;

let activePlan: RuntimePlan | undefined;

export function setActivePlan(planDocument: PlanDocument): void {
  activePlan = RuntimePlan.from(planDocument);
}

export function resetActivePlanForTests(): void {
  activePlan = undefined;
}

function runtimePlanFor(planDocument: PlanDocument | undefined): RuntimePlan {
  if (planDocument) return RuntimePlan.from(planDocument);
  if (activePlan) return activePlan;
  throw new Error("[alis] no active plan");
}

// Immediate reactions must stay synchronous so Syncfusion event arg mutations happen
// in the same event-handler turn. request/parallel/confirm, or sequence/branch paths that
// reach one, return Promise<void>.

export function executeReaction(
  reaction: ReactionGraph,
  planDocument?: PlanDocument,
  ctx?: ExecContext,
): ReactionCompletion {
  return executeReactionWith(reaction, runtimePlanFor(planDocument), ExecutionContext.from(ctx));
}

function executeReactionWith(
  reaction: ReactionGraph,
  runtimePlan: RuntimePlan,
  context: ExecutionContext,
): ReactionCompletion {
  switch (reaction.kind) {
    case "set":
      executeSet(reaction, runtimePlan, context);
      return;

    case "call":
      executeCall(reaction, runtimePlan, context);
      return;

    case "dispatch":
      executeDispatch(reaction, runtimePlan, context);
      return;

    case "inject":
      executeInject(reaction, runtimePlan, context);
      return;

    case "show-validation-errors":
      executeShowValidationErrors(reaction, runtimePlan, context);
      return;

    case "sequence":
      return executeSequence(reaction, runtimePlan, context);

    case "branch":
      return executeBranch(reaction, runtimePlan, context);

    case "request":
      return executeRequest(reaction.request, runtimePlan.document, context.raw);

    case "parallel":
      return executeParallel(reaction, runtimePlan, context);

    default:
      assertNever(reaction, "reaction kind");
  }
}

function executeSequence(
  reaction: SequenceReaction,
  runtimePlan: RuntimePlan,
  context: ExecutionContext,
): ReactionCompletion {
  for (const [index, step] of reaction.steps.entries()) {
    const result = executeReactionWith(step, runtimePlan, context);
    if (crossedAsyncBoundary(result)) {
      const remaining = reaction.steps.slice(index + 1);
      return result.then(() => executeRemainingSequence(remaining, runtimePlan, context));
    }
  }
}

async function executeRemainingSequence(
  steps: readonly ReactionGraph[],
  runtimePlan: RuntimePlan,
  context: ExecutionContext,
): Promise<void> {
  for (const step of steps) {
    const result = executeReactionWith(step, runtimePlan, context);
    if (crossedAsyncBoundary(result)) await result;
  }
}

function executeBranch(
  reaction: BranchReaction,
  runtimePlan: RuntimePlan,
  context: ExecutionContext,
): ReactionCompletion {
  return executeBranchFrom(reaction.cases, runtimePlan, context, 0);
}

async function executeParallel(
  reaction: ParallelReaction,
  runtimePlan: RuntimePlan,
  context: ExecutionContext,
): Promise<void> {
  const settledSteps = await Promise.allSettled(
    reaction.steps.map(step => reactionCompletion(executeReactionWith(step, runtimePlan, context))),
  );
  reportParallelStepFailures(settledSteps);

  switch (reaction.completion.kind) {
    case "none":
      return;

    case "on-settled":
      await waitForAsyncBoundary(executeReactionWith(reaction.completion.reaction, runtimePlan, context));
      return;

    default:
      assertNever(reaction.completion, "parallel completion");
  }
}

function executeSet(reaction: SetReaction, runtimePlan: RuntimePlan, context: ExecutionContext): void {
  const value = evaluateValue(reaction.value, runtimePlan.document, context.raw);

  switch (reaction.on.kind) {
    case "component":
      log.trace("set", { target: reaction.on.component, property: reaction.property, value });
      runtimePlan.objectForSource(reaction.on).set(reaction.property, value);
      return;

    case "payload":
      log.trace("set", { target: reaction.on.scope, property: reaction.property, value });
      requireMutablePayload(reaction.on, context, "set property")[reaction.property] = value;
      return;
  }
}

function executeCall(reaction: CallReaction, runtimePlan: RuntimePlan, context: ExecutionContext): void {
  const args = reaction.args.map(arg => evaluateValue(arg, runtimePlan.document, context.raw));

  switch (reaction.on.kind) {
    case "component":
    case "plugin":
      log.trace("call", {
        target: reaction.on.kind === "component" ? reaction.on.component : reaction.on.name,
        method: reaction.method,
        args,
      });
      runtimePlan.objectForSource(reaction.on).call(reaction.method, args);
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

function executeDispatch(reaction: DispatchReaction, runtimePlan: RuntimePlan, context: ExecutionContext): void {
  const detail = dispatchPayload(reaction, runtimePlan.document, context);
  log.trace("dispatch", { event: reaction.event, detail });
  document.dispatchEvent(new CustomEvent(reaction.event, { detail }));
}

function executeInject(reaction: InjectReaction, runtimePlan: RuntimePlan, context: ExecutionContext): void {
  const container = runtimePlan.components.element(reaction.slot);
  const value = evaluateValue(reaction.value, runtimePlan.document, context.raw);
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
  runtimePlan: RuntimePlan,
  context: ExecutionContext,
): void {
  const payload: ServerValidationPayload = context.serverValidationPayload();
  if (payload.kind === "available") {
    log.debug("show-validation.server", { id: reaction.container });
    showServerErrors(runtimePlan.document, reaction.container, payload.response);
    return;
  }

  log.debug("show-validation.client", { id: reaction.container });
  validateContainer(runtimePlan.document, reaction.container, context.raw);
}

function reportParallelStepFailures(results: readonly PromiseSettledResult<void>[]): void {
  for (const result of results) {
    if (result.status === "rejected") {
      log.error("parallel.step-failed", { error: toJavaScriptString(result.reason) });
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

function dispatchPayload(reaction: DispatchReaction, planDocument: PlanDocument, context: ExecutionContext): unknown {
  if (reaction.payload.kind === "none") return {};

  return evaluateValue(reaction.payload.data, planDocument, context.raw);
}

function executeBranchFrom(
  cases: readonly BranchCase[],
  runtimePlan: RuntimePlan,
  context: ExecutionContext,
  startIndex: number,
): ReactionCompletion {
  for (let index = startIndex; index < cases.length; index++) {
    const branchCase = cases[index]!;

    const guardMatches = branchGuardMatches(branchCase, runtimePlan.document, context);
    if (guardMatches instanceof Promise) {
      return executeAfterAsyncBranchGuard(cases, runtimePlan, context, index, branchCase, guardMatches);
    }

    if (guardMatches) return executeReactionWith(branchCase.reaction, runtimePlan, context);
  }

  log.trace("branch.no-match", { caseCount: cases.length });
}

async function executeAfterAsyncBranchGuard(
  cases: readonly BranchCase[],
  runtimePlan: RuntimePlan,
  context: ExecutionContext,
  index: number,
  branchCase: BranchCase,
  guardMatches: Promise<boolean>,
): Promise<void> {
  if (await guardMatches) {
    await waitForAsyncBoundary(executeReactionWith(branchCase.reaction, runtimePlan, context));
    return;
  }

  await waitForAsyncBoundary(executeBranchFrom(cases, runtimePlan, context, index + 1));
}

function branchGuardMatches(
  branchCase: BranchCase,
  planDocument: PlanDocument,
  context: ExecutionContext,
): boolean | Promise<boolean> {
  switch (branchCase.guard.kind) {
    case "default":
      return true;
    case "when":
      return evaluateConditionInCurrentLane(branchCase.guard.condition, planDocument, context.raw);
    default:
      return assertNever(branchCase.guard, "branch guard");
  }
}
