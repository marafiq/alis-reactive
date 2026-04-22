// BDD: shapeEquals is the one value-equality primitive for shape-applied values.
// === on its own silently reports reference inequality for Date objects;
// .includes() / array equality inherit the same problem. This test locks in
// the semantics: two equal-value dates are equal; two distinct-reference dates
// with the same instant are equal; nullable/array compositions recurse.

import { describe, it, expect } from "vitest";
import { shapeEquals } from "../core/shape-convert";
import type { Shape } from "../types";

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
  arrayOfNullableDate: { kind: "array", item: { kind: "nullable", inner: { kind: "date" } } } as Shape,
};

describe("shapeEquals — Shape.Date uses value equality, not reference equality", () => {
  it("returns true for two Date instances representing the same instant", () => {
    const a = new Date(Date.UTC(2026, 8, 15));
    const b = new Date(Date.UTC(2026, 8, 15));
    expect(a === b).toBe(false);          // proves they are distinct refs
    expect(shapeEquals(a, b, S.date)).toBe(true);
  });

  it("returns false for two Dates at different instants", () => {
    expect(shapeEquals(
      new Date(Date.UTC(2026, 8, 15)),
      new Date(Date.UTC(2026, 8, 16)),
      S.date)).toBe(false);
  });
});

describe("shapeEquals — null handling", () => {
  it("null === null → true", () => {
    expect(shapeEquals(null, null, S.date)).toBe(true);
  });

  it("null vs non-null → false", () => {
    expect(shapeEquals(null, new Date(), S.date)).toBe(false);
    expect(shapeEquals(new Date(), null, S.date)).toBe(false);
  });
});

describe("shapeEquals — Nullable(T) short-circuits null then recurses on inner", () => {
  it("null vs null under Nullable(Date) → true", () => {
    expect(shapeEquals(null, null, S.nullableDate)).toBe(true);
  });

  it("two same-instant Dates under Nullable(Date) → true", () => {
    const a = new Date(Date.UTC(2026, 0, 1));
    const b = new Date(Date.UTC(2026, 0, 1));
    expect(shapeEquals(a, b, S.nullableDate)).toBe(true);
  });

  it("null vs Date under Nullable(Date) → false", () => {
    expect(shapeEquals(null, new Date(), S.nullableDate)).toBe(false);
  });
});

describe("shapeEquals — Array(T) compares element-wise using item shape", () => {
  it("two arrays of equal-valued strings → true", () => {
    expect(shapeEquals(["a", "b"], ["a", "b"], S.arrayOfString)).toBe(true);
  });

  it("two arrays of same-instant Dates → true (each element compared by value)", () => {
    const arr1 = [new Date(Date.UTC(2026, 0, 1)), new Date(Date.UTC(2026, 11, 31))];
    const arr2 = [new Date(Date.UTC(2026, 0, 1)), new Date(Date.UTC(2026, 11, 31))];
    expect(shapeEquals(arr1, arr2, S.arrayOfDate)).toBe(true);
  });

  it("two arrays that differ in one element → false", () => {
    const arr1 = [new Date(Date.UTC(2026, 0, 1))];
    const arr2 = [new Date(Date.UTC(2027, 0, 1))];
    expect(shapeEquals(arr1, arr2, S.arrayOfDate)).toBe(false);
  });

  it("arrays of different length → false", () => {
    expect(shapeEquals(["a"], ["a", "b"], S.arrayOfString)).toBe(false);
  });

  it("Array(Nullable(Date)) handles mixed null / date elements", () => {
    const a = [null, new Date(Date.UTC(2026, 0, 1)), null];
    const b = [null, new Date(Date.UTC(2026, 0, 1)), null];
    expect(shapeEquals(a, b, S.arrayOfNullableDate)).toBe(true);
  });

  it("Array(Nullable(Date)) returns false when a null differs from a date", () => {
    const a = [null, new Date(Date.UTC(2026, 0, 1))];
    const b = [new Date(Date.UTC(2026, 0, 1)), null];
    expect(shapeEquals(a, b, S.arrayOfNullableDate)).toBe(false);
  });
});

describe("shapeEquals — primitive shapes fall through to ===", () => {
  it("string equality", () => {
    expect(shapeEquals("x", "x", S.string)).toBe(true);
    expect(shapeEquals("x", "y", S.string)).toBe(false);
  });

  it("number equality", () => {
    expect(shapeEquals(42, 42, S.number)).toBe(true);
    expect(shapeEquals(42, 43, S.number)).toBe(false);
  });

  it("boolean equality", () => {
    expect(shapeEquals(true, true, S.boolean)).toBe(true);
    expect(shapeEquals(true, false, S.boolean)).toBe(false);
  });

  it("Shape.None falls back to ===", () => {
    const obj = { a: 1 };
    expect(shapeEquals(obj, obj, S.none)).toBe(true);
    expect(shapeEquals({ a: 1 }, { a: 1 }, S.none)).toBe(false); // different refs
  });
});
