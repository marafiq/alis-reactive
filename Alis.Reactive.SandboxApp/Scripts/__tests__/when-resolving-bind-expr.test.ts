import { describe, it, expect } from "vitest";
import { resolveEventPath } from "../resolution/resolver";
import { coerce } from "../core/coerce";
import type { ExecContext } from "../types";

describe("when resolving a BindExpr against execution context", () => {
  describe("flat property access", () => {
    it("resolves a string property", () => {
      const ctx: ExecContext = { evt: { name: "Alice" } };
      expect(resolveEventPath("evt.name", ctx)).toBe("Alice");
    });

    it("resolves a number property", () => {
      const ctx: ExecContext = { evt: { age: 30 } };
      expect(resolveEventPath("evt.age", ctx)).toBe(30);
    });

    it("resolves a boolean property", () => {
      const ctx: ExecContext = { evt: { active: true } };
      expect(resolveEventPath("evt.active", ctx)).toBe(true);
    });

    it("resolves a null property as null", () => {
      const ctx: ExecContext = { evt: { value: null } };
      expect(resolveEventPath("evt.value", ctx)).toBeNull();
    });

    it("resolves a missing property as undefined", () => {
      const ctx: ExecContext = { evt: { name: "Alice" } };
      expect(resolveEventPath("evt.missing", ctx)).toBeUndefined();
    });

    it("resolves zero as zero (not falsy undefined)", () => {
      const ctx: ExecContext = { evt: { count: 0 } };
      expect(resolveEventPath("evt.count", ctx)).toBe(0);
    });

    it("resolves empty string as empty string", () => {
      const ctx: ExecContext = { evt: { text: "" } };
      expect(resolveEventPath("evt.text", ctx)).toBe("");
    });

    it("resolves false as false (not undefined)", () => {
      const ctx: ExecContext = { evt: { enabled: false } };
      expect(resolveEventPath("evt.enabled", ctx)).toBe(false);
    });
  });

  describe("nested property access", () => {
    it("resolves 2-level nested property", () => {
      const ctx: ExecContext = { evt: { address: { city: "Seattle" } } };
      expect(resolveEventPath("evt.address.city", ctx)).toBe("Seattle");
    });

    it("resolves 3-level nested property", () => {
      const ctx: ExecContext = {
        evt: { user: { address: { zip: "98101" } } },
      };
      expect(resolveEventPath("evt.user.address.zip", ctx)).toBe("98101");
    });

    it("resolves 4-level nested property", () => {
      const ctx: ExecContext = {
        evt: { a: { b: { c: { d: 42 } } } },
      };
      expect(resolveEventPath("evt.a.b.c.d", ctx)).toBe(42);
    });

    it("returns undefined when intermediate is null", () => {
      const ctx: ExecContext = { evt: { address: null } };
      expect(resolveEventPath("evt.address.city", ctx)).toBeUndefined();
    });

    it("returns undefined when intermediate is undefined", () => {
      const ctx: ExecContext = { evt: {} };
      expect(resolveEventPath("evt.address.city", ctx)).toBeUndefined();
    });

    it("walks through primitive properties (JS semantics)", () => {
      const ctx: ExecContext = { evt: { name: "Alice" } };
      expect(resolveEventPath("evt.name.length", ctx)).toBe(5);
    });

    it("returns the nested object itself when path ends at object", () => {
      const address = { city: "Seattle", zip: "98101" };
      const ctx: ExecContext = { evt: { address } };
      expect(resolveEventPath("evt.address", ctx)).toEqual(address);
    });
  });

  describe("edge cases", () => {
    it("returns undefined when context is undefined", () => {
      expect(resolveEventPath("evt.name", undefined)).toBeUndefined();
    });

    it("returns undefined when context is empty object", () => {
      expect(resolveEventPath("evt.name", {})).toBeUndefined();
    });

    it("returns undefined when evt root is missing", () => {
      const ctx: ExecContext = {};
      expect(resolveEventPath("evt.name", ctx)).toBeUndefined();
    });

    it("resolves the evt root itself", () => {
      const evt = { name: "Alice" };
      const ctx: ExecContext = { evt };
      expect(resolveEventPath("evt", ctx)).toEqual(evt);
    });

    it("handles array values at leaf", () => {
      const ctx: ExecContext = { evt: { items: [1, 2, 3] } };
      expect(resolveEventPath("evt.items", ctx)).toEqual([1, 2, 3]);
    });
  });

  describe("resolution with string coercion", () => {
    it("coerces number to string", () => {
      const ctx: ExecContext = { evt: { count: 42 } };
      expect(String(resolveEventPath("evt.count", ctx) ?? "")).toBe("42");
    });

    it("coerces boolean true to 'true'", () => {
      const ctx: ExecContext = { evt: { active: true } };
      expect(String(resolveEventPath("evt.active", ctx) ?? "")).toBe("true");
    });

    it("coerces boolean false to 'false'", () => {
      const ctx: ExecContext = { evt: { active: false } };
      expect(String(resolveEventPath("evt.active", ctx) ?? "")).toBe("false");
    });

    it("coerces null to empty string", () => {
      const ctx: ExecContext = { evt: { value: null } };
      expect(String(resolveEventPath("evt.value", ctx) ?? "")).toBe("");
    });

    it("returns empty string for missing path", () => {
      const ctx: ExecContext = { evt: {} };
      expect(String(resolveEventPath("evt.nonexistent", ctx) ?? "")).toBe("");
    });

    it("returns empty string for undefined context", () => {
      expect(String(resolveEventPath("evt.name", undefined) ?? "")).toBe("");
    });

    it("leaves string as-is", () => {
      const ctx: ExecContext = { evt: { name: "Alice" } };
      expect(String(resolveEventPath("evt.name", ctx) ?? "")).toBe("Alice");
    });

    it("coerces zero to '0'", () => {
      const ctx: ExecContext = { evt: { count: 0 } };
      expect(String(resolveEventPath("evt.count", ctx) ?? "")).toBe("0");
    });

    it("coerces large number precisely", () => {
      const ctx: ExecContext = { evt: { big: 9007199254740991 } };
      expect(String(resolveEventPath("evt.big", ctx) ?? "")).toBe("9007199254740991");
    });

    it("coerces float precisely", () => {
      const ctx: ExecContext = { evt: { pi: 3.14159 } };
      expect(String(resolveEventPath("evt.pi", ctx) ?? "")).toBe("3.14159");
    });
  });

  describe("resolution with typed coercion", () => {
    it("resolves string value and coerces to number", () => {
      const ctx: ExecContext = { evt: { amount: "42" } };
      expect(coerce(resolveEventPath("evt.amount", ctx), "number")).toEqual({ ok: true, value: 42 });
    });

    it("resolves number value and coerces to string", () => {
      const ctx: ExecContext = { evt: { count: 99 } };
      expect(coerce(resolveEventPath("evt.count", ctx), "string")).toEqual({ ok: true, value: "99" });
    });

    it("resolves string 'true' and coerces to boolean", () => {
      const ctx: ExecContext = { evt: { flag: "true" } };
      expect(coerce(resolveEventPath("evt.flag", ctx), "boolean")).toEqual({ ok: true, value: true });
    });

    it("resolves number 1 and coerces to boolean", () => {
      const ctx: ExecContext = { evt: { flag: 1 } };
      expect(coerce(resolveEventPath("evt.flag", ctx), "boolean")).toEqual({ ok: true, value: true });
    });

    it("resolves missing path and coerces to string as empty", () => {
      const ctx: ExecContext = { evt: {} };
      expect(coerce(resolveEventPath("evt.missing", ctx), "string")).toEqual({ ok: true, value: "" });
    });

    it("resolves missing path and coerces to number as zero", () => {
      const ctx: ExecContext = { evt: {} };
      expect(coerce(resolveEventPath("evt.missing", ctx), "number")).toEqual({ ok: true, value: 0 });
    });

    it("resolves missing path and coerces to boolean as false", () => {
      const ctx: ExecContext = { evt: {} };
      expect(coerce(resolveEventPath("evt.missing", ctx), "boolean")).toEqual({ ok: true, value: false });
    });

    it("resolves nested path and coerces to number", () => {
      const ctx: ExecContext = { evt: { stats: { score: "95.5" } } };
      expect(coerce(resolveEventPath("evt.stats.score", ctx), "number")).toEqual({ ok: true, value: 95.5 });
    });

    it("raw coercion returns the exact resolved value", () => {
      const obj = { nested: true };
      const ctx: ExecContext = { evt: { data: obj } };
      const r = coerce(resolveEventPath("evt.data", ctx), "raw");
      expect(r.ok).toBe(true);
      if (r.ok) expect(r.value).toBe(obj);
    });
  });

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
      expect(coerce(resolveEventPath("evt.value", ctx), "number")).toEqual({ ok: true, value: 42 });
      expect(coerce(resolveEventPath("evt.score", ctx), "number")).toEqual({ ok: true, value: 95.5 });
    });

    it("resolves value for equality comparison (Eq/NotEq)", () => {
      expect(resolveEventPath("evt.text", ctx)).toBe("Seattle");
      expect(resolveEventPath("evt.value", ctx)).toBe(42);
    });

    it("resolves value for presence check (IsNull/NotNull)", () => {
      expect(resolveEventPath("evt.status", ctx)).toBeNull();
      expect(resolveEventPath("evt.text", ctx)).not.toBeNull();
      expect(resolveEventPath("evt.missing", ctx)).toBeUndefined();
    });

    it("resolves value for truthiness check (Truthy/Falsy)", () => {
      expect(coerce(resolveEventPath("evt.isActive", ctx), "boolean")).toEqual({ ok: true, value: true });
      expect(coerce(resolveEventPath("evt.zero", ctx), "boolean")).toEqual({ ok: true, value: false });
      expect(coerce(resolveEventPath("evt.empty", ctx), "boolean")).toEqual({ ok: true, value: false });
      expect(coerce(resolveEventPath("evt.status", ctx), "boolean")).toEqual({ ok: true, value: false });
    });

    it("resolves value for emptiness check (IsEmpty/NotEmpty)", () => {
      const items = resolveEventPath("evt.items", ctx) as unknown[];
      expect(Array.isArray(items) && items.length > 0).toBe(true);

      const empty = resolveEventPath("evt.empty", ctx);
      expect(empty === "" || empty === null || empty === undefined).toBe(true);
    });

    it("resolves value for text assertion (Contains/StartsWith)", () => {
      const r = coerce(resolveEventPath("evt.text", ctx), "string");
      expect(r.ok).toBe(true);
      if (r.ok) {
        expect((r.value as string).includes("eat")).toBe(true);
        expect((r.value as string).startsWith("Sea")).toBe(true);
      }
    });

    it("resolves value for range assertion (Between)", () => {
      const r = coerce(resolveEventPath("evt.value", ctx), "number");
      expect(r.ok).toBe(true);
      if (r.ok) expect((r.value as number) >= 10 && (r.value as number) <= 100).toBe(true);
    });

    it("resolves nested value for cross-path comparison", () => {
      const nestedCount = coerce(resolveEventPath("evt.nested.count", ctx), "number");
      const topValue = coerce(resolveEventPath("evt.value", ctx), "number");
      expect(nestedCount).toEqual({ ok: true, value: 7 });
      expect(topValue).toEqual({ ok: true, value: 42 });
    });
  });
});
