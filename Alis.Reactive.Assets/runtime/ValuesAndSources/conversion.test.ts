import { afterAll, beforeAll, describe, expect, test, vi } from "vitest";
import { converted, nothing, value, type Read } from "./index";

type Shape = Parameters<typeof converted>[1];
type Culture = Parameters<typeof converted>[2];
type TimeZone = Parameters<typeof converted>[3];

const germanOnNet10: Culture = {
  name: "de-DE",
  collation: "de-DE",
  decimal: ",",
  group: ".",
  integer: { group: false, exponent: false },
  fraction: { group: true, exponent: true },
};

const germanOnNet48: Culture = { ...germanOnNet10, fraction: { group: false, exponent: true } };

const americanOnNet10: Culture = { ...germanOnNet10, name: "en-US", collation: "en-US", decimal: ".", group: "," };

const clock: TimeZone = { kind: "clock" };
const newYork: TimeZone = { kind: "zone", id: "America/New_York" };
const kolkata: TimeZone = { kind: "zone", id: "Asia/Kolkata" };

const text: Shape = { kind: "string" };
const fraction: Shape = { kind: "number", of: "fraction", text: "view" };
const integer: Shape = { kind: "number", of: "integer", text: "view" };
const invariantNumber: Shape = { kind: "number", of: "fraction", text: "invariant" };
const truth: Shape = { kind: "boolean" };
const date: Shape = { kind: "date" };
const instant: Shape = { kind: "instant" };
const file: Shape = { kind: "file" };
const any: Shape = { kind: "any" };
const nullable = (inner: Shape): Shape => ({ kind: "nullable", inner });
const list = (item: Shape): Shape => ({ kind: "array", item });
const record = (fields: Readonly<Record<string, Shape>>): Shape => ({ kind: "object", fields });

const none = { kind: "nothing" };

function as(held: unknown, shape: Shape): Read {
  return converted(value(held), shape, germanOnNet10, clock);
}

function asUnder(culture: Culture, held: unknown, shape: Shape): Read {
  return converted(value(held), shape, culture, clock);
}

function asIn(timeZone: TimeZone, held: unknown, shape: Shape): Read {
  return converted(value(held), shape, germanOnNet10, timeZone);
}

function heldOf(read: Read): unknown {
  return read.kind === "value" ? read.value : read;
}

function wallOf(read: Read): unknown {
  if (read.kind === "nothing" || !(read.value instanceof Date)) return read;
  const at = read.value;
  return [at.getFullYear(), at.getMonth() + 1, at.getDate(), at.getHours(), at.getMinutes(), at.getSeconds(), at.getMilliseconds()];
}

function momentOf(read: Read): unknown {
  return read.kind === "value" && read.value instanceof Date ? read.value.toISOString() : read;
}

class Visit {
  readonly seen: unknown;
  readonly bed: string;

  constructor(seen: unknown, bed: string) {
    this.seen = seen;
    this.bed = bed;
  }
}

beforeAll(() => {
  vi.stubEnv("TZ", "Asia/Tokyo");
});

afterAll(() => {
  vi.unstubAllEnvs();
});

