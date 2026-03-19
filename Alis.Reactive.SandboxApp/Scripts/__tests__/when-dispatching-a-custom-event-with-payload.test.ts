import { describe, it, expect } from "vitest";
import { boot } from "../lifecycle/boot";

describe("when dispatching a custom event with payload", () => {
  it("delivers payload containing all supported primitive types", () => {
    let detail: unknown = null;
    document.addEventListener("typed-payload", (e) => {
      detail = (e as CustomEvent).detail;
    });

    const payload = {
      intValue: 42,
      longValue: 9007199254740991,
      doubleValue: 3.14159,
      floatValue: 2.718,
      stringValue: "hello",
      boolValue: true,
      dateTimeValue: "2026-03-08T14:30:00Z",
      dateValue: "2026-03-08",
    };

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{
            kind: "dispatch",
            event: "typed-payload",
            payload,
          }],
        },
      }],
    });

    expect(detail).toEqual(payload);
  });

  it("preserves integer precision", () => {
    let detail: unknown = null;
    document.addEventListener("int-precision", (e) => {
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
            event: "int-precision",
            payload: { value: 2147483647 },
          }],
        },
      }],
    });

    expect((detail as any).value).toBe(2147483647);
  });

  it("preserves boolean values", () => {
    let detail: unknown = null;
    document.addEventListener("bool-payload", (e) => {
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
            event: "bool-payload",
            payload: { active: true, deleted: false },
          }],
        },
      }],
    });

    expect((detail as any).active).toBe(true);
    expect((detail as any).deleted).toBe(false);
  });

  it("passes payload through a custom-event chain", () => {
    let finalDetail: unknown = null;
    document.addEventListener("chain-end-typed", (e) => {
      finalDetail = (e as CustomEvent).detail;
    });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "dispatch",
              event: "chain-start-typed",
              payload: { step: 1 },
            }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "chain-start-typed" },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "dispatch",
              event: "chain-end-typed",
              payload: { step: 2, origin: "chain" },
            }],
          },
        },
      ],
    });

    expect(finalDetail).toEqual({ step: 2, origin: "chain" });
  });
});
