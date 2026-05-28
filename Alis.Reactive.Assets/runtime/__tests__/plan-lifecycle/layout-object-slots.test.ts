import { describe, expect, it } from "vitest";
import { BrowserPlanStore } from "../../lifecycle/browser-plans";
import {
  objectContractWithWritableProperty,
  layoutComponent,
  browserPlanWiring,
  partialPlan,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("layout object slots", () => {
  it("lets multiple slots share one layout-owned app component", () => {
    const browserPlans = new BrowserPlanStore();
    const { wiring } = browserPlanWiring();
    const planId = "Resident.Root";
    const toastTypeKey = "fusion.component.alisFusionToast";

    browserPlans.register(rootPlan(planId));

    browserPlans.loadPartialSlot("first-toast-slot", [
      partialPlan(planId, {
        types: {
          [toastTypeKey]: objectContractWithWritableProperty("title"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], wiring);
    browserPlans.loadPartialSlot("second-toast-slot", [
      partialPlan(planId, {
        types: {
          [toastTypeKey]: objectContractWithWritableProperty("content"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], wiring);

    expect(browserPlans.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(browserPlans.get(planId)?.types[toastTypeKey].properties ?? {}))
      .toEqual(["title", "content"]);

    browserPlans.unloadPartialSlot("first-toast-slot");

    expect(browserPlans.get(planId)?.components.alisFusionToast).toBeDefined();
    expect(Object.keys(browserPlans.get(planId)?.types[toastTypeKey].properties ?? {}))
      .toEqual(["content"]);

    browserPlans.unloadPartialSlot("second-toast-slot");

    expect(browserPlans.get(planId)?.components.alisFusionToast).toBeUndefined();
    expect(browserPlans.get(planId)?.types[toastTypeKey]).toBeUndefined();
  });

});
