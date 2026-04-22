// BDD: wire formatting is the HTTP boundary — it converts shape-applied
// runtime values into strings for headers, URL params, form-data, and JSON.
// Date objects become ISO strings; nulls pass through.

import { describe, it, expect } from "vitest";
import { formatForWire } from "../core/wire-format";
import type { Shape } from "../types";

const S = {
  string: { kind: "string" } as Shape,
  number: { kind: "number" } as Shape,
  date: { kind: "date" } as Shape,
  nullableDate: { kind: "nullable", inner: { kind: "date" } } as Shape,
  none: { kind: "none" } as Shape,
};

describe("formatForWire — Date values become ISO strings", () => {
  it("converts a Date with Shape.Date to an ISO string", () => {
    const d = new Date(Date.UTC(2026, 8, 15));
    expect(formatForWire(d, S.date)).toBe("2026-09-15T00:00:00.000Z");
  });

  it("converts a Date with Shape.Nullable(Date) to an ISO string", () => {
    const d = new Date(Date.UTC(2026, 8, 15));
    expect(formatForWire(d, S.nullableDate)).toBe("2026-09-15T00:00:00.000Z");
  });
});

describe("formatForWire — null under Nullable(Date) stays null", () => {
  it("returns null for null input under Shape.Nullable(Date)", () => {
    expect(formatForWire(null, S.nullableDate)).toBeNull();
  });
});

describe("formatForWire — passthrough shapes", () => {
  it("returns unchanged value for Shape.None", () => {
    expect(formatForWire("anything", S.none)).toBe("anything");
  });

  it("returns a string unchanged for Shape.String", () => {
    expect(formatForWire("hello", S.string)).toBe("hello");
  });

  it("returns a number unchanged for Shape.Number", () => {
    expect(formatForWire(42, S.number)).toBe(42);
  });
});
