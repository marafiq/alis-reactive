import { describe, expect, it } from "vitest";
import { AppliedBrowserPlans } from "../../lifecycle/merge-plan";
import {
  jsTypeWithWritableProperty,
  layoutComponent,
  mergeHooks,
  partialPlan,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("layout object contributions", () => {
  it("lets multiple slots share one layout-owned app component", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const toastTypeKey = "fusion.component.alisFusionToast";

    browserPlans.register(rootPlan(planId));

    browserPlans.loadPartialSlot("first-toast-slot", [
      partialPlan(planId, {
        types: {
          [toastTypeKey]: jsTypeWithWritableProperty("title"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("second-toast-slot", [
      partialPlan(planId, {
        types: {
          [toastTypeKey]: jsTypeWithWritableProperty("content"),
        },
        components: {
          alisFusionToast: layoutComponent("alisFusionToast", toastTypeKey),
        },
      }),
    ], hooks);

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
