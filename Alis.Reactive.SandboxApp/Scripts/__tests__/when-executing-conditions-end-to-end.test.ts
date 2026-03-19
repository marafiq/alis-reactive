import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan } from "../types";

let boot: typeof import("../boot").boot;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <span id="r1">—</span>
    <span id="r2">—</span>
    <span id="r3">—</span>
    <span id="grade">—</span>
    <span id="echo">—</span>
    <div id="panel" hidden></div>
    <div id="badge" hidden></div>
  </body></html>`);
  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Event = dom.window.Event;
  const mod = await import("../boot");
  boot = mod.boot;
});

describe("conditions end-to-end via boot", () => {

  // ── Comparison operators ──

  it("eq matches exact string value", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.status" }, coerceAs: "string", op: "eq", operand: "active" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "yes" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "no" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { status: "active" } }));
    expect(document.getElementById("r1")!.textContent).toBe("yes");
  });

  it("eq falls to else when no match", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.status" }, coerceAs: "string", op: "eq", operand: "active" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "yes" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "no" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { status: "blocked" } }));
    expect(document.getElementById("r1")!.textContent).toBe("no");
  });

  it("neq matches when value differs", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.role" }, coerceAs: "string", op: "neq", operand: "guest" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "authorized" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { role: "admin" } }));
    expect(document.getElementById("r1")!.textContent).toBe("authorized");
  });

  it("gt compares numbers correctly", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.score" }, coerceAs: "number", op: "gt", operand: 80 },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "high" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "low" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { score: 95 } }));
    expect(document.getElementById("r1")!.textContent).toBe("high");
    document.getElementById("r1")!.textContent = "—";
    document.dispatchEvent(new CustomEvent("test", { detail: { score: 50 } }));
    expect(document.getElementById("r1")!.textContent).toBe("low");
  });

  it("between matches range inclusive", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.temp" }, coerceAs: "number", op: "between", operand: [60, 80] },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "comfortable" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "extreme" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { temp: 72 } }));
    expect(document.getElementById("r1")!.textContent).toBe("comfortable");
  });

  // ── Presence operators ──

  it("truthy matches non-empty values", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.active" }, coerceAs: "boolean", op: "truthy" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "active" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "inactive" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { active: true } }));
    expect(document.getElementById("r1")!.textContent).toBe("active");
  });

  it("falsy matches empty/false/null values", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.name" }, coerceAs: "string", op: "falsy" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "missing" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { name: "" } }));
    expect(document.getElementById("r1")!.textContent).toBe("missing");
  });

  it("is-null detects null values", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.address" }, coerceAs: "raw", op: "is-null" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "no address" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { address: null } }));
    expect(document.getElementById("r1")!.textContent).toBe("no address");
  });

  it("not-empty detects non-empty string", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.name" }, coerceAs: "string", op: "not-empty" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "has name" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { name: "Margaret" } }));
    expect(document.getElementById("r1")!.textContent).toBe("has name");
  });

  it("is-empty detects empty array", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.items" }, coerceAs: "raw", op: "is-empty" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "empty" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { items: [] } }));
    expect(document.getElementById("r1")!.textContent).toBe("empty");
  });

  // ── Membership operators ──

  it("in matches value in set", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.status" }, coerceAs: "string", op: "in", operand: ["active", "pending"] },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "valid" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "invalid" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { status: "pending" } }));
    expect(document.getElementById("r1")!.textContent).toBe("valid");
  });

  it("not-in excludes values in set", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.role" }, coerceAs: "string", op: "not-in", operand: ["banned", "suspended"] },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "allowed" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { role: "member" } }));
    expect(document.getElementById("r1")!.textContent).toBe("allowed");
  });

  // ── Text operators ──

  it("contains matches substring", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.name" }, coerceAs: "string", op: "contains", operand: "son" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "matched" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { name: "Thompson" } }));
    expect(document.getElementById("r1")!.textContent).toBe("matched");
  });

  it("starts-with matches prefix", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.name" }, coerceAs: "string", op: "starts-with", operand: "Dr." },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "doctor" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { name: "Dr. Smith" } }));
    expect(document.getElementById("r1")!.textContent).toBe("doctor");
  });

  it("min-length checks string length", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.code" }, coerceAs: "string", op: "min-length", operand: 5 },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "long enough" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "too short" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { code: "ABC" } }));
    expect(document.getElementById("r1")!.textContent).toBe("too short");
    document.getElementById("r1")!.textContent = "—";
    document.dispatchEvent(new CustomEvent("test", { detail: { code: "ABCDEF" } }));
    expect(document.getElementById("r1")!.textContent).toBe("long enough");
  });

  // ── Composite guards ──

  it("all guard requires both conditions true", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "all", guards: [
          { kind: "value", source: { kind: "event", path: "evt.active" }, coerceAs: "boolean", op: "truthy" },
          { kind: "value", source: { kind: "event", path: "evt.score" }, coerceAs: "number", op: "gt", operand: 50 },
        ]},
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "both" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "nope" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { active: true, score: 80 } }));
    expect(document.getElementById("r1")!.textContent).toBe("both");
    document.getElementById("r1")!.textContent = "—";
    document.dispatchEvent(new CustomEvent("test", { detail: { active: true, score: 30 } }));
    expect(document.getElementById("r1")!.textContent).toBe("nope");
  });

  it("any guard requires at least one condition true", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "any", guards: [
          { kind: "value", source: { kind: "event", path: "evt.role" }, coerceAs: "string", op: "eq", operand: "admin" },
          { kind: "value", source: { kind: "event", path: "evt.role" }, coerceAs: "string", op: "eq", operand: "superadmin" },
        ]},
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "access" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "denied" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { role: "superadmin" } }));
    expect(document.getElementById("r1")!.textContent).toBe("access");
    document.getElementById("r1")!.textContent = "—";
    document.dispatchEvent(new CustomEvent("test", { detail: { role: "guest" } }));
    expect(document.getElementById("r1")!.textContent).toBe("denied");
  });

  it("not guard inverts the inner guard", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "not", inner: { kind: "value", source: { kind: "event", path: "evt.blocked" }, coerceAs: "boolean", op: "truthy" } },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "allowed" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { blocked: false } }));
    expect(document.getElementById("r1")!.textContent).toBe("allowed");
  });

  // ── ElseIf chain — first match wins ──

  it("elseif chain picks first matching branch", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.score" }, coerceAs: "number", op: "gte", operand: 90 },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "grade", mutation: { kind: "set-prop", prop: "textContent" }, value: "A" }] } },
        { guard: { kind: "value", source: { kind: "event", path: "evt.score" }, coerceAs: "number", op: "gte", operand: 80 },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "grade", mutation: { kind: "set-prop", prop: "textContent" }, value: "B" }] } },
        { guard: { kind: "value", source: { kind: "event", path: "evt.score" }, coerceAs: "number", op: "gte", operand: 70 },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "grade", mutation: { kind: "set-prop", prop: "textContent" }, value: "C" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "grade", mutation: { kind: "set-prop", prop: "textContent" }, value: "F" }] } },
      ]},
    }]});

    document.dispatchEvent(new CustomEvent("test", { detail: { score: 95 } }));
    expect(document.getElementById("grade")!.textContent).toBe("A");

    document.dispatchEvent(new CustomEvent("test", { detail: { score: 85 } }));
    expect(document.getElementById("grade")!.textContent).toBe("B");

    document.dispatchEvent(new CustomEvent("test", { detail: { score: 72 } }));
    expect(document.getElementById("grade")!.textContent).toBe("C");

    document.dispatchEvent(new CustomEvent("test", { detail: { score: 40 } }));
    expect(document.getElementById("grade")!.textContent).toBe("F");
  });

  // ── Pre-commands + condition ──

  it("pre-commands execute before condition evaluates", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional",
        commands: [{ kind: "mutate-element", target: "echo", mutation: { kind: "set-prop", prop: "textContent" }, value: "pre" }],
        branches: [
          { guard: { kind: "value", source: { kind: "event", path: "evt.ok" }, coerceAs: "boolean", op: "truthy" },
            reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "branch" }] } },
        ],
      },
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { ok: true } }));
    expect(document.getElementById("echo")!.textContent).toBe("pre");
    expect(document.getElementById("r1")!.textContent).toBe("branch");
  });

  // ── Array coercion ──

  it("array-contains checks item in array", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.tags" }, coerceAs: "array", op: "array-contains", operand: "urgent", elementCoerceAs: "string" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "has urgent" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "no urgent" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { tags: ["normal", "urgent", "review"] } }));
    expect(document.getElementById("r1")!.textContent).toBe("has urgent");
    document.getElementById("r1")!.textContent = "—";
    document.dispatchEvent(new CustomEvent("test", { detail: { tags: ["normal", "review"] } }));
    expect(document.getElementById("r1")!.textContent).toBe("no urgent");
  });

  // ── Date coercion ──

  it("date coercion compares dates as milliseconds", () => {
    const cutoff = new Date("2024-06-01").getTime();
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "conditional", branches: [
        { guard: { kind: "value", source: { kind: "event", path: "evt.date" }, coerceAs: "date", op: "gt", operand: "2024-06-01" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "after" }] } },
        { reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "before" }] } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { date: "2024-07-15" } }));
    expect(document.getElementById("r1")!.textContent).toBe("after");
  });
});
