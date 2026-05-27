import { afterEach, describe, expect, it, vi } from "vitest";
import {
  evaluateCondition,
  evaluateConditionAsync,
  evaluateConditionInCurrentLane,
} from "../conditions/conditions";
import type {
  CollectionItemCompareCondition,
  Condition,
  EqualityCompareCondition,
  EqualityCompareOp,
  ExecContext,
  JsonValue,
  LiteralProducer,
  MembershipCompareCondition,
  MembershipCompareOp,
  OrderedCompareCondition,
  OrderedCompareOp,
  PayloadPathReadProducer,
  PlanDocument,
  RangeCompareCondition,
  RangeComparisonProducer,
  RegexCompareCondition,
  Shape,
  NumericLiteralProducer,
  TextLiteralProducer,
  TextCompareCondition,
  TextCompareOp,
  TextLengthCompareCondition,
  UnaryCompareCondition,
  UnaryCompareOp,
  ValueProducer,
  ValidationCondition,
} from "../types";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const booleanShape: Shape = { kind: "boolean" };
const dateShape: Shape = { kind: "date" };
const rawShape: Shape = { kind: "raw" };
const noneShape: Shape = { kind: "none" };
const stringArrayShape: Shape = { kind: "array", item: stringShape };

type BrowserWindowWithConfirm = typeof window & {
  alis?: { confirm?: (message: string) => boolean | Promise<boolean> };
};

function plan(): PlanDocument {
  return {
    version: 3,
    planId: "Condition.Runtime",
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [],
  };
}

function literal(value: JsonValue, shape: Shape): LiteralProducer {
  return { kind: "literal", value, shape };
}

function textLiteral(value: string): TextLiteralProducer {
  return { kind: "literal", value, shape: stringShape };
}

function numericLiteral(value: number): NumericLiteralProducer {
  return { kind: "literal", value, shape: numberShape };
}

function eventPayloadValue(member: string, shape: Shape): PayloadPathReadProducer {
  return {
    kind: "read",
    from: { kind: "payload", scope: "event", type: { kind: "untyped" } },
    member,
    path: [{ kind: "property", name: member }],
    shape,
    access: { kind: "property" },
  };
}

function range(low: JsonValue, high: JsonValue, itemShape: Shape): RangeComparisonProducer {
  return {
    kind: "array",
    items: [literal(low, itemShape), literal(high, itemShape)],
    shape: { kind: "array", item: itemShape },
  };
}

function unary(op: UnaryCompareOp, left: JsonValue, shape: Shape = rawShape): UnaryCompareCondition {
  return {
    kind: "compare",
    left: literal(left, shape),
    op,
    right: { kind: "none" },
    shape,
    itemShape: noneShape,
  };
}

function equality(
  op: EqualityCompareOp,
  left: JsonValue,
  right: JsonValue,
  shape: Shape,
): EqualityCompareCondition {
  return equalityFromProducers(op, literal(left, shape), literal(right, shape), shape);
}

function equalityFromProducers(
  op: EqualityCompareOp,
  left: ValueProducer,
  right: ValueProducer,
  shape: Shape,
): EqualityCompareCondition {
  return {
    kind: "compare",
    left,
    op,
    right: { kind: "value", value: right },
    shape,
    itemShape: noneShape,
  };
}

function ordered(
  op: OrderedCompareOp,
  left: JsonValue,
  right: JsonValue,
  shape: Shape,
): OrderedCompareCondition {
  return orderedFromProducers(op, literal(left, shape), literal(right, shape), shape);
}

function orderedFromProducers(
  op: OrderedCompareOp,
  left: ValueProducer,
  right: ValueProducer,
  shape: Shape,
): OrderedCompareCondition {
  return {
    kind: "compare",
    left,
    op,
    right: { kind: "value", value: right },
    shape,
    itemShape: noneShape,
  };
}

