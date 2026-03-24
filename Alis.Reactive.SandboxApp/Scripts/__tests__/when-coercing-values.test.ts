import { describe, it, expect } from "vitest";
import { coerce, toString, toNumber, toBoolean, toDate, toArray } from "../core/coerce";

describe("when coercing values", () => {

  // ── toString ──────────────────────────────────────────────

  describe("toString", () => {
    it("converts null to empty string", () => {
      expect(toString(null)).toBe("");
    });

    it("converts undefined to empty string", () => {
      expect(toString(undefined)).toBe("");
    });

    it("converts number to string", () => {
      expect(toString(42)).toBe("42");
    });

    it("converts zero to string", () => {
      expect(toString(0)).toBe("0");
    });

    it("converts boolean true to string", () => {
      expect(toString(true)).toBe("true");
    });

    it("converts boolean false to string", () => {
      expect(toString(false)).toBe("false");
    });

    it("passes string through", () => {
      expect(toString("hello")).toBe("hello");
    });

    it("converts empty string to empty string", () => {
      expect(toString("")).toBe("");
    });

    it("converts NaN to string", () => {
      expect(toString(NaN)).toBe("NaN");
    });

    it("Date round-trips through toString and toDate (ISO is the canonical form)", () => {
      const d = new Date("2024-03-15T08:30:00Z");
      expect(toDate(toString(d))).toBe(d.getTime());
    });

    it("throws on plain object — exposes misconfigured plan immediately", () => {
      expect(() => toString({ name: "John" })).toThrow(/plain object/);
    });

    it("converts array to JSON string preserving structure", () => {
      expect(toString(["Peanuts", "Dairy"])).toBe('["Peanuts","Dairy"]');
    });
  });

  // ── toNumber ──────────────────────────────────────────────

  describe("toNumber", () => {
    it("converts null to 0", () => {
      expect(toNumber(null)).toBe(0);
    });

    it("converts undefined to 0", () => {
      expect(toNumber(undefined)).toBe(0);
    });

    it("converts numeric string to number", () => {
      expect(toNumber("42")).toBe(42);
    });

    it("converts decimal string to number", () => {
      expect(toNumber("42.5")).toBe(42.5);
    });

    it("converts negative string to number", () => {
      expect(toNumber("-10")).toBe(-10);
    });

    it("converts non-numeric string to 0", () => {
      expect(toNumber("not a number")).toBe(0);
    });

    it("converts empty string to 0", () => {
      expect(toNumber("")).toBe(0);
    });

    it("converts boolean true to 1", () => {
      expect(toNumber(true)).toBe(1);
    });

    it("converts boolean false to 0", () => {
      expect(toNumber(false)).toBe(0);
    });

    it("passes number through", () => {
      expect(toNumber(99)).toBe(99);
    });

    it("converts Infinity to Infinity", () => {
      expect(toNumber(Infinity)).toBe(Infinity);
    });

    it("converts string Infinity to Infinity", () => {
      expect(toNumber("Infinity")).toBe(Infinity);
    });
  });

  // ── toBoolean ─────────────────────────────────────────────

  describe("toBoolean", () => {
    // String-specific coercion (HTML form values)
    it("converts 'false' to false", () => {
      expect(toBoolean("false")).toBe(false);
    });

    it("converts '0' to false", () => {
      expect(toBoolean("0")).toBe(false);
    });

    it("converts empty string to false", () => {
      expect(toBoolean("")).toBe(false);
    });

    it("converts 'true' to true", () => {
      expect(toBoolean("true")).toBe(true);
    });

    it("converts '1' to true", () => {
      expect(toBoolean("1")).toBe(true);
    });

    it("converts any non-empty string to true", () => {
      expect(toBoolean("anything")).toBe(true);
    });

    it("converts 'False' (capitalized) to true — only lowercase 'false' is false", () => {
      expect(toBoolean("False")).toBe(true);
    });

    // Non-string coercion (standard Boolean)
    it("converts null to false", () => {
      expect(toBoolean(null)).toBe(false);
    });

    it("converts undefined to false", () => {
      expect(toBoolean(undefined)).toBe(false);
    });

    it("converts 0 to false", () => {
      expect(toBoolean(0)).toBe(false);
    });

    it("converts NaN to false", () => {
      expect(toBoolean(NaN)).toBe(false);
    });

    it("converts 1 to true", () => {
      expect(toBoolean(1)).toBe(true);
    });

    it("converts object to true", () => {
      expect(toBoolean({})).toBe(true);
    });

    it("converts empty array to true", () => {
      expect(toBoolean([])).toBe(true);
    });
  });

  // ── toDate ────────────────────────────────────────────────

  describe("toDate", () => {
    it("converts null to NaN", () => {
      expect(toDate(null)).toBeNaN();
    });

    it("converts undefined to NaN", () => {
      expect(toDate(undefined)).toBeNaN();
    });

    it("converts garbage string to NaN", () => {
      expect(toDate("not-a-date")).toBeNaN();
    });

    it("converts empty string to NaN", () => {
      expect(toDate("")).toBeNaN();
    });

    // Date objects (Syncfusion components return these)
    it("passes Date object through via getTime()", () => {
      const d = new Date(2025, 2, 19); // March 19, 2025 local
      expect(toDate(d)).toBe(d.getTime());
    });

    // Date-only strings — the critical off-by-one test
    it("parses YYYY-MM-DD as LOCAL midnight, not UTC", () => {
      const result = toDate("2025-03-19");
      const expected = new Date(2025, 2, 19).getTime(); // local midnight
      expect(result).toBe(expected);
    });

    it("date-only string matches Date object for same date", () => {
      const fromString = toDate("2025-03-19");
      const fromObject = toDate(new Date(2025, 2, 19));
      expect(fromString).toBe(fromObject);
    });

    it("parses different date-only strings to different timestamps", () => {
      expect(toDate("2025-03-19")).not.toBe(toDate("2025-03-20"));
    });

    // ISO datetime strings
    it("parses ISO datetime with timezone", () => {
      const result = toDate("2025-03-19T14:30:00Z");
      expect(result).toBe(new Date("2025-03-19T14:30:00Z").getTime());
    });

    it("parses ISO datetime without timezone as local", () => {
      const result = toDate("2025-03-19T14:30:00");
      expect(result).toBe(new Date("2025-03-19T14:30:00").getTime());
    });

    // Timestamps
    it("converts numeric timestamp to same timestamp", () => {
      const ts = Date.now();
      expect(toDate(ts)).toBe(ts);
    });
  });

  // ── toArray ───────────────────────────────────────────────

  describe("toArray", () => {
    it("passes array through", () => {
      const arr = [1, 2, 3];
      expect(toArray(arr)).toBe(arr); // same reference
    });

    it("passes empty array through", () => {
      const arr: unknown[] = [];
      expect(toArray(arr)).toBe(arr);
    });

    it("converts null to empty array", () => {
      expect(toArray(null)).toEqual([]);
    });

    it("converts undefined to empty array", () => {
      expect(toArray(undefined)).toEqual([]);
    });

    it("converts empty string to empty array", () => {
      expect(toArray("")).toEqual([]);
    });

    it("wraps string in array", () => {
      expect(toArray("hello")).toEqual(["hello"]);
    });

    it("wraps number in array", () => {
      expect(toArray(42)).toEqual([42]);
    });

    it("wraps object in array", () => {
      const obj = { a: 1 };
      expect(toArray(obj)).toEqual([obj]);
    });

    it("wraps false in array (not empty)", () => {
      expect(toArray(false)).toEqual([false]);
    });

    it("wraps 0 in array (not empty)", () => {
      expect(toArray(0)).toEqual([0]);
    });
  });

  // ── coerce() dispatcher ───────────────────────────────────

  describe("coerce dispatch", () => {
    it("dispatches string to toString", () => {
      expect(coerce(42, "string")).toBe("42");
    });

    it("dispatches number to toNumber", () => {
      expect(coerce("42", "number")).toBe(42);
    });

    it("dispatches boolean to toBoolean", () => {
      expect(coerce("false", "boolean")).toBe(false);
    });

    it("dispatches date to toDate", () => {
      const result = coerce("2025-03-19", "date") as number;
      expect(result).toBe(new Date(2025, 2, 19).getTime());
    });

    it("dispatches array to toArray", () => {
      expect(coerce("hello", "array")).toEqual(["hello"]);
    });

    it("dispatches raw as passthrough", () => {
      const obj = { complex: true };
      expect(coerce(obj, "raw")).toBe(obj);
    });

    it("raw preserves null", () => {
      expect(coerce(null, "raw")).toBe(null);
    });

    it("raw preserves undefined", () => {
      expect(coerce(undefined, "raw")).toBe(undefined);
    });
  });
});
