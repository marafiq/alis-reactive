import { afterEach, describe, expect, it } from "vitest";
import { AppliedPlans } from "../lifecycle/applied-plans";
import { revalidateField, showServerErrors, validateContainer } from "../validation/orchestrator";
import type { ComponentObject, ComponentValidation, BrowserObjectContract, PlanDocument, ReadExpression, Shape, ValueExpression } from "../types/index";

const stringShape: Shape = { kind: "string" };
const noneShape: Shape = { kind: "none" };

afterEach(() => {
  document.body.innerHTML = "";
});

function nativeInputType(): BrowserObjectContract {
  return {
    properties: {
      value: {
        path: [{ kind: "property", name: "value" }],
        shape: stringShape,
        access: "readwrite",
      },
    },
    methods: {},
    events: {},
  };
}

function nativeComponent(id: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.input",
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function validationContainer(id: string, rules: ComponentValidation[]): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.input",
    role: { kind: "validation-container" },
    binding: { kind: "none" },
    container: {
      kind: "validation-container",
      validationRules: rules,
    },
  };
}

function requiredRule(
  component: string,
  serverFieldName: string,
  valueComponent = component,
): ComponentValidation {
  return {
    component,
    serverFieldName,
    value: {
      kind: "read",
      from: { kind: "component", component: valueComponent },
      member: "value",
      path: [],
      shape: stringShape,
      access: { kind: "property" },
    },
    rules: [
      {
        name: "required",
        message: `${serverFieldName} is required`,
        execution: {
          kind: "none",
          activation: { kind: "always" },
          comparisonShape: noneShape,
        },
      },
    ],
  };
}

function conditionalRuleWithMissingActivationSource(): ComponentValidation {
  return {
    component: "resident-name-field",
    serverFieldName: "Name",
    value: readComponentValue("resident-name-field"),
    rules: [
      {
        name: "required",
        message: "Name is required",
        execution: {
          kind: "none",
          activation: {
            kind: "when",
            condition: {
              kind: "compare",
              left: readComponentValue("missing-activation-source"),
              op: "eq",
              right: { kind: "value", value: literal("yes") },
              shape: stringShape,
              itemShape: noneShape,
            },
          },
          comparisonShape: noneShape,
        },
      },
    ],
  };
}

function unmountedConditionalRuleWithMissingActivationSource(): ComponentValidation {
  return {
    ...conditionalRuleWithMissingActivationSource(),
    component: "unmounted-dependent-field",
    serverFieldName: "DependentField",
    value: readComponentValue("unmounted-dependent-field"),
  };
}

function readComponentValue(component: string): ReadExpression {
  return {
    kind: "read",
    from: { kind: "component", component },
    member: "value",
    path: [],
    shape: stringShape,
    access: { kind: "property" },
  };
}

function literal(value: string): ValueExpression {
  return {
    kind: "literal",
    value,
    shape: stringShape,
  };
}

function validationRuntimePlan(
  validationRules: ComponentValidation[],
  components: Record<string, ComponentObject> = {
    "resident-name-field": nativeComponent("resident-name-input"),
  },
): PlanDocument {
  return {
    version: 3,
    planId: "Runtime.ValidationServerErrors",
    scope: { kind: "root" },
    types: { "native.input": nativeInputType() },
    components: {
      "resident-form": validationContainer("resident-form", validationRules),
      ...components,
    },
    behaviors: [],
  };
}

function renderValidationDom(): void {
  document.body.innerHTML = `
    <form id="resident-form">
      <input id="resident-name-input" value="" />
      <span id="resident-name-input_error" hidden></span>
    </form>
    <div id="Runtime_ValidationServerErrors_validation_summary" hidden></div>
  `;
}

function renderValidationDomWithoutErrorSlot(): void {
  document.body.innerHTML = `
    <form id="resident-form">
      <input id="resident-name-input" value="" />
    </form>
    <div id="Runtime_ValidationServerErrors_validation_summary" hidden></div>
  `;
}

function renderValidationDomWithHiddenErrorSlot(): void {
  document.body.innerHTML = `
    <form id="resident-form">
      <div hidden>
        <input id="resident-name-input" value="" />
        <span id="resident-name-input_error" hidden></span>
      </div>
    </form>
    <div id="Runtime_ValidationServerErrors_validation_summary" hidden></div>
  `;
}

