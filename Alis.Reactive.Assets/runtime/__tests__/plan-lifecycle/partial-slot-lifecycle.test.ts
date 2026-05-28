import { describe, expect, it } from "vitest";
import { BrowserPlanStore } from "../../lifecycle/browser-plans";
import {
  behavior,
  component,
  objectContract,
  browserPlanWiring,
  partialPlan,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("partial slot lifecycle", () => {
  it("replaces the previous load from the same slot", () => {
    const browserPlans = new BrowserPlanStore();
    const { wiring, behaviorSignals } = browserPlanWiring();
    const planId = "Resident.Root";

    browserPlans.register(rootPlan(planId));

    const oldBehavior = behavior();
    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        types: { "native.element.address-line": objectContract() },
        components: { "address-line": component("address-line") },
        behaviors: [oldBehavior],
      }),
    ], wiring);

    const first = browserPlans.get(planId)!;
    expect(first.components["address-line"]).toBeDefined();
    expect(first.types["native.element.address-line"]).toBeDefined();
    expect(first.behaviors).toContain(oldBehavior);
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const newBehavior = behavior();
    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        types: { "native.element.zip-code": objectContract() },
        components: { "zip-code": component("zip-code") },
        behaviors: [newBehavior],
      }),
    ], wiring);

    const second = browserPlans.get(planId)!;
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(second.components["address-line"]).toBeUndefined();
    expect(second.types["native.element.address-line"]).toBeUndefined();
    expect(second.behaviors).not.toContain(oldBehavior);
    expect(second.components["zip-code"]).toBeDefined();
    expect(second.types["native.element.zip-code"]).toBeDefined();
    expect(second.behaviors).toContain(newBehavior);
  });

  it("unloads an active slot explicitly", () => {
    const browserPlans = new BrowserPlanStore();
    const { wiring, behaviorSignals } = browserPlanWiring();
    const planId = "Resident.Root";
    const loadedBehavior = behavior();

    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        types: { "native.element.address-line": objectContract() },
        components: { "address-line": component("address-line") },
        behaviors: [loadedBehavior],
      }),
    ], wiring);

    expect(browserPlans.get(planId)?.components["address-line"]).toBeDefined();
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const affectedPlanIds = browserPlans.unloadPartialSlot("address-slot");

    expect(affectedPlanIds).toEqual([planId]);
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(browserPlans.get(planId)).toBeUndefined();
  });

  it("uses the browser slot handle as lifecycle identity regardless of serialized plan scope", () => {
    const browserPlans = new BrowserPlanStore();
    const { wiring } = browserPlanWiring();
    const planId = "Resident.Root";

    browserPlans.register(rootPlan(planId));
    browserPlans.loadPartialSlot("address-slot", [
      {
        ...rootPlan(planId),
        types: { "native.element.address-line": objectContract() },
        components: { "address-line": component("address-line") },
      },
    ], wiring);

    expect(browserPlans.get(planId)?.components["address-line"]).toBeDefined();

    browserPlans.unloadPartialSlot("address-slot");

    expect(browserPlans.get(planId)?.components["address-line"]).toBeUndefined();
  });

  it("replaces a slot as one lifetime when it contains multiple plan documents", () => {
    const browserPlans = new BrowserPlanStore();
    const { wiring, behaviorSignals } = browserPlanWiring();
    const residentPlanId = "Resident.Root";
    const billingPlanId = "Billing.Root";
    const residentBehavior = behavior();
    const billingBehavior = behavior();

    const affectedPlanIds = browserPlans.loadPartialSlot("drawer-slot", [
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
    expect(browserPlans.get(residentPlanId)?.components["resident-name"]).toBeDefined();
    expect(browserPlans.get(billingPlanId)?.components["invoice-total"]).toBeDefined();
    expect(behaviorSignals[0]).toBe(behaviorSignals[1]);

    browserPlans.unloadPartialSlot("drawer-slot");

    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(browserPlans.get(residentPlanId)).toBeUndefined();
    expect(browserPlans.get(billingPlanId)).toBeUndefined();
  });

  it("keeps a non-root merged plan alive while another slot still owns entries", () => {
    const browserPlans = new BrowserPlanStore();
    const { wiring } = browserPlanWiring();
    const planId = "Resident.Dynamic";

    browserPlans.loadPartialSlot("type-slot", [
      partialPlan(planId, {
        types: { "native.element.shared": objectContract() },
      }),
    ], wiring);
    browserPlans.loadPartialSlot("component-slot", [
      partialPlan(planId, {
        components: { "address-line": component("address-line", "native.element.shared") },
      }),
    ], wiring);

    browserPlans.unloadPartialSlot("component-slot");

    expect(browserPlans.get(planId)?.components["address-line"]).toBeUndefined();
    expect(browserPlans.get(planId)?.types["native.element.shared"]).toBeDefined();

    browserPlans.unloadPartialSlot("type-slot");

    expect(browserPlans.get(planId)).toBeUndefined();
  });

  it("unloading one slot keeps sibling slot behavior for the same plan", () => {
    const browserPlans = new BrowserPlanStore();
    const { wiring, behaviorSignals } = browserPlanWiring();
    const planId = "Resident.Dynamic";
    const addressBehavior = behavior();
    const contactBehavior = behavior();

    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, { behaviors: [addressBehavior] }),
    ], wiring);
    browserPlans.loadPartialSlot("contact-slot", [
      partialPlan(planId, { behaviors: [contactBehavior] }),
    ], wiring);

    browserPlans.unloadPartialSlot("address-slot");

    const activePlan = browserPlans.get(planId);
    expect(activePlan?.behaviors).toEqual([contactBehavior]);
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(behaviorSignals[1]?.aborted).toBe(false);

    browserPlans.unloadPartialSlot("contact-slot");

    expect(browserPlans.get(planId)).toBeUndefined();
    expect(behaviorSignals[1]?.aborted).toBe(true);
  });
});
