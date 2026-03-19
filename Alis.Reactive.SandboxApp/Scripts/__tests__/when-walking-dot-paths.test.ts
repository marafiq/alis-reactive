import { describe, it, expect } from "vitest";
import { walk } from "../core/walk";

describe("when walking dot paths", () => {
  it("reads single property", () => expect(walk({ value: "x" }, "value")).toBe("x"));
  it("reads nested path", () => expect(walk({ a: { b: "c" } }, "a.b")).toBe("c"));
  it("returns undefined for null root", () => expect(walk(null, "x")).toBeUndefined());
  it("returns undefined for missing path", () => expect(walk({ a: 1 }, "b")).toBeUndefined());
  it("handles array index via string key", () => expect(walk([10, 20], "1")).toBe(20));
  it("handles deep nesting", () =>
    expect(walk({ evt: { address: { city: "NY" } } }, "evt.address.city")).toBe("NY"));
});