function membership(
  op: MembershipCompareOp,
  left: JsonValue,
  values: JsonValue[],
  itemShape: Shape,
): MembershipCompareCondition {
  return {
    kind: "compare",
    left: literal(left, itemShape),
    op,
    right: {
      kind: "value",
      value: {
        kind: "array",
        items: values.map(value => literal(value, itemShape)),
        shape: { kind: "array", item: itemShape },
      },
    },
    shape: itemShape,
    itemShape,
  };
}

function between(left: JsonValue, low: JsonValue, high: JsonValue, itemShape: Shape): RangeCompareCondition {
  return {
    kind: "compare",
    left: literal(left, itemShape),
    op: "between",
    right: { kind: "value", value: range(low, high, itemShape) },
    shape: itemShape,
    itemShape,
  };
}

function text(op: TextCompareOp, left: JsonValue, right: string, shape: Shape = stringShape): TextCompareCondition {
  return {
    kind: "compare",
    left: literal(left, shape),
    op,
    right: { kind: "value", value: textLiteral(right) },
    shape,
    itemShape: noneShape,
  };
}

function regex(left: JsonValue, pattern: string, shape: Shape = stringShape): RegexCompareCondition {
  return {
    kind: "compare",
    left: literal(left, shape),
    op: "matches",
    right: { kind: "value", value: textLiteral(pattern) },
    shape,
    itemShape: noneShape,
  };
}

function minLength(left: JsonValue, minimumLength: number, shape: Shape = stringShape): TextLengthCompareCondition {
  return {
    kind: "compare",
    left: literal(left, shape),
    op: "min-length",
    right: { kind: "value", value: numericLiteral(minimumLength) },
    shape,
    itemShape: noneShape,
  };
}

function arrayContains(left: JsonValue[], right: JsonValue, itemShape: Shape): CollectionItemCompareCondition {
  return {
    kind: "compare",
    left: literal(left, { kind: "array", item: itemShape }),
    op: "array-contains",
    right: { kind: "value", value: literal(right, itemShape) },
    shape: { kind: "array", item: itemShape },
    itemShape,
  };
}

function matches(condition: ValidationCondition, ctx?: ExecContext): boolean {
  return evaluateCondition(condition, plan(), ctx);
}

afterEach(() => {
  delete (window as BrowserWindowWithConfirm).alis;
});