describe("as(expr, shape): the rules every row keeps (spec/Core/conversions.md)", () => {
  test("a value that is nothing stays nothing under every shape, where a present value converts", () => {
    expect(converted(nothing, any, germanOnNet10, clock)).toStrictEqual(none);
    expect(converted(nothing, nullable(text), germanOnNet10, clock)).toStrictEqual(none);
    expect(converted(value(undefined), text, germanOnNet10, clock)).toStrictEqual(none);
    expect(as("Bed 4", any)).toStrictEqual({ kind: "value", value: "Bed 4" });
  });

  test("nullable(inner) reads nothing for a blank text, and converts any other text by its inner shape", () => {
    expect(as("", nullable(text))).toStrictEqual(none);
    expect(as(" \t", nullable(text))).toStrictEqual(none);
    expect(as("   ", nullable(fraction))).toStrictEqual(none);
    expect(as("1,5", nullable(fraction))).toStrictEqual({ kind: "value", value: 1.5 });
    expect(as("Bed 4", nullable(text))).toStrictEqual({ kind: "value", value: "Bed 4" });
    expect(as("", text)).toStrictEqual({ kind: "value", value: "" });
  });

  test("blank is .NET's char.IsWhiteSpace: next line, no-break space, line separator and ideographic space are blank, the byte order mark is not", () => {
    expect(as("\u0085\u00A0\u2028\u3000", nullable(text))).toStrictEqual(none);
    expect(as("\uFEFF", nullable(text))).toStrictEqual({ kind: "value", value: "\uFEFF" });
  });

  test("a nullable shape holds null, so a nullable date's null reads null where a date reads nothing for it (X3.10)", () => {
    expect(as(null, nullable(date))).toStrictEqual({ kind: "value", value: null });
    expect(as(null, date)).toStrictEqual(none);
    expect(as(null, text)).toStrictEqual(none);
  });

  test("a source not listed for a shape converts to nothing, where a listed one converts", () => {
    expect(as(1, truth)).toStrictEqual(none);
    expect(as(true, date)).toStrictEqual(none);
    expect(as({ bed: "Bed 4" }, text)).toStrictEqual(none);
    expect(as(1794231000000, date)).toStrictEqual(none);
    expect(as(true, truth)).toStrictEqual({ kind: "value", value: true });
  });
});

describe("as(expr, string)", () => {
  test("text converts to itself, an empty text included", () => {
    expect(as("Bed 4", text)).toStrictEqual({ kind: "value", value: "Bed 4" });
    expect(as("", text)).toStrictEqual({ kind: "value", value: "" });
  });

  test("a finite number shows as .NET writes it under de-DE: its shortest digits, the decimal comma, no group separator (probe:core-number-text-culture)", () => {
    expect(as(1234.5, text)).toStrictEqual({ kind: "value", value: "1234,5" });
    expect(as(0.30000000000000004, text)).toStrictEqual({ kind: "value", value: "0,30000000000000004" });
    expect(as(1e21, text)).toStrictEqual({ kind: "value", value: "1E+21" });
    expect(as(1e15, text)).toStrictEqual({ kind: "value", value: "1000000000000000" });
  });

  test("a number from 1E-04 to below 1E+17 shows in fixed point and outside it in .NET's exponent form (probe:core-number-text-exponent)", () => {
    expect(as(1e16, text)).toStrictEqual({ kind: "value", value: "10000000000000000" });
    expect(as(1e20, text)).toStrictEqual({ kind: "value", value: "1E+20" });
    expect(as(1e17, text)).toStrictEqual({ kind: "value", value: "1E+17" });
    expect(as(1e-4, text)).toStrictEqual({ kind: "value", value: "0,0001" });
    expect(as(1e-5, text)).toStrictEqual({ kind: "value", value: "1E-05" });
    expect(as(123456.5e12, text)).toStrictEqual({ kind: "value", value: "1,234565E+17" });
    expect(as(1e-7, text)).toStrictEqual({ kind: "value", value: "1E-07" });
  });

  test("a number shows the view's decimal separator, so under en-US 1234.5 shows 1234.5", () => {
    expect(asUnder(americanOnNet10, 1234.5, text)).toStrictEqual({ kind: "value", value: "1234.5" });
  });

  test("a number that is not finite converts to nothing", () => {
    expect(as(Number.NaN, text)).toStrictEqual(none);
    expect(as(Number.POSITIVE_INFINITY, text)).toStrictEqual(none);
  });

  test("a boolean shows as true or false", () => {
    expect(as(true, text)).toStrictEqual({ kind: "value", value: "true" });
    expect(as(false, text)).toStrictEqual({ kind: "value", value: "false" });
  });

  test("a list shows each item converted to text, joined by a comma", () => {
    expect(as([1.5, true, "Bed 4"], text)).toStrictEqual({ kind: "value", value: "1,5,true,Bed 4" });
    expect(as([[1, 2], 3], text)).toStrictEqual({ kind: "value", value: "1,2,3" });
  });

  test("a Date's text is not built, since the shape does not say whether it holds a DateTime or a DateTimeOffset, and a Date that is no moment converts to nothing", () => {
    expect(() => as(new Date(2026, 10, 9), text)).toThrow("This page cannot run the text of a Date");
    expect(as(new Date(Number.NaN), text)).toStrictEqual(none);
  });

  test("a list with an item that converts to nothing is not built, and negative zero's text is not built", () => {
    expect(() => as(["Bed 4", null], text)).toThrow("This page cannot run the text of a list with an item that converts to nothing");
    expect(() => as(-0, text)).toThrow("This page cannot run the text of negative zero");
  });
});

