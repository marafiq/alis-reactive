import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../core/evaluate";
import type { PayloadSource, PlanDocument, Shape, ValidationCondition, ValueExpression } from "../../types";

const stringShape: Shape = { kind: "string" };
const noneShape: Shape = { kind: "none" };
const rawShape: Shape = { kind: "raw" };
const elementSource: PayloadSource = { kind: "payload", scope: "element", type: { kind: "untyped" } };

function plan(): PlanDocument {
  return {
    version: 3,
    planId: "Runtime.ArrayDsl.Filter",
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

function elementMemberEquals(member: string, expected: string): ValidationCondition {
  return {
    kind: "compare",
    left: {
      kind: "read",
      from: elementSource,
      member,
      path: [{ kind: "property", name: member }],
      shape: stringShape,
      access: { kind: "property" },
    },
    op: "eq",
    right: { kind: "value", value: { kind: "literal", value: expected, shape: stringShape } },
    shape: stringShape,
    itemShape: noneShape,
  };
}

function elementSelfNotEquals(excluded: string): ValidationCondition {
  return {
    kind: "compare",
    left: {
      kind: "read",
      from: elementSource,
      member: "elementValue",
      path: [],
      shape: stringShape,
      access: { kind: "property" },
    },
    op: "neq",
    right: { kind: "value", value: { kind: "literal", value: excluded, shape: stringShape } },
    shape: stringShape,
    itemShape: noneShape,
  };
}

function filter(value: unknown, predicate: ValidationCondition, itemShape: Shape = rawShape): ValueExpression {
  return {
    kind: "array-op",
    op: "filter",
    source: { kind: "literal", value, shape: rawShape } as ValueExpression,
    predicate,
    itemShape,
    shape: { kind: "array", item: itemShape },
  };
}

describe("array-op filter (per-element sync predicate via the DI sync-condition leaf)", () => {
  it("filters object elements by an element-member predicate", () => {
    const residents = [
      { id: 1, status: "active" },
      { id: 2, status: "discharged" },
      { id: 3, status: "active" },
    ];

    expect(evaluateValue(filter(residents, elementMemberEquals("status", "active")), plan())).toEqual([
      { id: 1, status: "active" },
      { id: 3, status: "active" },
    ]);
  });

  it("filters primitive (string[]) elements via the x => x element-self read", () => {
    const tags = ["fall-risk", "none", "memory-care"];

    expect(evaluateValue(filter(tags, elementSelfNotEquals("none"), stringShape), plan())).toEqual([
      "fall-risk",
      "memory-care",
    ]);
  });

  it("filters a DOMTokenList after array-like normalization", () => {
    const el = document.createElement("div");
    el.className = "risk-fall care-memory plain";
    // keep only the tokens equal to themselves that are not "plain"
    expect(evaluateValue(filter(el.classList, elementSelfNotEquals("plain"), stringShape), plan())).toEqual([
      "risk-fall",
      "care-memory",
    ]);
  });
});
