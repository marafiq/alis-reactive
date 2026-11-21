import { describe, expect, test } from "vitest";
import { arrayOf } from "./index";

type Read = ReturnType<typeof arrayOf>;

const nothing: Read = { kind: "nothing" };
const value = (held: unknown): Read => ({ kind: "value", value: held });

describe("when a list an object's iterator yields is read, toArray is the array of its items, as Array.from reads them", () => {
  test("a value that iterates is the array of the items it yields, in order", () => {
    expect(arrayOf(value(new Set(["North", "East"])))).toStrictEqual(value(["North", "East"]));
  });

  test("a text iterates its characters by code point", () => {
    expect(arrayOf(value("a\u{1F600}"))).toStrictEqual(value(["a", "\u{1F600}"]));
  });

  test("a value that reads nothing or holds null reads nothing", () => {
    expect(arrayOf(nothing)).toStrictEqual(nothing);
    expect(arrayOf(value(null))).toStrictEqual(nothing);
  });

  test("a value with no iterator stops the pipeline", () => {
    expect(() => arrayOf(value({ result: [], count: 0 }))).toThrow("The value an object yields no items: it has no iterator.");
  });
});
