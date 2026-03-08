import { describe, it, expect } from "vitest";
import { boot } from "../boot";

describe("when resolving payload source in mutate-element", () => {
  it("resolves flat string property from event detail", () => {
    document.body.innerHTML = '<span id="name">—</span>';

    boot({
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
            commands: [{ kind: "mutate-element", target: "name", action: "set-text", source: "evt.stringValue" }],
          },
        },
      ],
    });

    expect(document.getElementById("name")!.textContent).toBe("hello");
  });

  it("resolves flat int property from event detail", () => {
    document.body.innerHTML = '<span id="count">—</span>';

    boot({
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
            commands: [{ kind: "mutate-element", target: "count", action: "set-text", source: "evt.intValue" }],
          },
        },
      ],
    });

    expect(document.getElementById("count")!.textContent).toBe("42");
  });

  it("resolves flat bool property from event detail", () => {
    document.body.innerHTML = '<span id="active">—</span>';

    boot({
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
            commands: [{ kind: "mutate-element", target: "active", action: "set-text", source: "evt.boolValue" }],
          },
        },
      ],
    });

    expect(document.getElementById("active")!.textContent).toBe("true");
  });

  it("resolves nested property via dot-path", () => {
    document.body.innerHTML = '<span id="city">—</span>';

    boot({
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
            commands: [{ kind: "mutate-element", target: "city", action: "set-text", source: "evt.address.city" }],
          },
        },
      ],
    });

    expect(document.getElementById("city")!.textContent).toBe("Seattle");
  });

  it("resolves all nested address properties", () => {
    document.body.innerHTML = '<span id="street">—</span><span id="city2">—</span><span id="zip">—</span>';

    boot({
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
              { kind: "mutate-element", target: "street", action: "set-text", source: "evt.address.street" },
              { kind: "mutate-element", target: "city2", action: "set-text", source: "evt.address.city" },
              { kind: "mutate-element", target: "zip", action: "set-text", source: "evt.address.zip" },
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
              { kind: "mutate-element", target: "t-int", action: "set-text", source: "evt.intValue" },
              { kind: "mutate-element", target: "t-long", action: "set-text", source: "evt.longValue" },
              { kind: "mutate-element", target: "t-double", action: "set-text", source: "evt.doubleValue" },
              { kind: "mutate-element", target: "t-string", action: "set-text", source: "evt.stringValue" },
              { kind: "mutate-element", target: "t-bool", action: "set-text", source: "evt.boolValue" },
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
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "mutate-element", target: "static-test", action: "set-text", value: "static" }],
          },
        },
      ],
    });

    expect(document.getElementById("static-test")!.textContent).toBe("static");
  });

  it("renders empty string when source path not found", () => {
    document.body.innerHTML = '<span id="missing">—</span>';

    boot({
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
            commands: [{ kind: "mutate-element", target: "missing", action: "set-text", source: "evt.nonexistent.deep" }],
          },
        },
      ],
    });

    expect(document.getElementById("missing")!.textContent).toBe("");
  });
});
