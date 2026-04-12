// execute.ts — Reaction executor. The dumb runtime.
// Dispatches on reaction.kind. Uses shared resolver for ALL component access.
// No fallbacks. Every component reference must be in plan.components.

import type {
  Plan, Reaction, SequenceReaction, ParallelReaction, BranchReaction,
  SetReaction, CallReaction, DispatchReaction,
  InjectReaction, ShowValidationErrorsReaction,
  ExecContext, Condition,
} from "../types";
import {
  resolveSource, resolveElement, getJsTypeForSource,
  setProperty, callMethod,
} from "../resolution/resolver";
import { evaluateValue } from "../core/evaluate";
export { evaluateValue };
import { evaluateCondition, evaluateConditionAsync } from "../conditions/conditions";
import { setValueEvaluator } from "../conditions/conditions";
import { validateContainer, showServerErrors } from "../validation";
import { executeRequest } from "./http";
import { injectHtml } from "./inject";
import { assertNever } from "../core/assert-never";
import { tracer } from "../tracing";

const t = tracer("execute");

let activePlan: Plan | undefined;

export function setActivePlan(plan: Plan): void {
  activePlan = plan;
}

function requirePlan(plan?: Plan): Plan {
  return plan ?? activePlan ?? (() => { throw new Error("[alis] no active plan"); })();
}

// Wire the value evaluator into conditions (breaks circular dep)
setValueEvaluator(evaluateValue);

// ── Main executor ─────────────────────────────────────────
//
// Returns void for sync reaction kinds (set, call, dispatch, inject,
// show-validation-errors, branch with non-confirm conditions).
// Returns Promise<void> for async kinds (request, parallel) and
// sequences/branches that contain async steps.
//
// This sync-first design ensures SF event arg mutations (args.cancel,
// args.preventDefaultAction) execute in the same tick as the event
// callback — before SF checks them synchronously.

export function executeReaction(
  reaction: Reaction,
  plan?: Plan,
  ctx?: ExecContext,
): void | Promise<void> {
  const p = requirePlan(plan);

  switch (reaction.kind) {
    // ── Sync kinds: return void ──────────────────────────
    case "set":
      executeSet(reaction, p, ctx);
      return;

    case "call":
      executeCall(reaction, p, ctx);
      return;

    case "dispatch":
      executeDispatch(reaction, p, ctx);
      return;

    case "inject":
      executeInject(reaction, p, ctx);
      return;

    case "show-validation-errors":
      executeShowValidationErrors(reaction, p, ctx);
      return;

    // ── Mixed kinds: void or Promise ─────────────────────
    case "sequence":
      return executeSequence(reaction, p, ctx);

    case "branch":
      return executeBranch(reaction, p, ctx);

    // ── Async kinds: return Promise ──────────────────────
    case "request":
      return executeRequest(reaction.request, p, ctx);

    case "parallel":
      return executeParallel(reaction, p, ctx);

    default:
      assertNever(reaction, "reaction kind");
  }
}

// ── Sequence executor ─────────────────────────────────────
//
// Runs steps synchronously until hitting an async step.
// Sync steps (set, call, dispatch, inject, branch) execute in the
// same tick. When an async step is encountered, returns a Promise
// that awaits it and continues the remaining steps.

function executeSequence(
  reaction: SequenceReaction,
  plan: Plan,
  ctx?: ExecContext,
): void | Promise<void> {
  for (let i = 0; i < reaction.steps.length; i++) {
    const result = executeReaction(reaction.steps[i], plan, ctx);
    if (result instanceof Promise) {
      // Sync prefix done. Return Promise for async step + remaining.
      const remaining = reaction.steps.slice(i + 1);
      return result.then(async () => {
        for (const step of remaining) {
          const r = executeReaction(step, plan, ctx);
          if (r instanceof Promise) await r;
        }
      });
    }
  }
  // All steps were sync — return void
}

// ── Branch executor ───────────────────────────────────────
//
// Evaluates conditions synchronously (compare, all, any, not).
// Falls back to async only when a condition contains ConfirmCondition.

function executeBranch(
  reaction: BranchReaction,
  plan: Plan,
  ctx?: ExecContext,
): void | Promise<void> {
  for (const c of reaction.cases) {
    // Confirm conditions require async — delegate entire branch
    if (c.when && hasConfirm(c.when)) {
      return executeBranchAsync(reaction, plan, ctx);
    }
    // Sync condition evaluation
    if (!c.when || evaluateCondition(c.when, plan, ctx)) {
      return executeReaction(c.reaction, plan, ctx);
    }
  }
  t.trace("branch.no-match", { caseCount: reaction.cases.length });
}

function hasConfirm(condition: Condition): boolean {
  switch (condition.kind) {
    case "confirm": return true;
    case "all": case "any": return condition.terms.some(hasConfirm);
    case "not": return hasConfirm(condition.term);
    default: return false;
  }
}

async function executeBranchAsync(
  reaction: BranchReaction,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  for (const c of reaction.cases) {
    if (!c.when || await evaluateConditionAsync(c.when, plan, ctx)) {
      const r = executeReaction(c.reaction, plan, ctx);
      if (r instanceof Promise) await r;
      return;
    }
  }
  t.trace("branch.no-match", { caseCount: reaction.cases.length });
}

// ── Parallel executor ─────────────────────────────────────

