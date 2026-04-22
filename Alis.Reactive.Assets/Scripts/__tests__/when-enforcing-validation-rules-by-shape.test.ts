// BDD: validation rules must operate correctly for every Shape kind.
// Validation is the consumer that most obviously depends on shape:
// "min 18" means different things for number vs date vs string.
// These tests lock in the semantics so future refactors can't silently
// change how a given rule+shape pair evaluates.

import { describe, it, expect } from "vitest";
import { ruleFails } from "../validation/rule-engine";
import type { ValidationRule, Shape, ValueProducer } from "../types";

const S = {
  string: { kind: "string" } as Shape,
  number: { kind: "number" } as Shape,
  boolean: { kind: "boolean" } as Shape,
  date: { kind: "date" } as Shape,
  none: { kind: "none" } as Shape,
  nullableDate: { kind: "nullable", inner: { kind: "date" } } as Shape,
  nullableNumber: { kind: "nullable", inner: { kind: "number" } } as Shape,
  arrayOfString: { kind: "array", item: { kind: "string" } } as Shape,
};

const emptyVP: ValueProducer = { kind: "none" } as unknown as ValueProducer;
const lit = (value: unknown, shape: Shape = S.none): ValueProducer =>
  ({ kind: "literal", value, shape }) as ValueProducer;

const rule = (
  name: string, shape: Shape, constraint: ValueProducer = emptyVP, message = "fail",
): ValidationRule =>
  ({ name, message, constraint, otherValue: emptyVP, when: { kind: "none" }, shape }) as unknown as ValidationRule;

// ── required / empty — shape-agnostic, but lock the empty semantics ──

describe("required — fails when value is null/empty/false/empty-array; passes otherwise", () => {
  const r = rule("required", S.string);

  it("fails on null", () => expect(ruleFails(r, null)).toBe(true));
  it("fails on empty string", () => expect(ruleFails(r, "")).toBe(true));
  it("fails on empty array", () => expect(ruleFails(r, [])).toBe(true));
  it("fails on false (checkbox semantics)", () => expect(ruleFails(r, false)).toBe(true));
  it("passes on a non-empty string", () => expect(ruleFails(r, "x")).toBe(false));
  it("passes on a non-zero number", () => expect(ruleFails(r, 1)).toBe(false));
  it("passes on a Date", () => expect(ruleFails(r, new Date())).toBe(false));
});

// ── Length (string-shape only) ──

describe("minLength / maxLength — string length semantics", () => {
  const minR = rule("minLength", S.string, lit(5));
  const maxR = rule("maxLength", S.string, lit(10));

  it("minLength passes when value has the exact minimum length", () =>
    expect(ruleFails(minR, "12345")).toBe(false));

  it("minLength fails when value is shorter", () =>
    expect(ruleFails(minR, "1234")).toBe(true));

  it("maxLength passes at exactly the limit", () =>
    expect(ruleFails(maxR, "1234567890")).toBe(false));

  it("maxLength fails when value exceeds the limit", () =>
    expect(ruleFails(maxR, "12345678901")).toBe(true));

  it("length rules pass when value is empty (only required catches empty)", () =>
    expect(ruleFails(minR, "")).toBe(false));
});

// ── Numeric comparisons ──

describe("min / max / gt / lt — Nullable(Number) field (e.g., decimal? model property)", () => {
  const minR = rule("min", S.nullableNumber, lit(18, S.number));
  const maxR = rule("max", S.nullableNumber, lit(65, S.number));
  const gtR  = rule("gt",  S.nullableNumber, lit(0, S.number));
  const ltR  = rule("lt",  S.nullableNumber, lit(100, S.number));

  it("min passes at the boundary (inclusive)", () => expect(ruleFails(minR, 18)).toBe(false));
  it("min fails below the boundary", () => expect(ruleFails(minR, 17)).toBe(true));
  it("max passes at the boundary (inclusive)", () => expect(ruleFails(maxR, 65)).toBe(false));
  it("max fails above the boundary", () => expect(ruleFails(maxR, 66)).toBe(true));

  it("gt fails at the boundary (exclusive)", () => expect(ruleFails(gtR, 0)).toBe(true));
  it("gt passes above the boundary", () => expect(ruleFails(gtR, 1)).toBe(false));
  it("lt fails at the boundary (exclusive)", () => expect(ruleFails(ltR, 100)).toBe(true));
  it("lt passes below the boundary", () => expect(ruleFails(ltR, 99)).toBe(false));

  it("string-encoded number values go through shape convert before comparison", () =>
    expect(ruleFails(minR, "20")).toBe(false));
});

