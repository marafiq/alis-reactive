import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../../values/evaluate";
import type { PayloadSource, PlanDocument, Shape, ValidationCondition, ValueExpression } from "../../../types/index";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const noneShape: Shape = { kind: "none" };
const rawShape: Shape = { kind: "raw" };
const elementSource: PayloadSource = { kind: "payload", scope: "element" };

const residents = [
  { name: "Ada", status: "active", age: 71, balance: 120 },
  { name: "Bo", status: "discharged", age: 64, balance: 50 },
  { name: "Cy", status: "active", age: 80, balance: 200 },
];

function emptyArrayPlan(): PlanDocument {
  return { version: 3, planId: "Runtime.ArrayDsl.Ops", scope: { kind: "root" }, types: {}, components: {}, behaviors: [] };
}

function source(): ValueExpression {
  return { kind: "literal", value: residents, shape: rawShape } as ValueExpression;
}

function memberRead(member: string, shape: Shape): ValueExpression {
  return {
    kind: "read",
    from: elementSource,
    member,
    path: [{ kind: "property", name: member }],
    shape,
    access: { kind: "property" },
  };
}

function compare(member: string, op: string, value: unknown, shape: Shape): ValidationCondition {
  return {
    kind: "compare",
    left: memberRead(member, shape),
    op,
    right: { kind: "value", value: { kind: "literal", value, shape } },
    shape,
    itemShape: noneShape,
  } as unknown as ValidationCondition;
}

function arrayOp(node: Partial<ValueExpression> & { op: string }): ValueExpression {
  return { kind: "array-op", source: source(), itemShape: rawShape, ...node } as ValueExpression;
}

describe("array-op ops over an object array (element-member props)", () => {
  it("map projects each element member", () => {
    const node = arrayOp({ op: "map", projection: memberRead("name", stringShape), shape: { kind: "array", item: stringShape } });
    expect(evaluateValue(node, emptyArrayPlan())).toEqual(["Ada", "Bo", "Cy"]);
  });

  it("sum totals a numeric element member", () => {
    const node = arrayOp({ op: "sum", projection: memberRead("balance", numberShape), shape: numberShape });
    expect(evaluateValue(node, emptyArrayPlan())).toBe(370);
  });

  it("counts a filtered array (filter -> count — the shape Count(predicate) emits)", () => {
    const filtered = arrayOp({ op: "filter", predicate: compare("status", "eq", "active", stringShape), shape: { kind: "array", item: rawShape } });
    const node: ValueExpression = { kind: "array-op", op: "count", source: filtered, itemShape: rawShape, shape: numberShape };
    expect(evaluateValue(node, emptyArrayPlan())).toBe(2);
  });

  it("any without a predicate is a non-empty check (true here)", () => {
    expect(evaluateValue(arrayOp({ op: "any", shape: { kind: "boolean" } }), emptyArrayPlan())).toBe(true);
  });

  it("find returns the whole matching element when no projection is given", () => {
    const node = arrayOp({ op: "find", predicate: compare("status", "eq", "active", stringShape), shape: rawShape });
    expect(evaluateValue(node, emptyArrayPlan())).toEqual({ name: "Ada", status: "active", age: 71, balance: 120 });
  });

  it("orderBy places elements with a missing (NaN) key last, deterministically", () => {
    const withMissing = [{ name: "A", age: 30 }, { name: "B" }, { name: "C", age: 10 }];
    const node: ValueExpression = {
      kind: "array-op", op: "orderBy",
      source: { kind: "literal", value: withMissing, shape: rawShape } as ValueExpression,
      projection: memberRead("age", numberShape), itemShape: rawShape, shape: { kind: "array", item: rawShape },
    };
    const ordered = evaluateValue(node, emptyArrayPlan()) as Array<{ name: string }>;
    expect(ordered.map(r => r.name)).toEqual(["C", "A", "B"]);
  });

  it("orderBy is stable for equal keys (preserves input order)", () => {
    const ties = [{ name: "A", age: 50 }, { name: "B", age: 50 }, { name: "C", age: 50 }];
    const node: ValueExpression = {
      kind: "array-op", op: "orderBy",
      source: { kind: "literal", value: ties, shape: rawShape } as ValueExpression,
      projection: memberRead("age", numberShape), itemShape: rawShape, shape: { kind: "array", item: rawShape },
    };
    const ordered = evaluateValue(node, emptyArrayPlan()) as Array<{ name: string }>;
    expect(ordered.map(r => r.name)).toEqual(["A", "B", "C"]);
  });

  it("any returns true when an element matches", () => {
    const node = arrayOp({ op: "any", predicate: compare("age", "gt", 75, numberShape), shape: { kind: "boolean" } });
    expect(evaluateValue(node, emptyArrayPlan())).toBe(true);
  });

  it("all returns true only when every element matches", () => {
    const over60 = arrayOp({ op: "all", predicate: compare("age", "gt", 60, numberShape), shape: { kind: "boolean" } });
    const over70 = arrayOp({ op: "all", predicate: compare("age", "gt", 70, numberShape), shape: { kind: "boolean" } });
    expect(evaluateValue(over60, emptyArrayPlan())).toBe(true);
    expect(evaluateValue(over70, emptyArrayPlan())).toBe(false);
  });

  it("find returns the first matching element, projected", () => {
    const node = arrayOp({
      op: "find",
      predicate: compare("status", "eq", "active", stringShape),
      projection: memberRead("name", stringShape),
      shape: stringShape,
    });
    expect(evaluateValue(node, emptyArrayPlan())).toBe("Ada");
  });

  it("find returns null when nothing matches", () => {
    const node = arrayOp({ op: "find", predicate: compare("status", "eq", "transferred", stringShape), shape: rawShape });
    expect(evaluateValue(node, emptyArrayPlan())).toBeNull();
  });

  it("orderBy sorts ascending by a key projection", () => {
    const node = arrayOp({ op: "orderBy", projection: memberRead("age", numberShape), shape: { kind: "array", item: rawShape } });
    const ordered = evaluateValue(node, emptyArrayPlan()) as Array<{ name: string }>;
    expect(ordered.map(r => r.name)).toEqual(["Bo", "Ada", "Cy"]);
  });

  it("orderByDescending sorts descending by a key projection", () => {
    const node = arrayOp({ op: "orderByDescending", projection: memberRead("balance", numberShape), shape: { kind: "array", item: rawShape } });
    const ordered = evaluateValue(node, emptyArrayPlan()) as Array<{ name: string }>;
    expect(ordered.map(r => r.name)).toEqual(["Cy", "Ada", "Bo"]);
  });

  it("chains filter -> sum (active residents' balances)", () => {
    const filtered = arrayOp({
      op: "filter",
      predicate: compare("status", "eq", "active", stringShape),
      shape: { kind: "array", item: rawShape },
    });
    const node: ValueExpression = {
      kind: "array-op",
      op: "sum",
      source: filtered,
      projection: memberRead("balance", numberShape),
      itemShape: rawShape,
      shape: numberShape,
    };
    const activeBalanceTotal = evaluateValue(node, emptyArrayPlan());
    expect(activeBalanceTotal).toBe(320);
  });
});

