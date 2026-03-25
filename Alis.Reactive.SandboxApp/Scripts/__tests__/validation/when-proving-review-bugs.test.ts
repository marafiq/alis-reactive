import { describe, it, expect } from "vitest";
import type { ValidationRule } from "../../types";
import { ruleFails } from "../../validation/rule-engine";

// ══════════════════════════════════════════════════════════
// Bug #1: NaN fail-open — compareValues returns NaN for date coercion,
// comparison operators (< > <= >=) all return false → rule passes.
// Number coercion converts garbage to 0 (not NaN), so the bug is date-specific.
// ══════════════════════════════════════════════════════════

const noPeer = { readPeer: () => undefined };

describe("Bug #1: NaN fail-open in comparison rules (date coercion)", () => {

  it("min rule should FAIL with invalid date (coerces to NaN)", () => {
    const rule: ValidationRule = { rule: "min", message: "min date", constraint: "2020-01-01", coerceAs: "date" };
    expect(ruleFails(rule, "not-a-date", noPeer)).toBe(true);
  });

  it("max rule should FAIL with invalid date (coerces to NaN)", () => {
    const rule: ValidationRule = { rule: "max", message: "max date", constraint: "2025-12-31", coerceAs: "date" };
    expect(ruleFails(rule, "not-a-date", noPeer)).toBe(true);
  });

  it("gt rule should FAIL with invalid date (coerces to NaN)", () => {
    const rule: ValidationRule = { rule: "gt", message: "after date", constraint: "2020-01-01", coerceAs: "date" };
    expect(ruleFails(rule, "not-a-date", noPeer)).toBe(true);
  });

  it("lt rule should FAIL with invalid date (coerces to NaN)", () => {
    const rule: ValidationRule = { rule: "lt", message: "before date", constraint: "2025-12-31", coerceAs: "date" };
    expect(ruleFails(rule, "not-a-date", noPeer)).toBe(true);
  });

  it("range rule should FAIL with invalid date (coerces to NaN)", () => {
    const rule: ValidationRule = { rule: "range", message: "date range", constraint: ["2020-01-01", "2025-12-31"], coerceAs: "date" };
    expect(ruleFails(rule, "not-a-date", noPeer)).toBe(true);
  });

  it("exclusiveRange rule should FAIL with invalid date (coerces to NaN)", () => {
    const rule: ValidationRule = { rule: "exclusiveRange", message: "exclusive date range", constraint: ["2020-01-01", "2025-12-31"], coerceAs: "date" };
    expect(ruleFails(rule, "not-a-date", noPeer)).toBe(true);
  });

  // Number coercion converts "abc" to 0 — NOT NaN. So number comparisons work correctly.
  it("number coercion: max with 'abc' passes (coerces to 0, 0 <= 100)", () => {
    const rule: ValidationRule = { rule: "max", message: "max 100", constraint: 100, coerceAs: "number" };
    expect(ruleFails(rule, "abc", noPeer)).toBe(false);
  });
});
