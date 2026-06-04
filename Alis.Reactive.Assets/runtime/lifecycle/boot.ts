// Boot wires behaviors and validation for active PlanDocuments.
// Boot snapshots and partial-slot composition stay in applied plan state.

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

export function boot(plan: PlanDocument): void {
  log.info("booting", { planId: plan.planId, behaviors: plan.behaviors.length });

  wireContainerValidation(plan, bootAbort.signal);

  wireBehaviors(plan.behaviors, plan, bootAbort.signal);

  setActivePlan(plan);
  appliedPlans.register(plan);
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
      wireTrigger(behavior.startsWhen, behavior.reaction, plan, signal);
    }
  }
  for (const behavior of deferred) {
    wireTrigger(behavior.startsWhen, behavior.reaction, plan, signal);
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
  const affectedPlanIds = appliedPlans.loadPartialSlot(slotId, incoming, activePlanWiring());

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
