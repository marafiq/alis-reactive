import { describe, it, expect } from "vitest";
import { firstMutationTarget } from "../execution/retry-indicator";
import type { Reaction } from "../types";

describe("when extracting mutation targets from reactions", () => {
  it("returns the first mutate-element target from a sequential reaction", () => {
    const reaction: Reaction = {
      kind: "sequential",
      commands: [
        { kind: "dispatch", event: "test" },
        { kind: "mutate-element", target: "notif-count", mutation: { kind: "set-prop", prop: "textContent" } },
        { kind: "mutate-element", target: "notif-message", mutation: { kind: "set-prop", prop: "textContent" } },
      ],
    };
    expect(firstMutationTarget(reaction)).toBe("notif-count");
  });

  it("returns the first mutate-element target from a conditional reaction", () => {
    const reaction: Reaction = {
      kind: "conditional",
      commands: [
        { kind: "mutate-element", target: "status", mutation: { kind: "set-prop", prop: "textContent" } },
      ],
      branches: [],
    };
    expect(firstMutationTarget(reaction)).toBe("status");
  });

  it("returns undefined when sequential reaction has no mutate-element commands", () => {
    const reaction: Reaction = {
      kind: "sequential",
      commands: [{ kind: "dispatch", event: "test" }],
    };
    expect(firstMutationTarget(reaction)).toBeUndefined();
  });

  it("returns undefined for http reactions", () => {
    const reaction: Reaction = {
      kind: "http",
      request: { verb: "GET", url: "/api/data" },
    };
    expect(firstMutationTarget(reaction)).toBeUndefined();
  });

  it("returns undefined for parallel-http reactions", () => {
    const reaction: Reaction = {
      kind: "parallel-http",
      requests: [
        { verb: "GET", url: "/api/a" },
        { verb: "GET", url: "/api/b" },
      ],
    };
    expect(firstMutationTarget(reaction)).toBeUndefined();
  });

  it("returns undefined when conditional reaction has no top-level commands", () => {
    const reaction: Reaction = {
      kind: "conditional",
      branches: [{ guard: null, reaction: { kind: "sequential", commands: [] } }],
    };
    expect(firstMutationTarget(reaction)).toBeUndefined();
  });
});
