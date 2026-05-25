// Boot — Plan lifecycle: boot, partial slot load/unload, reset.
// Single responsibility: wire behaviors (two-phase) and register plans.
// Delegates applied plan state to merge-plan.ts.

import type { Plan, Behavior } from "../types";
import { setLevel } from "../core/trace";
import { scope } from "../core/trace";
import { wireBehavior } from "../execution/trigger";
import { resetActivePlanForTests, setActivePlan } from "../execution/execute";
import { resetLiveClearForTests, wireLiveValidation } from "../validation/live-clear";
import { findSummaryElement, clearSummary, hideSummaryDiv } from "../validation/error-display";
import { resetNativeActionLinksForTests } from "../components/native/native-action-link";
import { resetPluginRegistryForTests } from "../core/plugin-registry";
import {
  applyPartialSlotLoad,
  applyPartialSlotUnload,
  getBootedPlan as getTrackedBootedPlan,
  type MergeHooks,
  registerBootedPlan,
  resetMergePlanState,
} from "./merge-plan";

const log = scope("boot");
const BOOTED_ATTR = "alisBooted";

let bootAbort = new AbortController();

interface ReactiveBootState {
  readonly booted: true;
  readonly planId: string;
}

interface ReactiveBootWindow extends Window {
  __alisReactiveBoot?: ReactiveBootState;
}

export function boot(plan: Plan): void {
  log.info("booting", { planId: plan.planId, behaviors: plan.behaviors.length });

  // Wire validation live-clear for components with container scopes
  wireContainerValidation(plan, bootAbort.signal);

  // Two-phase behavior wiring
  wireBehaviors(plan.behaviors, plan, bootAbort.signal);

  setActivePlan(plan);
  registerBootedPlan(plan);
  markReactiveBooted(plan);
  log.info("booted", { planId: plan.planId });
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

function wireContainerValidation(plan: Plan, signal?: AbortSignal): void {
  for (const [componentKey, component] of Object.entries(plan.components)) {
    if (component.container.kind !== "none") {
      wireLiveValidation(plan, componentKey, signal);
    }
  }
}

export function loadPartialSlot(partId: string, incoming: Plan[]): void {
  const affectedPlanIds = applyPartialSlotLoad(partId, incoming, mergeHooks());

  for (const planId of affectedPlanIds) {
    clearSummaryForPlan(planId);
  }

  const incomingComponentCount = incoming
    .map(plan => Object.keys(plan.components).length)
    .reduce((sum, count) => sum + count, 0);
  log.info("partial-slot.load", {
    partId,
    plans: incoming.length,
    newComponents: incomingComponentCount,
  });
}

export function unloadPartialSlot(partId: string): void {
  const affectedPlanIds = applyPartialSlotUnload(partId);

  for (const planId of affectedPlanIds) {
    clearSummaryForPlan(planId);
  }

  log.info("partial-slot.unload", {
    partId,
    affectedPlans: affectedPlanIds.length,
  });
}

export function getBootedPlan(planId: string): Plan | undefined {
  return getTrackedBootedPlan(planId);
}

export function resetBootStateForTests(): void {
  bootAbort.abort();
  bootAbort = new AbortController();
  resetRuntimeSingletonsForTests();
  delete document.documentElement.dataset[BOOTED_ATTR];
  delete (window as ReactiveBootWindow).__alisReactiveBoot;
}

export const trace = { setLevel };

function mergeHooks(): MergeHooks {
  return {
    wireBehaviors,
    wireContainerValidation,
  };
}

function resetRuntimeSingletonsForTests(): void {
  resetActivePlanForTests();
  resetMergePlanState();
  resetLiveClearForTests();
  resetNativeActionLinksForTests();
  resetPluginRegistryForTests();
}

function clearSummaryForPlan(planId: string): void {
  const el = findSummaryElement(planId);
  if (el) {
    clearSummary(el);
    hideSummaryDiv(el);
  }
}

function markReactiveBooted(plan: Plan): void {
  document.documentElement.dataset[BOOTED_ATTR] = "true";
  (window as ReactiveBootWindow).__alisReactiveBoot = {
    booted: true,
    planId: plan.planId,
  };
  document.dispatchEvent(new CustomEvent("alis:booted", { detail: { planId: plan.planId } }));
}
