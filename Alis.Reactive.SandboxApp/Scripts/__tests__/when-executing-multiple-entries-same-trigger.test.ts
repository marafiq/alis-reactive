import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan } from "../types";

let boot: typeof import("../lifecycle/boot").boot;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <span id="r1">—</span>
    <span id="r2">—</span>
    <span id="r3">—</span>
    <span id="echo">—</span>
    <span id="footer">—</span>
    <div id="panel" hidden></div>
  </body></html>`);
  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Event = dom.window.Event;

  const mod = await import("../lifecycle/boot");
  boot = mod.boot;
});

describe("multiple entries on the same trigger", () => {
  it("fires all entries when dom-ready has multiple entries", () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "first" }],
          },
        },
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "second" }],
          },
        },
      ],
    };
    boot(plan);
    expect(document.getElementById("r1")!.textContent).toBe("first");
    expect(document.getElementById("r2")!.textContent).toBe("second");
  });

  it("fires all entries when custom-event has multiple entries", async () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "my-event" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "entry-1" }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "my-event" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: { kind: "event", path: "evt.name" }, coerceAs: "string", op: "eq", operand: "hello" },
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "matched" }] },
              },
              {
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "else" }] },
              },
            ],
          },
        },
      ],
    };
    boot(plan);
    document.dispatchEvent(new CustomEvent("my-event", { detail: { name: "hello" } }));
    // Sequential entry runs sync; conditional branch is async — flush microtasks
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("r1")!.textContent).toBe("entry-1");
    expect(document.getElementById("r2")!.textContent).toBe("matched");
  });

  it("sequential entry + conditional entry + sequential entry all fire independently", async () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "test" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "echo", mutation: { kind: "set-prop", prop: "textContent" }, value: "before" }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "test" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: { kind: "event", path: "evt.active" }, coerceAs: "boolean", op: "truthy" },
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "active" }] },
              },
              {
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "inactive" }] },
              },
            ],
          },
        },
        {
          trigger: { kind: "custom-event", event: "test" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: { kind: "event", path: "evt.score" }, coerceAs: "number", op: "gt", operand: 80 },
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "high" }] },
              },
              {
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "low" }] },
              },
            ],
          },
        },
        {
          trigger: { kind: "custom-event", event: "test" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "footer", mutation: { kind: "set-prop", prop: "textContent" }, value: "after" }],
          },
        },
      ],
    };
    boot(plan);
    document.dispatchEvent(new CustomEvent("test", { detail: { active: true, score: 95 } }));

    // Sequential entries run sync; conditional branches are async — flush microtasks
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("echo")!.textContent).toBe("before");
    expect(document.getElementById("r1")!.textContent).toBe("active");
    expect(document.getElementById("r2")!.textContent).toBe("high");
    expect(document.getElementById("footer")!.textContent).toBe("after");
  });

  it("two independent conditions evaluate separately — first match does NOT skip second", async () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "test" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: { kind: "event", path: "evt.color" }, coerceAs: "string", op: "eq", operand: "red" },
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "is red" }] },
              },
              {
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "not red" }] },
              },
            ],
          },
        },
        {
          trigger: { kind: "custom-event", event: "test" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: { kind: "event", path: "evt.size" }, coerceAs: "number", op: "gt", operand: 10 },
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "big" }] },
              },
              {
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "small" }] },
              },
            ],
          },
        },
      ],
    };
    boot(plan);

    // Both conditions true — conditional branches are async, flush microtasks
    document.dispatchEvent(new CustomEvent("test", { detail: { color: "red", size: 20 } }));
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("r1")!.textContent).toBe("is red");
    expect(document.getElementById("r2")!.textContent).toBe("big");
  });

  it("first condition matches, second does not — both still evaluate", async () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "test" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: { kind: "event", path: "evt.color" }, coerceAs: "string", op: "eq", operand: "red" },
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "is red" }] },
              },
              {
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "not red" }] },
              },
            ],
          },
        },
        {
          trigger: { kind: "custom-event", event: "test" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: { kind: "event", path: "evt.size" }, coerceAs: "number", op: "gt", operand: 100 },
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "huge" }] },
              },
              {
                reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "normal" }] },
              },
            ],
          },
        },
      ],
    };
    boot(plan);

    // First matches (red), second falls to else (size=5 < 100) — conditional branches async, flush microtasks
    document.dispatchEvent(new CustomEvent("test", { detail: { color: "red", size: 5 } }));
    await new Promise(r => setTimeout(r, 0));
    expect(document.getElementById("r1")!.textContent).toBe("is red");
    expect(document.getElementById("r2")!.textContent).toBe("normal");
  });

  it("dom-ready entries fire before custom-event entries can dispatch into them", () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "after-boot" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "received" }],
          },
        },
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [
              { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "booted" },
              { kind: "dispatch", event: "after-boot" },
            ],
          },
        },
      ],
    };
    boot(plan);

    // Two-phase boot: custom-event listener wired first, then dom-ready fires and dispatches
    expect(document.getElementById("r1")!.textContent).toBe("booted");
    expect(document.getElementById("r2")!.textContent).toBe("received");
  });

  it("three dom-ready entries execute in plan order", () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "1" }] },
        },
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "2" }] },
        },
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [{ kind: "mutate-element", target: "r3", mutation: { kind: "set-prop", prop: "textContent" }, value: "3" }] },
        },
      ],
    };
    boot(plan);
    expect(document.getElementById("r1")!.textContent).toBe("1");
    expect(document.getElementById("r2")!.textContent).toBe("2");
    expect(document.getElementById("r3")!.textContent).toBe("3");
  });
});
