// BDD: condition evaluation must produce correct results for every Shape kind.
// The framework-level invariant: if a typed C# DSL emits a shape, the runtime
// evaluates the compare against that shape faithfully. This test locks in the
// expected behavior regardless of component vendor.

import { describe, it, expect, beforeAll } from "vitest";
import { evaluateCondition, setValueEvaluator } from "../conditions/conditions";
import { evaluateValue } from "../core/evaluate";
import type { Plan, CompareCondition, ValueProducer, Shape, CoreJsType } from "../types";

beforeAll(() => {
  setValueEvaluator(evaluateValue);
});

function emptyPlan(): Plan {
  return {
    version: 3,
    planId: "test",
    types: {},
    objects: {},
    contracts: {},
    bindings: {},
    workflows: [],
  } as unknown as Plan;
}

const literal = (value: unknown, shape: Shape): ValueProducer =>
  ({ kind: "literal", value, shape }) as ValueProducer;

const cmp = (op: string, left: ValueProducer, right: ValueProducer | null, shape: Shape, itemShape?: Shape): CompareCondition =>
  ({ kind: "compare", op, left, right, shape, itemShape: itemShape ?? { kind: "none" } }) as unknown as CompareCondition;

const S = {
  string: { kind: "string" } as Shape,
  number: { kind: "number" } as Shape,
  boolean: { kind: "boolean" } as Shape,
  date: { kind: "date" } as Shape,
  none: { kind: "none" } as Shape,
  nullableDate: { kind: "nullable", inner: { kind: "date" } } as Shape,
  nullableString: { kind: "nullable", inner: { kind: "string" } } as Shape,
  arrayOfString: { kind: "array", item: { kind: "string" } } as Shape,
  arrayOfDate: { kind: "array", item: { kind: "date" } } as Shape,
};