async function executeParallel(
  reaction: ParallelReaction,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  const childSpans = reaction.steps.map((_, i) => ctx?.span?.child(`parallel.step[${i}]`));
  const results = await Promise.allSettled(
    reaction.steps.map((s, i) => {
      try {
        const childCtx = childSpans[i] ? { ...ctx, span: childSpans[i] } : ctx;
        const r = executeReaction(s, plan, childCtx);
        return r instanceof Promise ? r : Promise.resolve();
      } catch (err) {
        return Promise.reject(err);
      }
    })
  );
  for (let i = 0; i < results.length; i++) {
    const r = results[i];
    if (r.status === "rejected") {
      t.error("parallel.step.fail", { stepIndex: i }, r.reason as Error);
      childSpans[i]?.end("error");
    } else {
      childSpans[i]?.end("ok");
    }
  }
  if (reaction.onSettled) {
    const r = executeReaction(reaction.onSettled, plan, ctx);
    if (r instanceof Promise) await r;
  }
}

// ── Set reaction ───────────────────────────────────────────

function executeSet(reaction: SetReaction, plan: Plan, ctx?: ExecContext): void {
  if (reaction.on.kind !== "component" && reaction.on.kind !== "payload") {
    throw new Error(`[alis] Set reaction does not support source kind "${reaction.on.kind}". Only component and payload sources can be mutation targets.`);
  }
  const root = resolveSource(plan, reaction.on, ctx);
  const value = evaluateValue(reaction.value, plan, ctx);
  const target = reaction.on.kind === "component" ? reaction.on.component : reaction.on.scope;
  t.trace("reaction.set", { component: target, property: reaction.property, value });

  if (reaction.on.kind === "payload") {
    // Payload objects (event args, response bodies) don't have JsTypes.
    // Set the property directly on the resolved payload root.
    if (root == null) throw new Error(`[alis] cannot set property on null payload (scope: ${reaction.on.scope})`);
    (root as Record<string, unknown>)[reaction.property] = value;
    return;
  }

  const jsType = getJsTypeForSource(plan, reaction.on);
  const prop = jsType.properties?.[reaction.property];
  if (!prop) throw new Error(`[alis] property "${reaction.property}" not found on type (available: ${Object.keys(jsType.properties ?? {}).join(", ")})`);
  setProperty(root, prop, value);
}

// ── Call reaction ──────────────────────────────────────────

function executeCall(reaction: CallReaction, plan: Plan, ctx?: ExecContext): void {
  if (reaction.on.kind !== "component" && reaction.on.kind !== "payload" && reaction.on.kind !== "plugin") {
    throw new Error(`[alis] Call reaction does not support source kind "${reaction.on.kind}". Only component, payload, and plugin sources can be call targets.`);
  }
  const root = resolveSource(plan, reaction.on, ctx);
  const args = reaction.args?.map(a => evaluateValue(a, plan, ctx)) ?? [];
  const target = reaction.on.kind === "component" ? reaction.on.component
    : reaction.on.kind === "plugin" ? (reaction.on as import("../types").PluginSource).name
    : reaction.on.scope;
  t.trace("reaction.call", { component: target, method: reaction.method, args });

  if (reaction.on.kind === "payload") {
    // Payload objects (event args, response bodies) don't have JsTypes.
    // Call the method directly on the resolved payload root.
    if (root == null) throw new Error(`[alis] cannot call method on null payload (scope: ${reaction.on.scope})`);
    const fn = (root as Record<string, unknown>)[reaction.method];
    if (typeof fn !== "function") throw new Error(`[alis] "${reaction.method}" is not a function on payload`);
    fn.apply(root, args);
    return;
  }

  const jsType = getJsTypeForSource(plan, reaction.on);
  const method = jsType.methods?.[reaction.method];
  if (!method) throw new Error(`[alis] method "${reaction.method}" not found on type (available: ${Object.keys(jsType.methods ?? {}).join(", ")})`);
  callMethod(root, method, args);
}

// ── Dispatch reaction ──────────────────────────────────────

function executeDispatch(reaction: DispatchReaction, plan: Plan, ctx?: ExecContext): void {
  const detail = reaction.data
    ? evaluateValue(reaction.data, plan, ctx)
    : {};
  t.trace("reaction.dispatch", { event: reaction.event, detail });
  document.dispatchEvent(new CustomEvent(reaction.event, { detail }));
}

// ── Inject reaction ────────────────────────────────────────

function executeInject(reaction: InjectReaction, plan: Plan, ctx?: ExecContext): void {
  // Every inject target MUST be registered in plan.components.
  // The C# Into() builder calls EnsureElement() to register it.
  const container = resolveElement(plan, reaction.component);
  const value = evaluateValue(reaction.value, plan, ctx);
  if (typeof value === "string") {
    injectHtml(container, value);
  } else {
    throw new Error(`[alis] inject expects string HTML, got ${typeof value}`);
  }
}

// ── Show validation errors reaction ────────────────────────

function executeShowValidationErrors(
  reaction: ShowValidationErrorsReaction,
  plan: Plan,
  ctx?: ExecContext,
): void {
  // validateContainer and showServerErrors are statically imported at the top.
  // The validation module is already loaded eagerly by boot.ts.
  // When called inside an error handler, ctx.response carries the server's
  // ProblemDetails body. Route to showServerErrors instead of client-side validation.
  if (ctx?.response && typeof ctx.response === "object") {
    showServerErrors(plan, reaction.container, ctx.response);
  } else {
    validateContainer(plan, reaction.container, ctx);
  }
}
