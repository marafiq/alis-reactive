import { afterEach, describe, expect, it } from "vitest";
import { PlanRegistry } from "../lifecycle/merge-plan";
import { showServerErrors, validateContainer } from "../validation/orchestrator";
import type { Component, ComponentValidation, JsType, Plan, Shape, ValueProducer } from "../types";

const stringShape: Shape = { kind: "string" };
const noneShape: Shape = { kind: "none" };

afterEach(() => {
  document.body.innerHTML = "";
});

function nativeInputType(): JsType {
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

function nativeComponent(id: string): Component {
  return {
    id,
    vendor: "native",
    type: "native.input",
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function validationContainer(id: string, rules: ComponentValidation[]): Component {
  return {
    id,
    vendor: "native",
    type: "native.input",
    contribution: { kind: "validation-container" },
    binding: { kind: "none" },
    container: {
      kind: "validation-container",
      components: [],
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
          constraint: { kind: "none" },
          otherValue: { kind: "none" },
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
          constraint: { kind: "none" },
          otherValue: { kind: "none" },
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

function peerRuleWithMissingPeerSource(): ComponentValidation {
  return {
    component: "resident-name-field",
    serverFieldName: "Name",
    value: readComponentValue("resident-name-field"),
    rules: [
      {
        name: "equalTo",
        message: "Name must match",
        execution: {
          constraint: { kind: "none" },
          otherValue: {
            kind: "value",
            value: readComponentValue("missing-peer-source"),
          },
          activation: { kind: "always" },
          comparisonShape: stringShape,
        },
      },
    ],
  };
}

function readComponentValue(component: string): ValueProducer {
  return {
    kind: "read",
    from: { kind: "component", component },
    member: "value",
    path: [],
    shape: stringShape,
    access: { kind: "property" },
  };
}

function literal(value: string): ValueProducer {
  return {
    kind: "literal",
    value,
    shape: stringShape,
  };
}

function plan(
  validationRules: ComponentValidation[],
  components: Record<string, Component> = {
    "resident-name-field": nativeComponent("resident-name-input"),
  },
): Plan {
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
    const runtimePlan = plan([
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
    const runtimePlan = plan([
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

  it("routes server errors for known fields to summary when the component is not in the current plan", () => {
    renderValidationDom();
    const runtimePlan = plan([
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
    const registry = new PlanRegistry();
    const runtimePlan = plan([
      requiredRule("zip-code-field", "Address.ZipCode", "zip-code-field"),
    ], {});

    registry.register(runtimePlan);
    registry.loadPartialSlot("address-slot", [
      {
        version: 3,
        planId: runtimePlan.planId,
        scope: { kind: "partial", partId: "server-address-plan" },
        types: {},
        components: {
          "zip-code-field": nativeComponent("zip-code-input"),
        },
        behaviors: [],
      },
    ], silentLifecycleHooks);
    registry.unloadPartialSlot("address-slot");

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
    const runtimePlan = plan([
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
    const runtimePlan = plan([
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
    const runtimePlan = plan([
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
  it("does not hide a miswired validation value producer behind missing field behavior", () => {
    renderValidationDom();
    const runtimePlan = plan([
      requiredRule("resident-name-field", "Name", "missing-component"),
    ]);

    expect(() => validateContainer(runtimePlan, "resident-form"))
      .toThrow("[alis] component not found: missing-component");
  });

  it("does not hide a miswired activation dependency behind conditional skip behavior", () => {
    renderValidationDom();
    const runtimePlan = plan([
      conditionalRuleWithMissingActivationSource(),
    ]);

    expect(() => validateContainer(runtimePlan, "resident-form"))
      .toThrow("[alis] component not found: missing-activation-source");
  });

  it("does not hide a miswired peer dependency behind an absent peer value", () => {
    renderValidationDom();
    const runtimePlan = plan([
      peerRuleWithMissingPeerSource(),
    ]);

    expect(() => validateContainer(runtimePlan, "resident-form"))
      .toThrow("[alis] component not found: missing-peer-source");
  });
});

const silentLifecycleHooks = {
  wireBehaviors: () => undefined,
  wireContainerValidation: () => undefined,
};
