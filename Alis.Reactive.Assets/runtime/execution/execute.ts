// execute.ts - Reaction executor. The dumb runtime.
// Dispatches on reaction.kind. Uses shared resolver for ALL component access.
// No fallbacks. Every component reference must be in plan.components.

import type {
  Plan, Reaction, SequenceReaction, ParallelReaction, BranchReaction,
  BranchCase, SetReaction, CallReaction, DispatchReaction,
  InjectReaction, ShowValidationErrorsReaction,
  ExecContext, Condition, PayloadSource, Source,
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
import { PlainObjectRecord } from "../domain/object-record";

const log = scope("execute");

let activeRuntimePlan: RuntimePlan | undefined;

export function setActivePlan(plan: Plan): void {
  activeRuntimePlan = RuntimePlan.from(plan);
}

export function resetActivePlanForTests(): void {
  activeRuntimePlan = undefined;
}

function runtimePlanFor(plan: Plan | undefined): RuntimePlan {
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
  plan?: Plan,
  ctx?: ExecContext,
): void | Promise<void> {
  return ReactionExecution.start(plan, ctx).execute(reaction);
}

class ReactionExecution {
  private constructor(
    private readonly plan: RuntimePlan,
    private readonly context: ExecutionContext,
  ) {}

  static start(plan: Plan | undefined, context: ExecContext | undefined): ReactionExecution {
    return new ReactionExecution(runtimePlanFor(plan), ExecutionContext.from(context));
  }

  execute(reaction: Reaction): void | Promise<void> {
    switch (reaction.kind) {
      case "set":
        this.executeSet(reaction);
        return;

      case "call":
        this.executeCall(reaction);
        return;

      case "dispatch":
        this.executeDispatch(reaction);
        return;

      case "inject":
        this.executeInject(reaction);
        return;

      case "show-validation-errors":
        this.executeShowValidationErrors(reaction);
        return;

      case "sequence":
        return this.executeSequence(reaction);

      case "branch":
        return this.executeBranch(reaction);

      case "request":
        return executeRequest(reaction.request, this.plan.document, this.context.raw);

      case "parallel":
        return this.executeParallel(reaction);

      default:
        assertNever(reaction, "reaction kind");
    }
  }

  private executeSequence(reaction: SequenceReaction): void | Promise<void> {
    for (const [index, step] of reaction.steps.entries()) {
      const completion = ReactionCompletion.from(this.execute(step));
      if (completion.isAsync) {
        const remaining = reaction.steps.slice(index + 1);
        return completion.thenRun(() => this.executeRemainingSequence(remaining));
      }
    }
  }

  private async executeRemainingSequence(steps: readonly Reaction[]): Promise<void> {
    for (const step of steps) {
      const completion = ReactionCompletion.from(this.execute(step));
      if (completion.isAsync) await completion.wait();
    }
  }

  private executeBranch(reaction: BranchReaction): void | Promise<void> {
    return BranchReactionExecution.start(
      reaction,
      this.plan.document,
      this.context,
      child => this.execute(child),
    ).execute();
  }

  private async executeParallel(reaction: ParallelReaction): Promise<void> {
    await ParallelReactionExecution
      .from(reaction, child => this.execute(child))
      .run();
  }

  private executeSet(reaction: SetReaction): void {
    const value = evaluateValue(reaction.value, this.plan.document, this.context.raw);
    const target = ReactionTarget.forSet(reaction.on, this.plan, this.context);
    log.trace("set", { target: target.label, property: reaction.property, value });
    target.set(reaction.property, value);
  }

  private executeCall(reaction: CallReaction): void {
    const args = reaction.args.map(arg => evaluateValue(arg, this.plan.document, this.context.raw));
    const target = ReactionTarget.forCall(reaction.on, this.plan, this.context);
    log.trace("call", { target: target.label, method: reaction.method, args });
    target.call(reaction.method, args);
  }

  private executeDispatch(reaction: DispatchReaction): void {
    const detail = DispatchPayload.from(reaction, this.plan.document, this.context);
    log.trace("dispatch", { event: reaction.event, detail });
    document.dispatchEvent(new CustomEvent(reaction.event, { detail }));
  }

  private executeInject(reaction: InjectReaction): void {
    const container = this.plan.components.element(reaction.target.component);
    const value = evaluateValue(reaction.value, this.plan.document, this.context.raw);
    if (typeof value === "string") {
      injectHtml(container, value, reaction.target);
      log.trace("inject.applied", { component: reaction.target.component, target: reaction.target.kind, size: value.length });
      return;
    }

    log.error("inject.wrong-type", { component: reaction.target.component, type: typeof value });
    throw new Error(`[alis] inject expects string HTML, got ${typeof value}`);
  }

  private executeShowValidationErrors(reaction: ShowValidationErrorsReaction): void {
    const payload = this.serverValidationPayload();
    if (payload.kind === "available") {
      log.debug("show-validation.server", { id: reaction.container });
      showServerErrors(this.plan.document, reaction.container, payload.response);
      return;
    }

    log.debug("show-validation.client", { id: reaction.container });
    validateContainer(this.plan.document, reaction.container, this.context.raw);
  }

  private serverValidationPayload(): ServerValidationPayload {
    return this.context.serverValidationPayload();
  }
}

type ReactionRunner = (reaction: Reaction) => void | Promise<void>;

class ParallelReactionExecution {
  private constructor(
    private readonly reaction: ParallelReaction,
    private readonly runReaction: ReactionRunner,
  ) {}

  static from(reaction: ParallelReaction, runReaction: ReactionRunner): ParallelReactionExecution {
    return new ParallelReactionExecution(reaction, runReaction);
  }

  async run(): Promise<void> {
    const settledSteps = await ParallelStepSettlements.from(this.reaction.steps, this.runReaction);
    settledSteps.reportFailures();
    await ParallelCompletionExecution.from(this.reaction.completion, this.runReaction).run();
  }
}

class ParallelStepSettlements {
  private constructor(private readonly results: PromiseSettledResult<void>[]) {}

  static async from(
    steps: readonly Reaction[],
    runReaction: ReactionRunner,
  ): Promise<ParallelStepSettlements> {
    const results = await Promise.allSettled(
      steps.map(step => ReactionCompletion.from(runReaction(step)).asPromise())
    );
    return new ParallelStepSettlements(results);
  }

  reportFailures(): void {
    for (const result of this.results) {
      if (result.status === "rejected") {
        log.error("parallel.step-failed", { error: String(result.reason) });
      }
    }
  }
}

abstract class ParallelCompletionExecution {
  static from(
    completion: ParallelReaction["completion"],
    runReaction: ReactionRunner,
  ): ParallelCompletionExecution {
    const parallelRunsCompletion = completion.kind === "on-settled";
    if (parallelRunsCompletion) {
      return new SettledParallelCompletionExecution(completion.reaction, runReaction);
    }

    return NoParallelCompletionExecution.instance;
  }

  abstract run(): Promise<void>;
}

class NoParallelCompletionExecution extends ParallelCompletionExecution {
  static readonly instance = new NoParallelCompletionExecution();

  async run(): Promise<void> {
    return;
  }
}

class SettledParallelCompletionExecution extends ParallelCompletionExecution {
  constructor(
    private readonly reaction: Reaction,
    private readonly runReaction: ReactionRunner,
  ) {
    super();
  }

  async run(): Promise<void> {
    await ReactionCompletion.from(this.runReaction(this.reaction)).wait();
  }
}

interface MutableMemberTarget {
  set(member: string, value: unknown): void;
  call(member: string, args: unknown[]): void;
}

class ReactionTarget {
  private constructor(
    readonly label: string,
    private readonly target: MutableMemberTarget,
  ) {}

  static forSet(source: Source, plan: RuntimePlan, context: ExecutionContext): ReactionTarget {
    switch (source.kind) {
      case "component":
        return new ReactionTarget(source.component, plan.objectForSource(source));

      case "payload":
        return new ReactionTarget(source.scope, payloadTarget(source, plan, context, "set property"));

      default:
        throw unsupportedSource("Set reaction", source, "component and payload sources");
    }
  }

  static forCall(source: Source, plan: RuntimePlan, context: ExecutionContext): ReactionTarget {
    switch (source.kind) {
      case "component":
        return new ReactionTarget(source.component, plan.objectForSource(source));

      case "plugin":
        return new ReactionTarget(source.name, plan.objectForSource(source));

      case "payload":
        return new ReactionTarget(source.scope, payloadTarget(source, plan, context, "call method"));

      default:
        throw unsupportedSource("Call reaction", source, "component, payload, and plugin sources");
    }
  }

  set(member: string, value: unknown): void {
    this.target.set(member, value);
  }

  call(member: string, args: unknown[]): void {
    this.target.call(member, args);
  }
}

function payloadTarget(
  source: PayloadSource,
  plan: RuntimePlan,
  context: ExecutionContext,
  operation: string,
): MutablePayloadObject {
  const root = plan.resolvePayload(source, context);
  return MutablePayloadObject.require(root, source, operation);
}

export abstract class ReactionCompletion {
  static from(result: void | Promise<void>): ReactionCompletion {
    const reactionContinuesAsync = result instanceof Promise;
    if (reactionContinuesAsync) return new AsyncReactionCompletion(result);

    return SyncReactionCompletion.instance;
  }

  abstract get isAsync(): boolean;

  abstract asPromise(): Promise<void>;

  abstract wait(): Promise<void>;

  abstract catchAsync(onRejected: (error: unknown) => void): void;

  thenRun(next: () => Promise<void>): Promise<void> {
    return this.asPromise().then(next);
  }
}

class SyncReactionCompletion extends ReactionCompletion {
  static readonly instance = new SyncReactionCompletion();

  get isAsync(): boolean {
    return false;
  }

  asPromise(): Promise<void> {
    return Promise.resolve();
  }

  async wait(): Promise<void> {
    return;
  }

  catchAsync(): void {
    return;
  }
}

class AsyncReactionCompletion extends ReactionCompletion {
  constructor(private readonly pending: Promise<void>) {
    super();
  }

  get isAsync(): boolean {
    return true;
  }

  asPromise(): Promise<void> {
    return this.pending;
  }

  async wait(): Promise<void> {
    await this.pending;
  }

  catchAsync(onRejected: (error: unknown) => void): void {
    this.pending.catch(onRejected);
  }
}

class DispatchPayload {
  static from(reaction: DispatchReaction, plan: Plan, context: ExecutionContext): unknown {
    if (reaction.payload.kind === "none") return {};

    return evaluateValue(reaction.payload.data, plan, context.raw);
  }
}

class MutablePayloadObject {
  private constructor(
    private readonly root: Record<string, unknown>,
    private readonly scope: string,
  ) {}

  static require(root: unknown, source: PayloadSource, operation: string): MutablePayloadObject {
    const payloadRoot = MutablePayloadRoot.require(root, source, operation);
    return new MutablePayloadObject(payloadRoot.record, source.scope);
  }

  set(property: string, value: unknown): void {
    this.root[property] = value;
  }

  call(method: string, args: unknown[]): void {
    const member = this.root[method];
    const memberIsCallable = typeof member === "function";
    if (!memberIsCallable) {
      throw new Error(`[alis] "${method}" is not a function on payload (scope: ${this.scope})`);
    }

    member.apply(this.root, args);
  }
}

class MutablePayloadRoot {
  private constructor(readonly record: Record<string, unknown>) {}

  static require(root: unknown, source: PayloadSource, operation: string): MutablePayloadRoot {
    const payloadWasProvided = !isMissingRuntimeValue(root);
    if (!payloadWasProvided) {
      throw new Error(`[alis] cannot ${operation} on null payload (scope: ${source.scope})`);
    }

    const payload = PlainObjectRecord.tryFrom(root);
    const payloadCanHoldMembers = payload !== undefined;
    if (!payloadCanHoldMembers) {
      throw new Error(`[alis] cannot ${operation} on ${typeof root} payload (scope: ${source.scope})`);
    }

    return new MutablePayloadRoot(payload.raw);
  }
}

class BranchReactionExecution {
  private constructor(
    private readonly reaction: BranchReaction,
    private readonly plan: Plan,
    private readonly context: ExecutionContext,
    private readonly executeReaction: (reaction: Reaction) => void | Promise<void>,
  ) {}

  static start(
    reaction: BranchReaction,
    plan: Plan,
    context: ExecutionContext,
    executeReaction: (reaction: Reaction) => void | Promise<void>,
  ): BranchReactionExecution {
    return new BranchReactionExecution(reaction, plan, context, executeReaction);
  }

  execute(): void | Promise<void> {
    const context = {
      cases: this.reaction.cases,
      plan: this.plan,
      context: this.context,
      executeReaction: this.executeReaction,
    };

    return SequentialBranchExecution.from(context).execute();
  }
}

interface BranchExecutionContext {
  readonly cases: BranchCase[];
  readonly plan: Plan;
  readonly context: ExecutionContext;
  readonly executeReaction: (reaction: Reaction) => void | Promise<void>;
}

class SequentialBranchExecution {
  private constructor(private readonly context: BranchExecutionContext) {}

  static from(context: BranchExecutionContext): SequentialBranchExecution {
    const branchHasNoCases = context.cases.length === 0;
    if (branchHasNoCases) throw new Error("[alis] branch reaction requires at least one case");

    return new SequentialBranchExecution(context);
  }

  execute(): void | Promise<void> {
    return this.executeFrom(0);
  }

  private executeFrom(startIndex: number): void | Promise<void> {
    for (let index = startIndex; index < this.context.cases.length; index++) {
      const branchCase = this.context.cases[index];
      if (branchCase === undefined) {
        throw new Error(`[alis] branch reaction case ${index} is missing`);
      }

      const guardMatches = BranchCondition.from(branchCase).matches(this.context);
      if (guardMatches instanceof Promise) {
        return this.executeAfterAsyncGuard(index, branchCase, guardMatches);
      }

      if (guardMatches) return this.context.executeReaction(branchCase.reaction);
    }

    log.trace("branch.no-match", { caseCount: this.context.cases.length });
  }

  private async executeAfterAsyncGuard(
    index: number,
    branchCase: BranchCase,
    guardMatches: Promise<boolean>,
  ): Promise<void> {
    if (await guardMatches) {
      await ReactionCompletion.from(this.context.executeReaction(branchCase.reaction)).wait();
      return;
    }

    await ReactionCompletion.from(this.executeFrom(index + 1)).wait();
  }
}

abstract class BranchCondition {
  static from(branchCase: BranchCase): BranchCondition {
    switch (branchCase.guard.kind) {
      case "default":
        return DefaultBranchCondition.instance;
      case "when":
        return new GuardedBranchCondition(branchCase.guard.condition);
      default:
        return assertNever(branchCase.guard, "branch guard");
    }
  }

  abstract matches(context: BranchExecutionContext): boolean | Promise<boolean>;
}

class DefaultBranchCondition extends BranchCondition {
  static readonly instance = new DefaultBranchCondition();

  matches(): boolean {
    return true;
  }
}

class GuardedBranchCondition extends BranchCondition {
  constructor(private readonly condition: Condition) {
    super();
  }

  matches(context: BranchExecutionContext): boolean | Promise<boolean> {
    return evaluateConditionInCurrentLane(this.condition, context.plan, context.context.raw);
  }
}

function unsupportedSource(owner: string, source: Source, expected: string): Error {
  return new Error(
    `[alis] ${owner} does not support source kind "${source.kind}". Only ${expected} can be targets.`
  );
}