describe("as(expr, number)", () => {
  test("a finite number converts to itself, a number that is not finite to nothing", () => {
    expect(as(-12.5, fraction)).toStrictEqual({ kind: "value", value: -12.5 });
    expect(as(Number.NaN, fraction)).toStrictEqual(none);
    expect(as(Number.NEGATIVE_INFINITY, fraction)).toStrictEqual(none);
  });

  test("a blank text converts to nothing, never to 0, where the text 0 reads 0", () => {
    expect(as("", fraction)).toStrictEqual(none);
    expect(as(" \u00A0", integer)).toStrictEqual(none);
    expect(as("", invariantNumber)).toStrictEqual(none);
    expect(as("0", integer)).toStrictEqual({ kind: "value", value: 0 });
  });

  test("an input's text reads as net10 binds a decimal under de-DE: the group separator, an exponent and white space (probe:core-form-decimal-group, probe:core-form-decimal-culture)", () => {
    expect(as("1.234,5", fraction)).toStrictEqual({ kind: "value", value: 1234.5 });
    expect(as("1,5e3", fraction)).toStrictEqual({ kind: "value", value: 1500 });
    expect(as(" 1,5 ", fraction)).toStrictEqual({ kind: "value", value: 1.5 });
    expect(as("1,5", fraction)).toStrictEqual({ kind: "value", value: 1.5 });
    expect(as("1.5", fraction)).toStrictEqual({ kind: "value", value: 15 });
  });

  test("an input's text reads as net48 binds a decimal under de-DE: NumberStyles.Float, which refuses the group separator (probe:mvc5-decimal-float-style, D3.11)", () => {
    expect(asUnder(germanOnNet48, "1.5", fraction)).toStrictEqual(none);
    expect(asUnder(germanOnNet48, "1.234,5", fraction)).toStrictEqual(none);
    expect(asUnder(germanOnNet48, "1,5e3", fraction)).toStrictEqual({ kind: "value", value: 1500 });
    expect(asUnder(germanOnNet48, "1,5", fraction)).toStrictEqual({ kind: "value", value: 1.5 });
  });

  test("under en-US the separators swap: 1.5 reads 1.5 and 1,5 reads 15 (probe:core-form-decimal-culture-en-us)", () => {
    expect(asUnder(americanOnNet10, "1.5", fraction)).toStrictEqual({ kind: "value", value: 1.5 });
    expect(asUnder(americanOnNet10, "1,5", fraction)).toStrictEqual({ kind: "value", value: 15 });
  });

  test("an integer's text refuses both separators and takes a sign and white space (probe:core-form-int-culture, probe:mvc5-int-integer-style)", () => {
    expect(as("1.234", integer)).toStrictEqual(none);
    expect(as("1,0", integer)).toStrictEqual(none);
    expect(as(" -1234 ", integer)).toStrictEqual({ kind: "value", value: -1234 });
    expect(as("+7", integer)).toStrictEqual({ kind: "value", value: 7 });
    expect(as("-0", integer)).toStrictEqual({ kind: "value", value: 0 });
  });

  test("an input's text the host refuses converts to nothing, never NaN or the text (X3.3)", () => {
    expect(as("abc", integer)).toStrictEqual(none);
    expect(as("1,5 kg", fraction)).toStrictEqual(none);
    expect(as("1e", fraction)).toStrictEqual(none);
    expect(as(",", fraction)).toStrictEqual(none);
  });

  test("a URL parameter's text reads under the invariant culture with NumberStyles.Float, whatever the view's culture (D3.11, D7.7)", () => {
    expect(as("1.5", invariantNumber)).toStrictEqual({ kind: "value", value: 1.5 });
    expect(as("-2.5e3", invariantNumber)).toStrictEqual({ kind: "value", value: -2500 });
    expect(as(" 7 ", invariantNumber)).toStrictEqual({ kind: "value", value: 7 });
    expect(as("1.5 kg", invariantNumber)).toStrictEqual(none);
  });

  test("a boolean converts to 1 or 0", () => {
    expect(as(true, integer)).toStrictEqual({ kind: "value", value: 1 });
    expect(as(false, integer)).toStrictEqual({ kind: "value", value: 0 });
  });

  test("a date or an instant converts to its milliseconds since 1970-01-01T00:00:00Z, and a Date that is no moment to nothing", () => {
    expect(as(new Date("2026-11-09T13:30:00Z"), integer)).toStrictEqual({ kind: "value", value: 1794231000000 });
    expect(as(new Date(Number.NaN), integer)).toStrictEqual(none);
  });

  test("a number text past the double's range is not built", () => {
    expect(() => as("1e400", fraction)).toThrow("This page cannot run a number text past the double's range");
  });
});

