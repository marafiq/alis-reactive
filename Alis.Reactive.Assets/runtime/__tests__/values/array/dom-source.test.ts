import { describe, expect, it } from "vitest";
import { evaluateValue } from "../../../values/evaluate";
import type { PlanDocument, Shape, ValidationCondition, ValueExpression } from "../../../types/index";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const noneShape: Shape = { kind: "none" };

function emptyArrayPlan(): PlanDocument {
  return { version: 3, planId: "Runtime.ArrayDsl.Dom", scope: { kind: "root" }, types: {}, components: {}, behaviors: [] };
}

function domRead(elementId: string, member: string): ValueExpression {
  return {
    kind: "read",
    from: { kind: "dom", element: elementId },
    member,
    path: [{ kind: "property", name: member }],
    shape: noneShape,
    access: { kind: "property" },
  } as ValueExpression;
}

function count(source: ValueExpression): ValueExpression {
  return { kind: "array-op", op: "count", source, itemShape: noneShape, shape: numberShape } as ValueExpression;
}

function filterByPrefix(source: ValueExpression, prefix: string): ValueExpression {
  return {
    kind: "array-op", op: "filter", source, predicate: selfStartsWith(prefix),
    itemShape: noneShape, shape: { kind: "array", item: stringShape },
  } as ValueExpression;
}

function selfStartsWith(prefix: string): ValidationCondition {
  return {
    kind: "compare",
    left: { kind: "read", from: { kind: "payload", scope: "element", type: { kind: "untyped" } }, member: "elementValue", path: [], shape: stringShape, access: { kind: "property" } },
    op: "starts-with",
    right: { kind: "value", value: { kind: "literal", value: prefix, shape: stringShape } },
    shape: stringShape,
    itemShape: noneShape,
  } as unknown as ValidationCondition;
}

describe("DOM element source — array ops over live DOM collections", () => {
  it("counts the CSS classes of a DOM element (classList DOMTokenList)", () => {
    document.body.innerHTML = `<div id="card" class="risk-fall care-memory plain"></div>`;
    expect(evaluateValue(count(domRead("card", "classList")), emptyArrayPlan())).toBe(3);
  });

  it("filters classList tokens by prefix and counts (filter -> count over DOM element members)", () => {
    document.body.innerHTML = `<div id="card" class="risk-fall risk-oxygen care-memory"></div>`;
    expect(evaluateValue(count(filterByPrefix(domRead("card", "classList"), "risk-")), emptyArrayPlan())).toBe(2);
  });

  it("counts child elements (children HTMLCollection)", () => {
    document.body.innerHTML = `<ul id="list"><li>a</li><li>b</li><li>c</li></ul>`;
    expect(evaluateValue(count(domRead("list", "children")), emptyArrayPlan())).toBe(3);
  });

  it("throws a clear boundary error when the DOM element is absent", () => {
    document.body.innerHTML = "";
    expect(() => evaluateValue(count(domRead("missing", "classList")), emptyArrayPlan())).toThrow(/dom source element "missing" not found/);
  });
});
