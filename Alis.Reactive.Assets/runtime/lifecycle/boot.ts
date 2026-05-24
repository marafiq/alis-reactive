// Boot — Plan lifecycle: boot, merge, reset.
// Single responsibility: wire behaviors (two-phase) and register plans.
// Delegates state to merge-plan.ts PlanRegistry.

import type { Plan, Behavior } from "../types";
import { setLevel } from "../core/trace";
import { scope } from "../core/trace";
import { wireBehavior } from "../execution/trigger";
import { resetActivePlanForTests, setActivePlan } from "../execution/execute";
import { RuntimePlan } from "../domain/runtime-plan";
import { resetLiveClearForTests, wireLiveValidation } from "../validation/live-clear";
import { findSummaryElement, clearSummary, hideSummaryDiv } from "../validation/error-display";
import { resetNativeActionLinksForTests } from "../components/native/native-action-link";
import { resetPluginRegistryForTests } from "../core/plugin-registry";
import {
  applyPartialSlotLoad,
  applyPartialSlotUnload,
  applyMergedPlan,
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

/** Wire live validation for all components that have container scopes. */
function wireContainerValidation(plan: Plan, signal?: AbortSignal): void {
  const runtimePlan = RuntimePlan.from(plan);
  for (const component of runtimePlan.components.entries()) {
    if (component.containerScope) {
      wireLiveValidation(plan, component.key, signal);
    }
  }
}

export function mergePlan(incoming: Plan): void {
  const merged = applyMergedPlan(incoming, mergeHooks());

  clearSummaryForPlan(merged.planId);

  const incomingComponentCount = RuntimePlan.from(incoming).components.entries().length;
  log.info("merged", { planId: merged.planId, newComponents: incomingComponentCount });
}

export function loadPartialSlot(partId: string, incoming: Plan[]): void {
  const result = applyPartialSlotLoad(partId, incoming, mergeHooks());

  for (const planId of result.affectedPlanIds) {
    clearSummaryForPlan(planId);
  }

  const incomingComponentCount = incoming
    .map(plan => RuntimePlan.from(plan).components.entries().length)
    .reduce((sum, count) => sum + count, 0);
  log.info("partial-slot.load", {
    partId,
    plans: result.loadedPlans.length,
    newComponents: incomingComponentCount,
  });
}

export function unloadPartialSlot(partId: string): void {
  const result = applyPartialSlotUnload(partId);

  for (const planId of result.affectedPlanIds) {
    clearSummaryForPlan(planId);
  }

  log.info("partial-slot.unload", {
    partId,
    affectedPlans: result.affectedPlanIds.length,
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
