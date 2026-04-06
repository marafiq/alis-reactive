// execute.ts — Reaction executor. The dumb runtime.
// Dispatches on reaction.kind. Uses shared resolver for ALL component access.
// No fallbacks. Every component reference must be in plan.components.

import type {
  Plan, Reaction, SequenceReaction, ParallelReaction, BranchReaction,
  SetReaction, CallReaction, RequestReaction, DispatchReaction,
  InjectReaction, ShowValidationErrorsReaction,
  ValueProducer, ExecContext, Source,
} from "../types";
import {
  resolveSource, getJsTypeForSource, readProperty as resolverReadProperty,
  setProperty, callMethod,
} from "../resolution/resolver";
import { evaluateConditionAsync } from "../conditions/conditions";
import { setValueEvaluator } from "../conditions/conditions";
import { executeRequest } from "./http";
import { injectHtml } from "./inject";
import { assertNever } from "../core/assert-never";
import { applyShape } from "../core/shape-convert";
import { walk } from "../core/walk";
import { walkPath } from "../core/walk";
import { scope } from "../core/trace";

const log = scope("execute");

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

export async function executeReaction(
  reaction: Reaction,
  plan?: Plan,
  ctx?: ExecContext,
): Promise<void> {
  const p = requirePlan(plan);

  switch (reaction.kind) {
    case "sequence":
      for (const step of reaction.steps) {
        await executeReaction(step, p, ctx);
      }
      return;

    case "parallel": {
      const results = await Promise.allSettled(reaction.steps.map(s => executeReaction(s, p, ctx)));
      for (const r of results) {
        if (r.status === "rejected") log.error("parallel step failed", { error: String(r.reason) });
      }
      if (reaction.onSettled) {
        await executeReaction(reaction.onSettled, p, ctx);
      }
      return;
    }

    case "branch":
      for (const c of reaction.cases) {
        if (!c.when || await evaluateConditionAsync(c.when, p, ctx)) {
          await executeReaction(c.reaction, p, ctx);
          return;
        }
      }
      log.trace("no-branch-taken");
      return;

    case "set":
      executeSet(reaction, p, ctx);
      return;

    case "call":
      executeCall(reaction, p, ctx);
      return;

    case "request":
      await executeRequest(reaction.request, p, ctx);
      return;

    case "dispatch":
      executeDispatch(reaction, p, ctx);
      return;

    case "inject":
      executeInject(reaction, p, ctx);
      return;

    case "show-validation-errors":
      await showValidationErrors(reaction, p, ctx);
      return;

    default:
      assertNever(reaction, "reaction kind");
  }
}

// ── Set reaction ───────────────────────────────────────────

function executeSet(reaction: SetReaction, plan: Plan, ctx?: ExecContext): void {
  const root = resolveSource(plan, reaction.on, ctx);
  const jsType = getJsTypeForSource(plan, reaction.on);
  const prop = jsType.properties?.[reaction.property];
  if (!prop) throw new Error(`[alis] property "${reaction.property}" not found on type`);
  const value = evaluateValue(reaction.value, plan, ctx);
  log.trace("set", { target: reaction.on.kind === "component" ? reaction.on.component : reaction.on.scope, property: reaction.property, value });
  setProperty(root, prop, value);
}

// ── Call reaction ──────────────────────────────────────────

function executeCall(reaction: CallReaction, plan: Plan, ctx?: ExecContext): void {
  const root = resolveSource(plan, reaction.on, ctx);
  const jsType = getJsTypeForSource(plan, reaction.on);
  const method = jsType.methods?.[reaction.method];
  if (!method) throw new Error(`[alis] method "${reaction.method}" not found on type`);
  const args = reaction.args?.map(a => evaluateValue(a, plan, ctx)) ?? [];
  log.trace("call", { target: reaction.on.kind === "component" ? reaction.on.component : reaction.on.scope, method: reaction.method, args });
  callMethod(root, method, args);
}

// ── Dispatch reaction ──────────────────────────────────────

function executeDispatch(reaction: DispatchReaction, plan: Plan, ctx?: ExecContext): void {
  const detail = reaction.data
    ? evaluateValue(reaction.data, plan, ctx)
    : {};
  log.trace("dispatch", { event: reaction.event, detail });
  document.dispatchEvent(new CustomEvent(reaction.event, { detail }));
}

// ── Inject reaction ────────────────────────────────────────

function executeInject(reaction: InjectReaction, plan: Plan, ctx?: ExecContext): void {
  // Every inject target MUST be registered in plan.components.
  // The C# Into() builder calls EnsureElement() to register it.
  const comp = plan.components[reaction.component];
  if (!comp) throw new Error(`[alis] inject target component not found: ${reaction.component}`);
  const container = document.getElementById(comp.id);
  if (!container) throw new Error(`[alis] inject target element not found: ${comp.id}`);
  const value = evaluateValue(reaction.value, plan, ctx);
  if (typeof value === "string") {
    injectHtml(container, value);
  } else {
    throw new Error(`[alis] inject expects string HTML, got ${typeof value}`);
  }
}

// ── Show validation errors reaction ────────────────────────

async function showValidationErrors(
  reaction: ShowValidationErrorsReaction,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  const { validateContainer } = await import("../validation");
  validateContainer(plan, reaction.container, ctx);
}

// ── Value evaluation ──────────────────────────────────────

export function evaluateValue(producer: ValueProducer, plan: Plan, ctx?: ExecContext): unknown {
  switch (producer.kind) {
    case "literal":
      return applyShape(producer.value, producer.shape);

    case "read": {
      const root = resolveSource(plan, producer.from, ctx);

      // Component source: look up member in JsType
      if (producer.from.kind === "component") {
        const jsType = getJsTypeForSource(plan, producer.from);
        const prop = jsType.properties?.[producer.member];
        if (prop) {
          return applyShape(resolverReadProperty(root, prop), producer.shape ?? prop.shape);
        }
        const method = jsType.methods?.[producer.member];
        if (method) {
          return applyShape(callMethod(root, method, []), producer.shape);
        }
        throw new Error(`[alis] member "${producer.member}" not found on type`);
      }
      // Payload source: walk member as dot-path on resolved payload
      if (producer.path) {
        
        return applyShape(walkPath(root as any, producer.path), producer.shape);
      }
      // Walk the member as a dot-path, skipping the scope prefix
      const dotParts = producer.member.split(".");
      if (dotParts.length <= 1) {
        return applyShape(root, producer.shape);
      }
      const valueParts = dotParts.slice(1);
      return applyShape(walk(root as any, valueParts.join(".")), producer.shape);
    }

    case "object": {
      const result: Record<string, unknown> = {};
      for (const [key, val] of Object.entries(producer.fields)) {
        result[key] = evaluateValue(val, plan, ctx);
      }
      return result;
    }

    case "array":
      return producer.items.map(i => evaluateValue(i, plan, ctx));

    default:
      assertNever(producer, "value producer kind");
  }
}

// Import helper for property reading
