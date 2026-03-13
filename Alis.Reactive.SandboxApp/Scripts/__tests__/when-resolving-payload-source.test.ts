import { describe, it, expect } from "vitest";
import { boot } from "../boot";

describe("when resolving payload source in mutate-element", () => {
  it("resolves flat string property from event detail", () => {
    document.body.innerHTML = '<span id="name">—</span>';

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "src-test-1", payload: { stringValue: "hello" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "src-test-1" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "name", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.stringValue" } }],
          },
        },
      ],
    });

    expect(document.getElementById("name")!.textContent).toBe("hello");
  });

  it("resolves flat int property from event detail", () => {
    document.body.innerHTML = '<span id="count">—</span>';

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "src-test-2", payload: { intValue: 42 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "src-test-2" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "count", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.intValue" } }],
          },
        },
      ],
    });

    expect(document.getElementById("count")!.textContent).toBe("42");
  });

  it("resolves flat bool property from event detail", () => {
    document.body.innerHTML = '<span id="active">—</span>';

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "src-test-3", payload: { boolValue: true } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "src-test-3" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "active", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.boolValue" } }],
          },
        },
      ],
    });

    expect(document.getElementById("active")!.textContent).toBe("true");
  });

  it("resolves nested property via dot-path", () => {
    document.body.innerHTML = '<span id="city">—</span>';

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "dispatch",
              event: "src-test-4",
              payload: { address: { city: "Seattle", zip: "98101" } },
            }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "src-test-4" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "city", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.address.city" } }],
          },
        },
      ],
    });

    expect(document.getElementById("city")!.textContent).toBe("Seattle");
  });

  it("resolves all nested address properties", () => {
    document.body.innerHTML = '<span id="street">—</span><span id="city2">—</span><span id="zip">—</span>';

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "dispatch",
              event: "src-test-5",
              payload: { address: { street: "123 Main St", city: "Seattle", zip: "98101" } },
            }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "src-test-5" },
          reaction: {
            kind: "sequential",
            commands: [
              { kind: "mutate-element", target: "street", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.address.street" } },
              { kind: "mutate-element", target: "city2", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.address.city" } },
              { kind: "mutate-element", target: "zip", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.address.zip" } },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("street")!.textContent).toBe("123 Main St");
    expect(document.getElementById("city2")!.textContent).toBe("Seattle");
    expect(document.getElementById("zip")!.textContent).toBe("98101");
  });

  it("resolves all primitive types correctly", () => {
    document.body.innerHTML = `
      <span id="t-int">—</span>
      <span id="t-long">—</span>
      <span id="t-double">—</span>
      <span id="t-string">—</span>
      <span id="t-bool">—</span>
    `;

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "dispatch",
              event: "src-test-6",
              payload: {
                intValue: 42,
                longValue: 9007199254740991,
                doubleValue: 3.14159,
                stringValue: "hello world",
                boolValue: true,
              },
            }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "src-test-6" },
          reaction: {
            kind: "sequential",
            commands: [
              { kind: "mutate-element", target: "t-int", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.intValue" } },
              { kind: "mutate-element", target: "t-long", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.longValue" } },
              { kind: "mutate-element", target: "t-double", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.doubleValue" } },
              { kind: "mutate-element", target: "t-string", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.stringValue" } },
              { kind: "mutate-element", target: "t-bool", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.boolValue" } },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("t-int")!.textContent).toBe("42");
    expect(document.getElementById("t-long")!.textContent).toBe("9007199254740991");
    expect(document.getElementById("t-double")!.textContent).toBe("3.14159");
    expect(document.getElementById("t-string")!.textContent).toBe("hello world");
    expect(document.getElementById("t-bool")!.textContent).toBe("true");
  });

  it("falls back to static value when no source", () => {
    document.body.innerHTML = '<span id="static-test">—</span>';

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "static-test", jsEmit: "el.textContent = val", value: "static" }],
          },
        },
      ],
    });

    expect(document.getElementById("static-test")!.textContent).toBe("static");
  });

  it("renders empty string when source path not found", () => {
    document.body.innerHTML = '<span id="missing">—</span>';

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "src-test-7", payload: { name: "test" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "src-test-7" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "missing", jsEmit: "el.textContent = val", source: { kind: "event", path: "evt.nonexistent.deep" } }],
          },
        },
      ],
    });

    expect(document.getElementById("missing")!.textContent).toBe("");
  });
});
