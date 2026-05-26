import { describe, expect, it } from "vitest";
import { composeInitialPlans } from "../../lifecycle/merge-plan";
import type { Component, ComponentValidation } from "../../types";
import {
  behavior,
  component,
  jsType,
  jsTypeWithReadableProperty,
  jsTypeWithWritableProperty,
  layoutComponent,
  partialPlan,
  registeredInputComponent,
  rootPlan,
  structuredPath,
  validationContainer,
  validationRule,
} from "../support/plan-lifecycle-fixtures";

describe("boot plan composition", () => {
  it("assembles one boot plan per plan id while preserving contribution order", () => {
    const residentPlanId = "Resident.Root";
    const residentReady = behavior();
    const addressReady = behavior();
    const billingPlan = rootPlan("Billing.Root");

    const composed = composeInitialPlans([
      {
        ...rootPlan(residentPlanId),
        types: { "native.element.resident-name": jsType() },
        components: { "resident-name": component("resident-name") },
        behaviors: [residentReady],
      },
      partialPlan(residentPlanId, {
        types: { "native.element.address-line": jsType() },
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

  it("emits a root-scoped boot plan even when a partial contribution appears first", () => {
    const planId = "Resident.Root";
    const partialReady = behavior();
    const rootReady = behavior();

    const composed = composeInitialPlans([
      partialPlan(planId, {
        types: { "native.element.address-line": jsType() },
        components: { "address-line": component("address-line") },
        behaviors: [partialReady],
      }),
      {
        ...rootPlan(planId),
        types: { "native.element.resident-name": jsType() },
        components: { "resident-name": component("resident-name") },
        behaviors: [rootReady],
      },
    ]);

    expect(composed).toHaveLength(1);
    expect(composed[0].scope).toEqual({ kind: "root" });
    expect(Object.keys(composed[0].components)).toEqual(["address-line", "resident-name"]);
    expect(composed[0].behaviors).toEqual([partialReady, rootReady]);
  });

  it("merges initial type fragments instead of letting root overwrite partial write access", () => {
    const planId = "Resident.Step";
    const componentId = "care-unit";
    const typeKey = "native.component.care-unit";

    const composed = composeInitialPlans([
      partialPlan(planId, {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: component(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId].binding.kind).toBe("registered-input");
  });

  it("does not let an initial reference-only contribution erase a registered input definition", () => {
    const planId = "Resident.Step";
    const componentId = "care-unit";
    const typeKey = "native.component.care-unit";

    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
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
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
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
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
    ]);

    expect(composed[0].types[typeKey].properties.value.access).toBe("readwrite");
    expect(composed[0].components[componentId]).toEqual(registeredInputComponent(componentId, typeKey));
  });

  it("keeps duplicate initial owned component definitions from changing binding state", () => {
    const planId = "Resident.Step";
    const componentId = "resident-name";
    const typeKey = "native.component.resident-name";
    const conflictingDefinition = registeredInputComponentBoundTo(
      componentId,
      typeKey,
      "Clinical.ResidentName",
    );

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      },
      partialPlan(planId, {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: conflictingDefinition },
      }),
    ])).toThrow('partial plan contribution "Resident.Step" cannot declare component "resident-name"');
  });

  it("keeps duplicate partial-first owned component definitions from changing binding state", () => {
    const planId = "Resident.Step";
    const componentId = "resident-name";
    const typeKey = "native.component.resident-name";
    const conflictingDefinition = registeredInputComponentBoundTo(
      componentId,
      typeKey,
      "Clinical.ResidentName",
    );

    expect(() => composeInitialPlans([
      partialPlan(planId, {
        types: { [typeKey]: jsTypeWithWritableProperty("value") },
        components: { [componentId]: registeredInputComponent(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithReadableProperty("value") },
        components: { [componentId]: conflictingDefinition },
      },
    ])).toThrow('root plan contribution cannot declare component "resident-name"');
  });

  it("requires initial layout-object contributions to keep the same runtime identity", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithWritableProperty("title") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      },
      partialPlan(planId, {
        types: { [typeKey]: jsTypeWithWritableProperty("content") },
        components: { [componentId]: layoutComponent("otherToast", typeKey) },
      }),
    ])).toThrow('partial plan contribution "Resident.Step" cannot declare component "alisFusionToast"');
  });

  it("merges an initial layout-object reference without replacing the root component", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";

    const composed = composeInitialPlans([
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithWritableProperty("title") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      },
      partialPlan(planId, {
        types: { [typeKey]: jsTypeWithWritableProperty("content") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      }),
    ]);

    expect(composed[0].components[componentId]).toEqual(layoutComponent(componentId, typeKey));
    expect(Object.keys(composed[0].types[typeKey].properties)).toEqual(["title", "content"]);
  });

  it("lets an initial partial layout object appear before the root contribution", () => {
    const planId = "Resident.Step";
    const componentId = "alisFusionToast";
    const typeKey = "fusion.component.alisFusionToast";

    const composed = composeInitialPlans([
      partialPlan(planId, {
        types: { [typeKey]: jsTypeWithWritableProperty("content") },
        components: { [componentId]: layoutComponent(componentId, typeKey) },
      }),
      {
        ...rootPlan(planId),
        types: { [typeKey]: jsTypeWithWritableProperty("title") },
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

  it("requires initial validation-container contributions to keep the same runtime identity", () => {
    const planId = "Resident.Root";

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
          ]),
        },
      },
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("other-form", [
            validationRule("city"),
          ]),
        },
      }),
    ])).toThrow('partial plan contribution "Resident.Root" cannot declare component "resident-form"');
  });

  it("requires partial-first validation-container contributions to keep the same runtime identity", () => {
    const planId = "Resident.Root";

    expect(() => composeInitialPlans([
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("city"),
          ]),
        },
      }),
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("other-form", [
            validationRule("first-name"),
          ]),
        },
      },
    ])).toThrow('root plan contribution cannot declare component "resident-form"');
  });

  it("keeps initial validation-container contributions from carrying binding state", () => {
    const planId = "Resident.Root";
    const invalidContainer = validationContainerBoundTo("resident-form", "Resident", [
      validationRule("city"),
    ]);

    expect(() => composeInitialPlans([
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
          ]),
        },
      },
      partialPlan(planId, {
        components: {
          "resident-form": invalidContainer,
        },
      }),
    ])).toThrow('partial plan contribution "Resident.Root" cannot declare component "resident-form"');
  });

  it("keeps partial-first validation-container contributions from carrying binding state", () => {
    const planId = "Resident.Root";
    const invalidContainer = validationContainerBoundTo("resident-form", "Resident", [
      validationRule("first-name"),
    ]);

    expect(() => composeInitialPlans([
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("city"),
          ]),
        },
      }),
      {
        ...rootPlan(planId),
        components: {
          "resident-form": invalidContainer,
        },
      },
    ])).toThrow('root plan contribution cannot declare component "resident-form"');
  });

  it("keeps invalid partial-first validation-container binding state from being erased by the root", () => {
    const planId = "Resident.Root";
    const invalidContainer = validationContainerBoundTo("resident-form", "Resident", [
      validationRule("city"),
    ]);

    expect(() => composeInitialPlans([
      partialPlan(planId, {
        components: {
          "resident-form": invalidContainer,
        },
      }),
      {
        ...rootPlan(planId),
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("first-name"),
          ]),
        },
      },
    ])).toThrow('root plan contribution cannot declare component "resident-form"');
  });
});

