import { describe, expect, it } from "vitest";
import { ruleFails } from "../validation/rule-engine";
import type {
  LengthValidationRule,
  LiteralEqualityValidationRule,
  LiteralExpression,
  NoOperandValidationRule,
  NumericLiteralExpression,
  OrderedComparisonValidationRule,
  PeerEqualityValidationRule,
  PeerOrderedComparisonValidationRule,
  RangeValidationRule,
  RangeLiteralExpression,
  ReadExpression,
  Shape,
} from "../types/index";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const dateShape: Shape = { kind: "date" };
const noneShape: Shape = { kind: "none" };
const booleanShape: Shape = { kind: "boolean" };
const nullableBooleanShape: Shape = { kind: "nullable", inner: booleanShape };
const stringArrayShape: Shape = { kind: "array", item: stringShape };
const anyShape: Shape = { kind: "any" };
const nullableNumberShape: Shape = { kind: "nullable", inner: numberShape };

function literal(value: string | number | boolean | null, shape: Shape = stringShape): LiteralExpression {
  return { kind: "literal", value, shape };
}

function numericLiteral(value: number): NumericLiteralExpression {
  return { kind: "literal", value, shape: numberShape };
}

function rangeLiteral(bounds: [number, number]): RangeLiteralExpression {
  return {
    kind: "literal",
    value: bounds,
    shape: { kind: "array", item: numberShape },
  };
}

function componentValue(component: string, shape: Shape): ReadExpression {
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
      kind: "none",
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
      kind: "constraint",
      value: numericLiteral(length),
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
      kind: "constraint",
      value: rangeLiteral(bounds),
      activation: { kind: "always" },
      comparisonShape,
    },
  };
}

function orderedRule(
  name: OrderedComparisonValidationRule["name"],
  value: string | number | boolean,
  valueShape: Shape,
  comparisonShape: Shape,
): OrderedComparisonValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      kind: "constraint",
      value: literal(value, valueShape),
      activation: { kind: "always" },
      comparisonShape,
    },
  };
}

function booleanRangeRule(name: RangeValidationRule["name"]): RangeValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      kind: "constraint",
      value: { kind: "literal", value: [false, true], shape: { kind: "array", item: booleanShape } },
      activation: { kind: "always" },
      comparisonShape: booleanShape,
    },
  };
}

function literalEqualityRule(
  name: LiteralEqualityValidationRule["name"],
  value: boolean,
): LiteralEqualityValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      kind: "constraint",
      value: literal(value, booleanShape),
      activation: { kind: "always" },
      comparisonShape: booleanShape,
    },
  };
}

function peerEqualityRule(name: PeerEqualityValidationRule["name"]): PeerEqualityValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      kind: "peer",
      value: componentValue("confirmPassword", stringShape),
      activation: { kind: "always" },
      comparisonShape: stringShape,
    },
  };
}

function booleanPeerRule(
  name: PeerOrderedComparisonValidationRule["name"],
  comparisonShape: Shape,
): PeerOrderedComparisonValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      kind: "peer",
      value: componentValue("dischargeApproved", comparisonShape),
      activation: { kind: "always" },
      comparisonShape,
    },
  };
}

function peerOrderedRule(name: PeerOrderedComparisonValidationRule["name"]): PeerOrderedComparisonValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      kind: "peer",
      value: componentValue("startDate", dateShape),
      activation: { kind: "always" },
      comparisonShape: dateShape,
    },
  };
}

