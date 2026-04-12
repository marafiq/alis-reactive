// Boot — Plan lifecycle: boot, merge, reset.
// Single responsibility: wire behaviors (two-phase) and register plans.
// Delegates state to merge-plan.ts PlanRegistry.

import type { Plan, Behavior } from "../types";
import { tracer } from "../tracing";
import { wireBehavior } from "../execution/trigger";
import { setActivePlan } from "../execution/execute";
import { wireLiveValidation } from "../validation/live-clear";
import { findSummaryElement, clearSummary, hideSummaryDiv } from "../validation/error-display";
import {
  applyMergedPlan,
  getBootedPlan as getTrackedBootedPlan,
  registerBootedPlan,
  resetMergePlanState,
} from "./merge-plan";

const t = tracer("boot");
const BOOTED_ATTR = "alisBooted";

let bootAbort = new AbortController();

export function boot(plan: Plan): void {
  t.info("boot.start", { planId: plan.planId, behaviors: plan.behaviors.length });

  // Wire validation live-clear for components with container scopes
  wireContainerValidation(plan);

  // Two-phase behavior wiring
  wireBehaviors(plan.behaviors, plan, bootAbort.signal);

  setActivePlan(plan);
  registerBootedPlan(plan);
  document.documentElement.dataset[BOOTED_ATTR] = "true";
  t.info("boot.complete", { planId: plan.planId });
}

/**
 * Two-phase wiring: wire all non-page-ready listeners first, then execute page-ready.
 * This ensures document-event listeners exist before page-ready dispatches into them.
 */
function wireBehaviors(behaviors: Behavior[], plan: Plan, signal?: AbortSignal): void {
  const deferred: Behavior[] = [];
  for (const behavior of behaviors) {
    if (behavior.startsWhen.kind === "page-ready") {
      deferred.push(behavior);
    } else {
      wireBehavior(behavior.startsWhen, behavior.reaction, plan, signal);
    }
  }
  for (const behavior of deferred) {
    wireBehavior(behavior.startsWhen, behavior.reaction, plan, signal);
  }
}

/** Wire live validation for all components that have container scopes. */
function wireContainerValidation(plan: Plan): void {
  for (const [key, comp] of Object.entries(plan.components)) {
    if (comp.container) {
      wireLiveValidation(plan, key);
    }
  }
}

export function mergePlan(incoming: Plan): void {
  const merged = applyMergedPlan(incoming, {
    wireBehaviors,
    wireContainerValidation,
  });

  clearSummaryForPlan(merged.planId);

  t.info("plan.merge", { planId: merged.planId, newComponents: Object.keys(incoming.components).length });
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


function clearSummaryForPlan(planId: string): void {
  const el = findSummaryElement(planId);
  if (el) {
    clearSummary(el);
    hideSummaryDiv(el);
  }
}
