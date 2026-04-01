// Boot — Plan lifecycle: boot, merge, reset
//
// Single responsibility: wire triggers (two-phase) and register plans.
// Delegates enrichment to enrichment.ts, state to merge-plan.ts PlanRegistry.

import type { Plan, Entry, ComponentEntry } from "../types";
import { setLevel } from "../core/trace";
import { scope } from "../core/trace";
import { wireTriggerSequence } from "../execution/trigger";
import { enrichEntries } from "./enrichment";
import { wireLiveValidation, unwireFields } from "../validation/live-clear";
import { findSummaryElement, clearSummary, hideSummaryDiv } from "../validation/error-display";
import { walkValidationDescriptors } from "./walk-reactions";
import {
  applyMergedPlan,
  getBootedPlan as getTrackedBootedPlan,
  registerBootedPlan,
  resetMergePlanState,
} from "./merge-plan";

const log = scope("boot");
const BOOTED_ATTR = "alisBooted";

let bootAbort = new AbortController();

export function boot(plan: Plan): void {
  log.info("booting", { entries: plan.entries.length });

  enrichEntries(plan.entries, plan.components);
  walkValidationDescriptors(plan.entries, wireLiveValidation);
  wireEntries(plan.entries, plan.components, bootAbort.signal);

  registerBootedPlan(plan);
  document.documentElement.dataset[BOOTED_ATTR] = "true";
  log.info("booted");
}

/**
 * Two-phase wiring: wire all non-dom-ready listeners first, then execute dom-ready.
 * This ensures custom-event listeners exist before dom-ready dispatches into them.
 */
function wireEntries(entries: Entry[], components: Record<string, ComponentEntry>, signal?: AbortSignal): void {
  const deferred: Array<{ trigger: Entry["trigger"]; reactions: Entry["reaction"][] }> = [];
  for (const group of groupEntriesByTrigger(entries)) {
    if (group.trigger.kind === "dom-ready") {
      deferred.push(group);
    } else {
      wireTriggerSequence(group.trigger, group.reactions, components, signal);
    }
  }
  for (const group of deferred) {
    wireTriggerSequence(group.trigger, group.reactions, components, signal);
  }
}

function groupEntriesByTrigger(
  entries: Entry[]
): Array<{ trigger: Entry["trigger"]; reactions: Entry["reaction"][] }> {
  const groups = new Map<string, { trigger: Entry["trigger"]; reactions: Entry["reaction"][] }>();
  const ordered: Array<{ trigger: Entry["trigger"]; reactions: Entry["reaction"][] }> = [];

  for (const entry of entries) {
    const key = triggerKey(entry.trigger);
    let group = groups.get(key);
    if (!group) {
      group = { trigger: entry.trigger, reactions: [] };
      groups.set(key, group);
      ordered.push(group);
    }
    group.reactions.push(entry.reaction);
  }

  return ordered;
}

function triggerKey(trigger: Entry["trigger"]): string {
  switch (trigger.kind) {
    case "dom-ready":
      return "dom-ready";
    case "custom-event":
      return `custom-event:${trigger.event}`;
    case "component-event":
      return `component-event:${trigger.componentId}:${trigger.vendor}:${trigger.jsEvent}:${trigger.readExpr ?? ""}`;
    case "server-push":
      return `server-push:${trigger.url}:${trigger.eventType ?? ""}`;
    case "signalr":
      return `signalr:${trigger.hubUrl}:${trigger.methodName}`;
  }
}

export function mergePlan(incoming: Plan): void {
  const merged = applyMergedPlan(incoming, { enrichEntries, wireEntries, unwireFields });

  walkValidationDescriptors(merged.entries, wireLiveValidation);
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
  delete document.documentElement.dataset[BOOTED_ATTR];
}

export const trace = { setLevel };

function clearSummaryForPlan(planId: string): void {
  const el = findSummaryElement(planId);
  if (el) {
    clearSummary(el);
    hideSummaryDiv(el);
  }
}
