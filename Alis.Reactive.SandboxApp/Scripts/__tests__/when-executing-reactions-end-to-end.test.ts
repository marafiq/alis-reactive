import { describe, it, expect, beforeEach } from "vitest";
import { JSDOM } from "jsdom";
import type { Plan } from "../types";

let boot: typeof import("../boot").boot;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <span id="a">—</span>
    <span id="b">—</span>
    <span id="c">—</span>
    <div id="panel" hidden></div>
    <input id="NameField" value="Margaret" />
    <input id="AgeField" type="number" value="82" />
    <input id="ActiveField" type="checkbox" />
  </body></html>`);
  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;
  (globalThis as any).Event = dom.window.Event;
  const mod = await import("../boot");
  boot = mod.boot;
});

describe("reaction execution end-to-end", () => {

  // ── Sequential commands ──

  it("sequential commands execute in order", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, value: "1" },
        { kind: "mutate-element", target: "b", mutation: { kind: "set-prop", prop: "textContent" }, value: "2" },
        { kind: "mutate-element", target: "c", mutation: { kind: "set-prop", prop: "textContent" }, value: "3" },
      ]},
    }]});
    expect(document.getElementById("a")!.textContent).toBe("1");
    expect(document.getElementById("b")!.textContent).toBe("2");
    expect(document.getElementById("c")!.textContent).toBe("3");
  });

  // ── Dispatch chain ──

  it("dispatch fires custom event caught by another entry", () => {
    boot({ planId: "t", components: {}, entries: [
      { trigger: { kind: "custom-event", event: "step2" }, reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "b", mutation: { kind: "set-prop", prop: "textContent" }, value: "step2-done" },
      ]}},
      { trigger: { kind: "dom-ready" }, reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, value: "step1-done" },
        { kind: "dispatch", event: "step2" },
      ]}},
    ]});
    expect(document.getElementById("a")!.textContent).toBe("step1-done");
    expect(document.getElementById("b")!.textContent).toBe("step2-done");
  });

  it("dispatch with payload passes data to listener", () => {
    boot({ planId: "t", components: {}, entries: [
      { trigger: { kind: "custom-event", event: "data-ready" }, reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.name" } },
      ]}},
      { trigger: { kind: "dom-ready" }, reaction: { kind: "sequential", commands: [
        { kind: "dispatch", event: "data-ready", payload: { name: "Eleanor" } },
      ]}},
    ]});
    expect(document.getElementById("a")!.textContent).toBe("Eleanor");
  });

  // ── Element mutations ──

  it("show removes hidden attribute", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "panel", mutation: { kind: "call", method: "removeAttribute", args: [{ kind: "literal", value: "hidden" }] } },
      ]},
    }]});
    expect(document.getElementById("panel")!.hasAttribute("hidden")).toBe(false);
  });

  it("hide sets hidden attribute", () => {
    document.getElementById("panel")!.removeAttribute("hidden");
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "panel", mutation: { kind: "call", method: "setAttribute", args: [{ kind: "literal", value: "hidden" }, { kind: "literal", value: "" }] } },
      ]},
    }]});
    expect(document.getElementById("panel")!.hasAttribute("hidden")).toBe(true);
  });

  it("addClass adds CSS class", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "a", mutation: { kind: "call", method: "add", chain: "classList", args: [{ kind: "literal", value: "active" }] } },
      ]},
    }]});
    expect(document.getElementById("a")!.classList.contains("active")).toBe(true);
  });

  it("removeClass removes CSS class", () => {
    document.getElementById("a")!.classList.add("old");
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "a", mutation: { kind: "call", method: "remove", chain: "classList", args: [{ kind: "literal", value: "old" }] } },
      ]},
    }]});
    expect(document.getElementById("a")!.classList.contains("old")).toBe(false);
  });

  it("toggleClass toggles CSS class", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "a", mutation: { kind: "call", method: "toggle", chain: "classList", args: [{ kind: "literal", value: "highlight" }] } },
      ]},
    }]});
    expect(document.getElementById("a")!.classList.contains("highlight")).toBe(true);
  });

  it("setHtml sets innerHTML", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "innerHTML" }, value: "<b>bold</b>" },
      ]},
    }]});
    expect(document.getElementById("a")!.innerHTML).toBe("<b>bold</b>");
  });

  // ── Source resolution in mutations ──

  it("event source resolves dot path from payload", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "custom-event", event: "test" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "event", path: "evt.address.city" } },
      ]},
    }]});
    document.dispatchEvent(new CustomEvent("test", { detail: { address: { city: "Portland" } } }));
    expect(document.getElementById("a")!.textContent).toBe("Portland");
  });

  it("component source reads native input value", () => {
    boot({ planId: "t",
      components: { "Name": { id: "NameField", vendor: "native", readExpr: "value" } },
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "component", componentId: "NameField", vendor: "native", readExpr: "value" } },
        ]},
      }],
    });
    expect(document.getElementById("a")!.textContent).toBe("Margaret");
  });

  it("component source reads checkbox checked state", () => {
    (document.getElementById("ActiveField") as HTMLInputElement).checked = true;
    boot({ planId: "t",
      components: { "Active": { id: "ActiveField", vendor: "native", readExpr: "checked" } },
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "a", mutation: { kind: "set-prop", prop: "textContent" }, source: { kind: "component", componentId: "ActiveField", vendor: "native", readExpr: "checked" } },
        ]},
      }],
    });
    expect(document.getElementById("a")!.textContent).toBe("true");
  });

  // ── Vendor mutation — set-prop on native component ──

  it("set-prop on native component sets value", () => {
    boot({ planId: "t", components: {}, entries: [{
      trigger: { kind: "dom-ready" },
      reaction: { kind: "sequential", commands: [
        { kind: "mutate-element", target: "NameField", mutation: { kind: "set-prop", prop: "value" }, value: "Eleanor", vendor: "native" },
      ]},
    }]});
    expect((document.getElementById("NameField") as HTMLInputElement).value).toBe("Eleanor");
  });

  // ── Fail fast on missing target ──

  it("throws on missing target element", () => {
    expect(() => {
      boot({ planId: "t", components: {}, entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [
          { kind: "mutate-element", target: "nonexistent", mutation: { kind: "set-prop", prop: "textContent" }, value: "x" },
        ]},
      }]});
    }).toThrow();
  });
});
