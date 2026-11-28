import { describe, expect, test } from "vitest";
import { lower, trimmed, upper } from "./index";

type Read = ReturnType<typeof trimmed>;

const nothing: Read = { kind: "nothing" };
const value = (held: unknown): Read => ({ kind: "value", value: held });

describe("when a lambda trims a text, it removes the white space char.IsWhiteSpace accepts", () => {
  test("U+0085 and a no-break space are removed from both ends, where JavaScript's trim keeps U+0085", () => {
    expect(trimmed(value("\u0085 Ward 4\u00A0"), "both")).toStrictEqual(value("Ward 4"));
  });

  test("U+FEFF is kept, where JavaScript's trim removes it", () => {
    expect(trimmed(value("\uFEFFWard 4"), "both")).toStrictEqual(value("\uFEFFWard 4"));
  });

  test("TrimStart removes white space from the start alone, and TrimEnd from the end alone", () => {
    expect(trimmed(value("  Ward 4  "), "start")).toStrictEqual(value("Ward 4  "));
    expect(trimmed(value("  Ward 4  "), "end")).toStrictEqual(value("  Ward 4"));
  });

  test("a text that reads nothing or holds null reads nothing", () => {
    expect(trimmed(nothing, "both")).toStrictEqual(nothing);
    expect(trimmed(value(null), "both")).toStrictEqual(nothing);
  });

  test("a value that is no text stops the pipeline", () => {
    expect(() => trimmed(value(4), "both")).toThrow("The value 4 is read as text, and it is no text.");
  });
});

describe("when a lambda changes a text's case, it maps one code point at a time, as .NET does", () => {
  test("ToUpperInvariant keeps ß, which JavaScript writes as two letters", () => {
    expect(upper(value("straße"), "invariant")).toStrictEqual(value("STRAßE"));
  });

  test("ToUpperInvariant maps a Greek letter with a subscript iota to the one letter .NET gives", () => {
    expect(upper(value("ᾳᾀ"), "invariant")).toStrictEqual(value("ᾼᾈ"));
  });

  test("ToUpperInvariant keeps a dotless i, and ToLowerInvariant keeps a dotted capital I", () => {
    expect(upper(value("ı"), "invariant")).toStrictEqual(value("ı"));
    expect(lower(value("İ"), "invariant")).toStrictEqual(value("İ"));
  });

  test("ToLowerInvariant lowers a final sigma to σ, as one code point is lowered alone", () => {
    expect(lower(value("ΟΔΟΣ"), "invariant")).toStrictEqual(value("οδοσ"));
  });

  test("ToUpper and ToLower under a culture that is neither invariant nor Turkic map a dotless i to I and a dotted capital I to i", () => {
    expect(upper(value("ı"), "linguistic")).toStrictEqual(value("I"));
    expect(lower(value("İ"), "linguistic")).toStrictEqual(value("i"));
  });

  test("ToUpper and ToLower under a Turkic culture map i to a dotted capital I and I to a dotless i", () => {
    expect(upper(value("iı"), "turkic")).toStrictEqual(value("İI"));
    expect(lower(value("Iİ"), "turkic")).toStrictEqual(value("ıi"));
  });

  test("a text that reads nothing or holds null reads nothing, and a value that is no text stops the pipeline", () => {
    expect(upper(nothing, "invariant")).toStrictEqual(nothing);
    expect(lower(value(null), "invariant")).toStrictEqual(nothing);
    expect(() => upper(value(4), "invariant")).toThrow("The value 4 is read as text, and it is no text.");
  });
});
