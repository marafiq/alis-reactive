import { afterEach, describe, expect, it } from "vitest";
import { resolveGather } from "../../execution/gather";
import { AppliedBrowserPlans } from "../../lifecycle/merge-plan";
import type { Component, RequestPayloadTarget, JsType, PathSegment, Plan, RequestInput, Shape, StructuredPath, ValueProducer } from "../../types";

const stringShape: Shape = { kind: "string" };

afterEach(() => {
  document.body.innerHTML = "";
});

describe("all registered input gather lifecycle", () => {
  it("gathers mounted registered inputs and skips unbound or unmounted components", () => {
    document.body.innerHTML = `
      <input id="first-name" value="Ada" />
      <div id="drawer"></div>
    `;
    const resident = rootPlan("Resident.RegisteredInputSelection", {
      "first-name": inputComponent("first-name", "firstName"),
      "last-name": inputComponent("last-name", "lastName"),
      drawer: unboundComponent("drawer"),
    });

    expect(resolveGather(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
    });
  });

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

  it("emits explicit fields before dynamically gathered registered input assignments", () => {
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
      fields: [{
        target: target("selected"),
        source: literal("manual", stringShape),
      }, {
        target: target("address.city"),
        source: literal("Seattle", stringShape),
      }],
    }, "POST", resident, {}).body).toEqual({
      selected: "manual",
      firstName: "Ada",
      address: "12 Main",
    });
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

function unboundComponent(id: string): Component {
  return {
    id,
    vendor: "native",
    type: "native.input",
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
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
    fields: [],
    transport: "json",
    selection: { kind: "all-registered-inputs" },
  };
}

function literal(value: string, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}
