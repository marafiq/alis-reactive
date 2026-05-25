import { describe, expect, it } from "vitest";
import { AppliedBrowserPlans } from "../../lifecycle/merge-plan";
import {
  behavior,
  component,
  jsType,
  mergeHooks,
  partialPlan,
  rootPlan,
} from "../support/plan-lifecycle-fixtures";

describe("partial slot lifecycle", () => {
  it("replaces the previous contribution from the same slot", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks, behaviorSignals } = mergeHooks();
    const planId = "Resident.Root";

    browserPlans.register(rootPlan(planId));

    const oldBehavior = behavior();
    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, "address-slot", {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [oldBehavior],
      }),
    ], hooks);

    const first = browserPlans.get(planId)!;
    expect(first.components["address-line"]).toBeDefined();
    expect(first.types["native.element.address-line"]).toBeDefined();
    expect(first.behaviors).toContain(oldBehavior);
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const newBehavior = behavior();
    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, "address-slot", {
        types: { "native.element.zip-code": jsType() },
        components: { "zip-code": component("zip-code") },
        behaviors: [newBehavior],
      }),
    ], hooks);

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
    const browserPlans = new AppliedBrowserPlans();
    const { hooks, behaviorSignals } = mergeHooks();
    const planId = "Resident.Root";
    const loadedBehavior = behavior();

    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, "address-slot", {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [loadedBehavior],
      }),
    ], hooks);

    expect(browserPlans.get(planId)?.components["address-line"]).toBeDefined();
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const affectedPlanIds = browserPlans.unloadPartialSlot("address-slot");

    expect(affectedPlanIds).toEqual([planId]);
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(browserPlans.get(planId)).toBeUndefined();
  });

  it("replaces a slot as one lifetime when it contains multiple plan documents", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks, behaviorSignals } = mergeHooks();
    const residentPlanId = "Resident.Root";
    const billingPlanId = "Billing.Root";
    const residentBehavior = behavior();
    const billingBehavior = behavior();

    const affectedPlanIds = browserPlans.loadPartialSlot("drawer-slot", [
      partialPlan(residentPlanId, "server-part-id", {
        types: { "native.element.resident-name": jsType() },
        components: { "resident-name": component("resident-name") },
        behaviors: [residentBehavior],
      }),
      partialPlan(billingPlanId, "server-part-id", {
        types: { "native.element.invoice-total": jsType() },
        components: { "invoice-total": component("invoice-total") },
        behaviors: [billingBehavior],
      }),
    ], hooks);

    expect(affectedPlanIds).toEqual([residentPlanId, billingPlanId]);
    expect(browserPlans.get(residentPlanId)?.components["resident-name"]).toBeDefined();
    expect(browserPlans.get(billingPlanId)?.components["invoice-total"]).toBeDefined();
    expect(behaviorSignals[0]).toBe(behaviorSignals[1]);

    browserPlans.unloadPartialSlot("drawer-slot");

    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(browserPlans.get(residentPlanId)).toBeUndefined();
    expect(browserPlans.get(billingPlanId)).toBeUndefined();
  });

  it("keeps a non-root merged plan alive while another slot still owns artifacts", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Dynamic";

    browserPlans.loadPartialSlot("type-slot", [
      partialPlan(planId, "server-type-plan", {
        types: { "native.element.shared": jsType() },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("component-slot", [
      partialPlan(planId, "server-component-plan", {
        components: { "address-line": component("address-line", "native.element.shared") },
      }),
    ], hooks);

    browserPlans.unloadPartialSlot("component-slot");

    expect(browserPlans.get(planId)?.components["address-line"]).toBeUndefined();
    expect(browserPlans.get(planId)?.types["native.element.shared"]).toBeDefined();

    browserPlans.unloadPartialSlot("type-slot");

    expect(browserPlans.get(planId)).toBeUndefined();
  });
});
