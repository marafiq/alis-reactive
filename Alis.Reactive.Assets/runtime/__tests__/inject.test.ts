import { afterEach, describe, expect, it } from "vitest";
import { injectHtml } from "../execution/inject";
import { getBootedPlan, resetBootStateForTests } from "../lifecycle/boot";
import type { ComponentObject, InjectionTarget, BrowserObjectContract, PlanDocument } from "../types";

function objectContract(): BrowserObjectContract {
  return {
    properties: {},
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

function partialPlan(planId: string): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
    types: { "native.element.address-line": objectContract() },
    components: { "address-line": component("address-line") },
    behaviors: [],
  };
}

afterEach(() => {
  document.body.innerHTML = "";
  resetBootStateForTests();
});

describe("injectHtml partial slot lifecycle", () => {
  it("loads and unloads the partial slot declared by the injection target", () => {
    const slot = document.createElement("div");
    const target: InjectionTarget = {
      kind: "partial-slot",
      component: "address-container",
    };

    injectHtml(
      slot,
      `<script type="application/json" data-reactive-plan>${JSON.stringify(
        partialPlan("Resident.Root"),
      )}</script><input id="address-line" />`,
      target,
    );

    expect(getBootedPlan("Resident.Root")?.components["address-line"]).toBeDefined();
    expect(slot.querySelector("[data-reactive-plan]")).toBeNull();

    injectHtml(slot, "<p>No address selected</p>", target);

    expect(getBootedPlan("Resident.Root")).toBeUndefined();
    expect(slot.textContent).toContain("No address selected");
  });
});
