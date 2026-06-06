import { afterEach, describe, expect, it, vi } from "vitest";
import { initNativeActionLinks } from "../components/native/native-action-link";
import { registerPlugin, resolvePlugin } from "../plugins/catalog";
import { executeReaction } from "../execution/reactions/execute";
import { boot, resetBootStateForTests } from "../lifecycle/boot";
import type { PlanDocument } from "../types/index";

function emptyPlan(planId: string): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

afterEach(() => {
  vi.restoreAllMocks();
  resetBootStateForTests();
  document.body.innerHTML = "";
});

describe("runtime boot state", () => {
  it("resets runtime singletons so native action links initialize on the next boot", () => {
    const addEventListener = vi.spyOn(document, "addEventListener");

    initNativeActionLinks();
    resetBootStateForTests();
    initNativeActionLinks();

    const documentClickRegistrations = addEventListener.mock.calls.filter(
      ([eventName]) => eventName === "click",
    );

    expect(documentClickRegistrations).toHaveLength(2);
  });

  it("clears the active execution plan during boot reset", () => {
    boot(emptyPlan("Runtime.ActivePlanReset"));

    resetBootStateForTests();

    expect(() => executeReaction({ kind: "sequence", steps: [] }))
      .toThrow("[alis] no active plan");
  });

  it("clears registered plugin instances during boot reset", () => {
    registerPlugin("slugify", (value: string): string => value.toLowerCase());

    resetBootStateForTests();

    expect(() => resolvePlugin("slugify")).toThrow("plugin not found");
  });
});
