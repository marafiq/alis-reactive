import { describe, it, expect } from "vitest";
import { boot } from "../lifecycle/boot";

describe("when dom-ready chains into custom-event", () => {
  it("completes a three-hop chain: dom-ready → custom-event → custom-event", () => {
    const chain: string[] = [];
    document.addEventListener("hop-1", () => chain.push("hop-1"));
    document.addEventListener("hop-2", () => chain.push("hop-2"));
    document.addEventListener("hop-3", () => chain.push("hop-3"));

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "hop-1" }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "hop-1" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "hop-2" }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "hop-2" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "hop-3" }],
          },
        },
      ],
    });

    expect(chain).toEqual(["hop-1", "hop-2", "hop-3"]);
  });

  it("wires custom-event listeners before executing dom-ready", () => {
    let received = false;
    document.addEventListener("from-dom-ready", () => { received = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        // dom-ready is first in array, but should execute AFTER custom-event is wired
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "catch-me" }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "catch-me" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "from-dom-ready" }],
          },
        },
      ],
    });

    expect(received).toBe(true);
  });
});
