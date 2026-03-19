import { describe, it, expect } from "vitest";
import { coerce } from "../resolution/resolver";

describe("when coercing resolved values", () => {
  // --- string coercion ---

  describe("to string", () => {
    it("converts integer to its string representation", () => {
      expect(coerce(42, "string")).toBe("42");
    });

    it("converts float to its string representation", () => {
      expect(coerce(3.14, "string")).toBe("3.14");
    });

    it("converts zero to '0'", () => {
      expect(coerce(0, "string")).toBe("0");
    });

    it("converts negative number to string", () => {
      expect(coerce(-7, "string")).toBe("-7");
    });

    it("converts boolean true to 'true'", () => {
      expect(coerce(true, "string")).toBe("true");
    });

    it("converts boolean false to 'false'", () => {
      expect(coerce(false, "string")).toBe("false");
    });

    it("converts null to empty string", () => {
      expect(coerce(null, "string")).toBe("");
    });

    it("converts undefined to empty string", () => {
      expect(coerce(undefined, "string")).toBe("");
    });

    it("leaves string unchanged", () => {
      expect(coerce("hello", "string")).toBe("hello");
    });

    it("leaves empty string unchanged", () => {
      expect(coerce("", "string")).toBe("");
    });

    it("converts large integer precisely", () => {
      expect(coerce(9007199254740991, "string")).toBe("9007199254740991");
    });
  });

  // --- number coercion ---

  describe("to number", () => {
    it("converts numeric string to number", () => {
      expect(coerce("42", "number")).toBe(42);
    });

    it("converts float string to number", () => {
      expect(coerce("3.14", "number")).toBe(3.14);
    });

    it("converts negative string to number", () => {
      expect(coerce("-7", "number")).toBe(-7);
    });

    it("converts empty string to zero", () => {
      expect(coerce("", "number")).toBe(0);
    });

    it("converts non-numeric string to zero (NaN fallback)", () => {
      expect(coerce("abc", "number")).toBe(0);
    });

    it("converts boolean true to 1", () => {
      expect(coerce(true, "number")).toBe(1);
    });

    it("converts boolean false to 0", () => {
      expect(coerce(false, "number")).toBe(0);
    });

    it("converts null to 0", () => {
      expect(coerce(null, "number")).toBe(0);
    });

    it("converts undefined to 0 (NaN fallback)", () => {
      expect(coerce(undefined, "number")).toBe(0);
    });

    it("leaves integer unchanged", () => {
      expect(coerce(42, "number")).toBe(42);
    });

    it("leaves float unchanged", () => {
      expect(coerce(3.14, "number")).toBe(3.14);
    });

    it("leaves zero unchanged", () => {
      expect(coerce(0, "number")).toBe(0);
    });

    it("converts mixed string like '42abc' to zero (NaN fallback)", () => {
      expect(coerce("42abc", "number")).toBe(0);
    });

    it("converts string with whitespace to number", () => {
      expect(coerce(" 42 ", "number")).toBe(42);
    });
  });

  // --- boolean coercion ---

  describe("to boolean", () => {
    describe("from string values", () => {
      it("converts empty string to false", () => {
        expect(coerce("", "boolean")).toBe(false);
      });

      it("converts 'false' to false", () => {
        expect(coerce("false", "boolean")).toBe(false);
      });

      it("converts '0' to false", () => {
        expect(coerce("0", "boolean")).toBe(false);
      });

      it("converts 'true' to true", () => {
        expect(coerce("true", "boolean")).toBe(true);
      });

      it("converts '1' to true", () => {
        expect(coerce("1", "boolean")).toBe(true);
      });

      it("converts any non-empty non-false string to true", () => {
        expect(coerce("hello", "boolean")).toBe(true);
      });

      it("converts 'yes' to true", () => {
        expect(coerce("yes", "boolean")).toBe(true);
      });

      it("converts 'FALSE' to true (case-sensitive check)", () => {
        // Only lowercase "false" is treated as falsy
        expect(coerce("FALSE", "boolean")).toBe(true);
      });
    });

    describe("from non-string values", () => {
      it("converts null to false", () => {
        expect(coerce(null, "boolean")).toBe(false);
      });

      it("converts undefined to false", () => {
        expect(coerce(undefined, "boolean")).toBe(false);
      });

      it("converts 0 to false", () => {
        expect(coerce(0, "boolean")).toBe(false);
      });

      it("converts NaN to false", () => {
        expect(coerce(NaN, "boolean")).toBe(false);
      });

      it("converts 1 to true", () => {
        expect(coerce(1, "boolean")).toBe(true);
      });

      it("converts negative number to true", () => {
        expect(coerce(-1, "boolean")).toBe(true);
      });

      it("converts empty object to true", () => {
        expect(coerce({}, "boolean")).toBe(true);
      });

      it("converts empty array to true", () => {
        expect(coerce([], "boolean")).toBe(true);
      });

      it("leaves boolean true unchanged", () => {
        expect(coerce(true, "boolean")).toBe(true);
      });

      it("leaves boolean false unchanged", () => {
        expect(coerce(false, "boolean")).toBe(false);
      });
    });
  });

  // --- raw coercion ---

  describe("to raw (no coercion)", () => {
    it("returns string as-is", () => {
      expect(coerce("hello", "raw")).toBe("hello");
    });

    it("returns number as-is", () => {
      expect(coerce(42, "raw")).toBe(42);
    });

    it("returns boolean as-is", () => {
      expect(coerce(true, "raw")).toBe(true);
    });

    it("returns null as-is", () => {
      expect(coerce(null, "raw")).toBeNull();
    });

    it("returns undefined as-is", () => {
      expect(coerce(undefined, "raw")).toBeUndefined();
    });

    it("returns object reference unchanged", () => {
      const obj = { a: 1, b: 2 };
      expect(coerce(obj, "raw")).toBe(obj);
    });

    it("returns array reference unchanged", () => {
      const arr = [1, 2, 3];
      expect(coerce(arr, "raw")).toBe(arr);
    });
  });
});
