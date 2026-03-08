import { describe, it, expect } from "vitest";
import { resolve, resolveToString, resolveAs, coerce } from "../resolver";
import type { ExecContext } from "../types";

describe("when resolving a BindExpr against execution context", () => {
  // --- resolve(): flat property access ---

  describe("flat property access", () => {
    it("resolves a string property", () => {
      const ctx: ExecContext = { evt: { name: "Alice" } };
      expect(resolve("evt.name", ctx)).toBe("Alice");
    });

    it("resolves a number property", () => {
      const ctx: ExecContext = { evt: { age: 30 } };
      expect(resolve("evt.age", ctx)).toBe(30);
    });

    it("resolves a boolean property", () => {
      const ctx: ExecContext = { evt: { active: true } };
      expect(resolve("evt.active", ctx)).toBe(true);
    });

    it("resolves a null property as null", () => {
      const ctx: ExecContext = { evt: { value: null } };
      expect(resolve("evt.value", ctx)).toBeNull();
    });

    it("resolves a missing property as undefined", () => {
      const ctx: ExecContext = { evt: { name: "Alice" } };
      expect(resolve("evt.missing", ctx)).toBeUndefined();
    });

    it("resolves zero as zero (not falsy undefined)", () => {
      const ctx: ExecContext = { evt: { count: 0 } };
      expect(resolve("evt.count", ctx)).toBe(0);
    });

    it("resolves empty string as empty string", () => {
      const ctx: ExecContext = { evt: { text: "" } };
      expect(resolve("evt.text", ctx)).toBe("");
    });

    it("resolves false as false (not undefined)", () => {
      const ctx: ExecContext = { evt: { enabled: false } };
      expect(resolve("evt.enabled", ctx)).toBe(false);
    });
  });

  // --- resolve(): nested property access ---

  describe("nested property access", () => {
    it("resolves 2-level nested property", () => {
      const ctx: ExecContext = { evt: { address: { city: "Seattle" } } };
      expect(resolve("evt.address.city", ctx)).toBe("Seattle");
    });

    it("resolves 3-level nested property", () => {
      const ctx: ExecContext = {
        evt: { user: { address: { zip: "98101" } } },
      };
      expect(resolve("evt.user.address.zip", ctx)).toBe("98101");
    });

    it("resolves 4-level nested property", () => {
      const ctx: ExecContext = {
        evt: { a: { b: { c: { d: 42 } } } },
      };
      expect(resolve("evt.a.b.c.d", ctx)).toBe(42);
    });

    it("returns undefined when intermediate is null", () => {
      const ctx: ExecContext = { evt: { address: null } };
      expect(resolve("evt.address.city", ctx)).toBeUndefined();
    });

    it("returns undefined when intermediate is undefined", () => {
      const ctx: ExecContext = { evt: {} };
      expect(resolve("evt.address.city", ctx)).toBeUndefined();
    });

    it("returns undefined when intermediate is a primitive", () => {
      const ctx: ExecContext = { evt: { name: "Alice" } };
      expect(resolve("evt.name.length", ctx)).toBeUndefined();
    });

    it("returns the nested object itself when path ends at object", () => {
      const address = { city: "Seattle", zip: "98101" };
      const ctx: ExecContext = { evt: { address } };
      expect(resolve("evt.address", ctx)).toEqual(address);
    });
  });

  // --- resolve(): edge cases ---

  describe("edge cases", () => {
    it("returns undefined when context is undefined", () => {
      expect(resolve("evt.name", undefined)).toBeUndefined();
    });

    it("returns undefined when context is empty object", () => {
      expect(resolve("evt.name", {})).toBeUndefined();
    });

    it("returns undefined when evt root is missing", () => {
      const ctx: ExecContext = {};
      expect(resolve("evt.name", ctx)).toBeUndefined();
    });

    it("resolves the evt root itself", () => {
      const evt = { name: "Alice" };
      const ctx: ExecContext = { evt };
      expect(resolve("evt", ctx)).toEqual(evt);
    });

    it("handles array values at leaf", () => {
      const ctx: ExecContext = { evt: { items: [1, 2, 3] } };
      expect(resolve("evt.items", ctx)).toEqual([1, 2, 3]);
    });
  });

  // --- resolveToString(): string coercion for DOM rendering ---

  describe("resolveToString for DOM rendering", () => {
    it("coerces number to string", () => {
      const ctx: ExecContext = { evt: { count: 42 } };
      expect(resolveToString("evt.count", ctx)).toBe("42");
    });

    it("coerces boolean true to 'true'", () => {
      const ctx: ExecContext = { evt: { active: true } };
      expect(resolveToString("evt.active", ctx)).toBe("true");
    });

    it("coerces boolean false to 'false'", () => {
      const ctx: ExecContext = { evt: { active: false } };
      expect(resolveToString("evt.active", ctx)).toBe("false");
    });

    it("coerces null to empty string", () => {
      const ctx: ExecContext = { evt: { value: null } };
      expect(resolveToString("evt.value", ctx)).toBe("");
    });

    it("returns empty string for missing path", () => {
      const ctx: ExecContext = { evt: {} };
      expect(resolveToString("evt.nonexistent", ctx)).toBe("");
    });

    it("returns empty string for undefined context", () => {
      expect(resolveToString("evt.name", undefined)).toBe("");
    });

    it("leaves string as-is", () => {
      const ctx: ExecContext = { evt: { name: "Alice" } };
      expect(resolveToString("evt.name", ctx)).toBe("Alice");
    });

    it("coerces zero to '0'", () => {
      const ctx: ExecContext = { evt: { count: 0 } };
      expect(resolveToString("evt.count", ctx)).toBe("0");
    });

    it("coerces large number precisely", () => {
      const ctx: ExecContext = { evt: { big: 9007199254740991 } };
      expect(resolveToString("evt.big", ctx)).toBe("9007199254740991");
    });

    it("coerces float precisely", () => {
      const ctx: ExecContext = { evt: { pi: 3.14159 } };
      expect(resolveToString("evt.pi", ctx)).toBe("3.14159");
    });
  });

  // --- resolveAs(): combined resolution + coercion ---

  describe("resolveAs with typed coercion", () => {
    it("resolves string value and coerces to number", () => {
      const ctx: ExecContext = { evt: { amount: "42" } };
      expect(resolveAs("evt.amount", "number", ctx)).toBe(42);
    });

    it("resolves number value and coerces to string", () => {
      const ctx: ExecContext = { evt: { count: 99 } };
      expect(resolveAs("evt.count", "string", ctx)).toBe("99");
    });

    it("resolves string 'true' and coerces to boolean", () => {
      const ctx: ExecContext = { evt: { flag: "true" } };
      expect(resolveAs("evt.flag", "boolean", ctx)).toBe(true);
    });

    it("resolves number 1 and coerces to boolean", () => {
      const ctx: ExecContext = { evt: { flag: 1 } };
      expect(resolveAs("evt.flag", "boolean", ctx)).toBe(true);
    });

    it("resolves missing path and coerces to string as empty", () => {
      const ctx: ExecContext = { evt: {} };
      expect(resolveAs("evt.missing", "string", ctx)).toBe("");
    });

    it("resolves missing path and coerces to number as zero", () => {
      const ctx: ExecContext = { evt: {} };
      expect(resolveAs("evt.missing", "number", ctx)).toBe(0);
    });

    it("resolves missing path and coerces to boolean as false", () => {
      const ctx: ExecContext = { evt: {} };
      expect(resolveAs("evt.missing", "boolean", ctx)).toBe(false);
    });

    it("resolves nested path and coerces to number", () => {
      const ctx: ExecContext = { evt: { stats: { score: "95.5" } } };
      expect(resolveAs("evt.stats.score", "number", ctx)).toBe(95.5);
    });

    it("raw coercion returns the exact resolved value", () => {
      const obj = { nested: true };
      const ctx: ExecContext = { evt: { data: obj } };
      expect(resolveAs("evt.data", "raw", ctx)).toBe(obj);
    });
  });

  // --- Condition-ready scenarios (future: Guard Algebra) ---

  describe("condition evaluation patterns", () => {
    const ctx: ExecContext = {
      evt: {
        value: 42,
        text: "Seattle",
        isActive: true,
        score: "95.5",
        status: null,
        items: [1, 2, 3],
        empty: "",
        zero: 0,
        nested: { count: 7 },
      },
    };

    it("resolves value for numeric comparison (Gt/Lt/Gte/Lte)", () => {
      expect(resolveAs("evt.value", "number", ctx)).toBe(42);
      expect(resolveAs("evt.score", "number", ctx)).toBe(95.5);
    });

    it("resolves value for equality comparison (Eq/NotEq)", () => {
      expect(resolve("evt.text", ctx)).toBe("Seattle");
      expect(resolve("evt.value", ctx)).toBe(42);
    });

    it("resolves value for presence check (IsNull/NotNull)", () => {
      expect(resolve("evt.status", ctx)).toBeNull();
      expect(resolve("evt.text", ctx)).not.toBeNull();
      expect(resolve("evt.missing", ctx)).toBeUndefined();
    });

    it("resolves value for truthiness check (Truthy/Falsy)", () => {
      expect(coerce(resolve("evt.isActive", ctx), "boolean")).toBe(true);
      expect(coerce(resolve("evt.zero", ctx), "boolean")).toBe(false);
      expect(coerce(resolve("evt.empty", ctx), "boolean")).toBe(false);
      expect(coerce(resolve("evt.status", ctx), "boolean")).toBe(false);
    });

    it("resolves value for emptiness check (IsEmpty/NotEmpty)", () => {
      const items = resolve("evt.items", ctx) as unknown[];
      expect(Array.isArray(items) && items.length > 0).toBe(true);

      const empty = resolve("evt.empty", ctx);
      expect(empty === "" || empty === null || empty === undefined).toBe(true);
    });

    it("resolves value for text assertion (Contains/StartsWith)", () => {
      const text = resolveAs("evt.text", "string", ctx) as string;
      expect(text.includes("eat")).toBe(true);
      expect(text.startsWith("Sea")).toBe(true);
    });

    it("resolves value for range assertion (Between)", () => {
      const val = resolveAs("evt.value", "number", ctx) as number;
      expect(val >= 10 && val <= 100).toBe(true);
    });

    it("resolves nested value for cross-path comparison", () => {
      const nestedCount = resolveAs("evt.nested.count", "number", ctx);
      const topValue = resolveAs("evt.value", "number", ctx);
      expect(nestedCount).toBe(7);
      expect(topValue).toBe(42);
    });
  });
});