describe("as(expr, boolean)", () => {
  test("a boolean converts to itself", () => {
    expect(as(false, truth)).toStrictEqual({ kind: "value", value: false });
  });

  test("text that is true or false in any case, with .NET white space trimmed, reads that boolean", () => {
    expect(as(" TRUE\u00A0", truth)).toStrictEqual({ kind: "value", value: true });
    expect(as("False", truth)).toStrictEqual({ kind: "value", value: false });
    expect(as("\u3000true\u0085", truth)).toStrictEqual({ kind: "value", value: true });
  });

  test("any other text converts to nothing", () => {
    expect(as("yes", truth)).toStrictEqual(none);
    expect(as("1", truth)).toStrictEqual(none);
    expect(as("t rue", truth)).toStrictEqual(none);
    expect(as("\uFEFFtrue", truth)).toStrictEqual(none);
  });
});

describe("as(expr, date)", () => {
  test("a Date converts to itself, and a Date that is no moment to nothing", () => {
    const seen = new Date(2026, 10, 9, 8, 30);
    expect(heldOf(as(seen, date))).toBe(seen);
    expect(as(new Date(Number.NaN), date)).toStrictEqual(none);
  });

  test("text yyyy-MM-dd naming a real calendar day reads that day at local midnight", () => {
    expect(wallOf(as("2026-11-09", date))).toStrictEqual([2026, 11, 9, 0, 0, 0, 0]);
    expect(wallOf(as("2028-02-29", date))).toStrictEqual([2028, 2, 29, 0, 0, 0, 0]);
    expect(wallOf(as("0050-06-15", date))).toStrictEqual([50, 6, 15, 0, 0, 0, 0]);
    expect(wallOf(as("0001-01-01", date))).toStrictEqual([1, 1, 1, 0, 0, 0, 0]);
  });

  test("a day no calendar holds converts to nothing (probe:core-bound-refused-values)", () => {
    expect(as("2026-02-30", date)).toStrictEqual(none);
    expect(as("2026-02-29", date)).toStrictEqual(none);
    expect(as("2026-13-01", date)).toStrictEqual(none);
    expect(as("0000-01-01", date)).toStrictEqual(none);
  });

  test("text in a time form reads that local time, its fraction read to the millisecond (probe:js-date-milliseconds)", () => {
    expect(wallOf(as("2026-11-09T08:30", date))).toStrictEqual([2026, 11, 9, 8, 30, 0, 0]);
    expect(wallOf(as("2026-11-09T08:30:15", date))).toStrictEqual([2026, 11, 9, 8, 30, 15, 0]);
    expect(wallOf(as("2026-11-09T08:30:15.1234567", date))).toStrictEqual([2026, 11, 9, 8, 30, 15, 123]);
    expect(wallOf(as("2026-11-09T08:30:15.5", date))).toStrictEqual([2026, 11, 9, 8, 30, 15, 500]);
  });

  test("a time no clock holds, or a fraction past seven digits, converts to nothing (probe:core-bound-refused-values)", () => {
    expect(as("2026-11-09T24:00", date)).toStrictEqual(none);
    expect(as("2026-11-09T08:60", date)).toStrictEqual(none);
    expect(as("2026-11-09T08:30:60", date)).toStrictEqual(none);
    expect(as("2026-11-09T08:30:15.12345678", date)).toStrictEqual(none);
    expect(as("2026-11-09T08:30:15.", date)).toStrictEqual(none);
  });

  test("text with Z or an offset reads its wall fields as local time, the offset dropped (D3.9, probe:core-stj-date-time-forms)", () => {
    expect(wallOf(as("2026-11-09T08:30:00Z", date))).toStrictEqual([2026, 11, 9, 8, 30, 0, 0]);
    expect(wallOf(as("2026-11-09T08:30-08:00", date))).toStrictEqual([2026, 11, 9, 8, 30, 0, 0]);
    expect(wallOf(as("2026-11-09T08:30+14:00", date))).toStrictEqual([2026, 11, 9, 8, 30, 0, 0]);
  });

  test("an offset past 14 hours converts to nothing (probe:core-bound-refused-values)", () => {
    expect(as("2026-11-09T08:30+15:00", date)).toStrictEqual(none);
    expect(as("2026-11-09T08:30+14:01", date)).toStrictEqual(none);
  });

  test("text outside the forms converts to nothing, though new Date reads each of them", () => {
    expect(as("2026/11/09", date)).toStrictEqual(none);
    expect(as("2026-11-09 08:30", date)).toStrictEqual(none);
    expect(as("Nov 9 2026", date)).toStrictEqual(none);
    expect(as("2026-11-9", date)).toStrictEqual(none);
    expect(as("2026-11-09t08:30", date)).toStrictEqual(none);
    expect(as("2026-11-09T08:30z", date)).toStrictEqual(none);
    expect(as(" 2026-11-09", date)).toStrictEqual(none);
    expect(as("2026-11-09T08:30:00+0500", date)).toStrictEqual(none);
    expect(as("2026-11", date)).toStrictEqual(none);
    expect(as("+002026-11-09", date)).toStrictEqual(none);
  });
});

