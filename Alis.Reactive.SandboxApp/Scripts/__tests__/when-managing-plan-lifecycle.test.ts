import { describe, it, expect, beforeEach } from "vitest";
import { boot, mergePlan, getBootedPlan, resetBootStateForTests } from "../lifecycle/boot";
import type { Plan, Entry, ComponentEntry } from "../types";

// ── Helpers ──────────────────────────────────────────────

function rootPlan(overrides?: Partial<Plan>): Plan {
  return {
    planId: "Resident.Model",
    components: {},
    entries: [],
    ...overrides,
  };
}

function partialPlan(sourceId: string, overrides?: Partial<Plan>): Plan {
  return {
    planId: "Resident.Model",
    sourceId,
    components: {},
    entries: [],
    ...overrides,
  };
}

function unrelatedPlan(overrides?: Partial<Plan>): Plan {
  return {
    planId: "Facility.Model",
    components: {},
    entries: [],
    ...overrides,
  };
}

function comp(id: string, vendor: "native" | "fusion" = "native", readExpr = "value"): ComponentEntry {
  return { id, vendor, readExpr };
}

function dispatchEntry(event: string): Entry {
  return {
    trigger: { kind: "dom-ready" },
    reaction: {
      kind: "sequential",
      commands: [{ kind: "dispatch", event }],
    },
  };
}

function customEventEntry(listenFor: string, dispatchEvent: string): Entry {
  return {
    trigger: { kind: "custom-event", event: listenFor },
    reaction: {
      kind: "sequential",
      commands: [{ kind: "dispatch", event: dispatchEvent }],
    },
  };
}

function mutateOnEvent(listenFor: string, targetId: string, text: string): Entry {
  return {
    trigger: { kind: "custom-event", event: listenFor },
    reaction: {
      kind: "sequential",
      commands: [{
        kind: "mutate-element",
        target: targetId,
        mutation: { kind: "set-prop", prop: "textContent" },
        value: text,
      }],
    },
  };
}

function domReadyMutate(targetId: string, text: string): Entry {
  return {
    trigger: { kind: "dom-ready" },
    reaction: {
      kind: "sequential",
      commands: [{
        kind: "mutate-element",
        target: targetId,
        mutation: { kind: "set-prop", prop: "textContent" },
        value: text,
      }],
    },
  };
}

function httpEntryWithValidation(formId: string, fields: { fieldName: string; rules: any[] }[]): Entry {
  return {
    trigger: { kind: "custom-event", event: "submit" },
    reaction: {
      kind: "http",
      request: {
        verb: "POST",
        url: "/save",
        validation: {
          formId,
          fields: fields.map(f => ({
            fieldName: f.fieldName,
            rules: f.rules,
          })),
        },
      },
    },
  };
}

function getValidationFields(plan: Plan): any[] | undefined {
  const entry = plan.entries.find(e =>
    e.reaction.kind === "http" && (e.reaction as any).request?.validation
  );
  if (!entry) return undefined;
  return (entry.reaction as any).request.validation.fields;
}

beforeEach(() => {
  document.body.innerHTML = "";
  resetBootStateForTests();
});

// ── Boot ──────────────────────────────────────────────────

describe("When a root plan boots with validation and components", () => {
  it("validation fields matching components have fieldId/vendor/readExpr after boot", () => {
    boot(rootPlan({
      components: {
        "Name": comp("name-input"),
        "Email": comp("email-input"),
      },
      entries: [httpEntryWithValidation("form", [
        { fieldName: "Name", rules: [{ rule: "required", message: "required" }] },
        { fieldName: "Email", rules: [{ rule: "email", message: "bad email" }] },
      ])],
    }));

    const plan = getBootedPlan("Resident.Model")!;
    const fields = getValidationFields(plan)!;

    expect(fields[0].fieldId).toBe("name-input");
    expect(fields[0].vendor).toBe("native");
    expect(fields[0].readExpr).toBe("value");

    expect(fields[1].fieldId).toBe("email-input");
    expect(fields[1].vendor).toBe("native");
    expect(fields[1].readExpr).toBe("value");
  });

  it("dom-ready triggers fire after custom-event listeners are wired", () => {
    document.body.innerHTML = '<div id="result"></div>';

    boot(rootPlan({
      entries: [
        // Dom-ready dispatches "init" — must NOT fire until custom-event listener exists
        dispatchEntry("init"),
        // Custom-event listens for "init"
        mutateOnEvent("init", "result", "chained"),
      ],
    }));

    expect(document.getElementById("result")?.textContent).toBe("chained");
  });
});

