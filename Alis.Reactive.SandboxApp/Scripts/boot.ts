import type { Plan, Entry, ComponentEntry, Reaction, ValidationDescriptor } from "./types";
import { setLevel } from "./trace";
import { scope } from "./trace";
import { wireTrigger } from "./trigger";
import {
  applyMergedPlan,
  getBootedPlan as getTrackedBootedPlan,
  registerBootedPlan,
  resetMergePlanState,
} from "./merge-plan";

const log = scope("boot");

let bootAbort = new AbortController();

export function boot(plan: Plan): void {
  log.info("booting", { entries: plan.entries.length });

  enrichEntries(plan.entries, plan.components);
  wireEntries(plan.entries, plan.components, bootAbort.signal);

  registerBootedPlan(plan);
  log.info("booted");
}

/**
 * Two-phase wiring: wire all non-dom-ready listeners first, then execute dom-ready.
 * This ensures custom-event listeners exist before dom-ready dispatches into them.
 */
function wireEntries(entries: Entry[], components: Record<string, ComponentEntry>, signal?: AbortSignal): void {
  const deferred: Entry[] = [];
  for (const entry of entries) {
    if (entry.trigger.kind === "dom-ready") {
      deferred.push(entry);
    } else {
      wireTrigger(entry.trigger, entry.reaction, components, signal);
    }
  }
  for (const entry of deferred) {
    wireTrigger(entry.trigger, entry.reaction, components, signal);
  }
}

/**
 * Merge an AJAX-injected partial plan into an already-booted plan.
 * Merges components, enriches validation, wires new triggers (two-phase).
 *
 * When the incoming plan has a sourceId (set by inject.ts from container ID),
 * re-merging the same source aborts old listeners and replaces old entries,
 * preventing accumulation on partial reload.
 */
export function mergePlan(incoming: Plan): void {
  const merged = applyMergedPlan(incoming, { enrichEntries, wireEntries });
  log.info("merge", { planId: merged.planId, newComponents: Object.keys(incoming.components).length });
}

/** Returns the booted plan for a given planId (used by tests and runtime inspection). */
export function getBootedPlan(planId: string): Plan | undefined {
  return getTrackedBootedPlan(planId);
}

/** Test-only: abort all wired listeners and clear boot/merge state between Vitest runs. */
export function resetBootStateForTests(): void {
  bootAbort.abort();
  bootAbort = new AbortController();
  resetMergePlanState();
}

export const trace = { setLevel };

// -- Validation enrichment -----------------------------------------------

/** Walk entries and enrich validation fields from plan.components. */
export function enrichEntries(entries: Entry[], components: Record<string, ComponentEntry>): void {
  for (const entry of entries) {
    enrichReaction(entry.reaction, components);
  }
}

export function enrichReaction(reaction: Reaction, components: Record<string, ComponentEntry>): void {
  switch (reaction.kind) {
    case "http":
      enrichRequest(reaction.request, components);
      break;
    case "parallel-http":
      for (const req of reaction.requests) {
        enrichRequest(req, components);
      }
      break;
    case "conditional":
      for (const branch of reaction.branches) {
        enrichReaction(branch.reaction, components);
      }
      break;
  }
}

export function enrichRequest(
  req: { validation?: ValidationDescriptor; chained?: typeof req },
  components: Record<string, ComponentEntry>
): void {
  if (req.validation) {
    enrichValidationFields(req.validation, components);
  }
  if (req.chained) {
    enrichRequest(req.chained, components);
  }
}

export function enrichValidationFields(
  desc: ValidationDescriptor,
  components: Record<string, ComponentEntry>
): void {
  for (const f of desc.fields) {
    const comp = components[f.fieldName];
    if (comp) {
      f.fieldId = comp.id;
      f.vendor = comp.vendor;
      f.readExpr = comp.readExpr;
    } else {
      f.fieldId = undefined;
      f.vendor = undefined;
      f.readExpr = undefined;
      log.warn("validation field not in components", { fieldName: f.fieldName });
    }
  }
}
