import { afterEach, describe, expect, it } from "vitest";
import { injectHtml } from "../execution/inject";
import { getBootedPlan, resetBootStateForTests } from "../lifecycle/boot";
import type { Component, InjectionTarget, JsType, Plan } from "../types";

function jsType(): JsType {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

function component(id: string, type = `native.element.${id}`): Component {
  return {
    id,
    vendor: "native",
    type,
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function partialPlan(planId: string, partId: string): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "partial", partId },
    types: { "native.element.address-line": jsType() },
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
        partialPlan("Resident.Root", "server-generated-id"),
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