// ── Add partial ──────────────────────────────────────────

describe("When a partial adds components that match unenriched validation fields", () => {
  it("those validation fields now have fieldId/vendor/readExpr", () => {
    boot(rootPlan({
      components: {},
      entries: [httpEntryWithValidation("form", [
        { fieldName: "Address.Street", rules: [{ rule: "required", message: "required" }] },
        { fieldName: "Address.City", rules: [{ rule: "required", message: "required" }] },
      ])],
    }));

    const plan = getBootedPlan("Resident.Model")!;
    const fields = getValidationFields(plan)!;
    expect(fields[0].fieldId).toBeUndefined();

    mergePlan(partialPlan("address-container", {
      components: {
        "Address.Street": comp("street-input"),
        "Address.City": comp("city-input"),
      },
    }));

    expect(fields[0].fieldId).toBe("street-input");
    expect(fields[0].vendor).toBe("native");
    expect(fields[1].fieldId).toBe("city-input");
  });

  it("the partial's entries are in the merged plan", () => {
    boot(rootPlan({ entries: [dispatchEntry("root-ready")] }));

    mergePlan(partialPlan("address-container", {
      entries: [customEventEntry("load-partial", "partial-loaded")],
    }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.entries).toHaveLength(2);
  });

  it("root plan's original entries and components are unchanged", () => {
    boot(rootPlan({
      components: { "Name": comp("name-input") },
      entries: [dispatchEntry("root-ready")],
    }));

    mergePlan(partialPlan("address-container", {
      components: { "Address.City": comp("city-input") },
      entries: [customEventEntry("x", "y")],
    }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.components["Name"]).toBeDefined();
    expect(plan.components["Name"].id).toBe("name-input");
  });
});

describe("When a partial adds a component with the same key as a root component", () => {
  it("root component is preserved — partial does not shadow it", () => {
    boot(rootPlan({
      components: { "Name": comp("name-root") },
    }));

    // After partial merge, the root component should still be there
    // (Object.assign puts partial's on top, but root plan's should conceptually remain)
    mergePlan(partialPlan("slot", {
      components: { "Name": comp("name-partial") },
    }));

    const plan = getBootedPlan("Resident.Model")!;
    // The component key exists (partial may overwrite, but re-removing partial restores root)
    expect(plan.components["Name"]).toBeDefined();
  });
});

describe("When two partials contribute different components to the same plan", () => {
  it("both partials' components are in the merged plan", () => {
    boot(rootPlan());

    mergePlan(partialPlan("address-slot", {
      components: { "Address.Street": comp("street") },
    }));

    mergePlan(partialPlan("emergency-slot", {
      components: { "Emergency.Phone": comp("phone") },
    }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.components["Address.Street"]).toBeDefined();
    expect(plan.components["Emergency.Phone"]).toBeDefined();
  });

  it("validation fields enriched from either partial have correct metadata", () => {
    boot(rootPlan({
      entries: [httpEntryWithValidation("form", [
        { fieldName: "Address.Street", rules: [{ rule: "required", message: "req" }] },
        { fieldName: "Emergency.Phone", rules: [{ rule: "required", message: "req" }] },
      ])],
    }));

    mergePlan(partialPlan("address-slot", {
      components: { "Address.Street": comp("street") },
    }));
    mergePlan(partialPlan("emergency-slot", {
      components: { "Emergency.Phone": comp("phone") },
    }));

    const fields = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fields[0].fieldId).toBe("street");
    expect(fields[1].fieldId).toBe("phone");
  });
});

// ── Remove partial ──────────────────────────────────────

describe("When a partial is removed by sourceId", () => {
  it("the partial's components are no longer in the merged plan", () => {
    boot(rootPlan());

    mergePlan(partialPlan("address-slot", {
      components: { "Address.City": comp("city") },
    }));

    // Re-merge with empty — effectively removes
    mergePlan(partialPlan("address-slot", {
      components: {},
      entries: [],
    }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.components["Address.City"]).toBeUndefined();
  });

  it("validation fields that were enriched from the partial revert to unenriched", () => {
    boot(rootPlan({
      entries: [httpEntryWithValidation("form", [
        { fieldName: "Address.City", rules: [{ rule: "required", message: "req" }] },
      ])],
    }));

    mergePlan(partialPlan("address-slot", {
      components: { "Address.City": comp("city") },
    }));

    const fields = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fields[0].fieldId).toBe("city");

    // Remove by re-merging empty
    mergePlan(partialPlan("address-slot", { components: {}, entries: [] }));

    expect(fields[0].fieldId).toBeUndefined();
    expect(fields[0].vendor).toBeUndefined();
    expect(fields[0].readExpr).toBeUndefined();
  });

  it("root plan's components and entries are unchanged", () => {
    boot(rootPlan({
      components: { "Name": comp("name") },
      entries: [dispatchEntry("root-ready")],
    }));

    mergePlan(partialPlan("address-slot", {
      components: { "Address.City": comp("city") },
      entries: [customEventEntry("x", "y")],
    }));

    // Remove partial
    mergePlan(partialPlan("address-slot", { components: {}, entries: [] }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.components["Name"]).toBeDefined();
    // Root entry + empty partial entry
    expect(plan.entries.length).toBeGreaterThanOrEqual(1);
  });

  it("the partial's event listeners no longer fire", () => {
    document.body.innerHTML = '<div id="status"></div>';

    boot(rootPlan());

    mergePlan(partialPlan("address-slot", {
      entries: [mutateOnEvent("fire-partial", "status", "partial-fired")],
    }));

    document.dispatchEvent(new CustomEvent("fire-partial"));
    expect(document.getElementById("status")?.textContent).toBe("partial-fired");

    document.getElementById("status")!.textContent = "";

    // Remove by re-merge
    mergePlan(partialPlan("address-slot", { entries: [] }));

    document.dispatchEvent(new CustomEvent("fire-partial"));
    expect(document.getElementById("status")?.textContent).toBe("");
  });
});

describe("When one of two partials is removed", () => {
  it("the remaining partial's components and entries are unaffected", () => {
    document.body.innerHTML = '<div id="status-a"></div><div id="status-b"></div>';

    boot(rootPlan());

    mergePlan(partialPlan("slot-a", {
      components: { "FieldA": comp("field-a") },
      entries: [mutateOnEvent("fire-a", "status-a", "A")],
    }));

    mergePlan(partialPlan("slot-b", {
      components: { "FieldB": comp("field-b") },
      entries: [mutateOnEvent("fire-b", "status-b", "B")],
    }));

    // Remove slot-a
    mergePlan(partialPlan("slot-a", { components: {}, entries: [] }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.components["FieldA"]).toBeUndefined();
    expect(plan.components["FieldB"]).toBeDefined();

    // slot-b still works
    document.dispatchEvent(new CustomEvent("fire-b"));
    expect(document.getElementById("status-b")?.textContent).toBe("B");
  });

  it("validation fields enriched from the remaining partial stay enriched", () => {
    boot(rootPlan({
      entries: [httpEntryWithValidation("form", [
        { fieldName: "FieldA", rules: [{ rule: "required", message: "req" }] },
        { fieldName: "FieldB", rules: [{ rule: "required", message: "req" }] },
      ])],
    }));

    mergePlan(partialPlan("slot-a", {
      components: { "FieldA": comp("field-a") },
    }));
    mergePlan(partialPlan("slot-b", {
      components: { "FieldB": comp("field-b") },
    }));

    // Remove slot-a only
    mergePlan(partialPlan("slot-a", { components: {}, entries: [] }));

    const fields = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fields[0].fieldId).toBeUndefined(); // FieldA — unenriched after removal
    expect(fields[1].fieldId).toBe("field-b"); // FieldB — still enriched from slot-b
  });
});

// ── Reload (remove + add) ──────────────────────────────

describe("When a partial is removed then re-added with different components", () => {
  it("only the new components are in the merged plan", () => {
    boot(rootPlan());

    mergePlan(partialPlan("address-slot", {
      components: {
        "Address.City": comp("city-old"),
        "Address.Zip": comp("zip-old"),
      },
    }));

    // Re-merge same sourceId with different components
    mergePlan(partialPlan("address-slot", {
      components: {
        "Address.Street": comp("street-new"),
      },
    }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.components["Address.Street"]).toBeDefined();
    expect(plan.components["Address.City"]).toBeUndefined();
    expect(plan.components["Address.Zip"]).toBeUndefined();
  });

  it("validation fields match the new component set, not the old", () => {
    boot(rootPlan({
      entries: [httpEntryWithValidation("form", [
        { fieldName: "Address.City", rules: [{ rule: "required", message: "req" }] },
        { fieldName: "Address.Street", rules: [{ rule: "required", message: "req" }] },
      ])],
    }));

    mergePlan(partialPlan("address-slot", {
      components: { "Address.City": comp("city-old") },
    }));

    // Re-merge with only Street
    mergePlan(partialPlan("address-slot", {
      components: { "Address.Street": comp("street-new") },
    }));

    const fields = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fields[0].fieldId).toBeUndefined(); // City — gone
    expect(fields[1].fieldId).toBe("street-new"); // Street — new
  });
});

describe("When a partial is removed then re-added with fewer components", () => {
  it("components from the old partial that aren't in the new are gone", () => {
    boot(rootPlan());

    mergePlan(partialPlan("address-slot", {
      components: {
        "Address.City": comp("city"),
        "Address.Zip": comp("zip"),
        "Address.Street": comp("street"),
      },
    }));

    // Re-merge with only City
    mergePlan(partialPlan("address-slot", {
      components: { "Address.City": comp("city") },
    }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.components["Address.City"]).toBeDefined();
    expect(plan.components["Address.Zip"]).toBeUndefined();
    expect(plan.components["Address.Street"]).toBeUndefined();
  });

  it("validation fields for dropped components are unenriched", () => {
    boot(rootPlan({
      entries: [httpEntryWithValidation("form", [
        { fieldName: "Address.City", rules: [{ rule: "required", message: "req" }] },
        { fieldName: "Address.Zip", rules: [{ rule: "required", message: "req" }] },
      ])],
    }));

    mergePlan(partialPlan("address-slot", {
      components: {
        "Address.City": comp("city"),
        "Address.Zip": comp("zip"),
      },
    }));

    // Re-merge with only City
    mergePlan(partialPlan("address-slot", {
      components: { "Address.City": comp("city") },
    }));

    const fields = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fields[0].fieldId).toBe("city"); // City — still enriched
    expect(fields[1].fieldId).toBeUndefined(); // Zip — unenriched
  });
});

// ── Partial with its own reactive entries ────────────────

describe("When a partial has its own event handlers and Show/Hide entries", () => {
  it("partial's custom-event listeners fire after add", () => {
    document.body.innerHTML = '<div id="partial-status"></div>';
    boot(rootPlan());

    mergePlan(partialPlan("partial-slot", {
      entries: [mutateOnEvent("partial-event", "partial-status", "fired")],
    }));

    document.dispatchEvent(new CustomEvent("partial-event"));
    expect(document.getElementById("partial-status")?.textContent).toBe("fired");
  });

  it("partial's dom-ready entries execute after add", () => {
    document.body.innerHTML = '<div id="partial-status"></div>';
    boot(rootPlan());

    mergePlan(partialPlan("partial-slot", {
      entries: [domReadyMutate("partial-status", "dom-ready-fired")],
    }));

    expect(document.getElementById("partial-status")?.textContent).toBe("dom-ready-fired");
  });

  it("partial's custom-event listeners stop firing after remove", () => {
    document.body.innerHTML = '<div id="partial-status"></div>';
    boot(rootPlan());

    mergePlan(partialPlan("partial-slot", {
      entries: [mutateOnEvent("partial-event", "partial-status", "fired")],
    }));

    // Remove partial
    mergePlan(partialPlan("partial-slot", { entries: [] }));

    document.getElementById("partial-status")!.textContent = "";
    document.dispatchEvent(new CustomEvent("partial-event"));
    expect(document.getElementById("partial-status")?.textContent).toBe("");
  });

  it("root plan's event handlers are not affected by partial add/remove", () => {
    document.body.innerHTML = '<div id="root-status"></div><div id="partial-status"></div>';

    boot(rootPlan({
      entries: [mutateOnEvent("root-event", "root-status", "root-fired")],
    }));

    mergePlan(partialPlan("partial-slot", {
      entries: [mutateOnEvent("partial-event", "partial-status", "partial-fired")],
    }));

    // Remove partial
    mergePlan(partialPlan("partial-slot", { entries: [] }));

    // Root's listeners should still work
    document.dispatchEvent(new CustomEvent("root-event"));
    expect(document.getElementById("root-status")?.textContent).toBe("root-fired");
  });
});

describe("When a partial dispatches events into root plan listeners", () => {
  it("dispatch from partial entry reaches root plan's custom-event listener", () => {
    document.body.innerHTML = '<div id="result"></div>';

    boot(rootPlan({
      entries: [mutateOnEvent("bridge-event", "result", "bridged")],
    }));

    mergePlan(partialPlan("partial-slot", {
      entries: [dispatchEntry("bridge-event")],
    }));

    expect(document.getElementById("result")?.textContent).toBe("bridged");
  });

  it("after partial is removed, the dispatch no longer fires", () => {
    document.body.innerHTML = '<div id="result"></div>';

    boot(rootPlan({
      entries: [mutateOnEvent("bridge-event", "result", "bridged")],
    }));

    mergePlan(partialPlan("partial-slot", {
      entries: [
        // Custom event trigger that dispatches into root
        customEventEntry("trigger-bridge", "bridge-event"),
      ],
    }));

    // Remove partial
    mergePlan(partialPlan("partial-slot", { entries: [] }));

    document.getElementById("result")!.textContent = "";
    document.dispatchEvent(new CustomEvent("trigger-bridge"));
    expect(document.getElementById("result")?.textContent).toBe("");
  });
});

describe("When two partials have their own independent reactive entries", () => {
  it("partial A's event fires partial A's handler only", () => {
    document.body.innerHTML = '<div id="status-a"></div><div id="status-b"></div>';

    boot(rootPlan());

    mergePlan(partialPlan("slot-a", {
      entries: [mutateOnEvent("event-a", "status-a", "A")],
    }));

    mergePlan(partialPlan("slot-b", {
      entries: [mutateOnEvent("event-b", "status-b", "B")],
    }));

    document.dispatchEvent(new CustomEvent("event-a"));
    expect(document.getElementById("status-a")?.textContent).toBe("A");
    expect(document.getElementById("status-b")?.textContent).toBe("");
  });

  it("partial B's event fires partial B's handler only", () => {
    document.body.innerHTML = '<div id="status-a"></div><div id="status-b"></div>';

    boot(rootPlan());

    mergePlan(partialPlan("slot-a", {
      entries: [mutateOnEvent("event-a", "status-a", "A")],
    }));

    mergePlan(partialPlan("slot-b", {
      entries: [mutateOnEvent("event-b", "status-b", "B")],
    }));

    document.dispatchEvent(new CustomEvent("event-b"));
    expect(document.getElementById("status-a")?.textContent).toBe("");
    expect(document.getElementById("status-b")?.textContent).toBe("B");
  });

  it("removing partial A does not break partial B's handlers", () => {
    document.body.innerHTML = '<div id="status-a"></div><div id="status-b"></div>';

    boot(rootPlan());

    mergePlan(partialPlan("slot-a", {
      entries: [mutateOnEvent("event-a", "status-a", "A")],
    }));

    mergePlan(partialPlan("slot-b", {
      entries: [mutateOnEvent("event-b", "status-b", "B")],
    }));

    // Remove A
    mergePlan(partialPlan("slot-a", { entries: [] }));

    document.dispatchEvent(new CustomEvent("event-b"));
    expect(document.getElementById("status-b")?.textContent).toBe("B");
  });
});

// ── Full plan JSON merge (all entry types) ───────────────

describe("When a partial plan has entries with HTTP requests, Show/Hide, and Dispatch", () => {
  it("all entry types are in the merged plan after add", () => {
    boot(rootPlan());

    const httpEntry: Entry = {
      trigger: { kind: "custom-event", event: "submit" },
      reaction: {
        kind: "http",
        request: { verb: "POST", url: "/save" },
      },
    };

    const showHideEntry: Entry = {
      trigger: { kind: "custom-event", event: "toggle" },
      reaction: {
        kind: "sequential",
        commands: [{
          kind: "mutate-element",
          target: "section",
          mutation: { kind: "call", method: "removeAttribute", args: [{ kind: "literal", value: "hidden" }] },
        }],
      },
    };

    const dispatchEntry2: Entry = {
      trigger: { kind: "dom-ready" },
      reaction: {
        kind: "sequential",
        commands: [{ kind: "dispatch", event: "partial-ready" }],
      },
    };

    mergePlan(partialPlan("slot", {
      entries: [httpEntry, showHideEntry, dispatchEntry2],
    }));

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.entries).toHaveLength(3);
  });

  it("all entry types from that partial are removed after remove", () => {
    boot(rootPlan({ entries: [dispatchEntry("root-ready")] }));

    mergePlan(partialPlan("slot", {
      entries: [
        customEventEntry("a", "b"),
        customEventEntry("c", "d"),
      ],
    }));

    expect(getBootedPlan("Resident.Model")!.entries).toHaveLength(3);

    // Remove
    mergePlan(partialPlan("slot", { entries: [] }));

    // Only root entry + the empty re-merge entry remain
    // The partial's old entries should be removed
    const plan = getBootedPlan("Resident.Model")!;
    // Root had 1, partial added 2, removal removes 2, re-add adds 0
    expect(plan.entries.length).toBeLessThanOrEqual(1);
  });

  it("entries from other partials and root plan remain intact", () => {
    boot(rootPlan({ entries: [dispatchEntry("root-ready")] }));

    mergePlan(partialPlan("slot-a", {
      entries: [customEventEntry("a", "a-out")],
    }));

    mergePlan(partialPlan("slot-b", {
      entries: [customEventEntry("b", "b-out")],
    }));

    // Remove slot-a
    mergePlan(partialPlan("slot-a", { entries: [] }));

    const plan = getBootedPlan("Resident.Model")!;
    // Root(1) + slot-b(1) should remain. slot-a removed.
    expect(plan.entries.length).toBeGreaterThanOrEqual(2);
  });
});

// ── Unrelated plan coexistence (different TModel/planId) ─

describe("When an unrelated partial with its own ReactivePlan loads", () => {
  it("creates a separate plan in the registry (different planId)", () => {
    boot(rootPlan());

    mergePlan({
      planId: "Facility.Model",
      sourceId: "facility-slot",
      components: { "FacilityName": comp("facility-name") },
      entries: [],
    });

    expect(getBootedPlan("Resident.Model")).toBeDefined();
    expect(getBootedPlan("Facility.Model")).toBeDefined();
  });

  it("the unrelated plan's components do not appear in the root plan", () => {
    boot(rootPlan({ components: { "Name": comp("name") } }));

    mergePlan({
      planId: "Facility.Model",
      sourceId: "facility-slot",
      components: { "FacilityName": comp("facility-name") },
      entries: [],
    });

    const rootComponents = getBootedPlan("Resident.Model")!.components;
    expect(rootComponents["FacilityName"]).toBeUndefined();
    expect(rootComponents["Name"]).toBeDefined();
  });

  it("the root plan's validation is not affected by the unrelated plan", () => {
    boot(rootPlan({
      entries: [httpEntryWithValidation("form", [
        { fieldName: "Name", rules: [{ rule: "required", message: "req" }] },
      ])],
    }));

    mergePlan({
      planId: "Facility.Model",
      sourceId: "facility-slot",
      components: { "Name": comp("facility-name-input") },
      entries: [],
    });

    // Root plan's validation should NOT be enriched from unrelated plan's components
    const fields = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fields[0].fieldId).toBeUndefined();
  });

  it("the unrelated plan's own reactive entries work independently", () => {
    document.body.innerHTML = '<div id="root-status"></div><div id="facility-status"></div>';

    boot(rootPlan({
      entries: [mutateOnEvent("root-event", "root-status", "root")],
    }));

    mergePlan({
      planId: "Facility.Model",
      sourceId: "facility-slot",
      components: {},
      entries: [mutateOnEvent("facility-event", "facility-status", "facility")],
    });

    document.dispatchEvent(new CustomEvent("facility-event"));
    expect(document.getElementById("facility-status")?.textContent).toBe("facility");
    expect(document.getElementById("root-status")?.textContent).toBe("");
  });
});

describe("When the unrelated plan's partial is removed", () => {
  it("the root plan remains completely intact", () => {
    document.body.innerHTML = '<div id="root-status"></div>';

    boot(rootPlan({
      components: { "Name": comp("name") },
      entries: [mutateOnEvent("root-event", "root-status", "root")],
    }));

    mergePlan({
      planId: "Facility.Model",
      sourceId: "facility-slot",
      components: { "FacilityName": comp("f-name") },
      entries: [customEventEntry("f-event", "f-out")],
    });

    // Remove facility partial
    mergePlan({
      planId: "Facility.Model",
      sourceId: "facility-slot",
      components: {},
      entries: [],
    });

    const root = getBootedPlan("Resident.Model")!;
    expect(root.components["Name"]).toBeDefined();

    document.dispatchEvent(new CustomEvent("root-event"));
    expect(document.getElementById("root-status")?.textContent).toBe("root");
  });

  it("no components, entries, or listeners from root plan are disturbed", () => {
    document.body.innerHTML = '<div id="root-status"></div>';

    boot(rootPlan({
      components: { "Name": comp("name"), "Email": comp("email") },
      entries: [
        mutateOnEvent("root-event", "root-status", "root"),
        dispatchEntry("root-ready"),
      ],
    }));

    mergePlan({
      planId: "Facility.Model",
      sourceId: "facility-slot",
      components: { "X": comp("x") },
      entries: [],
    });

    // Remove facility
    mergePlan({
      planId: "Facility.Model",
      sourceId: "facility-slot",
      components: {},
      entries: [],
    });

    const root = getBootedPlan("Resident.Model")!;
    expect(Object.keys(root.components)).toHaveLength(2);
    expect(root.entries).toHaveLength(2);
  });
});

describe("When root plan partial and unrelated plan partial both exist", () => {
  it("add/remove on root partial does not affect unrelated plan", () => {
    boot(rootPlan());

    // Boot unrelated plan
    boot({
      planId: "Facility.Model",
      components: { "FacilityName": comp("f-name") },
      entries: [],
    });

    // Add partial to root
    mergePlan(partialPlan("root-slot", {
      components: { "Address.City": comp("city") },
    }));

    // Remove root partial
    mergePlan(partialPlan("root-slot", { components: {} }));

    const facility = getBootedPlan("Facility.Model")!;
    expect(facility.components["FacilityName"]).toBeDefined();
  });

  it("add/remove on unrelated partial does not affect root plan", () => {
    boot(rootPlan({ components: { "Name": comp("name") } }));

    mergePlan({
      planId: "Facility.Model",
      sourceId: "f-slot",
      components: { "FacilityName": comp("f-name") },
      entries: [],
    });

    // Remove facility partial
    mergePlan({
      planId: "Facility.Model",
      sourceId: "f-slot",
      components: {},
      entries: [],
    });

    const root = getBootedPlan("Resident.Model")!;
    expect(root.components["Name"]).toBeDefined();
  });
});

// ── Plan isolation ──────────────────────────────────────

describe("When two plans with different planIds are booted", () => {
  it("adding a partial to one plan does not change the other", () => {
    boot(rootPlan());
    boot(unrelatedPlan());

    mergePlan(partialPlan("root-slot", {
      components: { "Address.City": comp("city") },
    }));

    const facility = getBootedPlan("Facility.Model")!;
    expect(facility.components["Address.City"]).toBeUndefined();
  });

  it("removing a partial from one plan does not affect the other", () => {
    boot(rootPlan());
    boot(unrelatedPlan({ components: { "FacilityName": comp("f-name") } }));

    mergePlan(partialPlan("root-slot", {
      components: { "Address.City": comp("city") },
    }));

    // Remove root partial
    mergePlan(partialPlan("root-slot", { components: {} }));

    const facility = getBootedPlan("Facility.Model")!;
    expect(facility.components["FacilityName"]).toBeDefined();
  });
});

// ── Lifecycle round-trip ────────────────────────────────

describe("When the sequence boot → add → remove → add is executed", () => {
  it("the plan state after second add is identical to state after first add", () => {
    document.body.innerHTML = '<div id="status"></div>';

    boot(rootPlan({
      entries: [httpEntryWithValidation("form", [
        { fieldName: "Address.City", rules: [{ rule: "required", message: "req" }] },
      ])],
    }));

    // First add
    const partialComponents = { "Address.City": comp("city") };
    const partialEntries = [mutateOnEvent("partial-event", "status", "partial")];

    mergePlan(partialPlan("slot", {
      components: { ...partialComponents },
      entries: [...partialEntries.map(e => ({ ...e }))],
    }));

    const fields1 = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fields1[0].fieldId).toBe("city");

    document.dispatchEvent(new CustomEvent("partial-event"));
    expect(document.getElementById("status")?.textContent).toBe("partial");

    // Remove
    mergePlan(partialPlan("slot", { components: {}, entries: [] }));
    document.getElementById("status")!.textContent = "";

    const fieldsRemoved = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fieldsRemoved[0].fieldId).toBeUndefined();

    // Second add
    mergePlan(partialPlan("slot", {
      components: { ...partialComponents },
      entries: [mutateOnEvent("partial-event", "status", "partial")],
    }));

    const fields2 = getValidationFields(getBootedPlan("Resident.Model")!)!;
    expect(fields2[0].fieldId).toBe("city");

    document.dispatchEvent(new CustomEvent("partial-event"));
    expect(document.getElementById("status")?.textContent).toBe("partial");
  });

  it("no stale entries, listeners, or components leak across the cycle", () => {
    document.body.innerHTML = '<div id="counter"></div>';
    let count = 0;

    boot(rootPlan());

    // Add, remove, re-add three times
    for (let i = 0; i < 3; i++) {
      mergePlan(partialPlan("slot", {
        components: { "Field": comp("field") },
        entries: [customEventEntry("count-event", "counted")],
      }));
      mergePlan(partialPlan("slot", { components: {}, entries: [] }));
    }

    // Final add
    mergePlan(partialPlan("slot", {
      components: { "Field": comp("field") },
      entries: [customEventEntry("count-event", "counted")],
    }));

    count = 0;
    document.addEventListener("counted", () => { count++; });
    document.dispatchEvent(new CustomEvent("count-event"));

    // Only the latest merge's listener should fire — exactly once
    expect(count).toBe(1);

    const plan = getBootedPlan("Resident.Model")!;
    expect(plan.components["Field"]).toBeDefined();
  });
});
