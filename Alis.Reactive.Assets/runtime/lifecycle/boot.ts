// Boot — PlanDocument lifecycle: boot, partial slot load/unload, reset.
// Single responsibility: wire behaviors (two-phase) and register plans.
// Delegates browser plan state to browser-plans.ts.

import type { PlanDocument, Behavior } from "../types";
import { setLevel } from "../core/trace";
import { scope } from "../core/trace";
import { wireBehavior } from "../execution/trigger";
import { resetActivePlanForTests, setActivePlan } from "../execution/execute";
import { resetLiveClearForTests, wireLiveValidation } from "../validation/live-clear";
import { findSummaryElement, clearSummary, hideSummaryDiv } from "../validation/error-display";
import { resetNativeActionLinksForTests } from "../components/native/native-action-link";
import { resetPluginCatalogForTests } from "../core/plugin-catalog";
import {
  applyPartialSlotLoad,
  applyPartialSlotUnload,
  getBootedPlan as getTrackedBootedPlan,
  type PlanLifecycleHooks,
  registerBootedPlan,
  resetBrowserPlansForTests,
} from "./browser-plans";

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

export function boot(plan: PlanDocument): void {
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
function wireBehaviors(behaviors: Behavior[], plan: PlanDocument, signal?: AbortSignal): void {
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

function wireContainerValidation(plan: PlanDocument, signal?: AbortSignal): void {
  for (const [componentKey, component] of Object.entries(plan.components)) {
    if (component.container.kind !== "none") {
      wireLiveValidation(plan, componentKey, signal);
    }
  }
}

export function loadPartialSlot(slotId: string, incoming: PlanDocument[]): void {
  const affectedPlanIds = applyPartialSlotLoad(slotId, incoming, planLifecycleHooks());

  for (const planId of affectedPlanIds) {
    clearSummaryForPlan(planId);
  }

  const incomingComponentCount = incoming
    .map(plan => Object.keys(plan.components).length)
    .reduce((sum, count) => sum + count, 0);
  log.info("partial-slot.load", {
    slotId,
    plans: incoming.length,
    newComponents: incomingComponentCount,
  });
}

export function unloadPartialSlot(slotId: string): void {
  const affectedPlanIds = applyPartialSlotUnload(slotId);

  for (const planId of affectedPlanIds) {
    clearSummaryForPlan(planId);
  }

  log.info("partial-slot.unload", {
    slotId,
    affectedPlans: affectedPlanIds.length,
  });
}

export function getBootedPlan(planId: string): PlanDocument | undefined {
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

function planLifecycleHooks(): PlanLifecycleHooks {
  return {
    wireBehaviors,
    wireContainerValidation,
  };
}

function resetRuntimeSingletonsForTests(): void {
  resetActivePlanForTests();
  resetBrowserPlansForTests();
  resetLiveClearForTests();
  resetNativeActionLinksForTests();
  resetPluginCatalogForTests();
}

function clearSummaryForPlan(planId: string): void {
  const el = findSummaryElement(planId);
  if (el) {
    clearSummary(el);
    hideSummaryDiv(el);
  }
}

function markReactiveBooted(plan: PlanDocument): void {
  document.documentElement.dataset[BOOTED_ATTR] = "true";
  (window as ReactiveBootWindow).__alisReactiveBoot = {
    booted: true,
    planId: plan.planId,
  };
  document.dispatchEvent(new CustomEvent("alis:booted", { detail: { planId: plan.planId } }));
}