describe("validation orchestrator server errors", () => {
  it("places server errors using the declared server field name", () => {
    renderValidationDom();
    const runtimePlan = validationRuntimePlan([
      requiredRule("resident-name-field", "Name"),
    ]);

    showServerErrors(runtimePlan, "resident-form", {
      errors: { Name: ["Name is required"] },
    });

    expect(document.getElementById("resident-name-input_error")?.textContent)
      .toBe("Name is required");
    expect(document.getElementById("Runtime_ValidationServerErrors_validation_summary")?.textContent)
      .toBe("");
  });

  it("does not treat component keys as server field names", () => {
    renderValidationDom();
    const runtimePlan = validationRuntimePlan([
      requiredRule("resident-name-field", "Name"),
    ]);

    showServerErrors(runtimePlan, "resident-form", {
      errors: { "resident-name-field": ["Wrong server field"] },
    });

    expect(document.getElementById("resident-name-input_error")?.textContent)
      .toBe("");
    expect(document.querySelector("[data-valmsg-summary-for='resident-name-field']")?.textContent)
      .toBe("Wrong server field");
  });

  it("routes server errors for known fields to summary when the component is not in the Active Plan", () => {
    renderValidationDom();
    const runtimePlan = validationRuntimePlan([
      requiredRule("notify-field", "ReceiveNotifications"),
    ], {});

    showServerErrors(runtimePlan, "resident-form", {
      errors: { ReceiveNotifications: ["Notification choice is required"] },
    });

    const summary = document.getElementById("Runtime_ValidationServerErrors_validation_summary");
    expect(summary?.hasAttribute("hidden")).toBe(false);
    expect(document.querySelector("[data-valmsg-summary-for='ReceiveNotifications']")?.textContent)
      .toBe("Notification choice is required");
  });

  it("routes server errors for unloaded partial fields to the summary", () => {
    renderValidationDom();
    const appliedPlans = new AppliedPlans();
    const runtimePlan = validationRuntimePlan([
      requiredRule("zip-code-field", "Address.ZipCode", "zip-code-field"),
    ], {});

    appliedPlans.register(runtimePlan);
    appliedPlans.loadPartialSlot("address-slot", [
      {
        version: 3,
        planId: runtimePlan.planId,
        scope: { kind: "partial" },
        types: {},
        components: {
          "zip-code-field": nativeComponent("zip-code-input"),
        },
        behaviors: [],
      },
    ], silentLifecycleHooks);
    appliedPlans.unloadPartialSlot("address-slot");

    showServerErrors(runtimePlan, "resident-form", {
      errors: { "Address.ZipCode": ["Zip code is required"] },
    });

    const summary = document.getElementById("Runtime_ValidationServerErrors_validation_summary");
    expect(summary?.hasAttribute("hidden")).toBe(false);
    expect(document.querySelector("[data-valmsg-summary-for='Address.ZipCode']")?.textContent)
      .toBe("Zip code is required");
  });

  it("routes server errors for known fields to summary when the component element is not mounted", () => {
    renderValidationDom();
    const runtimePlan = validationRuntimePlan([
      requiredRule("notify-field", "ReceiveNotifications"),
    ], {
      "notify-field": nativeComponent("notify-input"),
    });

    showServerErrors(runtimePlan, "resident-form", {
      errors: { ReceiveNotifications: ["Notification choice is required"] },
    });

    expect(document.querySelector("[data-valmsg-summary-for='ReceiveNotifications']")?.textContent)
      .toBe("Notification choice is required");
  });

  it("routes server errors for known fields to summary when the inline error slot is missing", () => {
    renderValidationDomWithoutErrorSlot();
    const runtimePlan = validationRuntimePlan([
      requiredRule("resident-name-field", "Name"),
    ]);

    showServerErrors(runtimePlan, "resident-form", {
      errors: { Name: ["Name is required"] },
    });

    expect(document.querySelector("[data-valmsg-summary-for='Name']")?.textContent)
      .toBe("Name is required");
  });

  it("routes server errors for known fields to summary when the inline error slot is hidden by layout", () => {
    renderValidationDomWithHiddenErrorSlot();
    const runtimePlan = validationRuntimePlan([
      requiredRule("resident-name-field", "Name"),
    ]);

    showServerErrors(runtimePlan, "resident-form", {
      errors: { Name: ["Name is required"] },
    });

    expect(document.getElementById("resident-name-input_error")?.textContent)
      .toBe("");
    expect(document.querySelector("[data-valmsg-summary-for='Name']")?.textContent)
      .toBe("Name is required");
  });
});

