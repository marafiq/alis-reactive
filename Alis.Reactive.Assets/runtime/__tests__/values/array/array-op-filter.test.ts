import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../../values/evaluate";
import type { PayloadSource, PlanDocument, Shape, ValidationCondition, ValueExpression } from "../../../types/index";

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

const numberShape: Shape = { kind: "number" };

function memberCompare(member: string, op: string, value: unknown, shape: Shape): ValidationCondition {
  return {
    kind: "compare",
    left: { kind: "read", from: elementSource, member, path: [{ kind: "property", name: member }], shape, access: { kind: "property" } },
    op,
    right: { kind: "value", value: { kind: "literal", value, shape } },
    shape,
    itemShape: noneShape,
  } as unknown as ValidationCondition;
}

const all = (...terms: ValidationCondition[]): ValidationCondition => ({ kind: "all", terms } as unknown as ValidationCondition);
const any = (...terms: ValidationCondition[]): ValidationCondition => ({ kind: "any", terms } as unknown as ValidationCondition);
const not = (term: ValidationCondition): ValidationCondition => ({ kind: "not", term } as unknown as ValidationCondition);

const compoundResidents = [
  { name: "Ada", status: "active", age: 71 },
  { name: "Bo", status: "discharged", age: 64 },
  { name: "Cy", status: "active", age: 25 },
];
const names = (v: unknown): string[] => (v as Array<{ name: string }>).map(r => r.name);

describe("array-op filter (per-element sync predicate via the DI compare-engine leaf)", () => {
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
    expect(evaluateValue(filter(el.classList, elementSelfNotEquals("plain"), stringShape), plan())).toEqual([
      "risk-fall",
      "care-memory",
    ]);
  });

  it("filters by a compound AND predicate (all node) threading element scope to each term", () => {
    const pred = all(memberCompare("status", "eq", "active", stringShape), memberCompare("age", "gte", 65, numberShape));
    expect(names(evaluateValue(filter(compoundResidents, pred), plan()))).toEqual(["Ada"]);
  });

  it("filters by a compound OR predicate (any node)", () => {
    const pred = any(memberCompare("status", "eq", "active", stringShape), memberCompare("age", "lt", 30, numberShape));
    expect(names(evaluateValue(filter(compoundResidents, pred), plan()))).toEqual(["Ada", "Cy"]);
  });

  it("filters by a negated predicate (not node)", () => {
    const pred = not(memberCompare("status", "eq", "discharged", stringShape));
    expect(names(evaluateValue(filter(compoundResidents, pred), plan()))).toEqual(["Ada", "Cy"]);
  });
});
