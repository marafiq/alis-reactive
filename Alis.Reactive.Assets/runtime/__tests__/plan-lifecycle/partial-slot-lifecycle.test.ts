import { describe, expect, it } from "vitest";
import { PlanRegistry } from "../../lifecycle/merge-plan";
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
    const registry = new PlanRegistry();
    const { hooks, behaviorSignals } = mergeHooks();
    const planId = "Resident.Root";

    registry.register(rootPlan(planId));

    const oldBehavior = behavior();
    const first = registry.add(
      partialPlan(planId, "address-slot", {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [oldBehavior],
      }),
      hooks,
    );

    expect(first.components["address-line"]).toBeDefined();
    expect(first.types["native.element.address-line"]).toBeDefined();
    expect(first.behaviors).toContain(oldBehavior);
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const newBehavior = behavior();
    const second = registry.add(
      partialPlan(planId, "address-slot", {
        types: { "native.element.zip-code": jsType() },
        components: { "zip-code": component("zip-code") },
        behaviors: [newBehavior],
      }),
      hooks,
    );

    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(second.components["address-line"]).toBeUndefined();
    expect(second.types["native.element.address-line"]).toBeUndefined();
    expect(second.behaviors).not.toContain(oldBehavior);
    expect(second.components["zip-code"]).toBeDefined();
    expect(second.types["native.element.zip-code"]).toBeDefined();
    expect(second.behaviors).toContain(newBehavior);
  });

  it("unloads an active slot explicitly", () => {
    const registry = new PlanRegistry();
    const { hooks, behaviorSignals } = mergeHooks();
    const planId = "Resident.Root";
    const loadedBehavior = behavior();

    registry.add(
      partialPlan(planId, "address-slot", {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [loadedBehavior],
      }),
      hooks,
    );

    expect(registry.get(planId)?.components["address-line"]).toBeDefined();
    expect(behaviorSignals[0]?.aborted).toBe(false);

    const result = registry.unloadPartialSlot("address-slot");

    expect(result.affectedPlanIds).toEqual([planId]);
    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(registry.get(planId)).toBeUndefined();
  });

  it("rejects an empty slot load so unload remains explicit", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();

    expect(() => registry.loadPartialSlot("address-slot", [], hooks))
      .toThrow("unload the slot explicitly");
  });

  it("replaces a slot as one lifetime when it contains multiple plan documents", () => {
    const registry = new PlanRegistry();
    const { hooks, behaviorSignals } = mergeHooks();
    const residentPlanId = "Resident.Root";
    const billingPlanId = "Billing.Root";
    const residentBehavior = behavior();
    const billingBehavior = behavior();

    const result = registry.loadPartialSlot("drawer-slot", [
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

    expect(result.affectedPlanIds).toEqual([residentPlanId, billingPlanId]);
    expect(result.loadedPlans.map(plan => plan.planId)).toEqual([residentPlanId, billingPlanId]);
    expect(registry.get(residentPlanId)?.components["resident-name"]).toBeDefined();
    expect(registry.get(billingPlanId)?.components["invoice-total"]).toBeDefined();
    expect(behaviorSignals[0]).toBe(behaviorSignals[1]);

    registry.unloadPartialSlot("drawer-slot");

    expect(behaviorSignals[0]?.aborted).toBe(true);
    expect(registry.get(residentPlanId)).toBeUndefined();
    expect(registry.get(billingPlanId)).toBeUndefined();
  });

  it("keeps a non-root merged plan alive while another slot still owns artifacts", () => {
    const registry = new PlanRegistry();
    const { hooks } = mergeHooks();
    const planId = "Resident.Dynamic";

    registry.loadPartialSlot("type-slot", [
      partialPlan(planId, "server-type-plan", {
        types: { "native.element.shared": jsType() },
      }),
    ], hooks);
    registry.loadPartialSlot("component-slot", [
      partialPlan(planId, "server-component-plan", {
        components: { "address-line": component("address-line", "native.element.shared") },
      }),
    ], hooks);

    registry.unloadPartialSlot("component-slot");

    expect(registry.get(planId)?.components["address-line"]).toBeUndefined();
    expect(registry.get(planId)?.types["native.element.shared"]).toBeDefined();

    registry.unloadPartialSlot("type-slot");

    expect(registry.get(planId)).toBeUndefined();
  });
});