function expectRegisteredInputBinding(component: Component): Extract<Component["binding"], { kind: "registered-input" }> {
  expect(component.binding.kind).toBe("registered-input");
  if (component.binding.kind !== "registered-input") {
    throw new Error(`Expected component "${component.id}" to have registered input binding`);
  }

  return component.binding;
}

function registeredInputComponentBoundTo(
  componentId: string,
  typeKey: string,
  bindingPath: string,
): Component {
  const component = registeredInputComponent(componentId, typeKey);
  const binding = expectRegisteredInputBinding(component);
  component.binding = {
    ...binding,
    bindingPath,
    path: structuredPath(bindingPath),
  };

  return component;
}

function validationContainerBoundTo(
  componentId: string,
  bindingPath: string,
  rules: ComponentValidation[],
): Component {
  const component = validationContainer(componentId, rules);
  component.binding = {
    kind: "registered-input",
    bindingPath,
    path: structuredPath(bindingPath),
    valueMember: "value",
  };

  return component;
}

function expectValidationContainer(
  container: Component["container"],
): Extract<Component["container"], { kind: "validation-container" }> {
  expect(container.kind).toBe("validation-container");
  if (container.kind !== "validation-container") {
    throw new Error(`Expected validation-container scope, received "${container.kind}"`);
  }

  return container;
}
