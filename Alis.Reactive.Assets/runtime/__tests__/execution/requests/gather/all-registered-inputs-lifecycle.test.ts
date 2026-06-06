import { afterEach, describe, expect, it } from "vitest";
import { resolveRequestInput } from "../../../../execution/requests/gather";
import { AppliedPlans } from "../../../../lifecycle/applied-plans";
import type { ComponentObject, RequestPayloadTarget, BrowserObjectContract, PathSegment, PlanDocument, RequestInput, Shape, StructuredPath, ValueExpression } from "../../../../types/index";

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

    expect(resolveRequestInput(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
    });
  });

  it("removes partial registered inputs after the slot unloads", () => {
    document.body.innerHTML = `
      <input id="first-name" value="Ada" />
      <input id="address-line" value="12 Main" />
    `;
    const appliedPlans = new AppliedPlans();
    const planId = "Resident.PartialGather";
    const resident = rootPlan(planId, {
      "first-name": inputComponent("first-name", "firstName"),
    });

    appliedPlans.register(resident);
    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          "address-line": inputComponent("address-line", "addressLine"),
        },
      }),
    ], silentLifecycleHooks);

    expect(resolveRequestInput(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
      addressLine: "12 Main",
    });

    appliedPlans.unloadPartialSlot("address-slot");

    expect(resolveRequestInput(allRegisteredInputs(), "POST", resident, {}).body).toEqual({
      firstName: "Ada",
    });
  });

  it("writes authored body field assignments before runtime selected registered inputs", () => {
    document.body.innerHTML = `
      <input id="first-name" value="Ada" />
      <input id="address" value="12 Main" />
    `;
    const appliedPlans = new AppliedPlans();
    const planId = "Resident.PartialGatherAssignments";
    const resident = rootPlan(planId, {
      "first-name": inputComponent("first-name", "firstName"),
    });

    appliedPlans.register(resident);
    appliedPlans.loadPartialSlot("address-slot", [
      partialPlan(planId, {
        components: {
          address: inputComponent("address", "address"),
        },
      }),
    ], silentLifecycleHooks);

    expect(resolveRequestInput({
      ...allRegisteredInputs(),
      assignments: [{
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

function rootPlan(planId: string, components: Record<string, ComponentObject>): PlanDocument {
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
  planParts: Partial<Pick<PlanDocument, "components" | "behaviors" | "types">>,
): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
    types: planParts.types ?? {},
    components: planParts.components ?? {},
    behaviors: planParts.behaviors ?? [],
  };
}

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

function inputComponent(id: string, bindingPath: string): ComponentObject {
  return registeredInputComponent(id, bindingPath, "value");
}

function unboundComponent(id: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.input",
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function registeredInputComponent(id: string, bindingPath: string, valueMember: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.input",
    role: { kind: "plan-input" },
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
  return { kind: "payload", name, path: structuredPath(name) };
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
    assignments: [],
    bodyFormat: "json",
    registeredInputs: { kind: "all-registered-inputs" },
  };
}

function literal(value: string, shape: Shape): ValueExpression {
  return { kind: "literal", value, shape };
}
