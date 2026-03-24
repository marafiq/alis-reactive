/**
 * FAILING TESTS — prove the coercion gaps exist.
 *
 * These tests express the DESIRED behavior of the coerce module.
 * They FAIL today because:
 *   1. toString() doesn't handle Date, Array, or plain Object
 *   2. Gather never calls coerce — uses raw String()
 *   3. Text operators inherit whatever coerceAs was set, even if nonsensical
 *
 * Once the coerce module is made first-class, every test here will pass.
 */
import { describe, it, expect } from "vitest";
import { coerce, toString, toDate } from "../core/coerce";

// ═══════════════════════════════════════════════════════════════
// Gap 1: toString() with Date objects
//
// toString(new Date("2024-03-15T00:00:00Z"))
//   ACTUAL:   "Sat Mar 15 2024 19:00:00 GMT+..." (locale garbage)
//   EXPECTED: "2024-03-15T00:00:00.000Z" (ISO 8601 — server-ready)
//
// This breaks: gather FormData, gather GET params, text operators
// on Date sources, and any display that goes through toString.
// ═══════════════════════════════════════════════════════════════

describe("toString with Date objects", () => {
  it("converts Date to ISO 8601 string", () => {
    const d = new Date("2024-03-15T00:00:00Z");
    expect(toString(d)).toBe("2024-03-15T00:00:00.000Z");
  });

  it("converts local-midnight Date to ISO string", () => {
    const d = new Date(2024, 2, 15); // March 15, 2024 local
    const result = toString(d);
    // Must be a valid ISO string, not locale-dependent garbage
    expect(result).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/);
  });

  it("round-trips: toString(date) → toDate(isoString) === date.getTime()", () => {
    const d = new Date("2024-03-15T08:30:00Z");
    const str = toString(d);
    const back = toDate(str);
    expect(back).toBe(d.getTime());
  });
});

// ═══════════════════════════════════════════════════════════════
// Gap 2: toString() with plain objects — fail-fast, not silent
//
// toString({ name: "John" })
//   ACTUAL:   "[object Object]" (silent data loss)
//   EXPECTED: throw (fail-fast rule #10)
//
// A plain object reaching toString means the plan is misconfigured
// (wrong readExpr, missing coerceAs). Producing "[object Object]"
// hides the bug. Throwing exposes it immediately.
// ═══════════════════════════════════════════════════════════════

describe("toString with plain objects — fail-fast", () => {
  it("throws on plain object instead of producing [object Object]", () => {
    expect(() => toString({ name: "John" })).toThrow();
  });

  it("throws on nested object", () => {
    expect(() => toString({ address: { city: "NYC" } })).toThrow();
  });
});

// ═══════════════════════════════════════════════════════════════
// Gap 3: coerce("string") dispatches through toString
//         so Date and object gaps propagate to all callers
// ═══════════════════════════════════════════════════════════════

describe("coerce string with Date", () => {
  it("coerce(date, 'string') produces ISO string", () => {
    const d = new Date("2024-03-15T00:00:00Z");
    expect(coerce(d, "string")).toBe("2024-03-15T00:00:00.000Z");
  });
});

describe("coerce string with plain object", () => {
  it("coerce(obj, 'string') throws on plain object", () => {
    expect(() => coerce({ x: 1 }, "string")).toThrow();
  });
});
