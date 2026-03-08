import { describe, it, expect } from "vitest";
import type { Guard, ExecContext } from "../types";
import { evaluateGuard } from "../conditions";

function ctx(evt: Record<string, unknown>): ExecContext {
  return { evt };
}

describe("when evaluating guards", () => {
  // -- Comparison operators -----------------------------------------------

  describe("eq operator", () => {
    it("returns true when string values match", () => {
      const guard: Guard = { kind: "value", source: "evt.status", coerceAs: "string", op: "eq", operand: "active" };
      expect(evaluateGuard(guard, ctx({ status: "active" }))).toBe(true);
    });

    it("returns false when string values mismatch", () => {
      const guard: Guard = { kind: "value", source: "evt.status", coerceAs: "string", op: "eq", operand: "active" };
      expect(evaluateGuard(guard, ctx({ status: "inactive" }))).toBe(false);
    });
  });

  describe("neq operator", () => {
    it("returns true when string values differ", () => {
      const guard: Guard = { kind: "value", source: "evt.role", coerceAs: "string", op: "neq", operand: "admin" };
      expect(evaluateGuard(guard, ctx({ role: "user" }))).toBe(true);
    });

    it("returns false when string values are equal", () => {
      const guard: Guard = { kind: "value", source: "evt.role", coerceAs: "string", op: "neq", operand: "admin" };
      expect(evaluateGuard(guard, ctx({ role: "admin" }))).toBe(false);
    });
  });

  describe("gt operator", () => {
    it("returns true when value is greater", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gt", operand: 80 };
      expect(evaluateGuard(guard, ctx({ score: 95 }))).toBe(true);
    });

    it("returns false at boundary (equal)", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gt", operand: 80 };
      expect(evaluateGuard(guard, ctx({ score: 80 }))).toBe(false);
    });

    it("returns false when value is less", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gt", operand: 80 };
      expect(evaluateGuard(guard, ctx({ score: 50 }))).toBe(false);
    });
  });

  describe("gte operator", () => {
    it("returns true when value is above threshold", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 };
      expect(evaluateGuard(guard, ctx({ score: 95 }))).toBe(true);
    });

    it("returns true at boundary (equal)", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 };
      expect(evaluateGuard(guard, ctx({ score: 90 }))).toBe(true);
    });

    it("returns false when value is below threshold", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 };
      expect(evaluateGuard(guard, ctx({ score: 85 }))).toBe(false);
    });
  });

  describe("lt operator", () => {
    it("returns true when value is less", () => {
      const guard: Guard = { kind: "value", source: "evt.count", coerceAs: "number", op: "lt", operand: 10 };
      expect(evaluateGuard(guard, ctx({ count: 5 }))).toBe(true);
    });

    it("returns false at boundary (equal)", () => {
      const guard: Guard = { kind: "value", source: "evt.count", coerceAs: "number", op: "lt", operand: 10 };
      expect(evaluateGuard(guard, ctx({ count: 10 }))).toBe(false);
    });
  });

  describe("lte operator", () => {
    it("returns true when value is at boundary", () => {
      const guard: Guard = { kind: "value", source: "evt.count", coerceAs: "number", op: "lte", operand: 10 };
      expect(evaluateGuard(guard, ctx({ count: 10 }))).toBe(true);
    });

    it("returns true when value is below boundary", () => {
      const guard: Guard = { kind: "value", source: "evt.count", coerceAs: "number", op: "lte", operand: 10 };
      expect(evaluateGuard(guard, ctx({ count: 3 }))).toBe(true);
    });

    it("returns false when value is above boundary", () => {
      const guard: Guard = { kind: "value", source: "evt.count", coerceAs: "number", op: "lte", operand: 10 };
      expect(evaluateGuard(guard, ctx({ count: 15 }))).toBe(false);
    });
  });

  // -- Presence operators -------------------------------------------------

  describe("truthy operator", () => {
    it("returns true for non-empty string", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "raw", op: "truthy" };
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(true);
    });

    it("returns false for empty string", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "raw", op: "truthy" };
      expect(evaluateGuard(guard, ctx({ name: "" }))).toBe(false);
    });

    it("returns false for null", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "raw", op: "truthy" };
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(false);
    });
  });

  describe("falsy operator", () => {
    it("returns true for empty string", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "raw", op: "falsy" };
      expect(evaluateGuard(guard, ctx({ name: "" }))).toBe(true);
    });

    it("returns true for null", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "raw", op: "falsy" };
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(true);
    });

    it("returns false for non-empty string", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "raw", op: "falsy" };
      expect(evaluateGuard(guard, ctx({ name: "hello" }))).toBe(false);
    });
  });

  describe("is-null operator", () => {
    it("returns true for null", () => {
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "raw", op: "is-null" };
      expect(evaluateGuard(guard, ctx({ value: null }))).toBe(true);
    });

    it("returns true for undefined (missing key)", () => {
      const guard: Guard = { kind: "value", source: "evt.missing", coerceAs: "raw", op: "is-null" };
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });

    it("returns false for zero", () => {
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "raw", op: "is-null" };
      expect(evaluateGuard(guard, ctx({ value: 0 }))).toBe(false);
    });

    it("returns false for empty string", () => {
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "raw", op: "is-null" };
      expect(evaluateGuard(guard, ctx({ value: "" }))).toBe(false);
    });
  });

  describe("not-null operator", () => {
    it("returns true for zero (is present)", () => {
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "raw", op: "not-null" };
      expect(evaluateGuard(guard, ctx({ value: 0 }))).toBe(true);
    });

    it("returns true for empty string (is present)", () => {
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "raw", op: "not-null" };
      expect(evaluateGuard(guard, ctx({ value: "" }))).toBe(true);
    });

    it("returns false for null", () => {
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "raw", op: "not-null" };
      expect(evaluateGuard(guard, ctx({ value: null }))).toBe(false);
    });

    it("returns false for undefined (missing key)", () => {
      const guard: Guard = { kind: "value", source: "evt.missing", coerceAs: "raw", op: "not-null" };
      expect(evaluateGuard(guard, ctx({}))).toBe(false);
    });
  });

  // -- Logical composition ------------------------------------------------

  describe("all (AND) composition", () => {
    it("returns true when all guards pass", () => {
      const guard: Guard = {
        kind: "all",
        guards: [
          { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
          { kind: "value", source: "evt.status", coerceAs: "string", op: "eq", operand: "active" },
        ],
      };
      expect(evaluateGuard(guard, ctx({ score: 95, status: "active" }))).toBe(true);
    });

    it("returns false when one guard fails", () => {
      const guard: Guard = {
        kind: "all",
        guards: [
          { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
          { kind: "value", source: "evt.status", coerceAs: "string", op: "eq", operand: "active" },
        ],
      };
      expect(evaluateGuard(guard, ctx({ score: 95, status: "inactive" }))).toBe(false);
    });
  });

  describe("any (OR) composition", () => {
    it("returns true when one guard passes", () => {
      const guard: Guard = {
        kind: "any",
        guards: [
          { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "admin" },
          { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "superuser" },
        ],
      };
      expect(evaluateGuard(guard, ctx({ role: "superuser" }))).toBe(true);
    });

    it("returns false when no guard passes", () => {
      const guard: Guard = {
        kind: "any",
        guards: [
          { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "admin" },
          { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "superuser" },
        ],
      };
      expect(evaluateGuard(guard, ctx({ role: "viewer" }))).toBe(false);
    });
  });

  describe("nested all-inside-any composition", () => {
    it("passes when any nested all-group passes", () => {
      const guard: Guard = {
        kind: "any",
        guards: [
          {
            kind: "all",
            guards: [
              { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "admin" },
              { kind: "value", source: "evt.level", coerceAs: "number", op: "gte", operand: 5 },
            ],
          },
          {
            kind: "all",
            guards: [
              { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "superuser" },
              { kind: "value", source: "evt.level", coerceAs: "number", op: "gte", operand: 3 },
            ],
          },
        ],
      };
      // First all-group fails (role is superuser, not admin), second passes
      expect(evaluateGuard(guard, ctx({ role: "superuser", level: 4 }))).toBe(true);
    });
  });

  // -- Coercion integration -----------------------------------------------

  describe("coercion integration", () => {
    it("coerces string score to number for comparison", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 };
      // score is a string "95" — coercion to number makes comparison work
      expect(evaluateGuard(guard, ctx({ score: "95" }))).toBe(true);
    });

    it("coerces string boolean for truthy check", () => {
      const guard: Guard = { kind: "value", source: "evt.active", coerceAs: "boolean", op: "truthy" };
      // "true" as string → boolean true via coerce → truthy passes
      expect(evaluateGuard(guard, ctx({ active: "true" }))).toBe(true);
    });
  });

  // -- Operand coercion (C2 + I5 fix) --------------------------------------

  describe("operand coercion", () => {
    it("coerces string operand to number for eq comparison", () => {
      // Source "95" coerced to 95 (number). Operand 95 already number.
      // Both go through coerce("number") → match.
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "eq", operand: 95 };
      expect(evaluateGuard(guard, ctx({ score: "95" }))).toBe(true);
    });

    it("coerces number operand for string source in eq", () => {
      // Source 42, coerceAs "string" → "42". Operand "42" → "42". Match.
      const guard: Guard = { kind: "value", source: "evt.code", coerceAs: "string", op: "eq", operand: "42" };
      expect(evaluateGuard(guard, ctx({ code: 42 }))).toBe(true);
    });

    it("coerces operand for gte comparison", () => {
      // Source "100" → 100, operand "90" → 90. 100 >= 90 → true.
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: "90" };
      expect(evaluateGuard(guard, ctx({ score: "100" }))).toBe(true);
    });

    it("coerces operand for lt comparison", () => {
      const guard: Guard = { kind: "value", source: "evt.count", coerceAs: "number", op: "lt", operand: "10" };
      expect(evaluateGuard(guard, ctx({ count: "5" }))).toBe(true);
    });
  });

  // -- Presence operators with typed coercion (is-null uses raw) ----------

  describe("is-null with typed coerceAs", () => {
    it("correctly detects null even when coerceAs is string", () => {
      // Without the raw resolution fix, coerce(null, "string") → "" → not null. WRONG.
      // With the fix, is-null uses raw resolution, so null is correctly detected.
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "string", op: "is-null" };
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(true);
    });

    it("correctly detects null even when coerceAs is number", () => {
      // Without fix: coerce(null, "number") → 0 → not null. WRONG.
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "number", op: "is-null" };
      expect(evaluateGuard(guard, ctx({ value: null }))).toBe(true);
    });

    it("correctly detects non-null with typed coercion", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "string", op: "not-null" };
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(true);
    });

    it("correctly detects undefined as null with typed coercion", () => {
      const guard: Guard = { kind: "value", source: "evt.missing", coerceAs: "number", op: "is-null" };
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });
  });

  // -- Date coercion (ISO string → timestamp) ----------------------------

  describe("date coercion", () => {
    it("compares ISO date strings via gt", () => {
      // "2026-07-15" > "2026-06-01" → both coerced to timestamps → true
      const guard: Guard = { kind: "value", source: "evt.deadline", coerceAs: "date", op: "gt", operand: "2026-06-01T00:00:00" };
      expect(evaluateGuard(guard, ctx({ deadline: "2026-07-15T00:00:00" }))).toBe(true);
    });

    it("returns false when date is before operand", () => {
      const guard: Guard = { kind: "value", source: "evt.deadline", coerceAs: "date", op: "gt", operand: "2026-06-01T00:00:00" };
      expect(evaluateGuard(guard, ctx({ deadline: "2026-01-15T00:00:00" }))).toBe(false);
    });

    it("supports lte on dates", () => {
      const guard: Guard = { kind: "value", source: "evt.deadline", coerceAs: "date", op: "lte", operand: "2026-06-01T00:00:00" };
      expect(evaluateGuard(guard, ctx({ deadline: "2026-06-01T00:00:00" }))).toBe(true);
    });

    it("supports eq on dates", () => {
      const guard: Guard = { kind: "value", source: "evt.deadline", coerceAs: "date", op: "eq", operand: "2026-06-01T00:00:00" };
      expect(evaluateGuard(guard, ctx({ deadline: "2026-06-01T00:00:00" }))).toBe(true);
    });

    it("returns NaN for null date (is-null uses raw, not coerced)", () => {
      const guard: Guard = { kind: "value", source: "evt.deadline", coerceAs: "date", op: "is-null" };
      expect(evaluateGuard(guard, ctx({ deadline: null }))).toBe(true);
    });
  });

  // -- Long/float as number coercion ------------------------------------

  describe("long and float as number", () => {
    it("compares large numbers (long equivalent) via gt", () => {
      const guard: Guard = { kind: "value", source: "evt.balance", coerceAs: "number", op: "gt", operand: 1000000 };
      expect(evaluateGuard(guard, ctx({ balance: 5000000 }))).toBe(true);
    });

    it("compares float values via lte", () => {
      const guard: Guard = { kind: "value", source: "evt.rate", coerceAs: "number", op: "lte", operand: 0.5 };
      expect(evaluateGuard(guard, ctx({ rate: 0.3 }))).toBe(true);
    });

    it("compares decimal-precision doubles via gt", () => {
      const guard: Guard = { kind: "value", source: "evt.temp", coerceAs: "number", op: "gt", operand: 98.6 };
      expect(evaluateGuard(guard, ctx({ temp: 101.3 }))).toBe(true);
    });
  });

  // -- Edge cases ---------------------------------------------------------

  describe("edge cases", () => {
    it("returns false for eq when source path is missing", () => {
      const guard: Guard = { kind: "value", source: "evt.nonexistent", coerceAs: "raw", op: "eq", operand: "hello" };
      expect(evaluateGuard(guard, ctx({}))).toBe(false);
    });

    it("returns true for is-null when source path is missing", () => {
      const guard: Guard = { kind: "value", source: "evt.nonexistent", coerceAs: "raw", op: "is-null" };
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });

    it("throws on unknown guard kind", () => {
      const guard = { kind: "unknown", source: "evt.x", coerceAs: "raw", op: "eq", operand: 1 } as any;
      expect(() => evaluateGuard(guard, ctx({}))).toThrow("Unknown guard kind: unknown");
    });

    it("throws on unknown operator", () => {
      const guard: any = { kind: "value", source: "evt.x", coerceAs: "raw", op: "nope" };
      expect(() => evaluateGuard(guard, ctx({ x: 1 }))).toThrow("Unknown guard operator: nope");
    });
  });
});
