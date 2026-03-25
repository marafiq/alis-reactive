import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";


let boot: typeof import("../lifecycle/boot").boot;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <span id="r1">—</span>
    <span id="r2">—</span>
    <span id="r3">—</span>
    <span id="r4">—</span>
    <input id="Name" value="Margaret" />
    <input id="Nested_City" value="Portland" />
  </body></html>`);
  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Event = dom.window.Event;
  const mod = await import("../lifecycle/boot");
  boot = mod.boot;
});

describe("path resolution end-to-end", () => {

  // ── Flat event paths ──

  it("resolves flat string from event payload", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.name" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { name: "Eleanor" } }));
    expect(document.getElementById("r1")!.textContent).toBe("Eleanor");
  });

  it("resolves flat number from event payload", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.age" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { age: 82 } }));
    expect(document.getElementById("r1")!.textContent).toBe("82");
  });

  it("resolves flat boolean from event payload", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.active" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { active: true } }));
    expect(document.getElementById("r1")!.textContent).toBe("true");
  });

  // ── Nested event paths ──

  it("resolves two-level nested path", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.address.city" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { address: { city: "Portland" } } }));
    expect(document.getElementById("r1")!.textContent).toBe("Portland");
  });

  it("resolves three-level nested path", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.resident.address.zip" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { resident: { address: { zip: "97201" } } } }));
    expect(document.getElementById("r1")!.textContent).toBe("97201");
  });

  // ── Missing paths ──

  it("missing path resolves to empty string", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.nonexistent" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: {} }));
    expect(document.getElementById("r1")!.textContent).toBe("");
  });

  it("missing nested path resolves to empty string", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.address.city" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { address: null } }));
    expect(document.getElementById("r1")!.textContent).toBe("");
  });

  // ── Multiple fields from same payload ──

  it("resolves multiple fields from one event", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.first" } },
        { kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.last" } },
        { kind: "mutate-element", target: "r3", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.age" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { first: "Margaret", last: "Thompson", age: 82 } }));
    expect(document.getElementById("r1")!.textContent).toBe("Margaret");
    expect(document.getElementById("r2")!.textContent).toBe("Thompson");
    expect(document.getElementById("r3")!.textContent).toBe("82");
  });

  // ── Component source paths ──

  it("component source reads native input value", () => {
    boot({ planId: "t",
      components: { "Name": { id: "Name", vendor: "native", readExpr: "value", componentType: "textbox", coerceAs: "string" } },
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "component", componentId: "Name", vendor: "native", readExpr: "value" } },
        ]},
      }],
    });
    expect(document.getElementById("r1")!.textContent).toBe("Margaret");
  });

  // ── Source in conditions ──

  it("event source path used in guard evaluates correctly", async () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.address.city" }, coerceAs: "string", op: "eq", operand: "Portland" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "PDX" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { address: { city: "Portland" } } }));
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("r1")!.textContent).toBe("PDX");
  });

  // ── Coercion in path resolution ──

  it("number coercion converts string payload to number for comparison", async () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.score" }, coerceAs: "number", op: "gt", operand: 50 },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "high" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "low" }] } },
      ]},
    }]});
    // Score is a string "75" — coercion should convert to number
    document.dispatchEvent(new CustomEvent("test", { detail: { score: "75" } }));
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("r1")!.textContent).toBe("high");
  });

  it("boolean coercion converts string false to false", async () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.active" }, coerceAs: "boolean", op: "truthy" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "active" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "inactive" }] } },
      ]},
    }]});
    // String "false" should coerce to false in boolean context
    document.dispatchEvent(new CustomEvent("test", { detail: { active: "false" } }));
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("r1")!.textContent).toBe("inactive");
  });

  it("array coercion wraps scalar into array", async () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.tag" }, coerceAs: "array", op: "not-empty" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "has tags" }] } },
      ]},
    }]});
    // Single string should be wrapped into ["hello"] by array coercion
    document.dispatchEvent(new CustomEvent("test", { detail: { tag: "hello" } }));
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("r1")!.textContent).toBe("has tags");
  });

  it("null coerces to empty array", async () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.items" }, coerceAs: "array", op: "is-empty" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "empty" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { items: null } }));
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("r1")!.textContent).toBe("empty");
  });
});