describe("validation orchestrator client rules", () => {
  it("matches summary entries by component key without CSS selector interpolation", () => {
    renderValidationDomWithHiddenErrorSlot();
    const componentKey = 'resident["name"]';
    const runtimePlan = validationRuntimePlan([
      requiredRule(componentKey, "Name"),
    ], {
      [componentKey]: nativeComponent("resident-name-input"),
    });

    expect(validateContainer(runtimePlan, "resident-form")).toBe(false);
    expect(summaryTextFor(componentKey)).toBe("Name is required");
  });

  it("treats an unmounted activation dependency as inactive only for an unmounted validation field", () => {
    renderValidationDom();
    const runtimePlan = validationRuntimePlan([
      unmountedConditionalRuleWithMissingActivationSource(),
    ]);

    expect(validateContainer(runtimePlan, "resident-form")).toBe(true);
  });
});

describe("validation orchestrator summary", () => {
  function summaryElement(): HTMLElement {
    return document.getElementById("Runtime_ValidationServerErrors_validation_summary") as HTMLElement;
  }

  // The name field sits in a section the user then opens: its message slot was hidden at validation.
  function reportNameWhileItsSectionIsHidden(): PlanDocument {
    renderValidationDomWithHiddenErrorSlot();
    const runtimePlan = validationRuntimePlan([requiredRule("resident-name-field", "Name")]);
    validateContainer(runtimePlan, "resident-form");
    (document.getElementById("resident-name-input")!.parentElement as HTMLElement).hidden = false;
    return runtimePlan;
  }

  it("takes a field's error out of the summary once the field passes, and hides the empty summary", () => {
    const runtimePlan = reportNameWhileItsSectionIsHidden();
    expect(summaryTextFor("resident-name-field")).toBe("Name is required");

    (document.getElementById("resident-name-input") as HTMLInputElement).value = "Edith Collins";
    revalidateField(runtimePlan, "resident-form", "resident-name-field");

    expect(summaryTextFor("resident-name-field")).toBeUndefined();
    expect(summaryElement().hidden).toBe(true);
  });

  it("hides the summary when a field's error moves beside the field and no other entry remains", () => {
    const runtimePlan = reportNameWhileItsSectionIsHidden();

    revalidateField(runtimePlan, "resident-form", "resident-name-field");

    expect(document.getElementById("resident-name-input_error")?.textContent).toBe("Name is required");
    expect(summaryElement().children).toHaveLength(0);
    expect(summaryElement().hidden).toBe(true);
  });

  it("keeps one summary entry per field when a field whose message slot is hidden fails again", () => {
    renderValidationDomWithHiddenErrorSlot();
    const runtimePlan = validationRuntimePlan([requiredRule("resident-name-field", "Name")]);
    validateContainer(runtimePlan, "resident-form");

    revalidateField(runtimePlan, "resident-form", "resident-name-field");

    expect(summaryElement().children).toHaveLength(1);
    expect(summaryTextFor("resident-name-field")).toBe("Name is required");
    expect(summaryElement().hidden).toBe(false);
  });
});

const silentLifecycleHooks = {
  wireBehaviors: () => undefined,
  wireContainerValidation: () => undefined,
};

function summaryTextFor(name: string): string | undefined {
  const summary = document.getElementById("Runtime_ValidationServerErrors_validation_summary");
  if (summary === null) return undefined;

  for (const child of summary.children) {
    if (!(child instanceof HTMLElement)) continue;
    if (child.dataset.valmsgSummaryFor === name) return child.textContent ?? undefined;
  }

  return undefined;
}
