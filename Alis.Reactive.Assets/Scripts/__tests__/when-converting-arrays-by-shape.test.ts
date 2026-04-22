// BDD: Array shape convert must apply the item shape to every element.
// This is what the plan emits for multi-select, tag-list, date-array bindings.

import { describe, it, expect } from "vitest";
import { applyShape, convertByShape } from "../core/shape-convert";
import type { Shape } from "../types";

const S = {
  string: { kind: "string" } as Shape,
  number: { kind: "number" } as Shape,
  boolean: { kind: "boolean" } as Shape,
  date: { kind: "date" } as Shape,
  arrayOfString: { kind: "array", item: { kind: "string" } } as Shape,
  arrayOfNumber: { kind: "array", item: { kind: "number" } } as Shape,
  arrayOfBoolean: { kind: "array", item: { kind: "boolean" } } as Shape,
  arrayOfDate: { kind: "array", item: { kind: "date" } } as Shape,
  arrayOfNullableDate: { kind: "array", item: { kind: "nullable", inner: { kind: "date" } } } as Shape,
  arrayOfNullableString: { kind: "array", item: { kind: "nullable", inner: { kind: "string" } } } as Shape,
};

describe("applyShape with Array(String) — item shape applied to each element", () => {
  it("shape converts numbers to strings per element", () => {
    expect(applyShape([1, 2, 3], S.arrayOfString)).toEqual(["1", "2", "3"]);
  });

  it("shape converts mixed inputs to strings", () => {
    expect(applyShape([1, "two", true], S.arrayOfString)).toEqual(["1", "two", "true"]);
  });

  it("returns empty array unchanged", () => {
    expect(applyShape([], S.arrayOfString)).toEqual([]);
  });
});

describe("applyShape with Array(Number) — item shape applied to each element", () => {
  it("shape converts string-encoded numbers per element", () => {
    expect(applyShape(["1", "2", "3"], S.arrayOfNumber)).toEqual([1, 2, 3]);
  });

  it("shape converts booleans to 0/1 per element", () => {
    expect(applyShape([true, false, true], S.arrayOfNumber)).toEqual([1, 0, 1]);
  });

  it("shape converts Date elements to their epoch-ms numbers", () => {
    const a = new Date(Date.UTC(2025, 0, 1));
    const b = new Date(Date.UTC(2026, 0, 1));
    expect(applyShape([a, b], S.arrayOfNumber)).toEqual([a.getTime(), b.getTime()]);
  });
});

describe("applyShape with Array(Boolean) — item shape applied to each element", () => {
  it("shape converts truthy/falsy strings per element", () => {
    expect(applyShape(["true", "false", ""], S.arrayOfBoolean)).toEqual([true, false, false]);
  });

  it("shape converts 0/1 numbers per element", () => {
    expect(applyShape([1, 0, 1], S.arrayOfBoolean)).toEqual([true, false, true]);
  });
});

describe("applyShape with Array(Date) — item shape applied to each element", () => {
  it("shape converts ISO-date strings per element to Date objects", () => {
    const result = applyShape(["2025-01-01", "2025-12-31"], S.arrayOfDate) as Date[];
    expect(result).toHaveLength(2);
    expect(result[0]).toBeInstanceOf(Date);
    expect(result[0].getFullYear()).toBe(2025);
    expect(result[1]).toBeInstanceOf(Date);
    expect(result[1].getFullYear()).toBe(2025);
  });

  it("passes Date objects through unchanged", () => {
    const a = new Date(Date.UTC(2025, 0, 1));
    const b = new Date(Date.UTC(2026, 0, 1));
    const result = applyShape([a, b], S.arrayOfDate) as Date[];
    expect(result[0]).toBe(a);
    expect(result[1]).toBe(b);
  });
});

describe("applyShape with Array(Nullable(T)) — nulls preserved per element", () => {
  it("Array(Nullable(Date)) preserves null while coercing the rest", () => {
    const result = applyShape(["2025-01-01", null, "2025-12-31"], S.arrayOfNullableDate);
    expect(Array.isArray(result)).toBe(true);
    const r = result as (Date | null)[];
    expect(r[0]).toBeInstanceOf(Date);
    expect(r[1]).toBeNull();
    expect(r[2]).toBeInstanceOf(Date);
  });

  it("Array(Nullable(String)) preserves nulls alongside strings", () => {
    const result = applyShape(["a", null, "c"], S.arrayOfNullableString);
    expect(result).toEqual(["a", null, "c"]);
  });
});

describe("convertByShape on Array shapes — explicit Result envelope", () => {
  it("Array(String) wraps the shape-converted array in ok()", () => {
    const r = convertByShape([1, 2], S.arrayOfString);
    expect(r.ok).toBe(true);
    if (r.ok) expect(r.value).toEqual(["1", "2"]);
  });

  it("Array(Date) wraps the Date array in ok()", () => {
    const r = convertByShape(["2025-01-01"], S.arrayOfDate);
    expect(r.ok).toBe(true);
    if (r.ok) {
      const arr = r.value as Date[];
      expect(arr[0]).toBeInstanceOf(Date);
    }
  });
});
