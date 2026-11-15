import { describe, expect, test } from "vitest";
import { operated as operatedUnder, type Term } from "./index";

type Read = ReturnType<Term>;

const operated = (op: Parameters<typeof operatedUnder>[0], terms: readonly Term[]): Read => operatedUnder(op, terms, "en-US");

const nothing: Read = { kind: "nothing" };
const value = (read: unknown): Read => ({ kind: "value", value: read });

const held = (read: unknown): Term => () => value(read);
const readsNothing: Term = () => nothing;
const neverReached: Term = () => {
  throw new Error("a term no operator should reach");
};

function reached(read: Read, into: string[], name: string): Term {
  return () => {
    into.push(name);
    return read;
  };
}

describe("when a condition compares two values, it reads as .NET's lifted operators and JavaScript's === say", () => {
  test("eq holds over two values JavaScript's === holds over", () => {
    expect(operated("eq", [held("North"), held("North")])).toStrictEqual(value(true));
  });

  test("eq does not hold over a number and its text", () => {
    expect(operated("eq", [held(4), held("4")])).toStrictEqual(value(false));
  });

  test("eq holds over null and null", () => {
    expect(operated("eq", [held(null), held(null)])).toStrictEqual(value(true));
  });

  test("neq holds over null and a value", () => {
    expect(operated("neq", [held(null), held("North")])).toStrictEqual(value(true));
  });

  test("a comparison whose left operand reads nothing reads nothing, neq included", () => {
    expect(operated("neq", [readsNothing, held("Ruth")])).toStrictEqual(nothing);
  });

  test("a comparison whose right operand reads nothing reads nothing", () => {
    expect(operated("eq", [held("Ruth"), readsNothing])).toStrictEqual(nothing);
  });

  test("lt holds where the left number is the smaller", () => {
    expect(operated("lt", [held(2), held(3)])).toStrictEqual(value(true));
  });

  test("lt does not hold over two equal numbers, and lte does", () => {
    expect([operated("lt", [held(3), held(3)]), operated("lte", [held(3), held(3)])]).toStrictEqual([value(false), value(true)]);
  });

  test("gt and gte order two numbers the other way", () => {
    expect([operated("gt", [held(3), held(2)]), operated("gte", [held(2), held(3)])]).toStrictEqual([value(true), value(false)]);
  });

  test("an ordering orders two one-character texts by their code units", () => {
    expect(operated("lt", [held("B"), held("a")])).toStrictEqual(value(true));
  });

  test("an ordering with null does not hold, though JavaScript's null <= 2 is true", () => {
    expect([operated("lte", [held(null), held(2)]), operated("gte", [held(2), held(null)])]).toStrictEqual([value(false), value(false)]);
  });

  test("an ordering of a number and a text stops the pipeline", () => {
    expect(() => operated("lt", [held(2), held("3")])).toThrow("The values 2 and \"3\" have no order between them.");
  });
});

describe("when a condition joins its terms, it reads as SQL's three-valued logic with nothing as its unknown", () => {
  test("not turns true to false and false to true", () => {
    expect([operated("not", [held(true)]), operated("not", [held(false)])]).toStrictEqual([value(false), value(true)]);
  });

  test("not of nothing is nothing", () => {
    expect(operated("not", [readsNothing])).toStrictEqual(nothing);
  });

  test("and is true when both terms are true", () => {
    expect(operated("and", [held(true), held(true)])).toStrictEqual(value(true));
  });

  test("and is false when its right term is false, though its left reads nothing", () => {
    expect(operated("and", [readsNothing, held(false)])).toStrictEqual(value(false));
  });

  test("and is nothing when one term is true and the other reads nothing", () => {
    expect(operated("and", [held(true), readsNothing])).toStrictEqual(nothing);
  });

  test("ConditionTermsLazy: and does not reach its right term when its left is false", () => {
    expect(operated("and", [held(false), neverReached])).toStrictEqual(value(false));
  });

  test("ConditionTermsLazy: and reaches its right term when its left is true", () => {
    const terms: string[] = [];

    operated("and", [held(true), reached(value(true), terms, "right")]);

    expect(terms).toStrictEqual(["right"]);
  });

  test("or is true when its right term is true, though its left reads nothing", () => {
    expect(operated("or", [readsNothing, held(true)])).toStrictEqual(value(true));
  });

  test("or is false when both terms are false", () => {
    expect(operated("or", [held(false), held(false)])).toStrictEqual(value(false));
  });

  test("or is nothing when one term is false and the other reads nothing", () => {
    expect(operated("or", [held(false), readsNothing])).toStrictEqual(nothing);
  });

  test("ConditionTermsLazy: or does not reach its right term when its left is true", () => {
    expect(operated("or", [held(true), neverReached])).toStrictEqual(value(true));
  });

  test("ConditionTermsLazy: or reaches its right term when its left is false", () => {
    const terms: string[] = [];

    operated("or", [held(false), reached(value(false), terms, "right")]);

    expect(terms).toStrictEqual(["right"]);
  });
});

