import { describe, expect, it } from "vitest";
import { applyShape, convertByShape } from "../../shared/shape-convert";
import type { Shape } from "../../types/index";

const nullableNumber: Shape = { kind: "nullable", inner: { kind: "number" } };

describe("shape conversion of a nullable number", () => {
  it("gives blank text no value on the lenient and the strict path alike", () => {
    expect(applyShape("", nullableNumber)).toBeNull();
    expect(convertByShape("", nullableNumber)).toEqual({ ok: true, value: null });
  });
});
