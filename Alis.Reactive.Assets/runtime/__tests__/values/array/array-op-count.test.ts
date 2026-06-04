import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../../values/evaluate";
import type { PlanDocument, Shape, ValueExpression } from "../../../types/index";

const rawShape: Shape = { kind: "raw" };
const numberShape: Shape = { kind: "number" };

function emptyArrayPlan(): PlanDocument {
  return {
    version: 3,
    planId: "Runtime.ArrayDsl.Count",
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

function count(value: unknown): ValueExpression {
  return {
    kind: "array-op",
    op: "count",
    source: { kind: "literal", value, shape: rawShape } as ValueExpression,
    itemShape: rawShape,
    shape: numberShape,
  };
}

describe("array-op count + array-like normalization", () => {
  it("counts a true JS array", () => {
    expect(evaluateValue(count([{ id: 1 }, { id: 2 }, { id: 3 }]), emptyArrayPlan())).toBe(3);
  });

  it("normalizes a DOMTokenList (classList) via Array.from before counting", () => {
    const hostElement = document.createElement("div");
    hostElement.className = "risk-fall risk-oxygen care-memory";
    expect(evaluateValue(count(hostElement.classList), emptyArrayPlan())).toBe(3);
  });

  it("normalizes a generic iterable (Set) via Array.from", () => {
    expect(evaluateValue(count(new Set(["a", "b"])), emptyArrayPlan())).toBe(2);
  });

  it("treats null as an empty array (e.g. EJ2 multiSelect.value when nothing selected)", () => {
    expect(evaluateValue(count(null), emptyArrayPlan())).toBe(0);
  });

  it("wraps a scalar as a singleton (e.g. ChipList single-select returns a number)", () => {
    expect(evaluateValue(count(2), emptyArrayPlan())).toBe(1);
  });

  it("throws a fail-fast boundary error for a non-iterable object (e.g. dataset/DOMStringMap)", () => {
    expect(() => evaluateValue(count({ flag: "x" }), emptyArrayPlan())).toThrow(/not iterable/);
  });
});
