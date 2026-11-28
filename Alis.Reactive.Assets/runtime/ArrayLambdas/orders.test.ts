import { describe, expect, test } from "vitest";
import { comparedBooleans, comparedCulture, comparedNumbers, comparedOrdinal } from "./index";

type Read = ReturnType<typeof comparedNumbers>;

const nothing: Read = { kind: "nothing" };
const value = (held: unknown): Read => ({ kind: "value", value: held });

describe("when a sort compares two keys as numbers, it orders as Comparer<T>.Default does", () => {
  test("the smaller key comes first, and equal keys compare 0", () => {
    expect(comparedNumbers(value(2), value(10))).toStrictEqual(value(-1));
    expect(comparedNumbers(value(10), value(2))).toStrictEqual(value(1));
    expect(comparedNumbers(value(-0), value(0))).toStrictEqual(value(0));
  });

  test("null comes before every value, and two nulls compare 0", () => {
    expect(comparedNumbers(value(null), value(-5))).toStrictEqual(value(-1));
    expect(comparedNumbers(value(-5), value(null))).toStrictEqual(value(1));
    expect(comparedNumbers(value(null), value(null))).toStrictEqual(value(0));
  });

  test("NaN comes before every number and equals NaN, as a double's CompareTo orders it", () => {
    expect(comparedNumbers(value(Number.NaN), value(-5))).toStrictEqual(value(-1));
    expect(comparedNumbers(value(-5), value(Number.NaN))).toStrictEqual(value(1));
    expect(comparedNumbers(value(Number.NaN), value(Number.NaN))).toStrictEqual(value(0));
  });

  test("a key that reads nothing stops the pipeline", () => {
    expect(() => comparedNumbers(nothing, value(2))).toThrow("A sort key reads nothing.");
  });

  test("a key that holds no number stops the pipeline", () => {
    expect(() => comparedNumbers(value(2), value("3"))).toThrow('The sort key "3" is no number, which its order compares.');
  });
});

describe("when a sort compares two keys by code unit, a char or a date's key, it orders as .NET's ordinal comparison does", () => {
  test("B comes before a, by their UTF-16 code units", () => {
    expect(comparedOrdinal(value("B"), value("a"))).toStrictEqual(value(-1));
  });

  test("two date keys order as their instants do", () => {
    expect(comparedOrdinal(value("2026-11-14T08:00:00.0000000"), value("2026-11-14T07:59:59.9999999"))).toStrictEqual(value(1));
  });

  test("a key that holds no text stops the pipeline", () => {
    expect(() => comparedOrdinal(value("B"), value(2))).toThrow("The sort key 2 is no string, which its order compares.");
  });
});

describe("when a sort compares two keys as booleans, false comes first", () => {
  test("false before true, null before both", () => {
    expect(comparedBooleans(value(false), value(true))).toStrictEqual(value(-1));
    expect(comparedBooleans(value(true), value(true))).toStrictEqual(value(0));
    expect(comparedBooleans(value(null), value(false))).toStrictEqual(value(-1));
  });
});

describe("when a sort compares two texts by the view's culture, it orders by the plan's collation", () => {
  test("a comes before B under en-US, where its code unit comes after", () => {
    expect(comparedCulture(value("a"), value("B"), "en-US")).toStrictEqual(value(-1));
  });

  test("Muhr comes before Müller under de-DE", () => {
    expect(comparedCulture(value("Müller"), value("Muhr"), "de-DE")).toStrictEqual(value(1));
  });

  test("a culture the browser has no collation for stops the pipeline at its first comparison", () => {
    expect(() => comparedCulture(value("a"), value("B"), "xx-XX")).toThrow("The browser has no collation for the culture 'xx-XX'.");
  });
});
