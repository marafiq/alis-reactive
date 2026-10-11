// Boot applies the root PlanDocument and wires its behaviors and validation.
// Active Plan composition stays in applied plan state: immutable boot snapshot
// plus currently loaded partial slots.

import type { PlanDocument, Behavior } from "../types/index";
import { setLevel } from "../diagnostics/trace";
import { scope } from "../diagnostics/trace";
import { wireTrigger } from "../execution/triggers/trigger";
import { resetActivePlanForTests, setActivePlan } from "../execution/reactions/execute";
import { resetLiveClearForTests, wireLiveValidation } from "../validation/live-clear";
import { findSummaryElement, clearSummary, hideSummaryDiv } from "../validation/error-display";
import { resetNativeActionLinksForTests } from "../components/native/native-action-link";
import { resetPluginCatalogForTests } from "../plugins/catalog";
import {
  appliedPlans,
  type ActivePlanWiring,
} from "./applied-plans";

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

export function boot(bootPlan: PlanDocument): void {
  log.info("booting", { planId: bootPlan.planId, behaviors: bootPlan.behaviors.length });

  wireContainerValidation(bootPlan, bootAbort.signal);

  wireBehaviors(bootPlan.behaviors, bootPlan, bootAbort.signal);

  setActivePlan(bootPlan);
  appliedPlans.register(bootPlan);
  markReactiveBooted(bootPlan);
  log.info("booted", { planId: bootPlan.planId });
}

// Two-phase wiring: wire all non-page-ready listeners first, then execute page-ready.
// Document-event listeners must exist before page-ready dispatches into them.
function wireBehaviors(behaviors: Behavior[], activePlan: PlanDocument, signal?: AbortSignal): void {
  const deferred: Behavior[] = [];
  for (const behavior of behaviors) {
    if (behavior.startsWhen.kind === "page-ready") {
      deferred.push(behavior);
    } else {
      wireTrigger(behavior.startsWhen, behavior.reaction, activePlan, signal);
    }
  }
  for (const behavior of deferred) {
    wireTrigger(behavior.startsWhen, behavior.reaction, activePlan, signal);
  }
}

function wireContainerValidation(activePlan: PlanDocument, signal?: AbortSignal): void {
  for (const [componentKey, component] of Object.entries(activePlan.components)) {
    if (component.container.kind !== "none") {
      wireLiveValidation(activePlan, componentKey, signal);
    }
  }
}

export function loadPartialSlot(slotId: string, incomingPlans: PlanDocument[]): void {
  const affectedPlanIds = appliedPlans.loadPartialSlot(slotId, incomingPlans, activePlanWiring());

  for (const planId of affectedPlanIds) {
    clearSummaryForPlan(planId);
  }

  const incomingComponentCount = incomingPlans
    .map(incomingPlan => Object.keys(incomingPlan.components).length)
    .reduce((sum, count) => sum + count, 0);
  log.info("partial-slot.load", {
    slotId,
    plans: incomingPlans.length,
    newComponents: incomingComponentCount,
  });
}

export function unloadPartialSlot(slotId: string): void {
  const affectedPlanIds = appliedPlans.unloadPartialSlot(slotId);

  for (const planId of affectedPlanIds) {
    clearSummaryForPlan(planId);
  }

  log.info("partial-slot.unload", {
    slotId,
    affectedPlans: affectedPlanIds.length,
  });
}

export function getBootedPlan(planId: string): PlanDocument | undefined {
  return appliedPlans.get(planId);
}

export function resetBootStateForTests(): void {
  bootAbort.abort();
  bootAbort = new AbortController();
  resetRuntimeSingletonsForTests();
  delete document.documentElement.dataset[BOOTED_ATTR];
  delete (window as ReactiveBootWindow).__alisReactiveBoot;
}

export const trace = { setLevel };

function activePlanWiring(): ActivePlanWiring {
  return {
    wireBehaviors,
    wireContainerValidation,
  };
}

function resetRuntimeSingletonsForTests(): void {
  resetActivePlanForTests();
  appliedPlans.reset();
  resetLiveClearForTests();
  resetNativeActionLinksForTests();
  resetPluginCatalogForTests();
}

function clearSummaryForPlan(planId: string): void {
  const summaryElement = findSummaryElement(planId);
  if (summaryElement) {
    clearSummary(summaryElement);
    hideSummaryDiv(summaryElement);
  }
}

function markReactiveBooted(bootPlan: PlanDocument): void {
  document.documentElement.dataset[BOOTED_ATTR] = "true";
  (window as ReactiveBootWindow).__alisReactiveBoot = {
    booted: true,
    planId: bootPlan.planId,
  };
  document.dispatchEvent(new CustomEvent("alis:booted", { detail: { planId: bootPlan.planId } }));
}