describe("when a value is chosen from terms, only the term it reads is reached", () => {
  test("coalesce reads its first argument when it holds a value, and does not reach its second", () => {
    expect(operated("coalesce", [held("Ward 4"), neverReached])).toStrictEqual(value("Ward 4"));
  });

  test("coalesce reads its second argument when its first holds null", () => {
    expect(operated("coalesce", [held(null), held("none")])).toStrictEqual(value("none"));
  });

  test("coalesce reads its second argument when its first reads nothing", () => {
    expect(operated("coalesce", [readsNothing, held("none")])).toStrictEqual(value("none"));
  });

  test("cond reads its second argument when its first reads true, and does not reach its third", () => {
    expect(operated("cond", [held(true), held("then"), neverReached])).toStrictEqual(value("then"));
  });

  test("cond reads its third argument when its first reads false, and does not reach its second", () => {
    expect(operated("cond", [held(false), neverReached, held("else")])).toStrictEqual(value("else"));
  });

  test("cond reads its third argument when its first reads nothing", () => {
    expect(operated("cond", [readsNothing, neverReached, held("else")])).toStrictEqual(value("else"));
  });
});

describe("when the op switch runs a list's operator, each operator reaches its own executor", () => {
  test("nullableValue reads nothing on null, nonNull stops on it, and toArray is the array an iterator yields", () => {
    expect(operated("nullableValue", [held(null)])).toStrictEqual(nothing);
    expect(() => operated("nonNull", [held(null)])).toThrow("A list's field holds null, and its type holds no null.");
    expect(operated("toArray", [held("ab")])).toStrictEqual(value(["a", "b"]));
  });

  test("compareNumbers, compareOrdinal, compareBooleans and compareCulture each compare by their own order", () => {
    expect(operated("compareNumbers", [held(2), held(10)])).toStrictEqual(value(-1));
    expect(operated("compareOrdinal", [held("a"), held("B")])).toStrictEqual(value(1));
    expect(operated("compareBooleans", [held(true), held(false)])).toStrictEqual(value(1));
    expect(operated("compareCulture", [held("a"), held("B")])).toStrictEqual(value(-1));
  });

  test("trim, trimStart and trimEnd each remove white space from their own ends", () => {
    expect(operated("trim", [held("\u0085 Ward 4 ")])).toStrictEqual(value("Ward 4"));
    expect(operated("trimStart", [held(" Ward 4 ")])).toStrictEqual(value("Ward 4 "));
    expect(operated("trimEnd", [held(" Ward 4 ")])).toStrictEqual(value(" Ward 4"));
  });

  test("each casing operator takes its own rule for the Turkish letters", () => {
    expect(operated("upperInvariant", [held("\u0131")])).toStrictEqual(value("\u0131"));
    expect(operated("upper", [held("\u0131")])).toStrictEqual(value("I"));
    expect(operated("upperTurkic", [held("i")])).toStrictEqual(value("\u0130"));
    expect(operated("lowerInvariant", [held("\u0130")])).toStrictEqual(value("\u0130"));
    expect(operated("lower", [held("\u0130")])).toStrictEqual(value("i"));
    expect(operated("lowerTurkic", [held("I")])).toStrictEqual(value("\u0131"));
  });

  test("sumInt, sumDouble and sumDecimal each add as their number type adds", () => {
    expect(() => operated("sumInt", [held([0.5])])).toThrow("The term 0.5 is no int, which an int sum adds.");
    expect(operated("sumDouble", [held([0.1, 0.2])])).toStrictEqual(value(0.30000000000000004));
    expect(operated("sumDecimal", [held([0.1, 0.2])])).toStrictEqual(value(0.3));
  });
});
