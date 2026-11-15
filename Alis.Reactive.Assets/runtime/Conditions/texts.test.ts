import { describe, expect, test } from "vitest";
import { operated as operatedUnder, type Term } from "./index";

type Read = ReturnType<Term>;

const operated = (op: Parameters<typeof operatedUnder>[0], terms: readonly Term[]): Read => operatedUnder(op, terms, "en-US");

const nothing: Read = { kind: "nothing" };
const value = (read: unknown): Read => ({ kind: "value", value: read });

const held = (read: unknown): Term => () => value(read);
const readsNothing: Term = () => nothing;

describe("when a text is tested for a part, it compares ordinally, as .NET's Contains, StartsWith and EndsWith do", () => {
  test("contains holds where the part is inside the text", () => {
    expect(operated("contains", [held("North wing"), held("h w")])).toStrictEqual(value(true));
  });

  test("contains compares by case, as an ordinal test does", () => {
    expect(operated("contains", [held("North wing"), held("north")])).toStrictEqual(value(false));
  });

  test("startsWith holds where the text begins with the part", () => {
    expect(operated("startsWith", [held("Ward 4"), held("Ward")])).toStrictEqual(value(true));
  });

  test("startsWith compares code units, so a soft hyphen before the part is not skipped as a culture's test skips it", () => {
    expect(operated("startsWith", [held("\u00ADward"), held("ward")])).toStrictEqual(value(false));
  });

  test("endsWith holds where the text ends with the part", () => {
    expect(operated("endsWith", [held("Ward 4"), held(" 4")])).toStrictEqual(value(true));
  });

  test("a text that holds null reads nothing", () => {
    expect(operated("contains", [held(null), held("North")])).toStrictEqual(nothing);
  });

  test("a text that reads nothing reads nothing", () => {
    expect(operated("endsWith", [readsNothing, held("4")])).toStrictEqual(nothing);
  });

  test("a part that reads nothing reads nothing", () => {
    expect(operated("startsWith", [held("Ward 4"), readsNothing])).toStrictEqual(nothing);
  });

  test("a value that is no text stops the pipeline", () => {
    expect(() => operated("contains", [held(4), held("4")])).toThrow("The value 4 is tested as text, and it is no text.");
  });

  test("a part that holds null stops the pipeline, as .NET's Contains refuses a null part", () => {
    expect(() => operated("contains", [held("Ward 4"), held(null)])).toThrow("The value null is looked for in a text, and it is no text.");
  });
});

describe("when a text or a list is tested for emptiness, nothing and null are empty", () => {
  test("nullOrEmpty holds on the empty text", () => {
    expect(operated("nullOrEmpty", [held("")])).toStrictEqual(value(true));
  });

  test("nullOrEmpty does not hold on a text of one space", () => {
    expect(operated("nullOrEmpty", [held(" ")])).toStrictEqual(value(false));
  });

  test("nullOrEmpty holds on null and on nothing, as string.IsNullOrEmpty holds on null", () => {
    expect([operated("nullOrEmpty", [held(null)]), operated("nullOrEmpty", [readsNothing])]).toStrictEqual([value(true), value(true)]);
  });

  test("nullOrEmpty of a value that is no text stops the pipeline", () => {
    expect(() => operated("nullOrEmpty", [held(0)])).toThrow("The value 0 is tested as text, and it is no text.");
  });

  test("empty holds on a list with no item and not on a list with one", () => {
    expect([operated("empty", [held([])]), operated("empty", [held(["Ada"])])]).toStrictEqual([value(true), value(false)]);
  });

  test("empty holds on null and on nothing", () => {
    expect([operated("empty", [held(null)]), operated("empty", [readsNothing])]).toStrictEqual([value(true), value(true)]);
  });

  test("empty of a text stops the pipeline, since a text is no list", () => {
    expect(() => operated("empty", [held("")])).toThrow("The value \"\" is tested as a list, and it is no list.");
  });
});

describe("when a text is matched against a pattern, it reads as Regex.IsMatch over the pattern's browser copy", () => {
  test("a text the pattern matches holds", () => {
    expect(operated("matches", [held("B12"), held("^[A-Z][0-9]{2}$")])).toStrictEqual(value(true));
  });

  test("a text the pattern does not match does not hold", () => {
    expect(operated("matches", [held("b12"), held("^[A-Z][0-9]{2}$")])).toStrictEqual(value(false));
  });

  test("a text that holds null reads nothing", () => {
    expect(operated("matches", [held(null), held("^[A-Z][0-9]{2}$")])).toStrictEqual(nothing);
  });

  test("a value that is no text stops the pipeline", () => {
    expect(() => operated("matches", [held(12), held("^[0-9]+$")])).toThrow("The value 12 is tested as text, and it is no text.");
  });
});

describe("when an item is looked for in a list the view supplies, it compares as JavaScript's includes does", () => {
  test("an item the list holds is found", () => {
    expect(operated("includes", [held(["Ada", "Ben"]), held("Ben")])).toStrictEqual(value(true));
  });

  test("an item the list lacks is not found", () => {
    expect(operated("includes", [held(["Ada", "Ben"]), held("Cy")])).toStrictEqual(value(false));
  });

  test("null is found in a list that holds null", () => {
    expect(operated("includes", [held(["Ada", null]), held(null)])).toStrictEqual(value(true));
  });

  test("NaN is found in a list that holds NaN, as .NET's default equality finds it", () => {
    expect(operated("includes", [held([Number.NaN]), held(Number.NaN)])).toStrictEqual(value(true));
  });

  test("an item that reads nothing reads nothing", () => {
    expect(operated("includes", [held(["Ada"]), readsNothing])).toStrictEqual(nothing);
  });

  test("nothing is found in a list of no item", () => {
    expect(operated("includes", [held([]), held("Ada")])).toStrictEqual(value(false));
  });
});
