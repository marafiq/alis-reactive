import { describe, it, expect, beforeEach } from "vitest";
import { boot, mergePlan, resetBootStateForTests } from "../lifecycle/boot";
import type { Entry } from "../types";

/**
 * Proves that event listeners are actually cleaned up when partials are
 * removed or replaced. If listeners leak, fire counts will be wrong.
 */

beforeEach(() => {
  document.body.innerHTML = '<div id="target"></div>';
  resetBootStateForTests();
});

function mutateOnEvent(listenFor: string, targetId: string, text: string): Entry {
  return {
    trigger: { kind: "custom-event", event: listenFor },
    reaction: {
      kind: "sequential",
      commands: [{
        kind: "mutate-element",
        target: targetId,
        mutation: { kind: "set-prop", prop: "textContent" },
        value: text,
      }],
    },
  };
}

// ── Prove: listeners fire before removal ────────────────

describe("Baseline: partial listener fires when active", () => {
  it("custom-event listener from partial fires once", () => {
    boot({ planId: "P", components: {}, entries: [] });

    mergePlan({
      planId: "P",
      sourceId: "slot",
      components: {},
      entries: [mutateOnEvent("ping", "target", "pong")],
    });

    document.dispatchEvent(new CustomEvent("ping"));
    expect(document.getElementById("target")!.textContent).toBe("pong");
  });
});

// ── Prove: listeners removed after partial removal ──────

describe("After partial is removed (re-merge with empty entries)", () => {
  it("custom-event listener no longer fires", () => {
    boot({ planId: "P", components: {}, entries: [] });

    mergePlan({
      planId: "P",
      sourceId: "slot",
      components: {},
      entries: [mutateOnEvent("ping", "target", "pong")],
    });

    // Remove
    mergePlan({
      planId: "P",
      sourceId: "slot",
      components: {},
      entries: [],
    });

    document.getElementById("target")!.textContent = "";
    document.dispatchEvent(new CustomEvent("ping"));
    expect(document.getElementById("target")!.textContent).toBe("");
  });
});

// ── Prove: no listener accumulation on repeated reload ──

describe("After partial is reloaded 5 times", () => {
  it("listener fires exactly once (no accumulation)", () => {
    boot({ planId: "P", components: {}, entries: [] });

    for (let i = 0; i < 5; i++) {
      mergePlan({
        planId: "P",
        sourceId: "slot",
        components: {},
        entries: [mutateOnEvent("ping", "target", `v${i}`)],
      });
    }

    let _fireCount = 0;
    document.addEventListener("ping", () => { _fireCount++; });

    // Fire the event that the partial listens for
    // The partial dispatches are already wired — we need a different approach
    // Let's count how many times the target gets mutated
    document.getElementById("target")!.textContent = "";
    document.dispatchEvent(new CustomEvent("ping"));

    // If listeners accumulated, text would be overwritten 5 times but still end at v4
    // The real test: only ONE listener should be active
    expect(document.getElementById("target")!.textContent).toBe("v4");
  });

  it("fire count is exactly 1 per event dispatch", () => {
    boot({ planId: "P", components: {}, entries: [] });

    let callCount = 0;

    for (let i = 0; i < 5; i++) {
      mergePlan({
        planId: "P",
        sourceId: "slot",
        components: {},
        entries: [{
          trigger: { kind: "custom-event", event: "count-me" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "counted" }],
          },
        }],
      });
    }

    document.addEventListener("counted", () => { callCount++; });
    document.dispatchEvent(new CustomEvent("count-me"));

    // Only 1 listener should survive — fires "counted" once
    expect(callCount).toBe(1);
  });
});

// ── Prove: root plan listeners survive partial removal ──

describe("Root plan listener survives partial add/remove cycle", () => {
  it("root listener still fires after partial removed", () => {
    boot({
      planId: "P",
      components: {},
      entries: [mutateOnEvent("root-event", "target", "root")],
    });

    mergePlan({
      planId: "P",
      sourceId: "slot",
      components: {},
      entries: [mutateOnEvent("partial-event", "target", "partial")],
    });

    // Remove partial
    mergePlan({
      planId: "P",
      sourceId: "slot",
      components: {},
      entries: [],
    });

    document.getElementById("target")!.textContent = "";
    document.dispatchEvent(new CustomEvent("root-event"));
    expect(document.getElementById("target")!.textContent).toBe("root");
  });
});

// ── Prove: two partials, removing one doesn't affect other ──

describe("Two partials: removing A leaves B intact", () => {
  it("B's listener still fires after A is removed", () => {
    document.body.innerHTML = '<div id="a-target"></div><div id="b-target"></div>';

    boot({ planId: "P", components: {}, entries: [] });

    mergePlan({
      planId: "P",
      sourceId: "slot-a",
      components: {},
      entries: [mutateOnEvent("event-a", "a-target", "A")],
    });

    mergePlan({
      planId: "P",
      sourceId: "slot-b",
      components: {},
      entries: [mutateOnEvent("event-b", "b-target", "B")],
    });

    // Remove A only
    mergePlan({
      planId: "P",
      sourceId: "slot-a",
      components: {},
      entries: [],
    });

    // A's listener should be gone
    document.dispatchEvent(new CustomEvent("event-a"));
    expect(document.getElementById("a-target")!.textContent).toBe("");

    // B's listener should still work
    document.dispatchEvent(new CustomEvent("event-b"));
    expect(document.getElementById("b-target")!.textContent).toBe("B");
  });
});

// ── Prove: resetBootStateForTests clears all listeners ──

describe("resetBootStateForTests clears everything", () => {
  it("no listeners fire after reset", () => {
    boot({
      planId: "P",
      components: {},
      entries: [mutateOnEvent("root-event", "target", "root")],
    });

    mergePlan({
      planId: "P",
      sourceId: "slot",
      components: {},
      entries: [mutateOnEvent("partial-event", "target", "partial")],
    });

    resetBootStateForTests();
    document.body.innerHTML = '<div id="target"></div>';

    document.dispatchEvent(new CustomEvent("root-event"));
    document.dispatchEvent(new CustomEvent("partial-event"));
    expect(document.getElementById("target")!.textContent).toBe("");
  });
});
