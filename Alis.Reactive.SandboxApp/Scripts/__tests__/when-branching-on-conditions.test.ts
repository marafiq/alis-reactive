import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan } from "../types";

// Helper to create event source
function es(path: string) {
  return { kind: "event" as const, path };
}

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
      planId: "Test.Model",
      components: {},
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
                guard: { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 90 },
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
      planId: "Test.Model",
      components: {},
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
                guard: { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 90 },
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

  it("evaluates multi-branch (ElseIf behavior) — first matching branch wins", () => {
    boot({
      planId: "Test.Model",
      components: {},
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
                guard: { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 90 },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "A" }],
                },
              },
              {
                guard: { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 80 },
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

  it("evaluates AND composition (all guards must pass)", () => {
    boot({
      planId: "Test.Model",
      components: {},
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
                    { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 90 },
                    { kind: "value", source: es("evt.status"), coerceAs: "string", op: "eq", operand: "active" },
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

  it("evaluates NOT guard (inverts result)", () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "not-check", payload: { role: "user" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "not-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "not", inner: { kind: "value", source: es("evt.role"), coerceAs: "string", op: "eq", operand: "admin" } },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Not Admin" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Is Admin" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Not Admin");
  });

  it("evaluates In membership", () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "in-check", payload: { category: "B" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "in-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: es("evt.category"), coerceAs: "string", op: "in", operand: ["A", "B", "C"] },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "In Group" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Not In Group" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("In Group");
  });

  it("evaluates Between range", () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "between-check", payload: { age: 30 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "between-check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: es("evt.age"), coerceAs: "number", op: "between", operand: [18, 65] },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Working Age" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Outside Range" }],
                },
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Working Age");
  });

  it("evaluates per-action when guard — skips command when false", () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "per-action-check", payload: { score: 50 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "per-action-check" },
          reaction: {
            kind: "sequential",
            commands: [
              { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Always" },
              {
                kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Bonus",
                when: { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 90 },
              },
            ],
          },
        },
      ],
    });

    // score=50 → per-action guard fails → "Bonus" skipped → stays "Always"
    expect(document.getElementById("result")!.textContent).toBe("Always");
  });

  it("evaluates per-action when guard — executes command when true", () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "per-action-check2", payload: { score: 95 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "per-action-check2" },
          reaction: {
            kind: "sequential",
            commands: [
              { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Always" },
              {
                kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Bonus",
                when: { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 90 },
              },
            ],
          },
        },
      ],
    });

    // score=95 → per-action guard passes → "Bonus" overwrites "Always"
    expect(document.getElementById("result")!.textContent).toBe("Bonus");
  });
});