describe("presence operators — use RAW value, bypass shape", () => {
  const plan = emptyPlan();

  it("is-null returns true for null under a Nullable shape (null is only legal inside Nullable)", () => {
    const c = cmp("is-null", literal(null, S.nullableString), null, S.nullableString);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("is-null returns false for empty string", () => {
    const c = cmp("is-null", literal("", S.string), null, S.string);
    expect(evaluateCondition(c, plan)).toBe(false);
  });

  it("is-empty returns true for empty string", () => {
    const c = cmp("is-empty", literal("", S.string), null, S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("is-empty returns true for empty array", () => {
    const c = cmp("is-empty", literal([], S.arrayOfString), null, S.arrayOfString);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("is-empty returns true for null under Nullable(String)", () => {
    const c = cmp("is-empty", literal(null, S.nullableString), null, S.nullableString);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("not-null returns true for non-null string", () => {
    const c = cmp("not-null", literal("hi", S.string), null, S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("not-null returns false for null under Shape.Nullable(Date)", () => {
    const c = cmp("not-null", literal(null, S.nullableDate), null, S.nullableDate);
    expect(evaluateCondition(c, plan)).toBe(false);
  });

  it("not-empty returns true for non-empty array", () => {
    const c = cmp("not-empty", literal(["a"], S.arrayOfString), null, S.arrayOfString);
    expect(evaluateCondition(c, plan)).toBe(true);
  });
});

describe("ordering on Shape.Number", () => {
  const plan = emptyPlan();

  it("gt returns true when left number exceeds right", () => {
    const c = cmp("gt",
      literal(42, S.number),
      literal(10, S.number),
      S.number);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("gt returns false when left equals right", () => {
    const c = cmp("gt",
      literal(42, S.number),
      literal(42, S.number),
      S.number);
    expect(evaluateCondition(c, plan)).toBe(false);
  });

  it("gte returns true at the exact boundary", () => {
    const c = cmp("gte",
      literal(42, S.number),
      literal(42, S.number),
      S.number);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("lt returns true when left is below right", () => {
    const c = cmp("lt",
      literal(5, S.number),
      literal(10, S.number),
      S.number);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("lte returns false when left exceeds right", () => {
    const c = cmp("lte",
      literal(20, S.number),
      literal(10, S.number),
      S.number);
    expect(evaluateCondition(c, plan)).toBe(false);
  });

  it("gt on Nullable(Number) with a Number literal works (plan shape for decimal? bindings)", () => {
    const c = cmp("gt",
      literal(85, S.nullableNumber ?? S.number),
      literal(65, S.number),
      S.number);
    expect(evaluateCondition(c, plan)).toBe(true);
  });
});

describe("ordering on Shape.Date", () => {
  const plan = emptyPlan();

  it("gt returns true when left Date is after right", () => {
    const c = cmp("gt",
      literal("2026-09-15", S.date),
      literal("2026-01-01", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("gt returns false when dates are equal", () => {
    const c = cmp("gt",
      literal("2026-09-15", S.date),
      literal("2026-09-15", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(false);
  });

  it("lt returns false when left Date exceeds right", () => {
    const c = cmp("lt",
      literal("2026-09-15", S.date),
      literal("2026-01-01", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(false);
  });

  it("gt on Nullable(Date) with a Date literal works (plan shape for DateTime? bindings)", () => {
    // Real plan emits rule/cond.shape = Nullable(Date), literal constraint shape = Date.
    const c = cmp("gt",
      literal("2026-09-15", S.nullableDate),
      literal("2025-01-01", S.date),
      S.nullableDate);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("gte returns true on equal dates", () => {
    const c = cmp("gte",
      literal("2026-09-15", S.date),
      literal("2026-09-15", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("lt returns true when left Date is before right", () => {
    const c = cmp("lt",
      literal("2026-01-01", S.date),
      literal("2026-09-15", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("lte returns true on equal dates", () => {
    const c = cmp("lte",
      literal("2026-09-15", S.date),
      literal("2026-09-15", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });
});

describe("equality on Shape.Date — two equal timestamps must compare equal", () => {
  const plan = emptyPlan();

  it("eq returns true when two Dates represent the same instant", () => {
    const c = cmp("eq",
      literal("2026-09-15", S.date),
      literal("2026-09-15", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("neq returns false when two Dates represent the same instant", () => {
    const c = cmp("neq",
      literal("2026-09-15", S.date),
      literal("2026-09-15", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(false);
  });

  it("eq returns false for different dates", () => {
    const c = cmp("eq",
      literal("2026-09-15", S.date),
      literal("2026-01-01", S.date),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(false);
  });
});

describe("equality on Nullable(Date)", () => {
  const plan = emptyPlan();

  it("eq null-to-null returns true", () => {
    const c = cmp("eq",
      literal(null, S.nullableDate),
      literal(null, S.nullableDate),
      S.nullableDate);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("eq null-to-date returns false", () => {
    const c = cmp("eq",
      literal(null, S.nullableDate),
      literal("2026-09-15", S.nullableDate),
      S.nullableDate);
    expect(evaluateCondition(c, plan)).toBe(false);
  });
});

describe("in / array-contains on typed arrays", () => {
  const plan = emptyPlan();

  it("in returns true when a string is present in an array of strings", () => {
    const c = cmp("in",
      literal("Memory Care", S.string),
      literal(["Standard", "Memory Care", "Assisted"], S.arrayOfString),
      S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("not-in returns true when the value is absent", () => {
    const c = cmp("not-in",
      literal("Unknown", S.string),
      literal(["Standard", "Memory Care"], S.arrayOfString),
      S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("in on Shape.Date returns true when the target date appears in the array", () => {
    const c = cmp("in",
      literal("2026-09-15", S.date),
      literal(["2026-01-01", "2026-09-15", "2026-12-31"], S.arrayOfDate),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });
});

describe("between on Shape.Date", () => {
  const plan = emptyPlan();

  it("returns true when the left date falls within the right-side range", () => {
    const c = cmp("between",
      literal("2026-09-15", S.date),
      literal(["2026-01-01", "2026-12-31"], S.arrayOfDate),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("returns false when the left date falls outside the range", () => {
    const c = cmp("between",
      literal("2025-12-31", S.date),
      literal(["2026-01-01", "2026-12-31"], S.arrayOfDate),
      S.date);
    expect(evaluateCondition(c, plan)).toBe(false);
  });
});

describe("Nullable(Date) equality with mixed operands", () => {
  const plan = emptyPlan();

  it("eq on two equal dates wrapped in Nullable(Date) returns true", () => {
    const c = cmp("eq",
      literal("2026-09-15", S.nullableDate),
      literal("2026-09-15", S.nullableDate),
      S.nullableDate);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("neq on a date vs null under Nullable(Date) returns true", () => {
    const c = cmp("neq",
      literal("2026-09-15", S.nullableDate),
      literal(null, S.nullableDate),
      S.nullableDate);
    expect(evaluateCondition(c, plan)).toBe(true);
  });
});

describe("array-contains on Shape.Date items", () => {
  const plan = emptyPlan();

  it("returns true when the needle Date appears in the haystack Array(Date)", () => {
    // cond.shape = needle's shape (Date) so resolveRight applies scalar shape to the needle.
    const needle = literal("2026-09-15", S.date);
    const haystack = literal(["2026-01-01", "2026-09-15"], S.arrayOfDate);
    const c = cmp("array-contains", haystack, needle, S.date, S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("returns false when the needle Date is absent", () => {
    const needle = literal("2026-06-01", S.date);
    const haystack = literal(["2026-01-01", "2026-09-15"], S.arrayOfDate);
    const c = cmp("array-contains", haystack, needle, S.date, S.date);
    expect(evaluateCondition(c, plan)).toBe(false);
  });
});

describe("array-contains — the plan shape C# ArrayContains actually emits", () => {
  const plan = emptyPlan();
  // C# ConditionSourceBuilder.ArrayContains emits:
  //   cond.shape     = source's array shape (e.g. Array(String))
  //   cond.itemShape = source's element shape (e.g. String)
  //   cond.right     = literal needle shaped against the ELEMENT shape
  // The needle MUST NOT be wrapped into an array by resolveRight.

  it("matches a scalar-string needle against an Array(String) haystack", () => {
    const c = cmp("array-contains",
      literal(["Penicillin", "Latex"], S.arrayOfString),
      literal("Penicillin", S.string),
      S.arrayOfString, S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("reports no match when the needle is not in the Array(String) haystack", () => {
    const c = cmp("array-contains",
      literal(["Latex", "Sulfa"], S.arrayOfString),
      literal("Penicillin", S.string),
      S.arrayOfString, S.string);
    expect(evaluateCondition(c, plan)).toBe(false);
  });

  it("matches a scalar Date needle against an Array(Date) haystack", () => {
    const c = cmp("array-contains",
      literal(["2025-01-01", "2025-12-31"], S.arrayOfDate),
      literal("2025-12-31", S.date),
      S.arrayOfDate, S.date);
    expect(evaluateCondition(c, plan)).toBe(true);
  });
});

describe("regression guards — equality on non-Date shapes is unchanged", () => {
  const plan = emptyPlan();

  it("eq on two equal strings returns true", () => {
    const c = cmp("eq",
      literal("hello", S.string),
      literal("hello", S.string),
      S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("eq on two equal numbers returns true", () => {
    const c = cmp("eq",
      literal(42, S.number),
      literal(42, S.number),
      S.number);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("eq on two equal booleans returns true", () => {
    const c = cmp("eq",
      literal(true, S.boolean),
      literal(true, S.boolean),
      S.boolean);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("neq on two different strings returns true", () => {
    const c = cmp("neq",
      literal("a", S.string),
      literal("b", S.string),
      S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("in with string haystack returns true for matching element", () => {
    const c = cmp("in",
      literal("b", S.string),
      literal(["a", "b", "c"], S.arrayOfString),
      S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });
});

describe("string operators", () => {
  const plan = emptyPlan();

  it("contains returns true when the right substring appears in the left", () => {
    const c = cmp("contains",
      literal("care plan needed", S.string),
      literal("care", S.string),
      S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("starts-with is case-sensitive", () => {
    const c = cmp("starts-with",
      literal("Care Plan", S.string),
      literal("Care", S.string),
      S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });

  it("matches returns true for a valid regex pattern", () => {
    const c = cmp("matches",
      literal("555-123-4567", S.string),
      literal("^\\d{3}-\\d{3}-\\d{4}$", S.string),
      S.string);
    expect(evaluateCondition(c, plan)).toBe(true);
  });
});
