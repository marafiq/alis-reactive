import { describe, it, expect } from "vitest";
import { boot } from "../lifecycle/boot";

describe("when triggering on dom-ready", () => {
  it("executes commands immediately when document is ready", () => {
    let executed = false;
    document.addEventListener("ready-evt", () => { executed = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "dispatch", event: "ready-evt" }],
        },
      }],
    });

    expect(executed).toBe(true);
  });
});
