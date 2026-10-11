// Replace-vs-append rules are shared by C# container merge and TS recompose.
// Vendors join by name; components replace by key using boot/slot join rules; behaviors append in order.

import type { PlanDocument } from "../types/index";
import { mergeBootComponent, mergeSlotComponent } from "./component-merge";

export const MergePolicy = {
  composeBootPlanInto(target: PlanDocument, incoming: PlanDocument): void {
    mergeVendors(target, incoming);

    for (const [componentKey, component] of Object.entries(incoming.components)) {
      mergeBootComponent(target, componentKey, component);
    }

    target.behaviors.push(...incoming.behaviors);
  },

  composeSlotPlanInto(
    target: PlanDocument,
    incoming: PlanDocument,
    bootPlan: PlanDocument | undefined,
  ): void {
    mergeVendors(target, incoming);

    for (const [componentKey, component] of Object.entries(incoming.components)) {
      mergeSlotComponent(
        target,
        componentKey,
        component,
        bootPlan?.components[componentKey] !== undefined,
      );
    }

    target.behaviors.push(...incoming.behaviors);
  },
};

// A vendor name means one definition (C# refuses two vendors with one name), so joining is a union.
function mergeVendors(target: PlanDocument, incoming: PlanDocument): void {
  Object.assign(target.vendors, incoming.vendors);
}

export function emptyPlan(planId: string): PlanDocument {
  return {
    version: 4,
    planId,
    scope: { kind: "root" },
    vendors: {},
    components: {},
    behaviors: [],
  };
}

export function snapshotPlan(plan: PlanDocument): PlanDocument {
  return {
    version: plan.version,
    planId: plan.planId,
    scope: { ...plan.scope },
    vendors: { ...plan.vendors },
    components: { ...plan.components },
    behaviors: [...plan.behaviors],
  };
}
