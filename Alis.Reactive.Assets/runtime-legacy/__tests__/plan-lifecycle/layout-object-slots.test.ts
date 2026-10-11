import { describe, expect, it } from "vitest";
import { AppliedPlans } from "../../lifecycle/applied-plans";
import {
  objectContractWithWritableProperty,
  layoutComponent,
  testPlanWiring,
  partialPlan,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("layout object slots", () => {
  it("lets multiple slots share one layout-owned app component", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";
    const toastTypeKey = "fusion.component.alisFusionToast";

    appliedPlans.register(rootPlan(planId));

    appliedPlans.loadPartialSlot("first-toast-slot", [
      partialPlan(planId, {
        types: {
          [toastTypeKey]: objectContractWithWritableProperty("title"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("second-toast-slot", [
      partialPlan(planId, {
        types: {
          [toastTypeKey]: objectContractWithWritableProperty("content"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], wiring);

    expect(appliedPlans.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(appliedPlans.get(planId)?.types[toastTypeKey]?.properties ?? {}))
      .toEqual(["title", "content"]);

    appliedPlans.unloadPartialSlot("first-toast-slot");

    expect(appliedPlans.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(appliedPlans.get(planId)?.types[toastTypeKey]?.properties ?? {}))
      .toEqual(["content"]);

    appliedPlans.unloadPartialSlot("second-toast-slot");

    expect(appliedPlans.get(planId)?.components.alisFusionToast).toBeUndefined();
    expect(appliedPlans.get(planId)?.types[toastTypeKey]).toBeUndefined();
  });

});
