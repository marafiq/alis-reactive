import { describe, it, expect } from "vitest";
import { boot } from "../boot";

describe("when composing a plan", () => {
  it("wires each entry independently", () => {
    const events: string[] = [];
    document.addEventListener("comp-a", () => events.push("a"));
    document.addEventListener("comp-b", () => events.push("b"));

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "comp-a" }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "trigger-comp-b" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "comp-b" }],
          },
        },
      ],
    });

    expect(events).toEqual(["a"]);

    document.dispatchEvent(new CustomEvent("trigger-comp-b"));
    expect(events).toEqual(["a", "b"]);
  });

  it("handles empty plan without error", () => {
    expect(() => boot({ entries: [] })).not.toThrow();
  });

  it("handles empty command list without error", () => {
    expect(() => boot({
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: { kind: "sequential", commands: [] },
      }],
    })).not.toThrow();
  });
});
