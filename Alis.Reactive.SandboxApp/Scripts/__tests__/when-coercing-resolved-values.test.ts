import { describe, it, expect } from "vitest";
import { coerce } from "../core/coerce";

describe("when coercing resolved values", () => {
  // --- string coercion ---

  describe("to string", () => {
    it("converts integer to its string representation", () => {
      expect(coerce(42, "string")).toEqual({ ok: true, value: "42" });
    });

    it("converts float to its string representation", () => {
      expect(coerce(3.14, "string")).toEqual({ ok: true, value: "3.14" });
    });

    it("converts zero to '0'", () => {
      expect(coerce(0, "string")).toEqual({ ok: true, value: "0" });
    });

    it("converts negative number to string", () => {
      expect(coerce(-7, "string")).toEqual({ ok: true, value: "-7" });
    });

    it("converts boolean true to 'true'", () => {
      expect(coerce(true, "string")).toEqual({ ok: true, value: "true" });
    });

    it("converts boolean false to 'false'", () => {
      expect(coerce(false, "string")).toEqual({ ok: true, value: "false" });
    });

    it("converts null to empty string", () => {
      expect(coerce(null, "string")).toEqual({ ok: true, value: "" });
    });

    it("converts undefined to empty string", () => {
      expect(coerce(undefined, "string")).toEqual({ ok: true, value: "" });
    });

    it("leaves string unchanged", () => {
      expect(coerce("hello", "string")).toEqual({ ok: true, value: "hello" });
    });

    it("leaves empty string unchanged", () => {
      expect(coerce("", "string")).toEqual({ ok: true, value: "" });
    });

    it("converts large integer precisely", () => {
      expect(coerce(9007199254740991, "string")).toEqual({ ok: true, value: "9007199254740991" });
    });
  });

  // --- number coercion ---

  describe("to number", () => {
    it("converts numeric string to number", () => {
      expect(coerce("42", "number")).toEqual({ ok: true, value: 42 });
    });

    it("converts float string to number", () => {
      expect(coerce("3.14", "number")).toEqual({ ok: true, value: 3.14 });
    });

    it("converts negative string to number", () => {
      expect(coerce("-7", "number")).toEqual({ ok: true, value: -7 });
    });

    it("converts empty string to zero", () => {
      expect(coerce("", "number")).toEqual({ ok: true, value: 0 });
    });

    it("converts non-numeric string to zero (NaN fallback)", () => {
      expect(coerce("abc", "number")).toEqual({ ok: true, value: 0 });
    });

    it("converts boolean true to 1", () => {
      expect(coerce(true, "number")).toEqual({ ok: true, value: 1 });
    });

    it("converts boolean false to 0", () => {
      expect(coerce(false, "number")).toEqual({ ok: true, value: 0 });
    });

    it("converts null to 0", () => {
      expect(coerce(null, "number")).toEqual({ ok: true, value: 0 });
    });

    it("converts undefined to 0 (NaN fallback)", () => {
      expect(coerce(undefined, "number")).toEqual({ ok: true, value: 0 });
    });

    it("leaves integer unchanged", () => {
      expect(coerce(42, "number")).toEqual({ ok: true, value: 42 });
    });

    it("leaves float unchanged", () => {
      expect(coerce(3.14, "number")).toEqual({ ok: true, value: 3.14 });
    });

    it("leaves zero unchanged", () => {
      expect(coerce(0, "number")).toEqual({ ok: true, value: 0 });
    });

    it("converts mixed string like '42abc' to zero (NaN fallback)", () => {
      expect(coerce("42abc", "number")).toEqual({ ok: true, value: 0 });
    });

    it("converts string with whitespace to number", () => {
      expect(coerce(" 42 ", "number")).toEqual({ ok: true, value: 42 });
    });
  });

  // --- boolean coercion ---

  describe("to boolean", () => {
    describe("from string values", () => {
      it("converts empty string to false", () => {
        expect(coerce("", "boolean")).toEqual({ ok: true, value: false });
      });

      it("converts 'false' to false", () => {
        expect(coerce("false", "boolean")).toEqual({ ok: true, value: false });
      });

      it("converts '0' to false", () => {
        expect(coerce("0", "boolean")).toEqual({ ok: true, value: false });
      });

      it("converts 'true' to true", () => {
        expect(coerce("true", "boolean")).toEqual({ ok: true, value: true });
      });

      it("converts '1' to true", () => {
        expect(coerce("1", "boolean")).toEqual({ ok: true, value: true });
      });

      it("converts any non-empty non-false string to true", () => {
        expect(coerce("hello", "boolean")).toEqual({ ok: true, value: true });
      });

      it("converts 'yes' to true", () => {
        expect(coerce("yes", "boolean")).toEqual({ ok: true, value: true });
      });

      it("converts 'FALSE' to true (case-sensitive check)", () => {
        // Only lowercase "false" is treated as falsy
        expect(coerce("FALSE", "boolean")).toEqual({ ok: true, value: true });
      });
    });

    describe("from non-string values", () => {
      it("converts null to false", () => {
        expect(coerce(null, "boolean")).toEqual({ ok: true, value: false });
      });

      it("converts undefined to false", () => {
        expect(coerce(undefined, "boolean")).toEqual({ ok: true, value: false });
      });

      it("converts 0 to false", () => {
        expect(coerce(0, "boolean")).toEqual({ ok: true, value: false });
      });

      it("converts NaN to false", () => {
        expect(coerce(NaN, "boolean")).toEqual({ ok: true, value: false });
      });

      it("converts 1 to true", () => {
        expect(coerce(1, "boolean")).toEqual({ ok: true, value: true });
      });

      it("converts negative number to true", () => {
        expect(coerce(-1, "boolean")).toEqual({ ok: true, value: true });
      });

      it("returns Err on plain object", () => {
        expect(coerce({}, "boolean").ok).toBe(false);
      });

      it("converts empty array to false (empty = no value)", () => {
        expect(coerce([], "boolean")).toEqual({ ok: true, value: false });
      });

      it("converts non-empty array to true", () => {
        expect(coerce([1], "boolean")).toEqual({ ok: true, value: true });
      });

      it("leaves boolean true unchanged", () => {
        expect(coerce(true, "boolean")).toEqual({ ok: true, value: true });
      });

      it("leaves boolean false unchanged", () => {
        expect(coerce(false, "boolean")).toEqual({ ok: true, value: false });
      });
    });
  });

  // --- raw coercion ---

  describe("to raw (no coercion)", () => {
    it("returns string as-is", () => {
      expect(coerce("hello", "raw")).toEqual({ ok: true, value: "hello" });
    });

    it("returns number as-is", () => {
      expect(coerce(42, "raw")).toEqual({ ok: true, value: 42 });
    });

    it("returns boolean as-is", () => {
      expect(coerce(true, "raw")).toEqual({ ok: true, value: true });
    });

    it("returns null as-is", () => {
      expect(coerce(null, "raw")).toEqual({ ok: true, value: null });
    });

    it("returns undefined as-is", () => {
      expect(coerce(undefined, "raw")).toEqual({ ok: true, value: undefined });
    });

    it("returns object reference unchanged", () => {
      const obj = { a: 1, b: 2 };
      const r = coerce(obj, "raw");
      expect(r.ok).toBe(true);
      if (r.ok) expect(r.value).toBe(obj);
    });

    it("returns array reference unchanged", () => {
      const arr = [1, 2, 3];
      const r = coerce(arr, "raw");
      expect(r.ok).toBe(true);
      if (r.ok) expect(r.value).toBe(arr);
    });
  });
});