describe("condition runtime", () => {
  describe("unary comparisons", () => {
    it("evaluates null, empty, and truthy state from the shaped left operand", () => {
      expect(matches(unary("is-null", null))).toBe(true);
      expect(matches(unary("not-null", ""))).toBe(true);
      expect(matches(unary("is-empty", "", stringShape))).toBe(true);
      expect(matches(unary("is-empty", [], stringArrayShape))).toBe(true);
      expect(matches(unary("not-empty", ["Ada"], stringArrayShape))).toBe(true);
      expect(matches(unary("truthy", "yes", booleanShape))).toBe(true);
      expect(matches(unary("falsy", "", booleanShape))).toBe(true);
    });
  });

  describe("value comparisons", () => {
    it("applies the declared shape before equality", () => {
      expect(matches(equality("eq", "72", 72, numberShape))).toBe(true);
      expect(matches(equality("neq", "Ada", "Grace", stringShape))).toBe(true);
      expect(matches(equality("eq", "2026-07-01", "2026-07-01", dateShape))).toBe(true);
    });

    it("orders values inside a single comparable shape", () => {
      expect(matches(ordered("gt", "72", 60, numberShape))).toBe(true);
      expect(matches(ordered("gte", true, false, booleanShape))).toBe(true);
      expect(matches(ordered("lt", "alpha", "beta", stringShape))).toBe(true);
      expect(matches(ordered("gt", "2026-07-15", "2026-06-01", dateShape))).toBe(true);
    });

    it("treats malformed ordered values as non-matching behavior", () => {
      expect(matches(ordered("gt", "not-a-date", "2026-06-01", dateShape))).toBe(false);
      expect(matches(ordered("gte", "not-a-number", 0, numberShape))).toBe(false);
      expect(matches(ordered("gt", "72", 60, rawShape))).toBe(false);
    });

    it("compares value producers on both sides of source-to-source conditions", () => {
      const left = eventPayloadValue("entered", numberShape);
      const right = eventPayloadValue("expected", numberShape);
      const context = { event: { entered: "72", expected: 70 } };

      expect(matches(orderedFromProducers("gt", left, right, numberShape), context)).toBe(true);
      expect(matches(equalityFromProducers("eq", left, right, numberShape), context)).toBe(false);
    });
  });

  describe("collection comparisons", () => {
    it("matches membership against shaped collection operands", () => {
      expect(matches(membership("in", "72", [60, 72, 90], numberShape))).toBe(true);
      expect(matches(membership("not-in", "Ada", ["Grace", "Katherine"], stringShape))).toBe(true);
    });

    it("matches inclusive ranges through the declared item shape", () => {
      expect(matches(between("72", 60, 90, numberShape))).toBe(true);
      expect(matches(between("2026-07-15", "2026-06-01", "2026-08-01", dateShape))).toBe(true);
    });

    it("uses the declared item shape for array-contains", () => {
      expect(matches(arrayContains(["1", "2"], 2, numberShape))).toBe(true);
      expect(matches(arrayContains(["routine", "urgent"], "urgent", stringShape))).toBe(true);
    });
  });

  describe("text comparisons", () => {
    it("matches substring, prefix, suffix, regex, and minimum length", () => {
      expect(matches(text("contains", "resident-ready", "ready"))).toBe(true);
      expect(matches(text("starts-with", "resident-ready", "resident"))).toBe(true);
      expect(matches(text("ends-with", "resident-ready", "ready"))).toBe(true);
      expect(matches(regex("RN-204", "^RN-\\d+$"))).toBe(true);
      expect(matches(minLength("Grace", 5))).toBe(true);
    });

    it("treats missing text and invalid regex patterns as non-matching behavior", () => {
      expect(matches(text("contains", null, "ready"))).toBe(false);
      expect(matches(regex("RN-204", "["))).toBe(false);
    });
  });

  describe("logical composition", () => {
    const active = equality("eq", true, true, booleanShape);
    const inactive = equality("eq", false, true, booleanShape);

    it("evaluates all, any, and not using nested validation conditions", () => {
      expect(matches({ kind: "all", terms: [active, active] })).toBe(true);
      expect(matches({ kind: "all", terms: [active, inactive] })).toBe(false);
      expect(matches({ kind: "any", terms: [inactive, active] })).toBe(true);
      expect(matches({ kind: "not", term: inactive })).toBe(true);
    });
  });

  describe("confirm conditions", () => {
    it("executes confirm only when the current lane reaches an async condition", async () => {
      const confirm = vi.fn(async () => true);
      (window as BrowserWindowWithConfirm).alis = { confirm };
      const condition: Condition = {
        kind: "all",
        terms: [
          equality("eq", true, true, booleanShape),
          { kind: "confirm", message: "Continue?" },
        ],
      };

      const completion = evaluateConditionInCurrentLane(condition, plan());

      expect(completion).toBeInstanceOf(Promise);
      await expect(completion).resolves.toBe(true);
      expect(confirm).toHaveBeenCalledWith("Continue?");
    });

    it("stays synchronous when logical terms decide before confirm is reached", () => {
      const confirm = vi.fn(() => true);
      (window as BrowserWindowWithConfirm).alis = { confirm };
      const condition: Condition = {
        kind: "any",
        terms: [
          equality("eq", true, true, booleanShape),
          { kind: "confirm", message: "Continue?" },
        ],
      };

      const completion = evaluateConditionInCurrentLane(condition, plan());

      expect(completion).toBe(true);
      expect(confirm).not.toHaveBeenCalled();
    });

    it("runs confirm from the async condition API", async () => {
      const confirm = vi.fn(() => false);
      (window as BrowserWindowWithConfirm).alis = { confirm };

      await expect(evaluateConditionAsync({ kind: "confirm", message: "Delete?" }, plan()))
        .resolves.toBe(false);
      expect(confirm).toHaveBeenCalledWith("Delete?");
    });
  });
});
