import { afterEach, describe, expect, it } from "vitest";
import { resolveGather } from "../../execution/gather";
import { AppliedBrowserPlans } from "../../lifecycle/merge-plan";
import type { Component, RequestPayloadTarget, JsType, PathSegment, Plan, RequestInput, Shape, StructuredPath, ValueProducer } from "../../types";

const stringShape: Shape = { kind: "string" };

afterEach(() => {
  document.body.innerHTML = "";
});

describe("all registered input gather lifecycle", () => {
  it("removes partial registered inputs after the slot unloads", () => {
    document.body.innerHTML = `
      <input id="first-name" value="Ada" />
      <input id="address-line" value="12 Main" />
    `;
    const browserPlans = new AppliedBrowserPlans();
    const planId = "Resident.PartialGather";
    const resident = rootPlan(planId, {
      "first-name": inputComponent("first-name", "firstName"),
    });

    browserPlans.register(resident);
    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "address-line": inputComponent("address-line", "addressLine"),
        },
      }),
    ], silentLifecycleHooks);

    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
      addressLine: "12 Main",
    });

    browserPlans.unloadPartialSlot("address-slot");

    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
    });
  });

  it("emits explicit, supplemental, and dynamically gathered registered input assignments", () => {
    document.body.innerHTML = `
      <input id="first-name" value="Ada" />
      <input id="address" value="12 Main" />
    `;
    const browserPlans = new AppliedBrowserPlans();
    const planId = "Resident.PartialGatherAssignments";
    const resident = rootPlan(planId, {
      "first-name": inputComponent("first-name", "firstName"),
    });

    browserPlans.register(resident);
    browserPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          address: inputComponent("address", "address"),
        },
      }),
    ], silentLifecycleHooks);

    expect(resolveGather({
      ...allRegisteredInputs(),
      declaredFields: [{
        target: target("selected"),
        source: literal("manual", stringShape),
      }],
      supplementalFields: [{
        target: target("address.city"),
        source: literal("Seattle", stringShape),
      }],
    }, "POST", resident, {}).body).toEqual({
      selected: "manual",
      firstName: "Ada",
      address: "12 Main",
    });
  });

  it("rejects a registered input whose value member is missing from the component contract", () => {
    document.body.innerHTML = `<input id="first-name" value="Ada" />`;
    const browserPlans = new AppliedBrowserPlans();
    const planId = "Resident.PartialGatherContract";
    const resident = rootPlan(planId, {});

    browserPlans.register(resident);
    browserPlans.loadPartialSlot("name-slot", [
      partialPlan(planId, {
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
  entries: Partial<Pick<Plan, "components" | "behaviors" | "types">>,
): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
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
      path: structuredPath(bindingPath),
      valueMember,
    },
    container: { kind: "none" },
  };
}

function target(name: string): RequestPayloadTarget {
  return { name, path: structuredPath(name) };
}

function structuredPath(name: string): StructuredPath {
  const [first, ...rest] = name.split(".").map(pathSegment);
  if (first === undefined) throw new Error(`Expected path for ${name}`);

  return [first, ...rest];
}

function pathSegment(part: string): PathSegment {
  return { kind: "property", name: part };
}

function allRegisteredInputs(): Extract<RequestInput, { kind: "gather" }> {
  return {
    kind: "gather",
    declaredFields: [],
    registeredInputFields: [],
    transport: "json",
    supplementalFields: [],
    selection: { kind: "all-registered-inputs" },
  };
}

function literal(value: string, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}
