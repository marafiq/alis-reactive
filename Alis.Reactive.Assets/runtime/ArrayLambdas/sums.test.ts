import { describe, expect, test } from "vitest";
import { summedDecimals, summedDoubles, summedInts } from "./index";

type Read = ReturnType<typeof summedInts>;

const nothing: Read = { kind: "nothing" };
const value = (held: unknown): Read => ({ kind: "value", value: held });

describe("when an int sum adds the map of its term, it adds as .NET's checked sum does", () => {
  test("the terms are added left to right", () => {
    expect(summedInts(value([2, 3, 5]))).toStrictEqual(value(10));
  });

  test("a list with no item sums to 0, and a list that reads nothing sums to nothing", () => {
    expect(summedInts(value([]))).toStrictEqual(value(0));
    expect(summedInts(nothing)).toStrictEqual(nothing);
  });

  test("a running total past the range of int stops the pipeline, though the whole total fits", () => {
    expect(() => summedInts(value([2_147_483_000, 1_000, -1_000]))).toThrow("An int sum passes the range of int at the term 1000, as .NET's checked sum refuses it.");
  });

  test("a term that is no int stops the pipeline, null among them", () => {
    expect(() => summedInts(value([1, null]))).toThrow("The term null is no int, which an int sum adds.");
    expect(() => summedInts(value([1, 1.5]))).toThrow("The term 1.5 is no int, which an int sum adds.");
  });
});

describe("when a double sum adds the map of its term, it is JavaScript's addition, the IEEE 754 addition .NET does", () => {
  test("the terms are added left to right in binary floating point", () => {
    expect(summedDoubles(value([0.1, 0.2]))).toStrictEqual(value(0.30000000000000004));
  });

  test("a term that is no number stops the pipeline", () => {
    expect(() => summedDoubles(value([0.1, null]))).toThrow("The term null is no number, which a double sum adds.");
  });

  test("a list that reads nothing sums to nothing", () => {
    expect(summedDoubles(nothing)).toStrictEqual(nothing);
  });
});

describe("when a decimal sum adds the map of its term, it adds exactly, as .NET adds decimals", () => {
  test("0.1 and 0.2 sum to 0.3, the terms added as whole numbers at the largest scale among them", () => {
    expect(summedDecimals(value([0.1, 0.2]))).toStrictEqual(value(0.3));
  });

  test("a term the app writes as decimal text is added as its number is", () => {
    expect(summedDecimals(value(["0.10", 0.2, "-0.05"]))).toStrictEqual(value(0.25));
  });

  test("an exact sum rounds nothing, so a dose of 0.125 stays 0.125", () => {
    expect(summedDecimals(value([0.125]))).toStrictEqual(value(0.125));
  });

  test("a list with no item sums to 0, and a list that reads nothing sums to nothing", () => {
    expect(summedDecimals(value([]))).toStrictEqual(value(0));
    expect(summedDecimals(nothing)).toStrictEqual(nothing);
  });

  test("a total past decimal.MaxValue stops the pipeline", () => {
    expect(() => summedDecimals(value(["79228162514264337593543950335", "1"]))).toThrow("A decimal sum passes decimal.MaxValue.");
  });

  test("a term that is no decimal the framework reads stops the pipeline, null and a text past 28 places among them", () => {
    expect(() => summedDecimals(value([1, null]))).toThrow("The value null is no decimal the framework reads.");
    expect(() => summedDecimals(value(["0.00000000000000000000000000001"]))).toThrow('The value "0.00000000000000000000000000001" is no decimal the framework reads.');
  });
});
