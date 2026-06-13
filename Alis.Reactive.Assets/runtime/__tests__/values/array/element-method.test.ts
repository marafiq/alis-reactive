import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../../values/evaluate";
import type { PlanDocument, Shape, ValueExpression } from "../../../types/index";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const rawShape: Shape = { kind: "raw" };
const elementSource = { kind: "payload", scope: "element" } as const;

type ElementMethodOptions = {
  method: string;
  shape: Shape;
  ownerPath?: string;
  args?: ValueExpression[];
};

function emptyArrayPlan(): PlanDocument {
  return { version: 3, planId: "Runtime.ArrayDsl.ElementMethod", scope: { kind: "root" }, types: {}, components: {}, behaviors: [] };
}

function literal(value: unknown, shape: Shape): ValueExpression {
  return { kind: "literal", value, shape } as ValueExpression;
}

function elementMember(member: string, shape: Shape): ValueExpression {
  return { kind: "read", from: elementSource, member, path: [{ kind: "property", name: member }], shape, access: { kind: "property" } } as ValueExpression;
}

function elementMethod({ method, shape, ownerPath = "", args = [] }: ElementMethodOptions): ValueExpression {
  const full = ownerPath ? `${ownerPath}.${method}` : method;
  const path = full.split(".").map(name => ({ kind: "property", name }));
  return { kind: "read", from: elementSource, member: method, path, shape, access: { kind: "method", args } } as unknown as ValueExpression;
}

function mapOp(value: unknown, projection: ValueExpression, itemResultShape: Shape): ValueExpression {
  return {
    kind: "array-op", op: "map",
    source: { kind: "literal", value, shape: rawShape } as ValueExpression,
    projection, itemShape: rawShape, shape: { kind: "array", item: itemResultShape },
  } as ValueExpression;
}

describe("per-element method calls (RuntimePath.call at the element scope)", () => {
  it("calls an argless method on each element (Date.getDay via fn.apply)", () => {
    const dates = [new Date(2026, 8, 14), new Date(2026, 8, 16)];
    const node = mapOp(dates, elementMethod({ method: "getDay", shape: numberShape }), numberShape);
    expect(evaluateValue(node, emptyArrayPlan())).toEqual(dates.map(d => d.getDay()));
  });

  it("binds `this` to the element when calling a custom method", () => {
    const items: any[] = [
      { score: 7, double() { return this.score * 2; } },
      { score: 3, double() { return this.score * 2; } },
    ];
    const node = mapOp(items, elementMethod({ method: "double", shape: numberShape }), numberShape);
    expect(evaluateValue(node, emptyArrayPlan())).toEqual([14, 6]);
  });

  it("passes a constant argument to the method", () => {
    const items: any[] = [{ greeting: "hi", say(suffix: string) { return this.greeting + suffix; } }];
    const node = mapOp(items, elementMethod({ method: "say", shape: stringShape, args: [literal("!", stringShape)] }), stringShape);
    expect(evaluateValue(node, emptyArrayPlan())).toEqual(["hi!"]);
  });

  it("passes an element-member read as a method argument", () => {
    const items: any[] = [{ base: 10, bonus: 5, add(b: number) { return this.base + b; } }];
    const node = mapOp(items, elementMethod({ method: "add", shape: numberShape, args: [elementMember("bonus", numberShape)] }), numberShape);
    expect(evaluateValue(node, emptyArrayPlan())).toEqual([15]);
  });

  it("calls a method on a nested owner, binding `this` to that owner", () => {
    const items: any[] = [{ address: { city: "NYC", getCity() { return this.city; } } }];
    const node = mapOp(items, elementMethod({ ownerPath: "address", method: "getCity", shape: stringShape }), stringShape);
    expect(evaluateValue(node, emptyArrayPlan())).toEqual(["NYC"]);
  });

  it("surfaces a boundary error when the named member is not a function", () => {
    const items: any[] = [{ notAFn: 42 }];
    const node = mapOp(items, elementMethod({ method: "notAFn", shape: numberShape }), numberShape);
    expect(() => evaluateValue(node, emptyArrayPlan())).toThrow(/not a function/);
  });
});
