import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan } from "../types";

let boot: typeof import("../lifecycle/boot").boot;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <span id="step1">—</span>
    <span id="step2">—</span>
    <span id="step3">—</span>
    <span id="chain-result">—</span>
    <div id="panel" hidden></div>
  </body></html>`);
  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Event = dom.window.Event;

  const mod = await import("../lifecycle/boot");
  boot = mod.boot;
});

describe("two-phase boot", () => {
  it("custom-event listeners are wired before dom-ready executes", () => {
    // This is the foundational invariant — if dom-ready dispatches an event,
    // the custom-event listener must already exist to catch it.
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [
              { kind: "mutate-element", target: "step1", mutation: { kind: "set-prop", prop: "textContent" }, value: "dom-ready fired" },
              { kind: "dispatch", event: "init" },
            ],
          },
        },
        {
          trigger: { kind: "custom-event", event: "init" },
          reaction: {
            kind: "sequential",
            commands: [
              { kind: "mutate-element", target: "step2", mutation: { kind: "set-prop", prop: "textContent" }, value: "init received" },
            ],
          },
        },
      ],
    };
    boot(plan);

    expect(document.getElementById("step1")!.textContent).toBe("dom-ready fired");
    expect(document.getElementById("step2")!.textContent).toBe("init received");
  });

  it("three-hop dispatch chain works: dom-ready → event-A → event-B → event-C", () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [
            { kind: "mutate-element", target: "step1", mutation: { kind: "set-prop", prop: "textContent" }, value: "1" },
            { kind: "dispatch", event: "hop-a" },
          ]},
        },
        {
          trigger: { kind: "custom-event", event: "hop-a" },
          reaction: { kind: "sequential", commands: [
            { kind: "mutate-element", target: "step2", mutation: { kind: "set-prop", prop: "textContent" }, value: "2" },
            { kind: "dispatch", event: "hop-b" },
          ]},
        },
        {
          trigger: { kind: "custom-event", event: "hop-b" },
          reaction: { kind: "sequential", commands: [
            { kind: "mutate-element", target: "step3", mutation: { kind: "set-prop", prop: "textContent" }, value: "3" },
          ]},
        },
      ],
    };
    boot(plan);

    expect(document.getElementById("step1")!.textContent).toBe("1");
    expect(document.getElementById("step2")!.textContent).toBe("2");
    expect(document.getElementById("step3")!.textContent).toBe("3");
  });

  it("entry order in plan does not matter — custom-event defined after dom-ready still works", () => {
    // Entries listed: custom-event first, dom-ready second
    // Boot must wire custom-event BEFORE executing dom-ready regardless of order
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "late-event" },
          reaction: { kind: "sequential", commands: [
            { kind: "mutate-element", target: "chain-result", mutation: { kind: "set-prop", prop: "textContent" }, value: "caught" },
          ]},
        },
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [
            { kind: "dispatch", event: "late-event" },
          ]},
        },
      ],
    };
    boot(plan);

    expect(document.getElementById("chain-result")!.textContent).toBe("caught");
  });

  it("dispatch with payload flows through the chain", () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: { kind: "sequential", commands: [
            { kind: "dispatch", event: "data-ready", payload: { name: "Margaret", age: 82 } },
          ]},
        },
        {
          trigger: { kind: "custom-event", event: "data-ready" },
          reaction: { kind: "sequential", commands: [
            { kind: "mutate-element", target: "step1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.name" } },
            { kind: "mutate-element", target: "step2", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.age" } },
          ]},
        },
      ],
    };
    boot(plan);

    expect(document.getElementById("step1")!.textContent).toBe("Margaret");
    expect(document.getElementById("step2")!.textContent).toBe("82");
  });

  it("multiple custom-event listeners for different events all wire correctly", () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        { trigger: { kind: "custom-event", event: "event-a" }, reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "step1", mutation: { kind: "set-prop", prop: "textContent" }, value: "a" },
        ]}},
        { trigger: { kind: "custom-event", event: "event-b" }, reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "step2", mutation: { kind: "set-prop", prop: "textContent" }, value: "b" },
        ]}},
        { trigger: { kind: "custom-event", event: "event-c" }, reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "step3", mutation: { kind: "set-prop", prop: "textContent" }, value: "c" },
        ]}},
      ],
    };
    boot(plan);

    document.dispatchEvent(new CustomEvent("event-b"));
    expect(document.getElementById("step1")!.textContent).toBe("—");
    expect(document.getElementById("step2")!.textContent).toBe("b");
    expect(document.getElementById("step3")!.textContent).toBe("—");

    document.dispatchEvent(new CustomEvent("event-a"));
    document.dispatchEvent(new CustomEvent("event-c"));
    expect(document.getElementById("step1")!.textContent).toBe("a");
    expect(document.getElementById("step3")!.textContent).toBe("c");
  });

  it("conditional reaction with pre-commands executes commands then evaluates branches", () => {
    const plan: Plan = {
      planId: "test",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "check" },
          reaction: {
            kind: "conditional",
            commands: [
              { kind: "mutate-element", target: "step1", mutation: { kind: "set-prop", prop: "textContent" }, value: "pre-command ran" },
            ],
            branches: [
              {
                guard: { kind: "value", source: { kind: "event", path: "evt.ok" }, coerceAs: "boolean", op: "truthy" },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "step2", mutation: { kind: "set-prop", prop: "textContent" }, value: "branch taken" },
                ]},
              },
            ],
          },
        },
      ],
    };
    boot(plan);
    document.dispatchEvent(new CustomEvent("check", { detail: { ok: true } }));

    expect(document.getElementById("step1")!.textContent).toBe("pre-command ran");
    expect(document.getElementById("step2")!.textContent).toBe("branch taken");
  });

  it("empty plan boots without errors", () => {
    const plan: Plan = { planId: "test", components: {}, entries: [] };
    expect(() => boot(plan)).not.toThrow();
  });
});
