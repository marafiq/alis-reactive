import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../../values/evaluate";
import { ExecutionContext } from "../../../browser-objects/execution-context";
import type { PayloadSource, PlanDocument, ValueExpression } from "../../../types/index";

const elementSource: PayloadSource = {
  kind: "payload",
  scope: "element",
  type: { kind: "untyped" },
};

function plan(): PlanDocument {
  return {
    version: 3,
    planId: "Runtime.ArrayDsl.ElementScope",
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

describe("element scope", () => {
  it("resolves the top of the element stack and stays immutable across pushes", () => {
    const base = ExecutionContext.empty();
    const outer = base.withElement({ id: "outer" });
    const inner = outer.withElement({ id: "inner" });

    expect(inner.resolvePayload(elementSource)).toEqual({ id: "inner" });
    expect(outer.resolvePayload(elementSource)).toEqual({ id: "outer" });
    expect(base.resolvePayload(elementSource)).toBeUndefined();
  });

  it("reads a member of the current object element", () => {
    const read: ValueExpression = {
      kind: "read",
      from: elementSource,
      member: "status",
      path: [{ kind: "property", name: "status" }],
      shape: { kind: "string" },
      access: { kind: "property" },
    };

    const value = evaluateValue(read, plan(), {
      element: [{ status: "active", age: 71 }],
    });

    expect(value).toBe("active");
  });

  it("reads the element itself (x => x) for a primitive element via the elementValue sentinel", () => {
    const read: ValueExpression = {
      kind: "read",
      from: elementSource,
      member: "elementValue",
      path: [],
      shape: { kind: "string" },
      access: { kind: "property" },
    };

    expect(evaluateValue(read, plan(), { element: ["peanuts"] })).toBe("peanuts");
  });

  it("applies the declared shape to a primitive element read", () => {
    const read: ValueExpression = {
      kind: "read",
      from: elementSource,
      member: "elementValue",
      path: [],
      shape: { kind: "number" },
      access: { kind: "property" },
    };

    expect(evaluateValue(read, plan(), { element: ["42"] })).toBe(42);
  });
});
