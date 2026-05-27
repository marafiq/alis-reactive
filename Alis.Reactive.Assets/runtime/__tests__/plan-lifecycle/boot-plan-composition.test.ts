import { describe, expect, it } from "vitest";
import { composeInitialPlans } from "../../lifecycle/merge-plan";
import type { ComponentObject } from "../../types";
import {
  behavior,
  component,
  objectContract,
  objectContractWithReadableProperty,
  objectContractWithWritableProperty,
  layoutComponent,
  partialPlan,
  registeredInputComponent,
  rootPlan,
  validationContainer,
  validationRule,
} from "../support/plan-lifecycle-fixtures";

describe("boot plan composition", () => {
  it("assembles one boot plan per plan id while preserving load order", () => {
    const residentPlanId = "Resident.Root";
    const residentReady = behavior();
    const addressReady = behavior();
    const billingPlan = rootPlan("Billing.Root");

    const composed = composeInitialPlans([
      {
        ...rootPlan(residentPlanId),
        types: { "native.element.resident-name": objectContract() },
        components: { "resident-name": component("resident-name") },
        behaviors: [residentReady],
      },
      partialPlan(residentPlanId, {
        types: { "native.element.address-line": objectContract() },
        components: { "address-line": component("address-line") },
        behaviors: [addressReady],
      }),
      billingPlan,
    ]);

    expect(composed.map(plan => plan.planId)).toEqual([residentPlanId, "Billing.Root"]);

    const resident = composed[0];
    expect(resident.scope).toEqual({ kind: "root" });
    expect(Object.keys(resident.types)).toEqual([
      "native.element.resident-name",
      "native.element.address-line",
    ]);
    expect(Object.keys(resident.components)).toEqual(["resident-name", "address-line"]);
    expect(resident.behaviors).toEqual([residentReady, addressReady]);
    expect(composed[1]).toEqual(billingPlan);
  });

  it("emits a root-scoped boot plan even when a partial plan appears first", () => {
    const planId = "Resident.Root";
    const partialReady = behavior();
    const rootReady = behavior();

    const composed = composeInitialPlans([
      partialPlan(planId, {
        types: { "native.element.address-line": objectContract() },
        components: { "address-line": component("address-line") },
        behaviors: [partialReady],
      }),
      {
        ...rootPlan(planId),
        types: { "native.element.resident-name": objectContract() },
        components: { "resident-name": component("resident-name") },
        behaviors: [rootReady],
      },
    ]);

    expect(composed).toHaveLength(1);
    expect(composed[0].scope).toEqual({ kind: "root" });
    expect(Object.keys(composed[0].components)).toEqual(["address-line", "resident-name"]);
    expect(composed[0].behaviors).toEqual([partialReady, rootReady]);
  });

  it("merges initial type contracts instead of letting root overwrite partial write access", () => {
    const planId = "Resident.Step";
    const componentId = "care-unit";
    const typeKey = "native.component.care-unit";

    const composed = composeInitialPlans([
      partialPlan(planId, {
        types: { [typeKey]: objectContractWithWritableProperty("value") },
        components: { [componentId]: component(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: objectContractWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId].binding.kind).toBe("registered-input");
  });

  it("does not let an initial reference-only plan erase a registered input definition", () => {
    const planId = "Resident.Step";
    const componentId = "care-unit";
    const typeKey = "native.component.care-unit";

    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: objectContractWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, {
        types: { [typeKey]: objectContractWithWritableProperty("value") },
        components: { [componentId]: component(componentId, typeKey) },
      }),
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId].binding.kind).toBe("registered-input");
  });

  it("coalesces duplicate initial owned component definitions from the first DOM", () => {
    const planId = "Resident.Step";
    const componentId = "resident-name";
    const typeKey = "native.component.resident-name";

    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: objectContractWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, {
        types: { [typeKey]: objectContractWithWritableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      }),
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId]).toEqual(registeredInputComponent(componentId, typeKey));
  });

  it("coalesces duplicate initial owned component definitions when the partial appears before the root", () => {
    const planId = "Resident.Step";
    const componentId = "resident-name";
    const typeKey = "native.component.resident-name";

    const composed = composeInitialPlans([
      partialPlan(planId, {
        types: { [typeKey]: objectContractWithWritableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: objectContractWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId]).toEqual(registeredInputComponent(componentId, typeKey));
  });

  it("merges an initial layout-object reference without replacing the root component", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";

    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: objectContractWithWritableProperty("title") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      },
      partialPlan(planId, {
        types: { [typeKey]: objectContractWithWritableProperty("content") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      }),
    ]);

    expect(composed[0].components[componentId]).toEqual(layoutComponent(componentId, typeKey));
    expect(Object.keys(composed[0].types[typeKey].properties)).toEqual(["title", "content"]);
  });

  it("lets an initial partial layout object appear before the root plan", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";

    const composed = composeInitialPlans([
      partialPlan(planId, {
        types: { [typeKey]: objectContractWithWritableProperty("content") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: objectContractWithWritableProperty("title") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      },
    ]);

    expect(composed[0].scope).toEqual({ kind: "root" });
    expect(composed[0].components[componentId]).toEqual(layoutComponent(componentId, typeKey));
    expect(Object.keys(composed[0].types[typeKey].properties)).toEqual(["content", "title"]);
  });

  it("uses component merge semantics when composing validation containers", () => {
    const planId = "Resident.Root";
    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
            validationRule("zip-code", "old zip required"),
          ]),
        },
      },
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "new zip required"),
            validationRule("city"),
          ]),
        },
      }),
    ]);

    const container = composed[0].components["resident-form"].container;
    const validationScope = expectValidationContainer(container);

    expect(validationScope.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "zip-code",
      "city",
    ]);
    expect(validationScope.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("new zip required");
  });

  it("uses component merge semantics when an initial validation container partial appears before the root", () => {
    const planId = "Resident.Root";
    const composed = composeInitialPlans([
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "new zip required"),
            validationRule("city"),
          ]),
        },
      }),
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
            validationRule("zip-code", "old zip required"),
          ]),
        },
      },
    ]);

    const container = composed[0].components["resident-form"].container;
    const validationScope = expectValidationContainer(container);

    expect(validationScope.validationRules.map(rule => rule.component)).toEqual([
      "zip-code",
      "city",
      "first-name",
    ]);
    expect(validationScope.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("old zip required");
  });

});

function expectValidationContainer(
  container: ComponentObject["container"],
): Extract<ComponentObject["container"], { kind: "validation-container" }> {
  expect(container.kind).toBe("validation-container");
  if (container.kind !== "validation-container") {
    throw new Error(`Expected validation-container scope, received "${container.kind}"`);
  }

  return container;
}
