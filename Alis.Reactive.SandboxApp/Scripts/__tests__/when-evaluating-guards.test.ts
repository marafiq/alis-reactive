import { describe, it, expect, vi } from "vitest";
import type { Guard, ExecContext } from "../types";
import { evaluateGuard, evaluateGuardAsync, isConfirmGuard } from "../conditions";

function ctx(evt: Record<string, unknown>): ExecContext {
  return { evt };
}

// Helper to create a ValueGuard with BindSource
function vg(path: string, coerceAs: string, op: string, operand?: unknown): Guard {
  return { kind: "value", source: { kind: "event", path }, coerceAs: coerceAs as any, op: op as any, operand };
}

describe("when evaluating guards", () => {
  // -- Comparison operators -----------------------------------------------

  describe("eq operator", () => {
    it("returns true when string values match", () => {
      const guard = vg("evt.status", "string", "eq", "active");
      expect(evaluateGuard(guard, ctx({ status: "active" }))).toBe(true);
    });

    it("returns false when string values mismatch", () => {
      const guard = vg("evt.status", "string", "eq", "active");
      expect(evaluateGuard(guard, ctx({ status: "inactive" }))).toBe(false);
    });
  });

  describe("neq operator", () => {
    it("returns true when string values differ", () => {
      const guard = vg("evt.role", "string", "neq", "admin");
      expect(evaluateGuard(guard, ctx({ role: "user" }))).toBe(true);
    });

    it("returns false when string values are equal", () => {
      const guard = vg("evt.role", "string", "neq", "admin");
      expect(evaluateGuard(guard, ctx({ role: "admin" }))).toBe(false);
    });
  });

  describe("gt operator", () => {
    it("returns true when value is greater", () => {
      const guard = vg("evt.score", "number", "gt", 80);
      expect(evaluateGuard(guard, ctx({ score: 95 }))).toBe(true);
    });

    it("returns false at boundary (equal)", () => {
      const guard = vg("evt.score", "number", "gt", 80);
      expect(evaluateGuard(guard, ctx({ score: 80 }))).toBe(false);
    });

    it("returns false when value is less", () => {
      const guard = vg("evt.score", "number", "gt", 80);
      expect(evaluateGuard(guard, ctx({ score: 50 }))).toBe(false);
    });
  });

  describe("gte operator", () => {
    it("returns true when value is above threshold", () => {
      const guard = vg("evt.score", "number", "gte", 90);
      expect(evaluateGuard(guard, ctx({ score: 95 }))).toBe(true);
    });

    it("returns true at boundary (equal)", () => {
      const guard = vg("evt.score", "number", "gte", 90);
      expect(evaluateGuard(guard, ctx({ score: 90 }))).toBe(true);
    });

    it("returns false when value is below threshold", () => {
      const guard = vg("evt.score", "number", "gte", 90);
      expect(evaluateGuard(guard, ctx({ score: 85 }))).toBe(false);
    });
  });

  describe("lt operator", () => {
    it("returns true when value is less", () => {
      const guard = vg("evt.count", "number", "lt", 10);
      expect(evaluateGuard(guard, ctx({ count: 5 }))).toBe(true);
    });

    it("returns false at boundary (equal)", () => {
      const guard = vg("evt.count", "number", "lt", 10);
      expect(evaluateGuard(guard, ctx({ count: 10 }))).toBe(false);
    });
  });

  describe("lte operator", () => {
    it("returns true when value is at boundary", () => {
      const guard = vg("evt.count", "number", "lte", 10);
      expect(evaluateGuard(guard, ctx({ count: 10 }))).toBe(true);
    });

    it("returns true when value is below boundary", () => {
      const guard = vg("evt.count", "number", "lte", 10);
      expect(evaluateGuard(guard, ctx({ count: 3 }))).toBe(true);
    });

    it("returns false when value is above boundary", () => {
      const guard = vg("evt.count", "number", "lte", 10);
      expect(evaluateGuard(guard, ctx({ count: 15 }))).toBe(false);
    });
  });

  // -- Presence operators -------------------------------------------------

  describe("truthy operator", () => {
    it("returns true for non-empty string", () => {
      const guard = vg("evt.name", "raw", "truthy");
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(true);
    });

    it("returns false for empty string", () => {
      const guard = vg("evt.name", "raw", "truthy");
      expect(evaluateGuard(guard, ctx({ name: "" }))).toBe(false);
    });

    it("returns false for null", () => {
      const guard = vg("evt.name", "raw", "truthy");
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(false);
    });
  });

  describe("falsy operator", () => {
    it("returns true for empty string", () => {
      const guard = vg("evt.name", "raw", "falsy");
      expect(evaluateGuard(guard, ctx({ name: "" }))).toBe(true);
    });

    it("returns true for null", () => {
      const guard = vg("evt.name", "raw", "falsy");
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(true);
    });

    it("returns false for non-empty string", () => {
      const guard = vg("evt.name", "raw", "falsy");
      expect(evaluateGuard(guard, ctx({ name: "hello" }))).toBe(false);
    });
  });

  describe("is-null operator", () => {
    it("returns true for null", () => {
      const guard = vg("evt.value", "raw", "is-null");
      expect(evaluateGuard(guard, ctx({ value: null }))).toBe(true);
    });

    it("returns true for undefined (missing key)", () => {
      const guard = vg("evt.missing", "raw", "is-null");
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });

    it("returns false for zero", () => {
      const guard = vg("evt.value", "raw", "is-null");
      expect(evaluateGuard(guard, ctx({ value: 0 }))).toBe(false);
    });

    it("returns false for empty string", () => {
      const guard = vg("evt.value", "raw", "is-null");
      expect(evaluateGuard(guard, ctx({ value: "" }))).toBe(false);
    });
  });

  describe("not-null operator", () => {
    it("returns true for zero (is present)", () => {
      const guard = vg("evt.value", "raw", "not-null");
      expect(evaluateGuard(guard, ctx({ value: 0 }))).toBe(true);
    });

    it("returns true for empty string (is present)", () => {
      const guard = vg("evt.value", "raw", "not-null");
      expect(evaluateGuard(guard, ctx({ value: "" }))).toBe(true);
    });

    it("returns false for null", () => {
      const guard = vg("evt.value", "raw", "not-null");
      expect(evaluateGuard(guard, ctx({ value: null }))).toBe(false);
    });

    it("returns false for undefined (missing key)", () => {
      const guard = vg("evt.missing", "raw", "not-null");
      expect(evaluateGuard(guard, ctx({}))).toBe(false);
    });
  });

  // -- NEW: is-empty / not-empty -------------------------------------------

  describe("is-empty operator", () => {
    it("returns true for empty string", () => {
      const guard = vg("evt.name", "raw", "is-empty");
      expect(evaluateGuard(guard, ctx({ name: "" }))).toBe(true);
    });

    it("returns true for null", () => {
      const guard = vg("evt.name", "raw", "is-empty");
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(true);
    });

    it("returns true for undefined", () => {
      const guard = vg("evt.name", "raw", "is-empty");
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });

    it("returns true for empty array", () => {
      const guard = vg("evt.items", "raw", "is-empty");
      expect(evaluateGuard(guard, ctx({ items: [] }))).toBe(true);
    });

    it("returns false for non-empty string", () => {
      const guard = vg("evt.name", "raw", "is-empty");
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(false);
    });

    it("returns false for non-empty array", () => {
      const guard = vg("evt.items", "raw", "is-empty");
      expect(evaluateGuard(guard, ctx({ items: [1, 2] }))).toBe(false);
    });
  });

  describe("not-empty operator", () => {
    it("returns true for non-empty string", () => {
      const guard = vg("evt.name", "raw", "not-empty");
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(true);
    });

    it("returns false for empty string", () => {
      const guard = vg("evt.name", "raw", "not-empty");
      expect(evaluateGuard(guard, ctx({ name: "" }))).toBe(false);
    });

    it("returns false for null", () => {
      const guard = vg("evt.name", "raw", "not-empty");
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(false);
    });
  });

  // -- NEW: in / not-in membership -----------------------------------------

  describe("in operator", () => {
    it("returns true when value is in array", () => {
      const guard = vg("evt.role", "string", "in", ["admin", "superuser"]);
      expect(evaluateGuard(guard, ctx({ role: "admin" }))).toBe(true);
    });

    it("returns false when value is not in array", () => {
      const guard = vg("evt.role", "string", "in", ["admin", "superuser"]);
      expect(evaluateGuard(guard, ctx({ role: "viewer" }))).toBe(false);
    });

    it("works with numbers", () => {
      const guard = vg("evt.code", "number", "in", [1, 2, 3]);
      expect(evaluateGuard(guard, ctx({ code: 2 }))).toBe(true);
    });
  });

  describe("not-in operator", () => {
    it("returns true when value is not in array", () => {
      const guard = vg("evt.role", "string", "not-in", ["blocked", "banned"]);
      expect(evaluateGuard(guard, ctx({ role: "admin" }))).toBe(true);
    });

    it("returns false when value is in array", () => {
      const guard = vg("evt.role", "string", "not-in", ["blocked", "banned"]);
      expect(evaluateGuard(guard, ctx({ role: "blocked" }))).toBe(false);
    });
  });

  // -- NEW: between --------------------------------------------------------

  describe("between operator", () => {
    it("returns true when value is within range", () => {
      const guard = vg("evt.age", "number", "between", [18, 65]);
      expect(evaluateGuard(guard, ctx({ age: 30 }))).toBe(true);
    });

    it("returns true at lower boundary", () => {
      const guard = vg("evt.age", "number", "between", [18, 65]);
      expect(evaluateGuard(guard, ctx({ age: 18 }))).toBe(true);
    });

    it("returns true at upper boundary", () => {
      const guard = vg("evt.age", "number", "between", [18, 65]);
      expect(evaluateGuard(guard, ctx({ age: 65 }))).toBe(true);
    });

    it("returns false when below range", () => {
      const guard = vg("evt.age", "number", "between", [18, 65]);
      expect(evaluateGuard(guard, ctx({ age: 10 }))).toBe(false);
    });

    it("returns false when above range", () => {
      const guard = vg("evt.age", "number", "between", [18, 65]);
      expect(evaluateGuard(guard, ctx({ age: 70 }))).toBe(false);
    });
  });

  // -- NEW: text operators -------------------------------------------------

  describe("contains operator", () => {
    it("returns true when string contains substring", () => {
      const guard = vg("evt.name", "string", "contains", "admin");
      expect(evaluateGuard(guard, ctx({ name: "superadmin" }))).toBe(true);
    });

    it("returns false when substring is absent", () => {
      const guard = vg("evt.name", "string", "contains", "admin");
      expect(evaluateGuard(guard, ctx({ name: "user" }))).toBe(false);
    });
  });

  describe("starts-with operator", () => {
    it("returns true when string starts with prefix", () => {
      const guard = vg("evt.email", "string", "starts-with", "admin@");
      expect(evaluateGuard(guard, ctx({ email: "admin@example.com" }))).toBe(true);
    });

    it("returns false when prefix doesn't match", () => {
      const guard = vg("evt.email", "string", "starts-with", "admin@");
      expect(evaluateGuard(guard, ctx({ email: "user@example.com" }))).toBe(false);
    });
  });

  describe("ends-with operator", () => {
    it("returns true when string ends with suffix", () => {
      const guard = vg("evt.file", "string", "ends-with", ".pdf");
      expect(evaluateGuard(guard, ctx({ file: "report.pdf" }))).toBe(true);
    });

    it("returns false when suffix doesn't match", () => {
      const guard = vg("evt.file", "string", "ends-with", ".pdf");
      expect(evaluateGuard(guard, ctx({ file: "report.doc" }))).toBe(false);
    });
  });

  describe("matches operator", () => {
    it("returns true when regex matches", () => {
      const guard = vg("evt.email", "string", "matches", "^[a-z]+@[a-z]+\\.[a-z]+$");
      expect(evaluateGuard(guard, ctx({ email: "admin@example.com" }))).toBe(true);
    });

    it("returns false when regex doesn't match", () => {
      const guard = vg("evt.email", "string", "matches", "^[a-z]+@[a-z]+\\.[a-z]+$");
      expect(evaluateGuard(guard, ctx({ email: "NOT_AN_EMAIL" }))).toBe(false);
    });

    it("returns false on invalid regex pattern", () => {
      const guard = vg("evt.name", "string", "matches", "[invalid");
      expect(evaluateGuard(guard, ctx({ name: "test" }))).toBe(false);
    });
  });

  describe("min-length operator", () => {
    it("returns true when string meets min length", () => {
      const guard = vg("evt.name", "string", "min-length", 3);
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(true);
    });

    it("returns true at exact length", () => {
      const guard = vg("evt.name", "string", "min-length", 3);
      expect(evaluateGuard(guard, ctx({ name: "Bob" }))).toBe(true);
    });

    it("returns false when string is too short", () => {
      const guard = vg("evt.name", "string", "min-length", 3);
      expect(evaluateGuard(guard, ctx({ name: "Al" }))).toBe(false);
    });

    it("returns false for null (coerces to empty string)", () => {
      const guard = vg("evt.name", "string", "min-length", 3);
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(false);
    });
  });

  // -- Logical composition ------------------------------------------------

  describe("all (AND) composition", () => {
    it("returns true when all guards pass", () => {
      const guard: Guard = {
        kind: "all",
        guards: [
          vg("evt.score", "number", "gte", 90),
          vg("evt.status", "string", "eq", "active"),
        ],
      };
      expect(evaluateGuard(guard, ctx({ score: 95, status: "active" }))).toBe(true);
    });

    it("returns false when one guard fails", () => {
      const guard: Guard = {
        kind: "all",
        guards: [
          vg("evt.score", "number", "gte", 90),
          vg("evt.status", "string", "eq", "active"),
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
          vg("evt.role", "string", "eq", "admin"),
          vg("evt.role", "string", "eq", "superuser"),
        ],
      };
      expect(evaluateGuard(guard, ctx({ role: "superuser" }))).toBe(true);
    });

    it("returns false when no guard passes", () => {
      const guard: Guard = {
        kind: "any",
        guards: [
          vg("evt.role", "string", "eq", "admin"),
          vg("evt.role", "string", "eq", "superuser"),
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
              vg("evt.role", "string", "eq", "admin"),
              vg("evt.level", "number", "gte", 5),
            ],
          },
          {
            kind: "all",
            guards: [
              vg("evt.role", "string", "eq", "superuser"),
              vg("evt.level", "number", "gte", 3),
            ],
          },
        ],
      };
      expect(evaluateGuard(guard, ctx({ role: "superuser", level: 4 }))).toBe(true);
    });
  });

  // -- NEW: InvertGuard (NOT) -----------------------------------------------

  describe("InvertGuard (not)", () => {
    it("negates a value guard", () => {
      const guard: Guard = { kind: "not", inner: vg("evt.role", "string", "eq", "admin") };
      expect(evaluateGuard(guard, ctx({ role: "user" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ role: "admin" }))).toBe(false);
    });

    it("negates an all guard", () => {
      const guard: Guard = {
        kind: "not",
        inner: {
          kind: "all",
          guards: [
            vg("evt.score", "number", "gte", 90),
            vg("evt.status", "string", "eq", "active"),
          ],
        },
      };
      // Both match → all=true → not=false
      expect(evaluateGuard(guard, ctx({ score: 95, status: "active" }))).toBe(false);
      // One fails → all=false → not=true
      expect(evaluateGuard(guard, ctx({ score: 95, status: "inactive" }))).toBe(true);
    });

    it("double negation cancels out", () => {
      const guard: Guard = {
        kind: "not",
        inner: { kind: "not", inner: vg("evt.value", "number", "gte", 10) },
      };
      expect(evaluateGuard(guard, ctx({ value: 15 }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ value: 5 }))).toBe(false);
    });
  });

  // -- NEW: ConfirmGuard (async) -------------------------------------------

  describe("ConfirmGuard", () => {
    it("isConfirmGuard detects confirm guard", () => {
      expect(isConfirmGuard({ kind: "confirm", message: "Sure?" })).toBe(true);
    });

    it("isConfirmGuard detects confirm inside not", () => {
      expect(isConfirmGuard({ kind: "not", inner: { kind: "confirm", message: "Sure?" } })).toBe(true);
    });

    it("isConfirmGuard detects confirm inside all", () => {
      expect(isConfirmGuard({
        kind: "all",
        guards: [
          vg("evt.x", "number", "gt", 0),
          { kind: "confirm", message: "Sure?" },
        ],
      })).toBe(true);
    });

    it("isConfirmGuard returns false for value guards", () => {
      expect(isConfirmGuard(vg("evt.x", "number", "gt", 0))).toBe(false);
    });

    it("evaluateGuardAsync resolves to true when confirm returns true", async () => {
      (globalThis as any).window = { alis: { confirm: () => Promise.resolve(true) } };
      const guard: Guard = { kind: "confirm", message: "Delete?" };
      expect(await evaluateGuardAsync(guard, ctx({}))).toBe(true);
      delete (globalThis as any).window;
    });

    it("evaluateGuardAsync resolves to false when confirm returns false", async () => {
      (globalThis as any).window = { alis: { confirm: () => Promise.resolve(false) } };
      const guard: Guard = { kind: "confirm", message: "Delete?" };
      expect(await evaluateGuardAsync(guard, ctx({}))).toBe(false);
      delete (globalThis as any).window;
    });

    it("evaluateGuardAsync handles all-with-confirm (short-circuits on false)", async () => {
      (globalThis as any).window = { alis: { confirm: () => Promise.resolve(true) } };
      const guard: Guard = {
        kind: "all",
        guards: [
          vg("evt.id", "number", "gt", 0),
          { kind: "confirm", message: "Delete this?" },
        ],
      };
      // id > 0 passes, confirm returns true → all passes
      expect(await evaluateGuardAsync(guard, ctx({ id: 5 }))).toBe(true);
      // id = 0 fails → short-circuits, confirm never called
      expect(await evaluateGuardAsync(guard, ctx({ id: 0 }))).toBe(false);
      delete (globalThis as any).window;
    });
  });

  // -- Coercion integration -----------------------------------------------

  describe("coercion integration", () => {
    it("coerces string score to number for comparison", () => {
      const guard = vg("evt.score", "number", "gte", 90);
      expect(evaluateGuard(guard, ctx({ score: "95" }))).toBe(true);
    });

    it("coerces string boolean for truthy check", () => {
      const guard = vg("evt.active", "boolean", "truthy");
      expect(evaluateGuard(guard, ctx({ active: "true" }))).toBe(true);
    });
  });

  // -- Operand coercion (C2 + I5 fix) --------------------------------------

  describe("operand coercion", () => {
    it("coerces string operand to number for eq comparison", () => {
      const guard = vg("evt.score", "number", "eq", 95);
      expect(evaluateGuard(guard, ctx({ score: "95" }))).toBe(true);
    });

    it("coerces number operand for string source in eq", () => {
      const guard = vg("evt.code", "string", "eq", "42");
      expect(evaluateGuard(guard, ctx({ code: 42 }))).toBe(true);
    });

    it("coerces operand for gte comparison", () => {
      const guard = vg("evt.score", "number", "gte", "90");
      expect(evaluateGuard(guard, ctx({ score: "100" }))).toBe(true);
    });

    it("coerces operand for lt comparison", () => {
      const guard = vg("evt.count", "number", "lt", "10");
      expect(evaluateGuard(guard, ctx({ count: "5" }))).toBe(true);
    });
  });

  // -- Presence operators with typed coercion ----------------------------

  describe("is-null with typed coerceAs", () => {
    it("correctly detects null even when coerceAs is string", () => {
      const guard = vg("evt.name", "string", "is-null");
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(true);
    });

    it("correctly detects null even when coerceAs is number", () => {
      const guard = vg("evt.value", "number", "is-null");
      expect(evaluateGuard(guard, ctx({ value: null }))).toBe(true);
    });

    it("correctly detects non-null with typed coercion", () => {
      const guard = vg("evt.name", "string", "not-null");
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(true);
    });

    it("correctly detects undefined as null with typed coercion", () => {
      const guard = vg("evt.missing", "number", "is-null");
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });
  });

  // -- Date coercion ---------------------------------------------------

  describe("date coercion", () => {
    it("compares ISO date strings via gt", () => {
      const guard = vg("evt.deadline", "date", "gt", "2026-06-01T00:00:00");
      expect(evaluateGuard(guard, ctx({ deadline: "2026-07-15T00:00:00" }))).toBe(true);
    });

    it("returns false when date is before operand", () => {
      const guard = vg("evt.deadline", "date", "gt", "2026-06-01T00:00:00");
      expect(evaluateGuard(guard, ctx({ deadline: "2026-01-15T00:00:00" }))).toBe(false);
    });

    it("supports lte on dates", () => {
      const guard = vg("evt.deadline", "date", "lte", "2026-06-01T00:00:00");
      expect(evaluateGuard(guard, ctx({ deadline: "2026-06-01T00:00:00" }))).toBe(true);
    });

    it("supports eq on dates", () => {
      const guard = vg("evt.deadline", "date", "eq", "2026-06-01T00:00:00");
      expect(evaluateGuard(guard, ctx({ deadline: "2026-06-01T00:00:00" }))).toBe(true);
    });

    it("returns NaN for null date (is-null uses raw, not coerced)", () => {
      const guard = vg("evt.deadline", "date", "is-null");
      expect(evaluateGuard(guard, ctx({ deadline: null }))).toBe(true);
    });
  });

  // -- Long/float as number coercion ------------------------------------

  describe("long and float as number", () => {
    it("compares large numbers (long equivalent) via gt", () => {
      const guard = vg("evt.balance", "number", "gt", 1000000);
      expect(evaluateGuard(guard, ctx({ balance: 5000000 }))).toBe(true);
    });

    it("compares float values via lte", () => {
      const guard = vg("evt.rate", "number", "lte", 0.5);
      expect(evaluateGuard(guard, ctx({ rate: 0.3 }))).toBe(true);
    });

    it("compares decimal-precision doubles via gt", () => {
      const guard = vg("evt.temp", "number", "gt", 98.6);
      expect(evaluateGuard(guard, ctx({ temp: 101.3 }))).toBe(true);
    });
  });

  // -- Nested payload paths ------------------------------------------------

  describe("nested payload path resolution", () => {
    it("evaluates eq on deep dot-path (address.city)", () => {
      const guard = vg("evt.address.city", "string", "eq", "Seattle");
      expect(evaluateGuard(guard, ctx({ address: { city: "Seattle", zip: "98101" } }))).toBe(true);
    });

    it("returns false when deep path value mismatches", () => {
      const guard = vg("evt.address.city", "string", "eq", "Seattle");
      expect(evaluateGuard(guard, ctx({ address: { city: "Portland" } }))).toBe(false);
    });

    it("handles 3-level deep path", () => {
      const guard = vg("evt.user.address.zip", "string", "eq", "98101");
      expect(evaluateGuard(guard, ctx({ user: { address: { zip: "98101" } } }))).toBe(true);
    });

    it("evaluates gte on nested numeric path", () => {
      const guard = vg("evt.stats.score", "number", "gte", 90);
      expect(evaluateGuard(guard, ctx({ stats: { score: 95 } }))).toBe(true);
    });
  });

  // -- Null safety in nested paths -----------------------------------------

  describe("null safety in nested paths", () => {
    it("returns true for is-null when intermediate object is null", () => {
      const guard = vg("evt.address.city", "string", "is-null");
      expect(evaluateGuard(guard, ctx({ address: null }))).toBe(true);
    });

    it("returns true for is-null when intermediate object is missing", () => {
      const guard = vg("evt.address.city", "string", "is-null");
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });

    it("returns true for is-null when leaf property is null", () => {
      const guard = vg("evt.address.city", "string", "is-null");
      expect(evaluateGuard(guard, ctx({ address: { city: null } }))).toBe(true);
    });

    it("returns false for not-null when intermediate object is null", () => {
      const guard = vg("evt.address.city", "string", "not-null");
      expect(evaluateGuard(guard, ctx({ address: null }))).toBe(false);
    });

    it("takes else branch for eq when nested object is null (no crash)", () => {
      const guard = vg("evt.address.city", "string", "eq", "Seattle");
      expect(evaluateGuard(guard, ctx({ address: null }))).toBe(false);
    });

    it("takes else branch for gte when nested object is null (no crash)", () => {
      const guard = vg("evt.stats.score", "number", "gte", 90);
      expect(evaluateGuard(guard, ctx({ stats: null }))).toBe(false);
    });

    it("handles null in AND compound with nested paths", () => {
      const guard: Guard = {
        kind: "all",
        guards: [
          vg("evt.id", "number", "gte", 1),
          vg("evt.address.city", "string", "not-null"),
        ],
      };
      expect(evaluateGuard(guard, ctx({ id: 5, address: null }))).toBe(false);
    });

    it("passes AND compound when nested path has value", () => {
      const guard: Guard = {
        kind: "all",
        guards: [
          vg("evt.id", "number", "gte", 1),
          vg("evt.address.city", "string", "not-null"),
        ],
      };
      expect(evaluateGuard(guard, ctx({ id: 5, address: { city: "NYC" } }))).toBe(true);
    });

    it("handles completely empty evt object without crashing", () => {
      const guard = vg("evt.address.city", "string", "eq", "Seattle");
      expect(evaluateGuard(guard, ctx({}))).toBe(false);
    });

    it("handles undefined context without crashing", () => {
      const guard = vg("evt.address.city", "string", "eq", "Seattle");
      expect(evaluateGuard(guard, undefined)).toBe(false);
    });
  });

  // -- Null complex object -------------------------------------------------

  describe("is-null on complex object (not leaf)", () => {
    it("returns true when object property is null", () => {
      const guard = vg("evt.address", "raw", "is-null");
      expect(evaluateGuard(guard, ctx({ address: null }))).toBe(true);
    });

    it("returns false when object property is present", () => {
      const guard = vg("evt.address", "raw", "is-null");
      expect(evaluateGuard(guard, ctx({ address: { city: "NYC" } }))).toBe(false);
    });

    it("returns true for not-null when object property is present", () => {
      const guard = vg("evt.address", "raw", "not-null");
      expect(evaluateGuard(guard, ctx({ address: { city: "NYC" } }))).toBe(true);
    });

    it("returns true for is-null when object property is missing entirely", () => {
      const guard = vg("evt.address", "raw", "is-null");
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });
  });

  // -- Edge cases ---------------------------------------------------------

  describe("edge cases", () => {
    it("returns false for eq when source path is missing", () => {
      const guard = vg("evt.nonexistent", "raw", "eq", "hello");
      expect(evaluateGuard(guard, ctx({}))).toBe(false);
    });

    it("returns true for is-null when source path is missing", () => {
      const guard = vg("evt.nonexistent", "raw", "is-null");
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });

    it("throws on unknown guard kind", () => {
      const guard = { kind: "unknown", source: { kind: "event", path: "evt.x" }, coerceAs: "raw", op: "eq", operand: 1 } as any;
      expect(() => evaluateGuard(guard, ctx({}))).toThrow("Unknown guard kind: unknown");
    });

    it("throws on unknown operator", () => {
      const guard: any = vg("evt.x", "raw", "nope");
      expect(() => evaluateGuard(guard, ctx({ x: 1 }))).toThrow("Unknown guard operator: nope");
    });
  });
});
