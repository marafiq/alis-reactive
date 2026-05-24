import { afterEach, describe, expect, it } from "vitest";
import { boot, loadPartialSlot, resetBootStateForTests, unloadPartialSlot } from "../lifecycle/boot";
import { PlanRegistry } from "../lifecycle/merge-plan";
import { resolveGather } from "../execution/gather";
import { showServerErrors } from "../validation/orchestrator";
import type {
  BranchCase,
  Component,
  ComponentValidation,
  Condition,
  JsType,
  ObjectProducer,
  Plan,
  Reaction,
  Shape,
  ValueProducer,
} from "../types";

const stringShape: Shape = { kind: "string" };
const booleanShape: Shape = { kind: "boolean" };
const noneShape: Shape = { kind: "none" };

afterEach(() => {
  resetBootStateForTests();
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
      textContent: {
        path: [{ kind: "property", name: "textContent" }],
        shape: stringShape,
        access: "readwrite",
      },
    },
    methods: {},
    events: {},
  };
}

function inputComponent(id: string, bindingPath: string): Component {
  return registeredInputComponent(id, bindingPath, "value");
}

function registeredInputComponent(id: string, bindingPath: string, valueMember: string): Component {
  return {
    id,
    vendor: "native",
    type: "native.input",
    contribution: { kind: "owned-definition" },
    binding: {
      kind: "registered-input",
      bindingPath,
      valueMember,
    },
    container: { kind: "none" },
  };
}

function displayComponent(id: string): Component {
  return {
    id,
    vendor: "native",
    type: "native.input",
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function validationContainer(id: string, validationRules: ComponentValidation[]): Component {
  return {
    id,
    vendor: "native",
    type: "native.input",
    contribution: { kind: "validation-container" },
    binding: { kind: "none" },
    container: {
      kind: "validation-container",
      components: [],
      validationRules,
    },
  };
}

function validationRule(componentKey: string, serverFieldName = componentKey): ComponentValidation {
  return {
    component: componentKey,
    value: { kind: "none" },
    serverFieldName,
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

function validationComponents(plan: Plan, componentKey: string): string[] {
  const container = plan.components[componentKey].container;
  expect(container.kind).toBe("validation-container");
  if (container.kind !== "validation-container") return [];

  return container.validationRules.map(rule => rule.component);
}

function rootPlan(planId: string, components: Record<string, Component>): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: { "native.input": nativeInputType() },
    components,
    behaviors: [],
  };
}

function partialPlan(
  planId: string,
  partId: string,
  entries: Partial<Pick<Plan, "components" | "behaviors" | "types">>,
): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "partial", partId },
    types: entries.types ?? {},
    components: entries.components ?? {},
    behaviors: entries.behaviors ?? [],
  };
}

function fusionEventType(): JsType {
  return {
    properties: {},
    methods: {},
    events: {
      changed: {
        channel: "change",
        payloadType: { kind: "untyped" },
      },
    },
  };
}

