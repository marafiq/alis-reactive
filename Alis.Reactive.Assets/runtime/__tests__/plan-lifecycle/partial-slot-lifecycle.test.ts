import { describe, expect, it } from "vitest";
import { AppliedPlans } from "../../lifecycle/applied-plans";
import {
  behavior,
  component,
  objectContract,
  testPlanWiring,
  partialPlan,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("partial slot composition", () => {
  it("replaces the previous load from the same slot", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring, behaviorSignals } = testPlanWiring();
    const planId = "Resident.Root";

    appliedPlans.register(rootPlan(planId));

    const oldBehavior = behavior();
    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        types: { "native.element.address-line": objectContract() },
        components: { "address-line": component("address-line") },
        behaviors: [oldBehavior],
      }),
    ], wiring);

    const first = appliedPlans.get(planId)!;
    expect(first.components["address-line"]).toBeDefined();
    expect(first.types["native.element.address-line"]).toBeDefined();
    expect(first.behaviors).toContain(oldBehavior);
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const newBehavior = behavior();
    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        types: { "native.element.zip-code": objectContract() },
        components: { "zip-code": component("zip-code") },
        behaviors: [newBehavior],
      }),
    ], wiring);

    const second = appliedPlans.get(planId)!;
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(second.components["address-line"]).toBeUndefined();
    expect(second.types["native.element.address-line"]).toBeUndefined();
    expect(second.behaviors).not.toContain(oldBehavior);
    expect(second.components["zip-code"]).toBeDefined();
    expect(second.types["native.element.zip-code"]).toBeDefined();
    expect(second.behaviors).toContain(newBehavior);
  });

  it("unloads an active slot explicitly", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring, behaviorSignals } = testPlanWiring();
    const planId = "Resident.Root";
    const loadedBehavior = behavior();

    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        types: { "native.element.address-line": objectContract() },
        components: { "address-line": component("address-line") },
        behaviors: [loadedBehavior],
      }),
    ], wiring);

    expect(appliedPlans.get(planId)?.components["address-line"]).toBeDefined();
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const affectedPlanIds = appliedPlans.unloadPartialSlot("address-slot");

    expect(affectedPlanIds).toEqual([planId]);
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(appliedPlans.get(planId)).toBeUndefined();
  });

  it("uses the DOM slot id as the unload handle regardless of serialized plan scope", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";

    appliedPlans.register(rootPlan(planId));
    appliedPlans.loadPartialSlot("address-slot", [
      {
        ...rootPlan(planId),
        types: { "native.element.address-line": objectContract() },
        components: { "address-line": component("address-line") },
      },
    ], wiring);

    expect(appliedPlans.get(planId)?.components["address-line"]).toBeDefined();

    appliedPlans.unloadPartialSlot("address-slot");

    expect(appliedPlans.get(planId)?.components["address-line"]).toBeUndefined();
  });

  it("replaces every plan document loaded by the same slot id together", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring, behaviorSignals } = testPlanWiring();
    const residentPlanId = "Resident.Root";
    const billingPlanId = "Billing.Root";
    const residentBehavior = behavior();
    const billingBehavior = behavior();

    const affectedPlanIds = appliedPlans.loadPartialSlot("drawer-slot", [
      partialPlan(residentPlanId, {
        types: { "native.element.resident-name": objectContract() },
        components: { "resident-name": component("resident-name") },
        behaviors: [residentBehavior],
      }),
      partialPlan(billingPlanId, {
        types: { "native.element.invoice-total": objectContract() },
        components: { "invoice-total": component("invoice-total") },
        behaviors: [billingBehavior],
      }),
    ], wiring);

    expect(affectedPlanIds).toEqual([residentPlanId, billingPlanId]);
    expect(appliedPlans.get(residentPlanId)?.components["resident-name"]).toBeDefined();
    expect(appliedPlans.get(billingPlanId)?.components["invoice-total"]).toBeDefined();
    expect(behaviorSignals[0]).toBe(behaviorSignals[1]);

    appliedPlans.unloadPartialSlot("drawer-slot");

    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(appliedPlans.get(residentPlanId)).toBeUndefined();
    expect(appliedPlans.get(billingPlanId)).toBeUndefined();
  });

  it("wires validation once per active plan when one slot contains multiple fragments for the same plan", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";

    appliedPlans.register(rootPlan(planId));
    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: { "address-line": component("address-line") },
      }),
      partialPlan(planId, {
        components: { "zip-code": component("zip-code") },
      }),
    ], wiring);

    expect(appliedPlans.get(planId)?.components["address-line"]).toBeDefined();
    expect(appliedPlans.get(planId)?.components["zip-code"]).toBeDefined();
    expect(wiring.wireContainerValidation).toHaveBeenCalledTimes(1);
  });

  it("keeps an active plan with only slot-owned entries", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Dynamic";

    appliedPlans.loadPartialSlot("type-slot", [
      partialPlan(planId, {
        types: { "native.element.shared": objectContract() },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("component-slot", [
      partialPlan(planId, {
        components: { "address-line": component("address-line", "native.element.shared") },
      }),
    ], wiring);

    appliedPlans.unloadPartialSlot("component-slot");

    expect(appliedPlans.get(planId)?.components["address-line"]).toBeUndefined();
    expect(appliedPlans.get(planId)?.types["native.element.shared"]).toBeDefined();

    appliedPlans.unloadPartialSlot("type-slot");

    expect(appliedPlans.get(planId)).toBeUndefined();
  });

  it("unloading one slot keeps sibling slot behavior for the same plan", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring, behaviorSignals } = testPlanWiring();
    const planId = "Resident.Dynamic";
    const addressBehavior = behavior();
    const contactBehavior = behavior();

    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, { behaviors: [addressBehavior] }),
    ], wiring);
    appliedPlans.loadPartialSlot("contact-slot", [
      partialPlan(planId, { behaviors: [contactBehavior] }),
    ], wiring);

    appliedPlans.unloadPartialSlot("address-slot");

    const activePlan = appliedPlans.get(planId);
    expect(activePlan?.behaviors).toEqual([contactBehavior]);
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(behaviorSignals[1]?.aborted).toBe(false);

    appliedPlans.unloadPartialSlot("contact-slot");

    expect(appliedPlans.get(planId)).toBeUndefined();
    expect(behaviorSignals[1]?.aborted).toBe(true);
  });
});
