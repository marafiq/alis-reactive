import { describe, expect, test } from "vitest";
import { operated, type Term } from "./index";

type Read = ReturnType<Term>;

const nothing: Read = { kind: "nothing" };
const value = (read: unknown): Read => ({ kind: "value", value: read });

const held = (read: unknown): Term => () => value(read);
const readsNothing: Term = () => nothing;

describe("when two decimals are compared, they are equal by value, as .NET's decimal equality holds", () => {
  test("0.10 and 0.1 written as text are equal, where their texts differ", () => {
    expect(operated("eqDecimal", [held("0.10"), held("0.1")])).toStrictEqual(value(true));
  });

  test("0.125 and 0.1 are not equal", () => {
    expect(operated("eqDecimal", [held("0.125"), held("0.1")])).toStrictEqual(value(false));
  });

  test("a JSON number reads its shortest text, so 0.1 equals the text 0.100", () => {
    expect(operated("eqDecimal", [held(0.1), held("0.100")])).toStrictEqual(value(true));
  });

  test("a number JavaScript writes with an exponent reads as its plain digits", () => {
    expect(operated("eqDecimal", [held(1e-7), held("0.0000001")])).toStrictEqual(value(true));
  });

  test("minus zero equals zero", () => {
    expect(operated("eqDecimal", [held("-0.0"), held("0")])).toStrictEqual(value(true));
  });

  test("null equals null and no decimal", () => {
    expect([operated("eqDecimal", [held(null), held(null)]), operated("eqDecimal", [held(null), held("0")])]).toStrictEqual([value(true), value(false)]);
  });

  test("an argument that reads nothing reads nothing", () => {
    expect(operated("eqDecimal", [readsNothing, held("0.1")])).toStrictEqual(nothing);
  });

  test("the greatest decimal's 29 digits are read whole", () => {
    expect(operated("eqDecimal", [held("79228162514264337593543950335"), held("79228162514264337593543950335")])).toStrictEqual(value(true));
  });

  test("a text whose digits without the point pass the greatest decimal's stops the pipeline, a trailing zero included", () => {
    expect(() => operated("eqDecimal", [held("79228162514264337593543950335.0"), held("0")])).toThrow(
      "The value \"79228162514264337593543950335.0\" is no decimal the framework reads.",
    );
  });

  test("a text past the greatest decimal's digits stops the pipeline", () => {
    expect(() => operated("eqDecimal", [held("79228162514264337593543950336"), held("0")])).toThrow(
      "The value \"79228162514264337593543950336\" is no decimal the framework reads.",
    );
  });

  test("a text of more than 28 places stops the pipeline", () => {
    expect(() => operated("eqDecimal", [held("0.00000000000000000000000000001"), held("0")])).toThrow(
      "The value \"0.00000000000000000000000000001\" is no decimal the framework reads.",
    );
  });

  test("a text with a point and no digit after it stops the pipeline", () => {
    expect(() => operated("eqDecimal", [held("1."), held("1")])).toThrow("The value \"1.\" is no decimal the framework reads.");
  });

  test("a boolean stops the pipeline", () => {
    expect(() => operated("eqDecimal", [held(true), held("1")])).toThrow("The value true is no decimal the framework reads.");
  });
});
