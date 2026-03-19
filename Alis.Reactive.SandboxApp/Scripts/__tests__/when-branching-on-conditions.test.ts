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

  // Re-import to get fresh module with new document
  const mod = await import("../lifecycle/boot");
  boot = mod.boot;
});

// Flush all pending microtasks so async conditional branches complete
function flushMicrotasks() {
  return new Promise(r => setTimeout(r, 0));
}

describe("when branching on conditions", () => {
  it("takes then-branch when guard passes", async () => {
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
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Pass" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Fail" }],
                },
              },
            ],
          },
        },
      ],
    });

    await flushMicrotasks();
    expect(document.getElementById("result")!.textContent).toBe("Pass");
  });

  it("takes else-branch when guard fails", async () => {
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
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Pass" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Fail" }],
                },
              },
            ],
          },
        },
      ],
    });

    await flushMicrotasks();
    expect(document.getElementById("result")!.textContent).toBe("Fail");
  });

  it("evaluates multi-branch (ElseIf behavior) — first matching branch wins", async () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "grade-check", payload: { score: 85 } }],
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
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "A" }],
                },
              },
              {
                guard: { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 80 },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "B" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "C" }],
                },
              },
            ],
          },
        },
      ],
    });

    await flushMicrotasks();
    expect(document.getElementById("result")!.textContent).toBe("B");
  });

  it("evaluates AND composition (all guards must pass)", async () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "check", payload: { active: true, score: 92 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "all",
                  guards: [
                    { kind: "value", source: es("evt.active"), coerceAs: "boolean", op: "truthy" },
                    { kind: "value", source: es("evt.score"), coerceAs: "number", op: "gte", operand: 90 },
                  ],
                },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Active High Scorer" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Nope" }],
                },
              },
            ],
          },
        },
      ],
    });

    await flushMicrotasks();
    expect(document.getElementById("result")!.textContent).toBe("Active High Scorer");
  });

  it("evaluates NOT guard (inverts result)", async () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "check", payload: { role: "member" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "not",
                  inner: { kind: "value", source: es("evt.role"), coerceAs: "string", op: "eq", operand: "admin" },
                },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Not Admin" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Admin" }],
                },
              },
            ],
          },
        },
      ],
    });

    await flushMicrotasks();
    expect(document.getElementById("result")!.textContent).toBe("Not Admin");
  });

  it("evaluates In membership", async () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "check", payload: { group: "beta" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: es("evt.group"), coerceAs: "string", op: "in", operand: ["alpha", "beta", "gamma"] },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "In Group" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Not In Group" }],
                },
              },
            ],
          },
        },
      ],
    });

    await flushMicrotasks();
    expect(document.getElementById("result")!.textContent).toBe("In Group");
  });

  it("evaluates Between range", async () => {
    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "check", payload: { age: 35 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "check" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: es("evt.age"), coerceAs: "number", op: "between", operand: [18, 65] },
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Working Age" }],
                },
              },
              {
                guard: null,
                reaction: {
                  kind: "sequential",
                  commands: [{ kind: "mutate-element", target: "result", mutation: { kind: "set-prop", prop: "textContent" }, value: "Out of Range" }],
                },
              },
            ],
          },
        },
      ],
    });

    await flushMicrotasks();
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
            commands: [
              {
                kind: "mutate-element",
                target: "result",
                mutation: { kind: "set-prop", prop: "textContent" },
                value: "blocked",
                when: { kind: "value", source: es("ctx.active"), coerceAs: "boolean", op: "truthy" },
              },
            ],
          },
        },
      ],
    });

    // No ctx.active — when guard false — textContent unchanged
    expect(document.getElementById("result")!.textContent).toBe("original");
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
            commands: [
              {
                kind: "mutate-element",
                target: "result",
                mutation: { kind: "set-prop", prop: "textContent" },
                value: "allowed",
                when: null,
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("allowed");
  });
});