// ── Date comparisons — this is where my refactor matters ──
// Real plan shape: a DateTime? model field registers as Nullable(Date).
// FluentValidationAdapter emits rule.shape = Nullable(Date) with a
// non-nullable literal constraint (Shape.Date) carrying the boundary date.

describe("min / max / gt / lt — Nullable(Date) field with Date-literal constraint (shape plan emits)", () => {
  const minR = rule("min", S.nullableDate, lit("2020-01-01", S.date));
  const maxR = rule("max", S.nullableDate, lit("2030-12-31", S.date));
  const gtR  = rule("gt",  S.nullableDate, lit("2025-01-01", S.date));
  const ltR  = rule("lt",  S.nullableDate, lit("2030-01-01", S.date));

  it("min passes when the input date is after the boundary", () =>
    expect(ruleFails(minR, "2022-06-15")).toBe(false));

  it("min fails when the input date is before the boundary", () =>
    expect(ruleFails(minR, "2019-12-31")).toBe(true));

  it("min passes at the exact boundary (inclusive)", () =>
    expect(ruleFails(minR, "2020-01-01")).toBe(false));

  it("max passes when the input is before the boundary", () =>
    expect(ruleFails(maxR, "2030-12-31")).toBe(false));

  it("max fails when the input exceeds the boundary", () =>
    expect(ruleFails(maxR, "2031-01-01")).toBe(true));

  it("gt fails at the boundary (exclusive)", () =>
    expect(ruleFails(gtR, "2025-01-01")).toBe(true));

  it("gt passes when the input date is strictly after the boundary", () =>
    expect(ruleFails(gtR, "2025-06-15")).toBe(false));

  it("lt fails at the boundary (exclusive)", () =>
    expect(ruleFails(ltR, "2030-01-01")).toBe(true));

  it("lt passes when the input is strictly before the boundary", () =>
    expect(ruleFails(ltR, "2029-12-31")).toBe(false));

  it("accepts a Date object as the input value", () =>
    expect(ruleFails(minR, new Date(Date.UTC(2025, 0, 1)))).toBe(false));
});

// ── Range / exclusiveRange ──

describe("range — inclusive bounds on numbers", () => {
  const r = rule("range", S.number, lit([1, 10]));

  it("passes at the lower bound", () => expect(ruleFails(r, 1)).toBe(false));
  it("passes at the upper bound", () => expect(ruleFails(r, 10)).toBe(false));
  it("passes in the middle", () => expect(ruleFails(r, 5)).toBe(false));
  it("fails below the lower bound", () => expect(ruleFails(r, 0)).toBe(true));
  it("fails above the upper bound", () => expect(ruleFails(r, 11)).toBe(true));
  it("passes on empty (only required catches empty)", () => expect(ruleFails(r, "")).toBe(false));
});

describe("range — inclusive bounds on dates (Nullable(Date) field)", () => {
  const r = rule("range", S.nullableDate, lit(["2025-01-01", "2025-12-31"]));

  it("passes at the lower date bound", () => expect(ruleFails(r, "2025-01-01")).toBe(false));
  it("passes at the upper date bound", () => expect(ruleFails(r, "2025-12-31")).toBe(false));
  it("passes on a date in the middle of the range", () => expect(ruleFails(r, "2025-06-15")).toBe(false));
  it("fails before the lower bound", () => expect(ruleFails(r, "2024-12-31")).toBe(true));
  it("fails after the upper bound", () => expect(ruleFails(r, "2026-01-01")).toBe(true));
});

describe("exclusiveRange — exclusive bounds", () => {
  const r = rule("exclusiveRange", S.number, lit([1, 10]));

  it("fails at the lower bound (exclusive)", () => expect(ruleFails(r, 1)).toBe(true));
  it("fails at the upper bound (exclusive)", () => expect(ruleFails(r, 10)).toBe(true));
  it("passes in the middle", () => expect(ruleFails(r, 5)).toBe(false));
});

