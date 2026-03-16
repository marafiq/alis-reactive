// Boot — Plan lifecycle: boot, merge, reset
//
// Single responsibility: wire triggers (two-phase) and register plans.
// Delegates enrichment to enrichment.ts, state to merge-plan.ts PlanRegistry.

import type { Plan, Entry, ComponentEntry } from "./types";
import { setLevel } from "./trace";
import { scope } from "./trace";
import { wireTrigger } from "./trigger";
import { enrichEntries } from "./enrichment";
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

export function mergePlan(incoming: Plan): void {
  const merged = applyMergedPlan(incoming, { enrichEntries, wireEntries });
  log.info("merge", { planId: merged.planId, newComponents: Object.keys(incoming.components).length });
}

export function getBootedPlan(planId: string): Plan | undefined {
  return getTrackedBootedPlan(planId);
}

export function resetBootStateForTests(): void {
  bootAbort.abort();
  bootAbort = new AbortController();
  resetMergePlanState();
}

export const trace = { setLevel };
