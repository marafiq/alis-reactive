import { describe, expect, it } from "vitest";
import { AppliedBrowserPlans } from "../../lifecycle/merge-plan";
import {
  mergeHooks,
  partialPlan,
  rootPlan,
  validationComponents,
  validationContainer,
  validationRule,
} from "../support/plan-lifecycle-fixtures";

describe("validation container contributions", () => {
  it("accepts matching validated component ids while adding new partial rules", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    browserPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
          validationRule("zip-code", "zip required"),
        ]),
      },
    });

    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "zip required"),
            validationRule("city"),
          ]),
        },
      }),
    ], hooks);

    const merged = browserPlans.get(planId)!;
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
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    browserPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
        ]),
      },
    });

    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("address-line"),
          ]),
        },
      }),
    ], hooks);

    expect(validationComponents(browserPlans.get(planId)!, "resident-form")).toEqual([
      "first-name",
      "address-line",
    ]);

    browserPlans.unloadPartialSlot("address-slot");

    expect(validationComponents(browserPlans.get(planId)!, "resident-form")).toEqual(["first-name"]);
  });

  it("unloading a validation extension keeps root rules for the same validated component", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    browserPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
          validationRule("zip-code", "root zip required"),
        ]),
      },
    });

    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "partial zip required"),
            validationRule("city"),
          ]),
        },
      }),
    ], hooks);

    const extendedContainer = expectValidationContainer(browserPlans.get(planId)!.components["resident-form"].container);
    expect(extendedContainer.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "zip-code",
      "city",
    ]);
    expect(extendedContainer.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("root zip required");

    browserPlans.unloadPartialSlot("address-slot");

    const restoredContainer = expectValidationContainer(browserPlans.get(planId)!.components["resident-form"].container);
    expect(restoredContainer.validationRules.map(rule => rule.component)).toEqual([
      "first-name",
      "zip-code",
    ]);
    expect(restoredContainer.validationRules.find(rule => rule.component === "zip-code")?.rules[0]?.message)
      .toBe("root zip required");
  });

  it("unloading one validation extension keeps sibling slot rules", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    browserPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [
          validationRule("first-name"),
        ]),
      },
    });

    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("address-line"),
          ]),
        },
      }),
    ], hooks);
    browserPlans.loadPartialSlot("contact-slot", [
      partialPlan(planId, {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("phone"),
          ]),
        },
      }),
    ], hooks);

    expect(validationComponents(browserPlans.get(planId)!, "resident-form")).toEqual([
      "first-name",
      "address-line",
      "phone",
    ]);

    browserPlans.unloadPartialSlot("address-slot");

    expect(validationComponents(browserPlans.get(planId)!, "resident-form")).toEqual([
      "first-name",
      "phone",
    ]);
  });
});

function expectValidationContainer(
  container: Component["container"],
): Extract<Component["container"], { kind: "validation-container" }> {
  expect(container.kind).toBe("validation-container");
  if (container.kind !== "validation-container") {
    throw new Error(`Expected validation-container scope, received "${container.kind}"`);
  }

  return container;
}
