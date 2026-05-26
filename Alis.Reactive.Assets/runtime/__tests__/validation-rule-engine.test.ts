import { describe, expect, it } from "vitest";
import { ruleFails } from "../validation/rule-engine";
import type {
  LengthValidationRule,
  LiteralProducer,
  NoOperandValidationRule,
  NumericLiteralProducer,
  OrderedComparisonValidationRule,
  PeerEqualityValidationRule,
  PeerOrderedComparisonValidationRule,
  RangeValidationRule,
  RangeLiteralProducer,
  ReadProducer,
  Shape,
} from "../types";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const dateShape: Shape = { kind: "date" };
const noneShape: Shape = { kind: "none" };

function literal(value: string | number | boolean | null, shape: Shape = stringShape): LiteralProducer {
  return { kind: "literal", value, shape };
}

function numericLiteral(value: number): NumericLiteralProducer {
  return { kind: "literal", value, shape: numberShape };
}

function rangeLiteral(bounds: [number, number]): RangeLiteralProducer {
  return {
    kind: "literal",
    value: bounds,
    shape: { kind: "array", item: numberShape },
  };
}

function componentValue(component: string, shape: Shape): ReadProducer {
  return {
    kind: "read",
    from: { kind: "component", component },
    member: "value",
    path: [],
    shape,
    access: { kind: "property" },
  };
}

function noOperandRule(name: NoOperandValidationRule["name"]): NoOperandValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      target: "none",
      constraint: { kind: "none" },
      otherValue: { kind: "none" },
      activation: { kind: "always" },
      comparisonShape: noneShape,
    },
  };
}

function lengthRule(name: LengthValidationRule["name"], length: number): LengthValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      target: "constraint",
      constraint: { kind: "value", value: numericLiteral(length) },
      otherValue: { kind: "none" },
      activation: { kind: "always" },
      comparisonShape: noneShape,
    },
  };
}

function rangeRule(
  name: RangeValidationRule["name"],
  bounds: [number, number],
  comparisonShape: Shape,
): RangeValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      target: "constraint",
      constraint: { kind: "value", value: rangeLiteral(bounds) },
      otherValue: { kind: "none" },
      activation: { kind: "always" },
      comparisonShape,
    },
  };
}

function orderedRule(
  name: OrderedComparisonValidationRule["name"],
  value: string | number,
  valueShape: Shape,
  comparisonShape: Shape,
): OrderedComparisonValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      target: "constraint",
      constraint: { kind: "value", value: literal(value, valueShape) },
      otherValue: { kind: "none" },
      activation: { kind: "always" },
      comparisonShape,
    },
  };
}

function peerEqualityRule(name: PeerEqualityValidationRule["name"]): PeerEqualityValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      target: "peer",
      constraint: { kind: "none" },
      otherValue: {
        kind: "value",
        value: componentValue("confirmPassword", stringShape),
      },
      activation: { kind: "always" },
      comparisonShape: stringShape,
    },
  };
}

function peerOrderedRule(name: PeerOrderedComparisonValidationRule["name"]): PeerOrderedComparisonValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      target: "peer",
      constraint: { kind: "none" },
      otherValue: {
        kind: "value",
        value: componentValue("startDate", dateShape),
      },
      activation: { kind: "always" },
      comparisonShape: dateShape,
    },
  };
}

describe("validation rule engine", () => {
  it("treats missing, false, empty text, and empty arrays as empty validation subjects", () => {
    const required = noOperandRule("required");

    expect(ruleFails({ rule: required, value: undefined })).toBe(true);
    expect(ruleFails({ rule: required, value: false })).toBe(true);
    expect(ruleFails({ rule: required, value: "" })).toBe(true);
    expect(ruleFails({ rule: required, value: [] })).toBe(true);
    expect(ruleFails({ rule: required, value: "Ada" })).toBe(false);
  });

  it("uses peer values as the target for equalTo and notEqualTo rules", () => {
    const equalToPeer = peerEqualityRule("equalTo");
    const notEqualToPeer = peerEqualityRule("notEqualTo");

    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: "West" })).toBe(false);
    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: "East" })).toBe(true);
    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: undefined })).toBe(true);

    expect(ruleFails({ rule: notEqualToPeer, value: "West", peerValue: "East" })).toBe(false);
    expect(ruleFails({ rule: notEqualToPeer, value: "West", peerValue: "West" })).toBe(true);
  });

  it("compares inclusive and exclusive ranges through the declared rule shape", () => {
    const inclusiveRange = rangeRule("range", [5, 10], numberShape);
    const exclusiveRange = rangeRule("exclusiveRange", [5, 10], numberShape);

    expect(ruleFails({ rule: inclusiveRange, value: "5" })).toBe(false);
    expect(ruleFails({ rule: inclusiveRange, value: "11" })).toBe(true);

    expect(ruleFails({ rule: exclusiveRange, value: "5" })).toBe(true);
    expect(ruleFails({ rule: exclusiveRange, value: "6" })).toBe(false);
  });

  it("evaluates length constraints from the expected length perspective", () => {
    const minLength = lengthRule("minLength", 8);
    const maxLength = lengthRule("maxLength", 8);

    expect(ruleFails({ rule: minLength, value: "abc" })).toBe(true);
    expect(ruleFails({ rule: minLength, value: "securepass" })).toBe(false);
    expect(ruleFails({ rule: maxLength, value: "securepass" })).toBe(true);
    expect(ruleFails({ rule: maxLength, value: "abc" })).toBe(false);
  });

  it("orders date values only after the declared shape produces comparable values", () => {
    const minDate = orderedRule("min", "2026-01-01", dateShape, dateShape);

    expect(ruleFails({ rule: minDate, value: "2026-01-01" })).toBe(false);
    expect(ruleFails({ rule: minDate, value: "2025-12-31" })).toBe(true);
    expect(ruleFails({ rule: minDate, value: "not-a-date" })).toBe(true);
  });

  it("uses peer values as the target for ordered comparison rules", () => {
    const greaterThanPeer = peerOrderedRule("gt");

    expect(ruleFails({ rule: greaterThanPeer, value: "2026-01-02", peerValue: "2026-01-01" })).toBe(false);
    expect(ruleFails({ rule: greaterThanPeer, value: "2026-01-01", peerValue: "2026-01-01" })).toBe(true);
    expect(ruleFails({ rule: greaterThanPeer, value: "2025-12-31", peerValue: "2026-01-01" })).toBe(true);
  });
});