function fusionComponent(id: string): Component {
  return {
    id,
    vendor: "fusion",
    type: "fusion.fake",
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function allRegisteredInputs() {
  return {
    kind: "gather" as const,
    components: [],
    transport: "json" as const,
    statics: { kind: "none" as const },
    selection: { kind: "all-registered-inputs" as const },
  };
}

function literal(value: string | boolean, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}

function objectValue(fields: Record<string, ValueProducer>): ObjectProducer {
  return { kind: "object", fields, shape: noneShape };
}

function falseCondition(): Condition {
  return {
    kind: "compare",
    left: literal(false, booleanShape),
    op: "truthy",
    right: { kind: "none" },
    shape: booleanShape,
    itemShape: noneShape,
  };
}

function setText(component: string, value: string): Reaction {
  return {
    kind: "set",
    on: { kind: "component", component },
    property: "textContent",
    value: literal(value, stringShape),
  };
}

function branch(cases: BranchCase[]): Reaction {
  return { kind: "branch", cases };
}

describe("partial lifecycle across runtime modules", () => {
  it("removes partial registered inputs from all-input gather after the slot unloads", () => {
    document.body.innerHTML = `
      <input id="first-name" value="Ada" />
      <input id="address-line" value="12 Main" />
    `;
    const registry = new PlanRegistry();
    const planId = "Resident.PartialGather";
    const resident = rootPlan(planId, {
      "first-name": inputComponent("first-name", "firstName"),
    });

    registry.register(resident);
    registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "server-address-plan", {
        components: {
          "address-line": inputComponent("address-line", "addressLine"),
        },
      }),
    ], {
      wireBehaviors: () => undefined,
      wireContainerValidation: () => undefined,
    });

    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
      addressLine: "12 Main",
    });

    registry.unloadPartialSlot("address-slot");

    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
    });
  });

  it("does not let dynamically gathered partial inputs replace explicit payload keys", () => {
    document.body.innerHTML = `<input id="address-line" value="12 Main" />`;
    const registry = new PlanRegistry();
    const planId = "Resident.PartialGatherExplicitKey";
    const resident = rootPlan(planId, {});

    registry.register(resident);
    registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "server-address-plan", {
        components: {
          "address-line": inputComponent("address-line", "addressLine"),
        },
      }),
    ], {
      wireBehaviors: () => undefined,
      wireContainerValidation: () => undefined,
    });

    expect(resolveGather({
      ...allRegisteredInputs(),
      components: [
        {
          key: "addressLine",
          value: literal("manual", stringShape),
        },
      ],
    }, "POST", resident, {}).body).toEqual({
      addressLine: "manual",
    });
  });

  it("does not let dynamically gathered partial inputs replace static payload keys", () => {
    document.body.innerHTML = `<input id="address-line" value="12 Main" />`;
    const registry = new PlanRegistry();
    const planId = "Resident.PartialGatherStaticKey";
    const resident = rootPlan(planId, {});

    registry.register(resident);
    registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "server-address-plan", {
        components: {
          "address-line": inputComponent("address-line", "addressLine"),
        },
      }),
    ], {
      wireBehaviors: () => undefined,
      wireContainerValidation: () => undefined,
    });

    expect(resolveGather({
      ...allRegisteredInputs(),
      statics: {
        kind: "value",
        value: objectValue({
          addressLine: literal("manual", stringShape),
        }),
      },
    }, "POST", resident, {}).body).toEqual({
      addressLine: "manual",
    });
  });

  it("does not let dynamically gathered partial inputs replace static nested payload paths", () => {
    document.body.innerHTML = `<input id="address" value="12 Main" />`;
    const registry = new PlanRegistry();
    const planId = "Resident.PartialGatherStaticNestedPath";
    const resident = rootPlan(planId, {});

    registry.register(resident);
    registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "server-address-plan", {
        components: {
          address: inputComponent("address", "address"),
        },
      }),
    ], {
      wireBehaviors: () => undefined,
      wireContainerValidation: () => undefined,
    });

    expect(resolveGather({
      ...allRegisteredInputs(),
      statics: {
        kind: "value",
        value: objectValue({
          "address.city": literal("Seattle", stringShape),
        }),
      },
    }, "POST", resident, {}).body).toEqual({
      address: {
        city: "Seattle",
      },
    });
  });

  it("fails dynamic all-input gather when a registered input value member is not declared", () => {
    document.body.innerHTML = `<input id="first-name" value="Ada" />`;
    const registry = new PlanRegistry();
    const planId = "Resident.PartialGatherContract";
    const resident = rootPlan(planId, {});

    registry.register(resident);
    registry.loadPartialSlot("name-slot", [
      partialPlan(planId, "server-name-plan", {
        components: {
          "first-name": registeredInputComponent("first-name", "firstName", "missingValue"),
        },
      }),
    ], {
      wireBehaviors: () => undefined,
      wireContainerValidation: () => undefined,
    });

    expect(() => resolveGather(allRegisteredInputs(), "POST", resident, {}))
      .toThrow('property "missingValue" not found on component "first-name"');
  });

  it("removes partial validation rules and gathered inputs from a root form after the slot unloads", () => {
    document.body.innerHTML = `
      <form id="resident-form"></form>
      <input id="first-name" value="Ada" />
      <input id="zip-code" value="90210" />
    `;
    const registry = new PlanRegistry();
    const planId = "Resident.PartialValidationGather";
    const resident = rootPlan(planId, {
      "resident-form": validationContainer("resident-form", [
        validationRule("first-name", "FirstName"),
      ]),
      "first-name": inputComponent("first-name", "firstName"),
    });

    registry.register(resident);
    registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "server-address-plan", {
        components: {
          "resident-form": validationContainer("resident-form", [
            validationRule("zip-code", "Address.ZipCode"),
          ]),
          "zip-code": inputComponent("zip-code", "zipCode"),
        },
      }),
    ], {
      wireBehaviors: () => undefined,
      wireContainerValidation: () => undefined,
    });

    expect(validationComponents(resident, "resident-form")).toEqual(["first-name", "zip-code"]);
    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
      zipCode: "90210",
    });

    registry.unloadPartialSlot("address-slot");

    expect(validationComponents(resident, "resident-form")).toEqual(["first-name"]);
    expect(resident.components["zip-code"]).toBeUndefined();
    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
    });
  });

  it("routes server errors for unloaded partial validation fields to the summary", () => {
    document.body.innerHTML = `
      <form id="resident-form"></form>
      <div id="Resident_PartialServerErrors_validation_summary" hidden></div>
    `;
    const registry = new PlanRegistry();
    const planId = "Resident.PartialServerErrors";
    const resident = rootPlan(planId, {
      "resident-form": validationContainer("resident-form", [
        validationRule("zip-code", "Address.ZipCode"),
      ]),
    });

    registry.register(resident);
    registry.loadPartialSlot("address-slot", [
      partialPlan(planId, "server-address-plan", {
        components: {
          "zip-code": inputComponent("zip-code", "zipCode"),
        },
      }),
    ], {
      wireBehaviors: () => undefined,
      wireContainerValidation: () => undefined,
    });

    registry.unloadPartialSlot("address-slot");

    showServerErrors(resident, "resident-form", {
      errors: {
        "Address.ZipCode": ["Zip code is required"],
      },
    });

    const summary = document.getElementById("Resident_PartialServerErrors_validation_summary");
    expect(summary?.hasAttribute("hidden")).toBe(false);
    expect(document.querySelector("[data-valmsg-summary-for='Address.ZipCode']")?.textContent)
      .toBe("Zip code is required");
  });

  it("unwires nested branch and else behavior when a partial slot unloads", () => {
    document.body.innerHTML = `<span id="status"></span>`;
    const planId = "Resident.PartialBranch";
    const resident = rootPlan(planId, {
      status: displayComponent("status"),
    });
    boot(resident);

    loadPartialSlot("drawer-slot", [
      partialPlan(planId, "server-drawer-plan", {
        behaviors: [
          {
            startsWhen: {
              kind: "document-event",
              event: "drawer:tick",
              payloadType: { kind: "untyped" },
            },
            reaction: branch([
              { guard: { kind: "when", condition: falseCondition() }, reaction: setText("status", "outer guarded") },
              {
                guard: { kind: "default" },
                reaction: branch([
                  { guard: { kind: "when", condition: falseCondition() }, reaction: setText("status", "inner guarded") },
                  { guard: { kind: "default" }, reaction: setText("status", "inner else") },
                ]),
              },
            ]),
          },
        ],
      }),
    ]);

    document.dispatchEvent(new CustomEvent("drawer:tick"));
    expect(document.getElementById("status")?.textContent).toBe("inner else");

    document.getElementById("status")!.textContent = "";
    unloadPartialSlot("drawer-slot");
    document.dispatchEvent(new CustomEvent("drawer:tick"));

    expect(document.getElementById("status")?.textContent).toBe("");
  });

  it("unwires fusion component events when a partial slot unloads", () => {
    document.body.innerHTML = `
      <span id="status"></span>
      <div id="fusion-source"></div>
    `;
    const fusionRoot = new FakeFusionRoot();
    const fusionElement = document.getElementById("fusion-source") as FusionHostElement;
    fusionElement.ej2_instances = [fusionRoot];

    const planId = "Resident.PartialFusionEvent";
    const resident = rootPlan(planId, {
      status: displayComponent("status"),
    });
    boot(resident);

    loadPartialSlot("fusion-slot", [
      partialPlan(planId, "server-fusion-plan", {
        types: { "fusion.fake": fusionEventType() },
        components: {
          source: fusionComponent("fusion-source"),
        },
        behaviors: [
          {
            startsWhen: {
              kind: "component-event",
              component: "source",
              event: "changed",
            },
            reaction: setText("status", "changed"),
          },
        ],
      }),
    ]);

    fusionRoot.emit("change");
    expect(document.getElementById("status")?.textContent).toBe("changed");

    document.getElementById("status")!.textContent = "";
    unloadPartialSlot("fusion-slot");
    fusionRoot.emit("change");

    expect(document.getElementById("status")?.textContent).toBe("");
  });
});

interface FusionHostElement extends HTMLElement {
  ej2_instances?: unknown[];
}

class FakeFusionRoot {
  private readonly handlers = new Map<string, Set<(args: unknown) => void>>();

  addEventListener(channel: string, handler: (args: unknown) => void): void {
    this.handlersFor(channel).add(handler);
  }

  removeEventListener(channel: string, handler: (args: unknown) => void): void {
    this.handlers.get(channel)?.delete(handler);
  }

  emit(channel: string): void {
    for (const handler of this.handlersFor(channel)) handler({});
  }

  private handlersFor(channel: string): Set<(args: unknown) => void> {
    let handlers = this.handlers.get(channel);
    if (handlers === undefined) {
      handlers = new Set<(args: unknown) => void>();
      this.handlers.set(channel, handlers);
    }

    return handlers;
  }
}
