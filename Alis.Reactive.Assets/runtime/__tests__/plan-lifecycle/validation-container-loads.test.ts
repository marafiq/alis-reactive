import { describe, expect, it } from "vitest";
import { AppliedPlans } from "../../lifecycle/applied-plans";
import {
  testPlanWiring,
  partialPlan,
  rootPlan,
  validationComponents,
  validationContainer,
  validationRule,
} from "../support/plan-lifecycle-fixtures";

describe("validation container loads", () => {
  it("accepts matching validated component ids while adding new partial rules", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";

    appliedPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
          validationRule("zip-code", "zip required"),
        ]),
      },
    });

    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "zip required"),
            validationRule("city"),
          ]),
        },
      }),
    ], wiring);

    const merged = appliedPlans.get(planId)!;
    const container = merged.components["resident-form"].container;
    const validationScope = expectValidationContainer(container);

    expect(validationScope.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "zip-code",
      "city",
    ]);
    expect(validationScope.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("zip required");
  });

  it("preserves root-owned validation containers when unloading a partial slot", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";

    appliedPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
        ]),
      },
    });

    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("address-line"),
          ]),
        },
      }),
    ], wiring);

    expect(validationComponents(appliedPlans.get(planId)!, "resident-form")).toEqual([
      "first-name",
      "address-line",
    ]);

    appliedPlans.unloadPartialSlot("address-slot");

    expect(validationComponents(appliedPlans.get(planId)!, "resident-form")).toEqual(["first-name"]);
  });

  it("unloading a validation extension keeps root rules for the same validated component", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";

    appliedPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
          validationRule("zip-code", "root zip required"),
        ]),
      },
    });

    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "partial zip required"),
            validationRule("city"),
          ]),
        },
      }),
    ], wiring);

    const extendedContainer = expectValidationContainer(appliedPlans.get(planId)!.components["resident-form"].container);
    expect(extendedContainer.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "zip-code",
      "city",
    ]);
    expect(extendedContainer.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("root zip required");

    appliedPlans.unloadPartialSlot("address-slot");

    const restoredContainer = expectValidationContainer(appliedPlans.get(planId)!.components["resident-form"].container);
    expect(restoredContainer.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "zip-code",
    ]);
    expect(restoredContainer.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("root zip required");
  });

  it("unloading one validation extension keeps sibling slot rules", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";

    appliedPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
        ]),
      },
    });

    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("address-line"),
          ]),
        },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("contact-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("phone"),
          ]),
        },
      }),
    ], wiring);

    expect(validationComponents(appliedPlans.get(planId)!, "resident-form")).toEqual([
      "first-name",
      "address-line",
      "phone",
    ]);

    appliedPlans.unloadPartialSlot("address-slot");

    expect(validationComponents(appliedPlans.get(planId)!, "resident-form")).toEqual([
      "first-name",
      "phone",
    ]);
  });

  it("unloading one validation extension keeps duplicate field rules from a later active slot", () => {
    const appliedPlans = new AppliedPlans();
    const { wiring } = testPlanWiring();
    const planId = "Resident.Root";

    appliedPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
        ]),
      },
    });

    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("city", "address city required"),
          ]),
        },
      }),
    ], wiring);
    appliedPlans.loadPartialSlot("contact-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("city", "contact city required"),
          ]),
        },
      }),
    ], wiring);

    const beforeUnload = expectValidationContainer(appliedPlans.get(planId)!.components["resident-form"].container);
    expect(beforeUnload.validationRules.map(rule => rule.component)).toEqual(["first-name", "city"]);
    expect(beforeUnload.validationRules.find(rule => rule.component === "city")?.rules[0]?.message)
      .toBe("address city required");

    appliedPlans.unloadPartialSlot("address-slot");

    const afterUnload = expectValidationContainer(appliedPlans.get(planId)!.components["resident-form"].container);
    expect(afterUnload.validationRules.map(rule => rule.component)).toEqual(["first-name", "city"]);
    expect(afterUnload.validationRules.find(rule => rule.component === "city")?.rules[0]?.message)
      .toBe("contact city required");
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