describe("array-op ops over an empty source", () => {
  const empty: ValueExpression = { kind: "literal", value: [], shape: rawShape } as ValueExpression;
  const op = (node: Partial<ValueExpression> & { op: string }): ValueExpression =>
    ({ kind: "array-op", source: empty, itemShape: rawShape, ...node } as ValueExpression);
  const pred = compare("status", "eq", "x", stringShape);

  it("count is 0", () => expect(evaluateValue(op({ op: "count", shape: numberShape }), emptyArrayPlan())).toBe(0));
  it("filter is []", () => expect(evaluateValue(op({ op: "filter", predicate: pred, shape: { kind: "array", item: rawShape } }), emptyArrayPlan())).toEqual([]));
  it("map is []", () => expect(evaluateValue(op({ op: "map", projection: memberRead("name", stringShape), shape: { kind: "array", item: stringShape } }), emptyArrayPlan())).toEqual([]));
  it("sum is 0", () => expect(evaluateValue(op({ op: "sum", projection: memberRead("balance", numberShape), shape: numberShape }), emptyArrayPlan())).toBe(0));
  it("any(predicate) is false", () => expect(evaluateValue(op({ op: "any", predicate: pred, shape: { kind: "boolean" } }), emptyArrayPlan())).toBe(false));
  it("any() is false", () => expect(evaluateValue(op({ op: "any", shape: { kind: "boolean" } }), emptyArrayPlan())).toBe(false));
  it("all(predicate) is vacuously true like Array.every", () => expect(evaluateValue(op({ op: "all", predicate: pred, shape: { kind: "boolean" } }), emptyArrayPlan())).toBe(true));
  it("find is null", () => expect(evaluateValue(op({ op: "find", predicate: pred, shape: rawShape }), emptyArrayPlan())).toBeNull());
  it("orderBy is []", () => expect(evaluateValue(op({ op: "orderBy", projection: memberRead("age", numberShape), shape: { kind: "array", item: rawShape } }), emptyArrayPlan())).toEqual([]));
});
