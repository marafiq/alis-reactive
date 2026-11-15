import { describe, expect, test } from "vitest";
import { nonNull, nullableValue } from "./index";

type Read = ReturnType<typeof nonNull>;

const nothing: Read = { kind: "nothing" };
const value = (held: unknown): Read => ({ kind: "value", value: held });

describe("when a Select's field has a type that holds no null, nonNull holds its list to that type", () => {
  test("a field that holds a value is that value", () => {
    expect(nonNull(value(0))).toStrictEqual(value(0));
  });

  test("SelectFieldHoldsItsType: a field that reads nothing or holds null stops the pipeline", () => {
    expect(() => nonNull(nothing)).toThrow("A list's field reads nothing, and its type holds no null.");
    expect(() => nonNull(value(null))).toThrow("A list's field holds null, and its type holds no null.");
  });
});

describe("when a lambda reads a nullable's Value, it reads the value, and nothing where .NET throws", () => {
  test("a nullable that holds a value reads it", () => {
    expect(nullableValue(value(2))).toStrictEqual(value(2));
  });

  test("a nullable that holds null reads nothing, and one that reads nothing reads nothing", () => {
    expect(nullableValue(value(null))).toStrictEqual(nothing);
    expect(nullableValue(nothing)).toStrictEqual(nothing);
  });
});
