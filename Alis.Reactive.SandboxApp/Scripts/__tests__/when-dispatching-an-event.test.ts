import { describe, it, expect } from "vitest";
import { boot } from "../lifecycle/boot";


describe("when dispatching an event", () => {
  it("fires CustomEvent on document", () => {
    let received = false;
    document.addEventListener("evt-a", () => { received = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "dispatch", event: "evt-a" }],
        },
      }],
    });

    expect(received).toBe(true);
  });

  it("delivers payload as event detail", () => {
    let detail: unknown = null;
    document.addEventListener("evt-b", (e) => {
      detail = (e as CustomEvent).detail;
    });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "dispatch",
            event: "evt-b",
            payload: { id: "abc", count: 42 },
          }],
        },
      }],
    });

    expect(detail).toEqual({ id: "abc", count: 42 });
  });

  it("provides empty object when no payload given", () => {
    let detail: unknown = undefined;
    document.addEventListener("evt-c", (e) => {
      detail = (e as CustomEvent).detail;
    });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "dispatch", event: "evt-c" }],
        },
      }],
    });

    expect(detail).toEqual({});
  });

  it("executes multiple commands in order", () => {
    const order: string[] = [];
    document.addEventListener("evt-d1", () => order.push("first"));
    document.addEventListener("evt-d2", () => order.push("second"));

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [
            { kind: "dispatch", event: "evt-d1" },
            { kind: "dispatch", event: "evt-d2" },
          ],
        },
      }],
    });

    expect(order).toEqual(["first", "second"]);
  });
});
