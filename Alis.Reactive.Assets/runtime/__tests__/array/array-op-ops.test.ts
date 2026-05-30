import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../core/evaluate";
import type { PayloadSource, PlanDocument, Shape, ValidationCondition, ValueExpression } from "../../types";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const noneShape: Shape = { kind: "none" };
const rawShape: Shape = { kind: "raw" };
const elementSource: PayloadSource = { kind: "payload", scope: "element", type: { kind: "untyped" } };

const residents = [
  { name: "Ada", status: "active", age: 71, balance: 120 },
  { name: "Bo", status: "discharged", age: 64, balance: 50 },
  { name: "Cy", status: "active", age: 80, balance: 200 },
];

function plan(): PlanDocument {
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
    expect(evaluateValue(node, plan())).toEqual(["Ada", "Bo", "Cy"]);
  });

  it("sum totals a numeric element member", () => {
    const node = arrayOp({ op: "sum", projection: memberRead("balance", numberShape), shape: numberShape });
    expect(evaluateValue(node, plan())).toBe(370);
  });

  it("count with predicate counts matching elements", () => {
    const node = arrayOp({ op: "count", predicate: compare("status", "eq", "active", stringShape), shape: numberShape });
    expect(evaluateValue(node, plan())).toBe(2);
  });

  it("any returns true when an element matches", () => {
    const node = arrayOp({ op: "any", predicate: compare("age", "gt", 75, numberShape), shape: { kind: "boolean" } });
    expect(evaluateValue(node, plan())).toBe(true);
  });

  it("all returns true only when every element matches", () => {
    const over60 = arrayOp({ op: "all", predicate: compare("age", "gt", 60, numberShape), shape: { kind: "boolean" } });
    const over70 = arrayOp({ op: "all", predicate: compare("age", "gt", 70, numberShape), shape: { kind: "boolean" } });
    expect(evaluateValue(over60, plan())).toBe(true);
    expect(evaluateValue(over70, plan())).toBe(false);
  });

  it("find returns the first matching element, projected", () => {
    const node = arrayOp({
      op: "find",
      predicate: compare("status", "eq", "active", stringShape),
      projection: memberRead("name", stringShape),
      shape: stringShape,
    });
    expect(evaluateValue(node, plan())).toBe("Ada");
  });

  it("find returns null when nothing matches", () => {
    const node = arrayOp({ op: "find", predicate: compare("status", "eq", "transferred", stringShape), shape: rawShape });
    expect(evaluateValue(node, plan())).toBeNull();
  });

  it("orderBy sorts ascending by a key projection", () => {
    const node = arrayOp({ op: "orderBy", projection: memberRead("age", numberShape), shape: { kind: "array", item: rawShape } });
    const ordered = evaluateValue(node, plan()) as Array<{ name: string }>;
    expect(ordered.map(r => r.name)).toEqual(["Bo", "Ada", "Cy"]);
  });

  it("orderByDescending sorts descending by a key projection", () => {
    const node = arrayOp({ op: "orderByDescending", projection: memberRead("balance", numberShape), shape: { kind: "array", item: rawShape } });
    const ordered = evaluateValue(node, plan()) as Array<{ name: string }>;
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
    // Ada(120) + Cy(200) = 320
    expect(evaluateValue(node, plan())).toBe(320);
  });
});