describe("as(expr, instant)", () => {
  test("a Date converts to itself under the browser's clock, and a Date that is no moment to nothing", () => {
    const seen = new Date("2026-11-09T13:30:00Z");
    expect(heldOf(asIn(clock, seen, instant))).toBe(seen);
    expect(asIn(clock, new Date(Number.NaN), instant)).toStrictEqual(none);
  });

  test("a Date under the view's zone is not built, since the shape does not say whether it holds a DateTimeOffset or a DateTime", () => {
    expect(() => asIn(newYork, new Date("2026-11-09T13:30:00Z"), instant)).toThrow("This page cannot run the instant of a Date under the time zone 'America/New_York'");
  });

  test("text with no offset takes the view's zone's offset for its own day (D3.12)", () => {
    expect(momentOf(asIn(newYork, "2026-11-09T08:30", instant))).toBe("2026-11-09T13:30:00.000Z");
    expect(momentOf(asIn(newYork, "2026-07-09T08:30", instant))).toBe("2026-07-09T12:30:00.000Z");
    expect(momentOf(asIn(kolkata, "2026-11-09T08:30", instant))).toBe("2026-11-09T03:00:00.000Z");
    expect(momentOf(asIn(newYork, "2026-11-09", instant))).toBe("2026-11-09T05:00:00.000Z");
  });

  test("text with no offset under the browser's clock takes the clock's offset", () => {
    expect(momentOf(asIn(clock, "2026-11-09T08:30", instant))).toBe("2026-11-08T23:30:00.000Z");
  });

  test("text with an offset is the moment at that offset, whatever the view's zone", () => {
    expect(momentOf(asIn(kolkata, "2026-11-09T08:30:00-08:00", instant))).toBe("2026-11-09T16:30:00.000Z");
    expect(momentOf(asIn(newYork, "2026-11-09T08:30:00Z", instant))).toBe("2026-11-09T08:30:00.000Z");
    expect(momentOf(asIn(newYork, "2026-11-09T08:30:15.1234567+05:30", instant))).toBe("2026-11-09T03:00:15.123Z");
  });

  test("an instant before the year 1 converts to nothing, where the first moment of the year 1 converts (probe:core-bound-refused-values)", () => {
    expect(asIn(newYork, "0001-01-01T00:00+01:00", instant)).toStrictEqual(none);
    expect(momentOf(asIn(newYork, "0001-01-01T00:00Z", instant))).toBe("0001-01-01T00:00:00.000Z");
  });

  test("text outside the forms, or naming no day, converts to nothing", () => {
    expect(asIn(newYork, "2026-11-09 08:30", instant)).toStrictEqual(none);
    expect(asIn(newYork, "Nov 9 2026", instant)).toStrictEqual(none);
    expect(asIn(newYork, "2026-02-30", instant)).toStrictEqual(none);
    expect(asIn(newYork, 1794231000000, instant)).toStrictEqual(none);
  });

  test("an instant past the year 9999 converts to nothing, where the last millisecond converts", () => {
    expect(asIn(newYork, "9999-12-31T23:30-05:00", instant)).toStrictEqual(none);
    expect(momentOf(asIn(newYork, "9999-12-31T23:59:59.9999999Z", instant))).toBe("9999-12-31T23:59:59.999Z");
  });
});

