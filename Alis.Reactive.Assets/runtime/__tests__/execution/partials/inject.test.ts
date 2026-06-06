import { afterEach, describe, expect, it } from "vitest";
import { resolveRequestInput } from "../../../execution/requests/gather";
import { injectPartial } from "../../../execution/partials/inject";
import { boot, getBootedPlan, resetBootStateForTests } from "../../../lifecycle/boot";
import type { ComponentObject, BrowserObjectContract, PathSegment, PlanDocument, RequestInput, Shape, StructuredPath } from "../../../types/index";

const stringShape: Shape = { kind: "string" };

function objectContract(): BrowserObjectContract {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

function inputContract(): BrowserObjectContract {
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

function component(id: string, type = `native.element.${id}`): ComponentObject {
  return {
    id,
    vendor: "native",
    type,
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function inputComponent(id: string, bindingPath: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.input",
    role: { kind: "plan-input" },
    binding: {
      kind: "registered-input",
      bindingPath,
      path: structuredPath(bindingPath),
      valueMember: "value",
    },
    container: { kind: "none" },
  };
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

function rootPlan(
  planId: string,
  planParts: Partial<Pick<PlanDocument, "components" | "behaviors" | "types">> = {},
): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: planParts.types ?? {},
    components: planParts.components ?? {},
    behaviors: planParts.behaviors ?? [],
  };
}

function partialPlan(
  planId: string,
  planParts: Partial<Pick<PlanDocument, "components" | "behaviors" | "types">> = {},
): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
    types: planParts.types ?? { "native.element.address-line": objectContract() },
    components: planParts.components ?? { "address-line": component("address-line") },
    behaviors: planParts.behaviors ?? [],
  };
}

afterEach(() => {
  document.body.innerHTML = "";
  resetBootStateForTests();
});

describe("injectPartial partial slot plan", () => {
  it("loads and unloads the plan declared by the partial slot", () => {
    const slot = document.createElement("div");
    const slotId = "address-container";

    injectPartial(
      slot,
      `<script type="application/json" data-reactive-plan>${JSON.stringify(
        partialPlan("Resident.Root"),
      )}</script><input id="address-line" />`,
      slotId,
    );

    expect(getBootedPlan("Resident.Root")?.components["address-line"]).toBeDefined();
    expect(slot.querySelector("[data-reactive-plan]")).toBeNull();

    injectPartial(slot, "<p>No address selected</p>", slotId);

    expect(getBootedPlan("Resident.Root")).toBeUndefined();
    expect(slot.textContent).toContain("No address selected");
  });

  it("removes injected registered inputs from gather when the slot unloads", () => {
    document.body.innerHTML = `
      <input id="first-name" value="Ada" />
      <div id="address-container"></div>
    `;
    const planId = "Resident.GatherThroughInject";
    const slot = document.getElementById("address-container")!;
    const slotId = "address-container";

    boot(rootPlan(planId, {
      types: { "native.input": inputContract() },
      components: { "first-name": inputComponent("first-name", "firstName") },
    }));

    injectPartial(
      slot,
      `<script type="application/json" data-reactive-plan>${JSON.stringify(
        partialPlan(planId, {
          components: { "address-line": inputComponent("address-line", "addressLine") },
        }),
      )}</script><input id="address-line" value="12 Main" />`,
      slotId,
    );

    expect(resolveRequestInput(allRegisteredInputs(), "POST", getBootedPlan(planId)!, {}).body)
      .toEqual({ firstName: "Ada", addressLine: "12 Main" });

    injectPartial(slot, "<p>No address selected</p>", slotId);

    expect(resolveRequestInput(allRegisteredInputs(), "POST", getBootedPlan(planId)!, {}).body)
      .toEqual({ firstName: "Ada" });
  });
});
