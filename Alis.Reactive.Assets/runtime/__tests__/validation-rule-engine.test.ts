import { describe, expect, it } from "vitest";
import { noPeerValue, peerValue, ruleFails } from "../validation/rule-engine";
import type { Shape, ValidationRule, ValidationRuleName, ValueProducer } from "../types";

const stringShape: Shape = { kind: "string" };
const numberShape: Shape = { kind: "number" };
const dateShape: Shape = { kind: "date" };
const noneShape: Shape = { kind: "none" };

function literal(value: string | number | boolean | null | (string | number)[], shape: Shape = stringShape): ValueProducer {
  return { kind: "literal", value, shape };
}

function rule(
  name: ValidationRuleName,
  overrides: Partial<ValidationRule["execution"]> = {},
): ValidationRule {
  return {
    name,
    message: `${name} failed`,
    execution: {
      constraint: { kind: "none" },
      otherValue: { kind: "none" },
      activation: { kind: "always" },
      comparisonShape: noneShape,
      ...overrides,
    },
  };
}

describe("validation rule engine", () => {
  it("treats missing, false, empty text, and empty arrays as empty validation subjects", () => {
    const required = rule("required");

    expect(ruleFails({ rule: required, value: undefined, peerValue: noPeerValue() })).toBe(true);
    expect(ruleFails({ rule: required, value: false, peerValue: noPeerValue() })).toBe(true);
    expect(ruleFails({ rule: required, value: "", peerValue: noPeerValue() })).toBe(true);
    expect(ruleFails({ rule: required, value: [], peerValue: noPeerValue() })).toBe(true);
    expect(ruleFails({ rule: required, value: "Ada", peerValue: noPeerValue() })).toBe(false);
  });

  it("uses peer values as the target for equalTo and notEqualTo rules", () => {
    const equalToPeer = rule("equalTo", { comparisonShape: stringShape });
    const notEqualToPeer = rule("notEqualTo", { comparisonShape: stringShape });

    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: peerValue("West") })).toBe(false);
    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: peerValue("East") })).toBe(true);
    expect(ruleFails({ rule: equalToPeer, value: "West", peerValue: noPeerValue() })).toBe(true);

    expect(ruleFails({ rule: notEqualToPeer, value: "West", peerValue: peerValue("East") })).toBe(false);
    expect(ruleFails({ rule: notEqualToPeer, value: "West", peerValue: peerValue("West") })).toBe(true);
  });

  it("compares inclusive and exclusive ranges through the declared rule shape", () => {
    const inclusiveRange = rule("range", {
      constraint: { kind: "value", value: literal([5, 10], { kind: "array", item: numberShape }) },
      comparisonShape: numberShape,
    });
    const exclusiveRange = rule("exclusiveRange", {
      constraint: { kind: "value", value: literal([5, 10], { kind: "array", item: numberShape }) },
      comparisonShape: numberShape,
    });

    expect(ruleFails({ rule: inclusiveRange, value: "5", peerValue: noPeerValue() })).toBe(false);
    expect(ruleFails({ rule: inclusiveRange, value: "11", peerValue: noPeerValue() })).toBe(true);

    expect(ruleFails({ rule: exclusiveRange, value: "5", peerValue: noPeerValue() })).toBe(true);
    expect(ruleFails({ rule: exclusiveRange, value: "6", peerValue: noPeerValue() })).toBe(false);
  });

  it("evaluates length constraints from the expected length perspective", () => {
    const minLength = rule("minLength", {
      constraint: { kind: "value", value: literal(8, numberShape) },
    });
    const maxLength = rule("maxLength", {
      constraint: { kind: "value", value: literal(8, numberShape) },
    });

    expect(ruleFails({ rule: minLength, value: "abc", peerValue: noPeerValue() })).toBe(true);
    expect(ruleFails({ rule: minLength, value: "securepass", peerValue: noPeerValue() })).toBe(false);
    expect(ruleFails({ rule: maxLength, value: "securepass", peerValue: noPeerValue() })).toBe(true);
    expect(ruleFails({ rule: maxLength, value: "abc", peerValue: noPeerValue() })).toBe(false);
  });

  it("orders date values only after the declared shape produces comparable values", () => {
    const minDate = rule("min", {
      constraint: { kind: "value", value: literal("2026-01-01", dateShape) },
      comparisonShape: dateShape,
    });

    expect(ruleFails({ rule: minDate, value: "2026-01-01", peerValue: noPeerValue() })).toBe(false);
    expect(ruleFails({ rule: minDate, value: "2025-12-31", peerValue: noPeerValue() })).toBe(true);
    expect(ruleFails({ rule: minDate, value: "not-a-date", peerValue: noPeerValue() })).toBe(true);
  });
});
