import { afterEach, describe, expect, it } from "vitest";
import { resolveGather } from "../../execution/gather";
import { PlanRegistry } from "../../lifecycle/merge-plan";
import type { Component, JsType, ObjectProducer, Plan, RequestInput, Shape, ValueProducer } from "../../types";

const stringShape: Shape = { kind: "string" };
const noneShape: Shape = { kind: "none" };

afterEach(() => {
  document.body.innerHTML = "";
});

describe("all registered input gather lifecycle", () => {
  it("removes partial registered inputs after the slot unloads", () => {
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
    ], silentLifecycleHooks);

    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
      addressLine: "12 Main",
    });

    registry.unloadPartialSlot("address-slot");

    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
    });
  });

  it("keeps explicit payload keys ahead of dynamically gathered partial inputs", () => {
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
    ], silentLifecycleHooks);

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

  it("keeps static payload keys ahead of dynamically gathered partial inputs", () => {
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
    ], silentLifecycleHooks);

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

  it("keeps static nested payload paths ahead of dynamically gathered partial inputs", () => {
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
    ], silentLifecycleHooks);

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

  it("rejects a registered input whose value member is missing from the component contract", () => {
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
    ], silentLifecycleHooks);

    expect(() => resolveGather(allRegisteredInputs(), "POST", resident, {}))
      .toThrow('property "missingValue" not found on component "first-name"');
  });
});

const silentLifecycleHooks = {
  wireBehaviors: () => undefined,
  wireContainerValidation: () => undefined,
};

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

function allRegisteredInputs(): Extract<RequestInput, { kind: "gather" }> {
  return {
    kind: "gather",
    components: [],
    transport: "json",
    statics: { kind: "none" },
    selection: { kind: "all-registered-inputs" },
  };
}

function literal(value: string, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}

function objectValue(fields: Record<string, ValueProducer>): ObjectProducer {
  return { kind: "object", fields, shape: noneShape };
}