// ── equalTo / notEqual / notEqualTo — Shape-aware equality ──

describe("equalTo — Nullable(Date) field equality uses value equality, not reference equality", () => {
  const r = rule("equalTo", S.nullableDate, lit("2025-06-15", S.date));

  it("passes when the input represents the same instant as the constraint", () =>
    expect(ruleFails(r, "2025-06-15")).toBe(false));

  it("fails when the input is a different date", () =>
    expect(ruleFails(r, "2025-01-01")).toBe(true));

  it("passes when the input is a Date instance with the matching instant (distinct reference)", () =>
    expect(ruleFails(r, new Date(2025, 5, 15))).toBe(false));
});

describe("equalTo — peer comparison via otherValue (StartDate == EndDate) — Nullable(Date) fields", () => {
  const r = rule("equalTo", S.nullableDate);

  it("passes when value and otherValue are the same date", () =>
    expect(ruleFails(r, "2025-06-15", "2025-06-15")).toBe(false));

  it("fails when value and otherValue differ", () =>
    expect(ruleFails(r, "2025-06-15", "2025-06-16")).toBe(true));
});

describe("notEqual / notEqualTo — Nullable(Date) fields", () => {
  const constR = rule("notEqual", S.nullableDate, lit("2025-06-15", S.date));

  it("notEqual fails when equal", () => expect(ruleFails(constR, "2025-06-15")).toBe(true));
  it("notEqual passes when different", () => expect(ruleFails(constR, "2025-01-01")).toBe(false));

  const peerR = rule("notEqualTo", S.nullableDate);
  it("notEqualTo with peer fails when values are the same date", () =>
    expect(ruleFails(peerR, "2025-06-15", "2025-06-15")).toBe(true));
});

describe("equalTo — Shape.String", () => {
  const r = rule("equalTo", S.string, lit("smith"));

  it("passes on exact match", () => expect(ruleFails(r, "smith")).toBe(false));
  it("fails on different string", () => expect(ruleFails(r, "johnson")).toBe(true));
});

describe("equalTo — Shape.Number", () => {
  const r = rule("equalTo", S.number, lit(42));

  it("passes when numbers match", () => expect(ruleFails(r, 42)).toBe(false));
  it("passes when string-encoded number matches after shape convert", () => expect(ruleFails(r, "42")).toBe(false));
  it("fails when numbers differ", () => expect(ruleFails(r, 43)).toBe(true));
});

// ── Pattern / format rules ──

describe("email / url / regex / creditCard — format validators", () => {
  it("email passes for a valid address", () =>
    expect(ruleFails(rule("email", S.string), "user@example.com")).toBe(false));

  it("email fails for a missing @", () =>
    expect(ruleFails(rule("email", S.string), "user.example.com")).toBe(true));

  it("url passes for a valid http URL", () =>
    expect(ruleFails(rule("url", S.string), "https://example.com")).toBe(false));

  it("url fails for a bare word", () =>
    expect(ruleFails(rule("url", S.string), "example.com")).toBe(true));

  it("regex passes when the pattern matches", () =>
    expect(ruleFails(rule("regex", S.string, lit("^\\d{3}-\\d{4}$")), "555-1234")).toBe(false));

  it("regex fails when the pattern does not match", () =>
    expect(ruleFails(rule("regex", S.string, lit("^\\d{3}-\\d{4}$")), "abc")).toBe(true));

  it("creditCard fails on an invalid Luhn number", () =>
    expect(ruleFails(rule("creditCard", S.string), "1234567890123456")).toBe(true));

  it("creditCard passes on a known-valid Luhn number", () =>
    expect(ruleFails(rule("creditCard", S.string), "4532015112830366")).toBe(false));
});

// ── atLeastOne — array semantics ──

describe("atLeastOne — array must be non-empty", () => {
  const r = rule("atLeastOne", S.arrayOfString);
  it("fails on empty array", () => expect(ruleFails(r, [])).toBe(true));
  it("passes on non-empty array", () => expect(ruleFails(r, ["a"])).toBe(false));
  it("fails on null (treated as empty)", () => expect(ruleFails(r, null)).toBe(true));
});
