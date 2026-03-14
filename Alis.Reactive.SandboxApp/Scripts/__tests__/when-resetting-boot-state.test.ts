import { describe, expect, it } from "vitest";
import { boot, resetBootStateForTests } from "../boot";

describe("when resetting boot state", () => {
  it("removes previously wired custom-event listeners between tests", () => {
    document.body.innerHTML = '<div id="status"></div>';

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "custom-event", event: "stale-event" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "mutate-element",
            target: "status",
            mutation: { kind: "set-prop", prop: "textContent" },
            value: "stale",
          }],
        },
      }],
    });

    resetBootStateForTests();
    document.body.innerHTML = "";

    document.dispatchEvent(new CustomEvent("stale-event", { detail: {} }));

    expect(document.body.innerHTML).toBe("");
  });
});