describe("as(expr, file)", () => {
  test("a File converts to itself, and anything else to nothing", () => {
    const chart = new File(["admitted"], "chart.pdf");
    expect(heldOf(as(chart, file))).toBe(chart);
    expect(as(new Blob(["admitted"]), file)).toStrictEqual(none);
    expect(as("chart.pdf", file)).toStrictEqual(none);
  });
});

describe("as(expr, array(item))", () => {
  test("an array whose elements each convert to themselves is that array", () => {
    const beds = ["Bed 4", "Bed 5"];
    const seen = [new Date(2026, 10, 9)];
    expect(heldOf(as(beds, list(text)))).toBe(beds);
    expect(heldOf(as(seen, list(date)))).toBe(seen);
  });

  test("any other array becomes a new array of its converted elements, and one that converts to nothing stays nothing", () => {
    expect(as(["1.5", 2, "abc", undefined], list(invariantNumber))).toStrictEqual({ kind: "value", value: [1.5, 2, undefined, undefined] });
  });

  test("a value that is not an array converts to nothing, never a list of one", () => {
    expect(as("Bed 4", list(text))).toStrictEqual(none);
    expect(as("", list(text))).toStrictEqual(none);
    expect(as({ 0: "Bed 4", length: 1 }, list(text))).toStrictEqual(none);
  });
});

describe("as(expr, object(fields))", () => {
  test("an object whose declared fields each convert to themselves is that object, its identity and prototype kept", () => {
    const visit = new Visit(new Date(2026, 10, 9), "Bed 4");
    const admitted = { bed: "Bed 4" };
    expect(heldOf(as(visit, record({ seen: date })))).toBe(visit);
    expect(heldOf(as(admitted, record({ seen: date })))).toBe(admitted);
  });

  test("a plain object a declared field of which converts is a copy, each declared field converted and the others kept", () => {
    const visit = { seen: "2026-11-09T08:30", bed: "Bed 4" };
    expect(asIn(newYork, visit, record({ seen: instant }))).toStrictEqual({ kind: "value", value: { seen: new Date("2026-11-09T13:30:00.000Z"), bed: "Bed 4" } });
    expect(visit).toStrictEqual({ seen: "2026-11-09T08:30", bed: "Bed 4" });
  });

  test("an object that is not plain, a declared field of which would convert, converts to nothing", () => {
    expect(as(new Visit("2026-11-09", "Bed 4"), record({ seen: date }))).toStrictEqual(none);
  });

  test("a value that is not an object converts to nothing", () => {
    expect(as("Bed 4", record({ seen: date }))).toStrictEqual(none);
    expect(as(null, record({ seen: date }))).toStrictEqual(none);
    expect(as(["2026-11-09"], record({ seen: date }))).toStrictEqual(none);
  });

  test("a copy whose declared field converts to nothing is not built", () => {
    expect(() => as({ seen: "abc" }, record({ seen: date }))).toThrow("This page cannot run a copy of an object whose field 'seen' converts to nothing");
  });
});

describe("as(expr, any)", () => {
  test("any value converts to itself, null included", () => {
    const visit = new Visit("2026-11-09", "Bed 4");
    expect(heldOf(as(visit, any))).toBe(visit);
    expect(as(null, any)).toStrictEqual({ kind: "value", value: null });
  });
});
