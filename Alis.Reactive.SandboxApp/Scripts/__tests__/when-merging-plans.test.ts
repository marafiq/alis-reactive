import { describe, it, expect, beforeEach } from "vitest";
import { boot, mergePlan, getBootedPlan } from "../lifecycle/boot";
import type { Plan } from "../types";

describe("when merging plans", () => {
  beforeEach(() => {
    document.body.innerHTML = "";
  });

  function parentPlan(): Plan {
    return {
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "parent-ready" }],
          },
        },
      ],
    };
  }

  function partialPlan(): Plan {
    return {
      planId: "Test.Model",
      sourceId: "address-container",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "load-partial" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "partial-loaded" }],
          },
        },
      ],
    };
  }

  it("does not accumulate entries on partial reload", () => {
    boot(parentPlan());
    mergePlan(partialPlan());
    mergePlan(partialPlan());
    mergePlan(partialPlan());

    let fireCount = 0;
    document.addEventListener("partial-loaded", () => { fireCount++; });
    document.dispatchEvent(new CustomEvent("load-partial"));

    // Only the latest merge's listener should survive — fires once, not 3 times
    expect(fireCount).toBe(1);
  });

  it("removes stale component registrations on partial re-merge", () => {
    boot(parentPlan());

    // First merge: partial registers City and Zip
    mergePlan({
      planId: "Test.Model",
      sourceId: "address-container",
      components: {
        "Address.City": { id: "city-input", vendor: "native", readExpr: "value" },
        "Address.Zip": { id: "zip-input", vendor: "native", readExpr: "value" },
      },
      entries: [],
    });

    const plan1 = getBootedPlan("Test.Model")!;
    expect(plan1.components["Address.City"]).toBeDefined();
    expect(plan1.components["Address.Zip"]).toBeDefined();

    // Re-merge same sourceId: only City — Zip was removed from partial
    mergePlan({
      planId: "Test.Model",
      sourceId: "address-container",
      components: {
        "Address.City": { id: "city-input", vendor: "native", readExpr: "value" },
      },
      entries: [],
    });

    const plan2 = getBootedPlan("Test.Model")!;
    expect(plan2.components["Address.City"]).toBeDefined();
    // Stale Zip registration must be gone
    expect(plan2.components["Address.Zip"]).toBeUndefined();
  });

  it("removes all component registrations when partial re-merges with empty components", () => {
    boot(parentPlan());

    mergePlan({
      planId: "Test.Model",
      sourceId: "address-container",
      components: {
        "Address.City": { id: "city-input", vendor: "native", readExpr: "value" },
      },
      entries: [],
    });

    // Re-merge same sourceId with no components at all
    mergePlan({
      planId: "Test.Model",
      sourceId: "address-container",
      components: {},
      entries: [],
    });

    const plan = getBootedPlan("Test.Model")!;
    expect(plan.components["Address.City"]).toBeUndefined();
  });

  it("does not remove parent components when partial re-merges", () => {
    // Parent owns its own component
    boot({
      planId: "Test.Model",
      components: {
        "Name": { id: "name-input", vendor: "native", readExpr: "value" },
      },
      entries: [],
    });

    // Partial adds a component
    mergePlan({
      planId: "Test.Model",
      sourceId: "address-container",
      components: {
        "Address.City": { id: "city-input", vendor: "native", readExpr: "value" },
      },
      entries: [],
    });

    // Re-merge partial with empty components
    mergePlan({
      planId: "Test.Model",
      sourceId: "address-container",
      components: {},
      entries: [],
    });

    const plan = getBootedPlan("Test.Model")!;
    // Parent's component must survive
    expect(plan.components["Name"]).toBeDefined();
    // Partial's component must be gone
    expect(plan.components["Address.City"]).toBeUndefined();
  });

  it("does not duplicate custom-event listeners on reload", () => {
    boot(parentPlan());

    // Merge the same partial twice
    mergePlan(partialPlan());
    mergePlan(partialPlan());

    let fireCount = 0;
    document.addEventListener("partial-loaded", () => { fireCount++; });
    document.dispatchEvent(new CustomEvent("load-partial"));

    // Should fire once, not twice
    expect(fireCount).toBe(1);
  });

  it("removes source-owned listeners and components when the same sourceId moves to a different planId", () => {
    document.body.innerHTML = '<div id="status-a"></div><div id="status-b"></div>';

    mergePlan({
      planId: "Plan.A",
      sourceId: "shared-slot",
      components: {
        "Root.Name": { id: "name-a", vendor: "native", readExpr: "value" },
      },
      entries: [{
        trigger: { kind: "custom-event", event: "fire-a" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "status-a",
            mutation: { kind: "set-prop", prop: "textContent" },
            value: "A",
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("fire-a"));
    expect(document.getElementById("status-a")?.textContent).toBe("A");

    document.getElementById("status-a")!.textContent = "";

    mergePlan({
      planId: "Plan.B",
      sourceId: "shared-slot",
      components: {
        "Root.Email": { id: "email-b", vendor: "native", readExpr: "value" },
      },
      entries: [{
        trigger: { kind: "custom-event", event: "fire-b" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "status-b",
            mutation: { kind: "set-prop", prop: "textContent" },
            value: "B",
          }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("fire-a"));
    document.dispatchEvent(new CustomEvent("fire-b"));

    expect(document.getElementById("status-a")?.textContent).toBe("");
    expect(document.getElementById("status-b")?.textContent).toBe("B");
    expect(getBootedPlan("Plan.A")).toBeUndefined();
    expect(getBootedPlan("Plan.B")?.components["Root.Email"]).toBeDefined();
  });

  it("clears stale validation bindings when a merged source removes its component", () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "submit" },
        reaction: {
          kind: "http",
          request: {
            verb: "POST",
            url: "/save",
            validation: {
              formId: "test-form",
              fields: [{
                fieldName: "Address.City",
                rules: [{ rule: "required", message: "City is required." }],
              }],
            },
          },
        },
      }],
    });

    const plan = getBootedPlan("Test.Model")!;
    const reaction = plan.entries[0].reaction;
    if (reaction.kind !== "http" || !reaction.request.validation) {
      throw new Error("Expected http reaction with validation.");
    }

    const field = reaction.request.validation.fields[0];
    expect(field.fieldId).toBeUndefined();
    expect(field.vendor).toBeUndefined();
    expect(field.readExpr).toBeUndefined();

    mergePlan({
      planId: "Test.Model",
      sourceId: "address-slot",
      components: {
        "Address.City": { id: "city-input", vendor: "native", readExpr: "value" },
      },
      entries: [],
    });

    expect(field.fieldId).toBe("city-input");
    expect(field.vendor).toBe("native");
    expect(field.readExpr).toBe("value");

    mergePlan({
      planId: "Test.Model",
      sourceId: "address-slot",
      components: {},
      entries: [],
    });

    expect(field.fieldId).toBeUndefined();
    expect(field.vendor).toBeUndefined();
    expect(field.readExpr).toBeUndefined();
  });
});