describe("validation rule engine", () => {
  it("treats missing values, empty text, and empty arrays as empty validation subjects", () => {
    const required = noOperandRule("required");

    expect(ruleFails({ rule: required, value: undefined, fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: required, value: "", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: required, value: [], fieldShape: stringArrayShape })).toBe(true);
    expect(ruleFails({ rule: required, value: "Ada", fieldShape: stringShape })).toBe(false);
  });

  it("treats false as empty only for a plain boolean field, as the server's NotEmpty does", () => {
    const required = noOperandRule("required");

    expect(ruleFails({ rule: required, value: false, fieldShape: booleanShape })).toBe(true);
    expect(ruleFails({ rule: required, value: true, fieldShape: booleanShape })).toBe(false);
    expect(ruleFails({ rule: required, value: false, fieldShape: nullableBooleanShape })).toBe(false);
    expect(ruleFails({ rule: required, value: null, fieldShape: nullableBooleanShape })).toBe(true);
    expect(ruleFails({ rule: required, value: false, fieldShape: anyShape })).toBe(false);
  });

  it("treats a nullable boolean's false as a value the empty rule rejects, as the server's Empty does", () => {
    const empty = noOperandRule("empty");

    expect(ruleFails({ rule: empty, value: false, fieldShape: nullableBooleanShape })).toBe(true);
    expect(ruleFails({ rule: empty, value: null, fieldShape: nullableBooleanShape })).toBe(false);
    expect(ruleFails({ rule: empty, value: false, fieldShape: booleanShape })).toBe(false);
  });

  it("treats a nullable boolean's false as a value for atLeastOne, equalTo and notEqual, as the server does", () => {
    expect(ruleFails({ rule: noOperandRule("atLeastOne"), value: false, fieldShape: nullableBooleanShape })).toBe(false);
    expect(ruleFails({ rule: literalEqualityRule("equalTo", true), value: false, fieldShape: nullableBooleanShape })).toBe(true);
    expect(ruleFails({ rule: literalEqualityRule("notEqual", false), value: false, fieldShape: nullableBooleanShape })).toBe(true);
  });

  it("treats text of only whitespace as blank for required and empty, as the server's NotEmpty and Empty do", () => {
    expect(ruleFails({ rule: noOperandRule("required"), value: "   ", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: noOperandRule("required"), value: "\t \n", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: noOperandRule("required"), value: "\u0085", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: noOperandRule("required"), value: "\uFEFF", fieldShape: stringShape })).toBe(false);
    expect(ruleFails({ rule: noOperandRule("empty"), value: "   ", fieldShape: stringShape })).toBe(false);
  });

  it("treats a plain number's 0 as blank for required and empty, and a nullable number's 0 as a value", () => {
    expect(ruleFails({ rule: noOperandRule("required"), value: 0, fieldShape: numberShape })).toBe(true);
    expect(ruleFails({ rule: noOperandRule("empty"), value: 0, fieldShape: numberShape })).toBe(false);
    expect(ruleFails({ rule: noOperandRule("required"), value: 0, fieldShape: nullableNumberShape })).toBe(false);
    expect(ruleFails({ rule: noOperandRule("empty"), value: 0, fieldShape: nullableNumberShape })).toBe(true);
  });

  it("fails atLeastOne on what the server's NotEmpty rejects: a plain boolean's false, whitespace, a plain number's 0", () => {
    const atLeastOne = noOperandRule("atLeastOne");

    expect(ruleFails({ rule: atLeastOne, value: false, fieldShape: booleanShape })).toBe(true);
    expect(ruleFails({ rule: atLeastOne, value: "   ", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: atLeastOne, value: 0, fieldShape: numberShape })).toBe(true);
    expect(ruleFails({ rule: atLeastOne, value: [], fieldShape: stringArrayShape })).toBe(true);
    expect(ruleFails({ rule: atLeastOne, value: ["Gluten-free"], fieldShape: stringArrayShape })).toBe(false);
  });

  it("keeps checking whitespace, 0 and a plain boolean's false in the rules that skip only a value never entered", () => {
    expect(ruleFails({ rule: lengthRule("minLength", 3), value: "  ", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: orderedRule("min", 1, numberShape, numberShape), value: 0, fieldShape: numberShape })).toBe(true);
    expect(ruleFails({ rule: literalEqualityRule("equalTo", true), value: false, fieldShape: booleanShape })).toBe(true);
    expect(ruleFails({ rule: literalEqualityRule("notEqual", false), value: false, fieldShape: booleanShape })).toBe(true);
  });

  it("uses peer values as the target for equalTo and notEqualTo rules", () => {
    const equalToPeer = peerEqualityRule("equalTo");
    const notEqualToPeer = peerEqualityRule("notEqualTo");

    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: "West", fieldShape: stringShape })).toBe(false);
    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: "East", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: undefined, fieldShape: stringShape })).toBe(true);

    expect(ruleFails({ rule: notEqualToPeer, value: "West", peerValue: "East", fieldShape: stringShape })).toBe(false);
    expect(ruleFails({ rule: notEqualToPeer, value: "West", peerValue: "West", fieldShape: stringShape })).toBe(true);
  });

  it("compares inclusive and exclusive ranges through the declared rule shape", () => {
    const inclusiveRange = rangeRule("range", [5, 10], numberShape);
    const exclusiveRange = rangeRule("exclusiveRange", [5, 10], numberShape);

    expect(ruleFails({ rule: inclusiveRange, value: "5", fieldShape: numberShape })).toBe(false);
    expect(ruleFails({ rule: inclusiveRange, value: "11", fieldShape: numberShape })).toBe(true);

    expect(ruleFails({ rule: exclusiveRange, value: "5", fieldShape: numberShape })).toBe(true);
    expect(ruleFails({ rule: exclusiveRange, value: "6", fieldShape: numberShape })).toBe(false);
  });

  it("evaluates length constraints from the expected length perspective", () => {
    const minLength = lengthRule("minLength", 8);
    const maxLength = lengthRule("maxLength", 8);

    expect(ruleFails({ rule: minLength, value: "abc", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: minLength, value: "securepass", fieldShape: stringShape })).toBe(false);
    expect(ruleFails({ rule: maxLength, value: "securepass", fieldShape: stringShape })).toBe(true);
    expect(ruleFails({ rule: maxLength, value: "abc", fieldShape: stringShape })).toBe(false);
  });

  it("orders booleans false before true in ordered, range and peer rules, as the server does", () => {
    expect(ruleFails({ rule: orderedRule("min", true, booleanShape, booleanShape), value: true, fieldShape: booleanShape })).toBe(false);
    expect(ruleFails({ rule: orderedRule("max", false, booleanShape, booleanShape), value: true, fieldShape: booleanShape })).toBe(true);
    expect(ruleFails({ rule: orderedRule("gt", false, booleanShape, booleanShape), value: true, fieldShape: booleanShape })).toBe(false);
    expect(ruleFails({ rule: orderedRule("lt", true, booleanShape, booleanShape), value: true, fieldShape: booleanShape })).toBe(true);
    expect(ruleFails({ rule: booleanRangeRule("range"), value: true, fieldShape: booleanShape })).toBe(false);
    expect(ruleFails({ rule: booleanRangeRule("exclusiveRange"), value: true, fieldShape: booleanShape })).toBe(true);

    const atMostApproved = booleanPeerRule("max", booleanShape);
    expect(ruleFails({ rule: atMostApproved, value: true, peerValue: false, fieldShape: booleanShape })).toBe(true);
    expect(ruleFails({ rule: atMostApproved, value: true, peerValue: true, fieldShape: booleanShape })).toBe(false);

    const moreThanApproved = booleanPeerRule("gt", nullableBooleanShape);
    expect(ruleFails({ rule: moreThanApproved, value: true, peerValue: false, fieldShape: nullableBooleanShape })).toBe(false);
    expect(ruleFails({ rule: moreThanApproved, value: false, peerValue: true, fieldShape: nullableBooleanShape })).toBe(true);
    expect(ruleFails({ rule: moreThanApproved, value: true, peerValue: null, fieldShape: nullableBooleanShape })).toBe(true);
  });

  it("orders a plain boolean's false too, now that it is a value rather than nothing entered", () => {
    expect(ruleFails({ rule: orderedRule("min", true, booleanShape, booleanShape), value: false, fieldShape: booleanShape })).toBe(true);
    expect(ruleFails({ rule: orderedRule("max", false, booleanShape, booleanShape), value: false, fieldShape: booleanShape })).toBe(false);
    expect(ruleFails({ rule: orderedRule("gt", false, booleanShape, booleanShape), value: false, fieldShape: booleanShape })).toBe(true);
    expect(ruleFails({ rule: orderedRule("lt", true, booleanShape, booleanShape), value: false, fieldShape: booleanShape })).toBe(false);
    expect(ruleFails({ rule: booleanRangeRule("range"), value: false, fieldShape: booleanShape })).toBe(false);
    expect(ruleFails({ rule: booleanPeerRule("max", booleanShape), value: false, peerValue: false, fieldShape: booleanShape })).toBe(false);
    expect(ruleFails({ rule: booleanPeerRule("gt", booleanShape), value: false, peerValue: true, fieldShape: booleanShape })).toBe(true);
  });

  it("orders date values only after the declared shape produces comparable values", () => {
    const minDate = orderedRule("min", "2026-01-01", dateShape, dateShape);

    expect(ruleFails({ rule: minDate, value: "2026-01-01", fieldShape: dateShape })).toBe(false);
    expect(ruleFails({ rule: minDate, value: "2025-12-31", fieldShape: dateShape })).toBe(true);
    expect(ruleFails({ rule: minDate, value: "not-a-date", fieldShape: dateShape })).toBe(true);
  });

  it("uses peer values as the target for ordered comparison rules", () => {
    const greaterThanPeer = peerOrderedRule("gt");

    expect(ruleFails({ rule: greaterThanPeer, value: "2026-01-02", peerValue: "2026-01-01", fieldShape: dateShape })).toBe(false);
    expect(ruleFails({ rule: greaterThanPeer, value: "2026-01-01", peerValue: "2026-01-01", fieldShape: dateShape })).toBe(true);
    expect(ruleFails({ rule: greaterThanPeer, value: "2025-12-31", peerValue: "2026-01-01", fieldShape: dateShape })).toBe(true);
  });
});
