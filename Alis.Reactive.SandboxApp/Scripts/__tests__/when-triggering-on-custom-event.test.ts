import { describe, it, expect } from "vitest";
import { boot } from "../boot";

describe("when triggering on custom event", () => {
  it("does not execute until the event fires", () => {
    let executed = false;
    document.addEventListener("output-1", () => { executed = true; });

    boot({
      entries: [{
        trigger: { kind: "custom-event", event: "input-1" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "dispatch", event: "output-1" }],
        },
      }],
    });

    expect(executed).toBe(false);
  });

  it("executes reaction when the subscribed event fires", () => {
    let executed = false;
    document.addEventListener("output-2", () => { executed = true; });

    boot({
      entries: [{
        trigger: { kind: "custom-event", event: "input-2" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "dispatch", event: "output-2" }],
        },
      }],
    });

    document.dispatchEvent(new CustomEvent("input-2"));
    expect(executed).toBe(true);
  });

  it("supports chaining: dispatch triggers another subscriber", () => {
    const chain: string[] = [];
    document.addEventListener("chain-mid", () => chain.push("mid"));
    document.addEventListener("chain-end", () => chain.push("end"));

    boot({
      entries: [
        {
          trigger: { kind: "custom-event", event: "chain-start" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "chain-mid" }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "chain-mid" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "chain-end" }],
          },
        },
      ],
    });

    expect(chain).toEqual([]);
    document.dispatchEvent(new CustomEvent("chain-start"));
    expect(chain).toEqual(["mid", "end"]);
  });
});
