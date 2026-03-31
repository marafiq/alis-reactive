import { describe, expect, it } from "vitest";
import {
  exhaustiveEndStateFixtures,
  ajaxPartialAddressFragmentPlan,
  ajaxPartialValidationRootPlan,
  filteringMutationPlan,
  nativeButtonClickPlan,
  parallelRequestPlan,
  requestSinkAndResponsePlan,
  serverPushPlan,
  signalRPlan,
  testWidgetNativeChangePlan,
  testWidgetSyncFusionChangedPlan,
  testWidgetSyncFusionItemsChangedPlan,
} from "../architecture-proof/end-state-plan-fixtures";

describe("when proving the end-state schema against real use-case families", () => {
  it("covers the expected workflow families", () => {
    expect(exhaustiveEndStateFixtures).toHaveLength(13);

    const planIds = exhaustiveEndStateFixtures.map(x => x.planId);
    const residentModelPlans = planIds.filter(x => x === "Resident.Model");
    expect(residentModelPlans).toHaveLength(2);
    expect(new Set(planIds).size).toBe(planIds.length - 1);

    expect(planIds).toEqual(expect.arrayContaining([
      testWidgetNativeChangePlan.planId,
      nativeButtonClickPlan.planId,
      testWidgetSyncFusionChangedPlan.planId,
      testWidgetSyncFusionItemsChangedPlan.planId,
      filteringMutationPlan.planId,
      requestSinkAndResponsePlan.planId,
      parallelRequestPlan.planId,
      ajaxPartialValidationRootPlan.planId,
      ajaxPartialAddressFragmentPlan.planId,
      serverPushPlan.planId,
      signalRPlan.planId,
    ]));
  });

  it("keeps component-event payload variants explicit", () => {
    expect(testWidgetNativeChangePlan.entries[0].trigger.kind).toBe("component-event");
    expect(testWidgetNativeChangePlan.entries[0].trigger.payload.kind).toBe("object");

    expect(nativeButtonClickPlan.entries[0].trigger.kind).toBe("component-event");
    expect(nativeButtonClickPlan.entries[0].trigger.payload.kind).toBe("none");

    expect(testWidgetSyncFusionChangedPlan.entries[0].trigger.kind).toBe("component-event");
    expect(testWidgetSyncFusionChangedPlan.entries[0].trigger.payload.kind).toBe("callback");
  });

  it("proves validation can stay pure while partial fragments add component registrations later", () => {
    const validation = ajaxPartialValidationRootPlan.entries[0].reaction.kind === "http"
      ? ajaxPartialValidationRootPlan.entries[0].reaction.request.validation
      : undefined;

    expect(validation?.fields.map(x => x.modelPath)).toEqual(expect.arrayContaining([
      "Resident.Address.Street",
      "Resident.Address.City",
      "Resident.Address.ZipCode",
    ]));

    expect(ajaxPartialValidationRootPlan.components["Resident.Address.Street"]).toBeUndefined();
    expect(ajaxPartialAddressFragmentPlan.components["Resident.Address.Street"]?.id).toBe("Resident_Address_Street");
  });
});
