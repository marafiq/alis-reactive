import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan } from "../types";

// Boot must be imported AFTER setting up the DOM
let boot: (plan: Plan) => void;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <span id="result">original</span>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Function = dom.window.Function;

  // Re-import to get fresh module with new document
  const mod = await import("../boot");
  boot = mod.boot;
});

describe("when branching on conditions", () => {
  it("takes then-branch when guard passes", () => {
    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "grade-check", payload: { score: 95 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "grade-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Pass" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Fail" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Pass");
  });

  it("takes else-branch when guard fails", () => {
    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "grade-check", payload: { score: 40 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "grade-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Pass" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Fail" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Fail");
  });

  it("takes then-branch without else when guard passes", () => {
    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "status-check", payload: { active: true } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "status-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.active", coerceAs: "raw", op: "truthy" },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Active" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Active");
  });

  it("skips all branches when no guard matches and no else", () => {
    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "status-check-2", payload: { active: false } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "status-check-2" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.active", coerceAs: "raw", op: "truthy" },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Active" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("original");
  });

  it("evaluates AND composition (all guards must pass)", () => {
    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "and-check", payload: { score: 95, status: "active" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "and-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "all",
                  guards: [
                    { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
                    { kind: "value", source: "evt.status", coerceAs: "string", op: "eq", operand: "active" },
                  ],
                },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Active High Scorer" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Nope" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Active High Scorer");
  });

  it("evaluates OR composition (any guard can pass)", () => {
    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "or-check", payload: { role: "superuser" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "or-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "any",
                  guards: [
                    { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "admin" },
                    { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "superuser" },
                  ],
                },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Authorized" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Denied" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Authorized");
  });

  it("evaluates multi-branch (ElseIf behavior) — first matching branch wins", () => {
    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "multi-branch", payload: { score: 85 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "multi-branch" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "A" }],
                },
              },
              {
                guard: { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 80 },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "B" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "C" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("B");
  });

  it("evaluates nested payload properties in compound guard", () => {
    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "nested-check", payload: { user: { role: "admin", level: 5 } } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "nested-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "all",
                  guards: [
                    { kind: "value", source: "evt.user.role", coerceAs: "string", op: "eq", operand: "admin" },
                    { kind: "value", source: "evt.user.level", coerceAs: "number", op: "gte", operand: 3 },
                  ],
                },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Super Admin" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Regular" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Super Admin");
  });
});
