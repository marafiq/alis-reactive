import { afterEach, beforeAll, describe, expect, it } from "vitest";
import { boot, loadPartialSlot, resetBootStateForTests } from "../lifecycle/boot";
import type { BrowserObjectContract, ComponentObject, PlanDocument, ReactionGraph, Shape } from "../types/index";

const stringShape: Shape = { kind: "string" };
const planId = "Training.Calendar";

beforeAll(async () => {
  document.body.innerHTML = `
    <span id="status"></span>
    <div id="alis-drawer" class="alis-drawer--visible">
      <h2 id="alis-drawer-title">Details</h2>
      <button id="alis-drawer-close"></button>
      <div id="alis-drawer-content"></div>
    </div>`;
  await import("../components/native/drawer");
});

afterEach(() => {
  resetBootStateForTests();
  elementById("status").textContent = "";
  elementById("alis-drawer").classList.add("alis-drawer--visible");
});

describe("native drawer", () => {
  it("closing it releases the partial it showed, so the partial's reactions stop", () => {
    boot(rootPlan());
    loadPartialSlot("alis-drawer-content", [partialListeningFor("drawer:tick", "ran")]);
    document.dispatchEvent(new CustomEvent("drawer:tick"));
    expect(textFor("status")).toBe("ran");
    elementById("status").textContent = "";

    elementById("alis-drawer-close").click();
    elementById("alis-drawer").dispatchEvent(new Event("transitionend"));
    document.dispatchEvent(new CustomEvent("drawer:tick"));

    expect(textFor("status")).toBe("");
  });

  it("closing it with Escape releases the partial it showed", () => {
    boot(rootPlan());
    loadPartialSlot("alis-drawer-content", [partialListeningFor("drawer:tick", "ran")]);

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape" }));
    elementById("alis-drawer").dispatchEvent(new Event("transitionend"));
    document.dispatchEvent(new CustomEvent("drawer:tick"));

    expect(textFor("status")).toBe("");
  });

  it("a partial loaded as it opens again, before the close ends, keeps its reactions", () => {
    boot(rootPlan());
    loadPartialSlot("alis-drawer-content", [partialListeningFor("drawer:tick", "first")]);

    elementById("alis-drawer-close").click();
    loadPartialSlot("alis-drawer-content", [partialListeningFor("drawer:tick", "reopened")]);
    elementById("alis-drawer").classList.add("alis-drawer--visible");
    elementById("alis-drawer").dispatchEvent(new Event("transitionend"));
    document.dispatchEvent(new CustomEvent("drawer:tick"));

    expect(textFor("status")).toBe("reopened");
  });
});

function rootPlan(): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: { "native.input": nativeInputType() },
    components: { status: displayComponent("status") },
    behaviors: [],
  };
}

function partialListeningFor(event: string, text: string): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
    types: {},
    components: {},
    behaviors: [{ startsWhen: { kind: "document-event", event }, reaction: setText("status", text) }],
  };
}

function nativeInputType(): BrowserObjectContract {
  return {
    properties: {
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

function displayComponent(id: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.input",
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function setText(component: string, value: string): ReactionGraph {
  return {
    kind: "set",
    on: { kind: "component", component },
    property: "textContent",
    value: { kind: "literal", value, shape: stringShape },
  };
}

function textFor(id: string): string {
  return elementById(id).textContent ?? "";
}

function elementById(id: string): HTMLElement {
  const element = document.getElementById(id);
  if (element === null) throw new Error(`Expected DOM fixture "${id}" to exist`);
  return element;
}
