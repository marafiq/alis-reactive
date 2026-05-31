// merge-policy.ts — the ONE replace-vs-append rule shared by C# container merge
// and TS recompose. Type contracts merge; components replace-by-key with the
// boot/slot join rules; behaviors append in order.

import type { PlanDocument } from "../types";
import { mergeBootComponent, mergeSlotComponent } from "./component-merge";
import { mergeObjectContracts } from "./object-contracts";

export const MergePolicy = {
  composeBootPlanInto(target: PlanDocument, incoming: PlanDocument): void {
    mergeTypeContracts(target, incoming);

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
    mergeTypeContracts(target, incoming);

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

function mergeTypeContracts(target: PlanDocument, incoming: PlanDocument): void {
  for (const [typeKey, contract] of Object.entries(incoming.types)) {
    target.types[typeKey] = mergeObjectContracts(target.types[typeKey], contract);
  }
}

export function emptyPlan(planId: string): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

export function snapshotPlan(plan: PlanDocument): PlanDocument {
  return {
    version: plan.version,
    planId: plan.planId,
    scope: { ...plan.scope },
    types: { ...plan.types },
    components: { ...plan.components },
    behaviors: [...plan.behaviors],
  };
}
