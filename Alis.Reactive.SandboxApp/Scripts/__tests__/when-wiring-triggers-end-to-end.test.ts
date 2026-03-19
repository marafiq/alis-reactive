import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan } from "../types";

let boot: typeof import("../lifecycle/boot").boot;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <span id="r1">—</span>
    <span id="r2">—</span>
    <span id="r3">—</span>
    <input id="Name" value="Margaret" />
    <select id="Country"><option value="US" selected>US</option></select>
  </body></html>`);
  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Event = dom.window.Event;
  const mod = await import("../lifecycle/boot");
  boot = mod.boot;
});

describe("trigger wiring end-to-end", () => {

  // ── DomReady ──

  it("dom-ready fires immediately on boot", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "booted" },
      ]},
    }]});
    expect(document.getElementById("r1")!.textContent).toBe("booted");
  });

  // ── CustomEvent ──

  it("custom-event fires on matching dispatch", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "my-event" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "received" },
      ]},
    }]});
    expect(document.getElementById("r1")!.textContent).toBe("—");
    document.dispatchEvent(new CustomEvent("my-event"));
    expect(document.getElementById("r1")!.textContent).toBe("received");
  });

  it("custom-event does not fire on non-matching event", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "event-a" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "a" },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("event-b"));
    expect(document.getElementById("r1")!.textContent).toBe("—");
  });

  it("custom-event receives payload from dispatch detail", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "data" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.msg" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("data", { detail: { msg: "hello" } }));
    expect(document.getElementById("r1")!.textContent).toBe("hello");
  });

  it("custom-event fires multiple times", () => {
    let count = 0;
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "tick" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.n" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("tick", { detail: { n: "1" } }));
    expect(document.getElementById("r1")!.textContent).toBe("1");
    document.dispatchEvent(new CustomEvent("tick", { detail: { n: "2" } }));
    expect(document.getElementById("r1")!.textContent).toBe("2");
    document.dispatchEvent(new CustomEvent("tick", { detail: { n: "3" } }));
    expect(document.getElementById("r1")!.textContent).toBe("3");
  });

  // ── ComponentEvent (native) ──

  it("component-event fires on native input change", () => {
    boot({ planId: "t",
      components: { "Name": { id: "Name", vendor: "native", readExpr: "value" } },
      entries: [{
        trigger: { kind: "component-event", componentId: "Name", jsEvent: "change", vendor: "native", readExpr: "value" },
        reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.value" } },
        ]},
      }],
    });
    (document.getElementById("Name") as HTMLInputElement).value = "Eleanor";
    document.getElementById("Name")!.dispatchEvent(new Event("change", { bubbles: true }));
    expect(document.getElementById("r1")!.textContent).toBe("Eleanor");
  });

  it("component-event fires on native select change", () => {
    boot({ planId: "t",
      components: { "Country": { id: "Country", vendor: "native", readExpr: "value" } },
      entries: [{
        trigger: { kind: "component-event", componentId: "Country", jsEvent: "change", vendor: "native", readExpr: "value" },
        reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.value" } },
        ]},
      }],
    });
    (document.getElementById("Country") as HTMLSelectElement).value = "US";
    document.getElementById("Country")!.dispatchEvent(new Event("change", { bubbles: true }));
    expect(document.getElementById("r1")!.textContent).toBe("US");
  });

  // ── Mixed triggers in same plan ──

  it("dom-ready and custom-event coexist in same plan", () => {
    boot({ planId: "t", components: {}, entries: [
      { trigger: { kind: "dom-ready" }, reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "ready" },
      ]}},
      { trigger: { kind: "custom-event", event: "action" }, reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, value: "acted" },
      ]}},
    ]});
    expect(document.getElementById("r1")!.textContent).toBe("ready");
    expect(document.getElementById("r2")!.textContent).toBe("—");
    document.dispatchEvent(new CustomEvent("action"));
    expect(document.getElementById("r2")!.textContent).toBe("acted");
  });

  it("dom-ready + component-event + custom-event all work together", () => {
    boot({ planId: "t",
      components: { "Name": { id: "Name", vendor: "native", readExpr: "value" } },
      entries: [
        { trigger: { kind: "dom-ready" }, reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "r1", mutation: { kind: "set-prop", prop: "textContent" }, value: "booted" },
        ]}},
        { trigger: { kind: "component-event", componentId: "Name", jsEvent: "change", vendor: "native", readExpr: "value" },
          reaction: { kind: "sequential", commands: [
            { kind: "mutate-element", target: "r2", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.value" } },
          ]},
        },
        { trigger: { kind: "custom-event", event: "save" }, reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "r3", mutation: { kind: "set-prop", prop: "textContent" }, value: "saved" },
        ]}},
      ],
    });
    expect(document.getElementById("r1")!.textContent).toBe("booted");

    (document.getElementById("Name") as HTMLInputElement).value = "Eleanor";
    document.getElementById("Name")!.dispatchEvent(new Event("change", { bubbles: true }));
    expect(document.getElementById("r2")!.textContent).toBe("Eleanor");

    document.dispatchEvent(new CustomEvent("save"));
    expect(document.getElementById("r3")!.textContent).toBe("saved");
  });
});
