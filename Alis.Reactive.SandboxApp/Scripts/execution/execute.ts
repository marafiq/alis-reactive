// execute.ts — The main reaction executor.
// Dispatches on Reaction.kind using the V3 reaction tree.
// Every action uses the SHARED resolver (resolveSource + JsType).

import type {
  Reaction, SetReaction, CallReaction, DispatchReaction,
  InjectReaction, ShowValidationErrorsReaction, Plan, ValueProducer,
} from "../types";
import type { ExecContext } from "../types";
import { scope } from "../core/trace";
import {
  resolveSource, getJsTypeForSource,
  setProperty, callMethod, readProperty,
} from "../resolution/resolver";
import { evaluateConditionAsync, setValueEvaluator } from "../conditions/conditions";
import { executeRequest } from "./http";
import { injectHtml } from "./inject";
import { assertNever } from "../core/assert-never";
import { applyShape } from "../core/coerce";
import { walkPath, walk } from "../core/walk";

const log = scope("execute");

/** Global plan reference — set by boot, used by executor. */
let activePlan: Plan | undefined;

export function setActivePlan(plan: Plan): void {
  activePlan = plan;
}

export function getActivePlan(): Plan | undefined {
  return activePlan;
}

export function clearActivePlanForTests(): void {
  activePlan = undefined;
}

function requirePlan(plan?: Plan): Plan {
  const p = plan ?? activePlan;
  if (!p) throw new Error("[alis] no active plan — was boot() called?");
  return p;
}

// ── Main dispatch ──────────────────────────────────────────

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
      showValidationErrors(reaction, p, ctx);
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
  log.trace("set", { property: reaction.property, value });
  setProperty(root, prop, value);
}

// ── Call reaction ──────────────────────────────────────────

function executeCall(reaction: CallReaction, plan: Plan, ctx?: ExecContext): void {
  const root = resolveSource(plan, reaction.on, ctx);
  const jsType = getJsTypeForSource(plan, reaction.on);
  const method = jsType.methods?.[reaction.method];
  if (!method) throw new Error(`[alis] method "${reaction.method}" not found on type`);
  const args = reaction.args?.map(a => evaluateValue(a, plan, ctx)) ?? [];
  log.trace("call", { method: reaction.method, args });
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

function showValidationErrors(
  reaction: ShowValidationErrorsReaction,
  plan: Plan,
  ctx?: ExecContext,
): void {
  // Delegates to the validation module
  // The container key identifies which ContainerScope holds the rules
  const comp = plan.components[reaction.container];
  if (!comp) throw new Error(`[alis] container component not found: ${reaction.container}`);
  if (!comp.container) throw new Error(`[alis] component "${reaction.container}" has no container scope`);

  // Import validation dynamically to avoid circular deps
  import("../validation").then(({ validateContainer }) => {
    validateContainer(plan, reaction.container, ctx);
  });
}

// ── Value evaluation ───────────────────────────────────────

export function evaluateValue(producer: ValueProducer, plan: Plan, ctx?: ExecContext): unknown {
  switch (producer.kind) {
    case "literal":
      return applyShape(producer.value, producer.shape);

    case "read": {
      const root = resolveSource(plan, producer.from, ctx);
      if (producer.from.kind === "component") {
        const jsType = getJsTypeForSource(plan, producer.from);
        const prop = jsType.properties?.[producer.member];
        if (prop) {
          return applyShape(readProperty(root, prop), producer.shape ?? prop.shape);
        }
        const method = jsType.methods?.[producer.member];
        if (method) {
          return applyShape(callMethod(root, method, []), producer.shape);
        }
        throw new Error(`[alis] member "${producer.member}" not found on type`);
      }
      // For payload sources: member is a dot-path (e.g. "evt.address.city" or "responseBody.data.name").
      // The prefix (evt, responseBody, etc.) was already resolved by resolveSource — strip it and walk the rest.
      // If a structured path exists, prefer it; otherwise walk the member as a dot-path on the payload root.
      if (producer.path) {
        return applyShape(walkPath(root as any, producer.path), producer.shape);
      }
      // Walk the member as a dot-path, skipping the prefix segment that resolveSource already resolved
      const dotParts = producer.member.split(".");
      // The first segment is the scope prefix (evt, responseBody, etc.) — skip it
      const valueParts = dotParts.length > 1 ? dotParts.slice(1) : dotParts;
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

// Break circular dependency: inject evaluateValue into conditions module
setValueEvaluator(evaluateValue);
