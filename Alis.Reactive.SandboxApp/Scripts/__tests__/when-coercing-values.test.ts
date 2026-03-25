import { describe, it, expect } from "vitest";
import { coerce, coerceOrThrow, toString, toNumber, toBoolean, toDate, toArray } from "../core/coerce";

describe("when coercing values", () => {

  // ── toString ──────────────────────────────────────────────

  describe("toString", () => {
    it("converts null to empty string", () => {
      expect(toString(null)).toEqual({ ok: true, value: "" });
    });

    it("converts undefined to empty string", () => {
      expect(toString(undefined)).toEqual({ ok: true, value: "" });
    });

    it("converts number to string", () => {
      expect(toString(42)).toEqual({ ok: true, value: "42" });
    });

    it("converts zero to string", () => {
      expect(toString(0)).toEqual({ ok: true, value: "0" });
    });

    it("converts boolean true to string", () => {
      expect(toString(true)).toEqual({ ok: true, value: "true" });
    });

    it("converts boolean false to string", () => {
      expect(toString(false)).toEqual({ ok: true, value: "false" });
    });

    it("passes string through", () => {
      expect(toString("hello")).toEqual({ ok: true, value: "hello" });
    });

    it("converts empty string to empty string", () => {
      expect(toString("")).toEqual({ ok: true, value: "" });
    });

    it("converts NaN to string", () => {
      expect(toString(NaN)).toEqual({ ok: true, value: "NaN" });
    });

    it("Date round-trips through toString and toDate (ISO is the canonical form)", () => {
      const d = new Date("2024-03-15T08:30:00Z");
      const strResult = toString(d);
      expect(strResult.ok).toBe(true);
      if (strResult.ok) {
        const dateResult = toDate(strResult.value);
        expect(dateResult.ok).toBe(true);
        if (dateResult.ok) expect(dateResult.value).toBe(d.getTime());
      }
    });

    it("throws on plain object — exposes misconfigured plan immediately", () => {
      expect(toString({ name: "John" }).ok).toBe(false);
    });

    it("converts array to JSON string preserving structure", () => {
      expect(toString(["Peanuts", "Dairy"])).toEqual({ ok: true, value: '["Peanuts","Dairy"]' });
    });
  });

  // ── toNumber ──────────────────────────────────────────────

  describe("toNumber", () => {
    it("converts null to 0", () => {
      expect(toNumber(null)).toEqual({ ok: true, value: 0 });
    });

    it("converts undefined to 0", () => {
      expect(toNumber(undefined)).toEqual({ ok: true, value: 0 });
    });

    it("converts numeric string to number", () => {
      expect(toNumber("42")).toEqual({ ok: true, value: 42 });
    });

    it("converts decimal string to number", () => {
      expect(toNumber("42.5")).toEqual({ ok: true, value: 42.5 });
    });

    it("converts negative string to number", () => {
      expect(toNumber("-10")).toEqual({ ok: true, value: -10 });
    });

    it("converts non-numeric string to 0", () => {
      expect(toNumber("not a number")).toEqual({ ok: true, value: 0 });
    });

    it("converts empty string to 0", () => {
      expect(toNumber("")).toEqual({ ok: true, value: 0 });
    });

    it("converts boolean true to 1", () => {
      expect(toNumber(true)).toEqual({ ok: true, value: 1 });
    });

    it("converts boolean false to 0", () => {
      expect(toNumber(false)).toEqual({ ok: true, value: 0 });
    });

    it("passes number through", () => {
      expect(toNumber(99)).toEqual({ ok: true, value: 99 });
    });

    it("converts Infinity to Infinity", () => {
      expect(toNumber(Infinity)).toEqual({ ok: true, value: Infinity });
    });

    it("converts string Infinity to Infinity", () => {
      expect(toNumber("Infinity")).toEqual({ ok: true, value: Infinity });
    });
  });

  // ── toBoolean ─────────────────────────────────────────────

  describe("toBoolean", () => {
    // String-specific coercion (HTML form values)
    it("converts 'false' to false", () => {
      expect(toBoolean("false")).toEqual({ ok: true, value: false });
    });

    it("converts '0' to false", () => {
      expect(toBoolean("0")).toEqual({ ok: true, value: false });
    });

    it("converts empty string to false", () => {
      expect(toBoolean("")).toEqual({ ok: true, value: false });
    });

    it("converts 'true' to true", () => {
      expect(toBoolean("true")).toEqual({ ok: true, value: true });
    });

    it("converts '1' to true", () => {
      expect(toBoolean("1")).toEqual({ ok: true, value: true });
    });

    it("converts any non-empty string to true", () => {
      expect(toBoolean("anything")).toEqual({ ok: true, value: true });
    });

    it("converts 'False' (capitalized) to true — only lowercase 'false' is false", () => {
      expect(toBoolean("False")).toEqual({ ok: true, value: true });
    });

    // Non-string coercion (standard Boolean)
    it("converts null to false", () => {
      expect(toBoolean(null)).toEqual({ ok: true, value: false });
    });

    it("converts undefined to false", () => {
      expect(toBoolean(undefined)).toEqual({ ok: true, value: false });
    });

    it("converts 0 to false", () => {
      expect(toBoolean(0)).toEqual({ ok: true, value: false });
    });

    it("converts NaN to false", () => {
      expect(toBoolean(NaN)).toEqual({ ok: true, value: false });
    });

    it("converts 1 to true", () => {
      expect(toBoolean(1)).toEqual({ ok: true, value: true });
    });

    it("throws on plain object — fail-fast for misconfigured plan", () => {
      expect(toBoolean({}).ok).toBe(false);
    });

    it("converts empty array to false (empty = no value)", () => {
      expect(toBoolean([])).toEqual({ ok: true, value: false });
    });

    it("converts non-empty array to true", () => {
      expect(toBoolean([1, 2])).toEqual({ ok: true, value: true });
    });
  });

  // ── toDate ────────────────────────────────────────────────

  describe("toDate", () => {
    it("converts null to NaN", () => {
      expect(toDate(null)).toEqual({ ok: true, value: NaN });
    });

    it("converts undefined to NaN", () => {
      expect(toDate(undefined)).toEqual({ ok: true, value: NaN });
    });

    it("converts garbage string to NaN", () => {
      expect(toDate("not-a-date")).toEqual({ ok: true, value: NaN });
    });

    it("converts empty string to NaN", () => {
      expect(toDate("")).toEqual({ ok: true, value: NaN });
    });

    // Date objects (Syncfusion components return these)
    it("passes Date object through via getTime()", () => {
      const d = new Date(2025, 2, 19); // March 19, 2025 local
      expect(toDate(d)).toEqual({ ok: true, value: d.getTime() });
    });

    // Date-only strings — the critical off-by-one test
    it("parses YYYY-MM-DD as LOCAL midnight, not UTC", () => {
      const result = toDate("2025-03-19");
      const expected = new Date(2025, 2, 19).getTime(); // local midnight
      expect(result).toEqual({ ok: true, value: expected });
    });

    it("date-only string matches Date object for same date", () => {
      const fromString = toDate("2025-03-19");
      const fromObject = toDate(new Date(2025, 2, 19));
      expect(fromString).toEqual(fromObject);
    });

    it("parses different date-only strings to different timestamps", () => {
      const r1 = toDate("2025-03-19");
      const r2 = toDate("2025-03-20");
      expect(r1.ok).toBe(true);
      expect(r2.ok).toBe(true);
      if (r1.ok && r2.ok) expect(r1.value).not.toBe(r2.value);
    });

    // ISO datetime strings
    it("parses ISO datetime with timezone", () => {
      const result = toDate("2025-03-19T14:30:00Z");
      expect(result).toEqual({ ok: true, value: new Date("2025-03-19T14:30:00Z").getTime() });
    });

    it("parses ISO datetime without timezone as local", () => {
      const result = toDate("2025-03-19T14:30:00");
      expect(result).toEqual({ ok: true, value: new Date("2025-03-19T14:30:00").getTime() });
    });

    // Timestamps
    it("converts numeric timestamp to same timestamp", () => {
      const ts = Date.now();
      expect(toDate(ts)).toEqual({ ok: true, value: ts });
    });
  });

  // ── toArray ───────────────────────────────────────────────

  describe("toArray", () => {
    it("passes array through", () => {
      const arr = [1, 2, 3];
      const r = toArray(arr);
      expect(r.ok).toBe(true);
      if (r.ok) expect(r.value).toBe(arr); // same reference
    });

    it("passes empty array through", () => {
      const arr: unknown[] = [];
      const r = toArray(arr);
      expect(r.ok).toBe(true);
      if (r.ok) expect(r.value).toBe(arr);
    });

    it("converts null to empty array", () => {
      expect(toArray(null)).toEqual({ ok: true, value: [] });
    });

    it("converts undefined to empty array", () => {
      expect(toArray(undefined)).toEqual({ ok: true, value: [] });
    });

    it("converts empty string to empty array", () => {
      expect(toArray("")).toEqual({ ok: true, value: [] });
    });

    it("wraps string in array", () => {
      expect(toArray("hello")).toEqual({ ok: true, value: ["hello"] });
    });

    it("wraps number in array", () => {
      expect(toArray(42)).toEqual({ ok: true, value: [42] });
    });

    it("throws on plain object — walk should decompose via readExpr", () => {
      expect(toArray({ a: 1 }).ok).toBe(false);
    });

    it("wraps Date in single-element array", () => {
      const d = new Date("2024-03-15T00:00:00Z");
      expect(toArray(d)).toEqual({ ok: true, value: [d] });
    });

    it("wraps false in array (not empty)", () => {
      expect(toArray(false)).toEqual({ ok: true, value: [false] });
    });

    it("wraps 0 in array (not empty)", () => {
      expect(toArray(0)).toEqual({ ok: true, value: [0] });
    });
  });

  // ── coerce() dispatcher ───────────────────────────────────

  describe("coerce dispatch", () => {
    it("dispatches string to toString", () => {
      expect(coerce(42, "string")).toEqual({ ok: true, value: "42" });
    });

    it("dispatches number to toNumber", () => {
      expect(coerce("42", "number")).toEqual({ ok: true, value: 42 });
    });

    it("dispatches boolean to toBoolean", () => {
      expect(coerce("false", "boolean")).toEqual({ ok: true, value: false });
    });

    it("dispatches date to toDate", () => {
      expect(coerce("2025-03-19", "date")).toEqual({ ok: true, value: new Date(2025, 2, 19).getTime() });
    });

    it("dispatches array to toArray", () => {
      expect(coerce("hello", "array")).toEqual({ ok: true, value: ["hello"] });
    });

    it("dispatches raw as passthrough", () => {
      const obj = { complex: true };
      const r = coerce(obj, "raw");
      expect(r.ok).toBe(true);
      if (r.ok) expect(r.value).toBe(obj);
    });

    it("raw preserves null", () => {
      expect(coerce(null, "raw")).toEqual({ ok: true, value: null });
    });

    it("raw preserves undefined", () => {
      expect(coerce(undefined, "raw")).toEqual({ ok: true, value: undefined });
    });
  });

  // ── Result pattern ────────────────────────────────────────

  describe("Result pattern", () => {
    it("coerceOrThrow unwraps Ok value", () => {
      expect(coerceOrThrow("42", "number")).toBe(42);
    });

    it("coerceOrThrow throws on Err", () => {
      expect(() => coerceOrThrow({}, "string")).toThrow(/plain object/);
    });

    it("coerce dispatches and returns Result", () => {
      expect(coerce(42, "string")).toEqual({ ok: true, value: "42" });
      expect(coerce({}, "number").ok).toBe(false);
    });
  });
});
