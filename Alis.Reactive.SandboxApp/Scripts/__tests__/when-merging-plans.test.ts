import { describe, it, expect, beforeEach } from "vitest";
import { boot, mergePlan } from "../boot";
import type { Plan } from "../types";

describe("when merging plans", () => {
  beforeEach(() => {
    document.body.innerHTML = "";
  });

  function parentPlan(): Plan {
    return {
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "parent-ready" }],
          },
        },
      ],
    };
  }

  function partialPlan(): Plan {
    return {
      planId: "Test.Model",
      sourceId: "address-container",
      components: {},
      entries: [
        {
          trigger: { kind: "custom-event", event: "load-partial" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "partial-loaded" }],
          },
        },
      ],
    };
  }

  it("does not accumulate entries on partial reload", () => {
    boot(parentPlan());
    mergePlan(partialPlan());
    mergePlan(partialPlan());
    mergePlan(partialPlan());

    let fireCount = 0;
    document.addEventListener("partial-loaded", () => { fireCount++; });
    document.dispatchEvent(new CustomEvent("load-partial"));

    // Only the latest merge's listener should survive — fires once, not 3 times
    expect(fireCount).toBe(1);
  });

  it("does not duplicate custom-event listeners on reload", () => {
    boot(parentPlan());

    // Merge the same partial twice
    mergePlan(partialPlan());
    mergePlan(partialPlan());

    let fireCount = 0;
    document.addEventListener("partial-loaded", () => { fireCount++; });
    document.dispatchEvent(new CustomEvent("load-partial"));

    // Should fire once, not twice
    expect(fireCount).toBe(1);
  });
});
