// Boot — Plan lifecycle: boot, merge, reset
//
// Single responsibility: wire triggers (two-phase) and register plans.
// Delegates enrichment to enrichment.ts, state to merge-plan.ts PlanRegistry.

import type { Plan, Entry, ComponentEntry } from "./types";
import { setLevel } from "./core/trace";
import { scope } from "./core/trace";
import { wireTrigger } from "./trigger";
import { enrichEntries } from "./enrichment";
import { wireLiveClearing } from "./validation/live-clear";
import { findSummaryElement, clearSummary, hideSummaryDiv } from "./validation/error-display";
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
  wireLiveClearingForEntries(plan.entries);
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

  wireLiveClearingForEntries(merged.entries);
  clearSummaryForPlan(merged.planId);

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

function wireLiveClearingForEntries(entries: Entry[]): void {
  for (const entry of entries) {
    wireLiveClearingForReaction(entry.reaction);
  }
}

function wireLiveClearingForReaction(reaction: { kind: string; request?: any; requests?: any[]; branches?: any[] }): void {
  switch (reaction.kind) {
    case "http":
      if (reaction.request?.validation) wireLiveClearing(reaction.request.validation);
      break;
    case "parallel-http":
      for (const req of reaction.requests ?? []) {
        if (req.validation) wireLiveClearing(req.validation);
      }
      break;
    case "conditional":
      for (const branch of reaction.branches ?? []) wireLiveClearingForReaction(branch.reaction);
      break;
  }
}

function clearSummaryForPlan(planId: string): void {
  const el = findSummaryElement(planId);
  if (el) {
    clearSummary(el);
    hideSummaryDiv(el);
  }
}
