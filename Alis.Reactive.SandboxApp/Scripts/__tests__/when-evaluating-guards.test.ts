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
  });
});
