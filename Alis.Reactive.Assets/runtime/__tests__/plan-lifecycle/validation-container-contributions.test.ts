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
      partialPlan(planId, "address-form", {
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

  it("rejects an extension with a mismatched runtime identity", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";

    browserPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [validationRule("first-name")]),
      },
    });

    expect(() => browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, "address-form", {
        components: {
          "resident-form": validationContainer("other-form", [validationRule("city")]),
        },
      }),
    ], hooks)).toThrow('partial plan contribution "address-slot" cannot declare component "resident-form"');

    expect(validationComponents(browserPlans.get(planId)!, "resident-form")).toEqual(["first-name"]);
  });

  it("rejects an extension that carries registered input binding state", () => {
    const browserPlans = new AppliedBrowserPlans();
    const { hooks } = mergeHooks();
    const planId = "Resident.Root";
    const invalidExtension = validationContainer("resident-form", [validationRule("city")]);
    invalidExtension.binding = {
      kind: "registered-input",
      bindingPath: "ResidentForm",
      valueMember: "value",
    };

    browserPlans.register({
      ...rootPlan(planId),
      components: {
        "resident-form": validationContainer("resident-form", [validationRule("first-name")]),
      },
    });

    expect(() => browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, "address-form", {
        components: {
          "resident-form": invalidExtension,
        },
      }),
    ], hooks)).toThrow('partial plan contribution "address-slot" cannot declare component "resident-form"');

    expect(validationComponents(browserPlans.get(planId)!, "resident-form")).toEqual(["first-name"]);
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
      partialPlan(planId, "server-part-id", {
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
