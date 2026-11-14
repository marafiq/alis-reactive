import { describe, expect, test } from "vitest";
import { operated, type Term } from "./index";

type Read = ReturnType<Term>;

const nothing: Read = { kind: "nothing" };
const value = (read: unknown): Read => ({ kind: "value", value: read });

const held = (read: unknown): Term => () => value(read);
const readsNothing: Term = () => nothing;

describe("when a DateTime a live object holds is compared, it reads as its key, its wall fields to the tick", () => {
  test("a Date reads its local fields to the millisecond", () => {
    expect(operated("dateKey", [held(new Date(2026, 10, 9, 8, 30, 15, 120))])).toStrictEqual(value("2026-11-09T08:30:15.1200000"));
  });

  test("a day's text reads that day at midnight", () => {
    expect(operated("dateKey", [held("2026-11-09")])).toStrictEqual(value("2026-11-09T00:00:00.0000000"));
  });

  test("a day and time's text keeps all seven digits of its fraction", () => {
    expect(operated("dateKey", [held("2026-11-09T08:30:00.1234567")])).toStrictEqual(value("2026-11-09T08:30:00.1234567"));
  });

  test("a time written to the minute reads no seconds", () => {
    expect(operated("dateKey", [held("2026-11-09T08:30")])).toStrictEqual(value("2026-11-09T08:30:00.0000000"));
  });

  test("a 30th of February names no value, so it reads nothing", () => {
    expect(operated("dateKey", [held("2026-02-30T08:30:00")])).toStrictEqual(nothing);
  });

  test("a time at hour 24 names no value, so it reads nothing", () => {
    expect(operated("dateKey", [held("2026-11-09T24:00:00")])).toStrictEqual(nothing);
  });

  test("a text with an offset is outside its forms, so it stops the pipeline", () => {
    expect(() => operated("dateKey", [held("2026-11-09T08:30:00Z")])).toThrow(
      "The value \"2026-11-09T08:30:00Z\" is outside the forms the DateTime is compared in.",
    );
  });

  test("a number is outside its forms, so it stops the pipeline", () => {
    expect(() => operated("dateKey", [held(20261109)])).toThrow("The value 20261109 is outside the forms the DateTime is compared in.");
  });

  test("a Date of no time stops the pipeline", () => {
    expect(() => operated("dateKey", [held(new Date(Number.NaN))])).toThrow("The value a Date of no time is outside the forms the DateTime is compared in.");
  });

  test("nothing reads nothing and null reads null", () => {
    expect([operated("dateKey", [readsNothing]), operated("dateKey", [held(null)])]).toStrictEqual([nothing, value(null)]);
  });

  test("a value that names no value reads the start C# wrote beside it", () => {
    expect(operated("dateKey", [held("2026-02-30"), held("0001-01-01T00:00:00")])).toStrictEqual(value("0001-01-01T00:00:00.0000000"));
  });

  test("a value outside its forms reads the start C# wrote beside it, and does not stop", () => {
    expect(operated("dateKey", [held("next Tuesday"), held(null)])).toStrictEqual(value(null));
  });

  test("a Date and the text of its wall time are equal keys", () => {
    expect(
      operated("eq", [() => operated("dateKey", [held(new Date(2026, 10, 9, 8, 30))]), () => operated("dateKey", [held("2026-11-09T08:30:00")])]),
    ).toStrictEqual(value(true));
  });
});

describe("when a DateTime the app's JSON wrote is compared, it reads its wall fields with its offset dropped", () => {
  test("a text with an offset reads its wall time", () => {
    expect(operated("wallKey", [held("2026-11-09T08:30:00-05:00")])).toStrictEqual(value("2026-11-09T08:30:00.0000000"));
  });

  test("a day's text alone is outside its forms", () => {
    expect(() => operated("wallKey", [held("2026-11-09")])).toThrow("The value \"2026-11-09\" is outside the forms the DateTime is compared in.");
  });
});

describe("when a DateTimeOffset is compared, it reads as its instant's UTC fields to the tick", () => {
  test("a text with an offset reads its wall less its offset", () => {
    expect(operated("instantKey", [held("2026-11-09T08:30:00.5-05:00")])).toStrictEqual(value("2026-11-09T13:30:00.5000000"));
  });

  test("a Date reads its UTC fields", () => {
    expect(operated("instantKey", [held(new Date(Date.UTC(2026, 10, 9, 18, 0)))])).toStrictEqual(value("2026-11-09T18:00:00.0000000"));
  });

  test("one instant at two offsets is one key", () => {
    expect(
      operated("eq", [() => operated("instantKey", [held("2026-11-09T13:00:00-05:00")]), () => operated("instantKey", [held("2026-11-09T18:00:00Z")])]),
    ).toStrictEqual(value(true));
  });

  test("a text with no offset is outside its forms", () => {
    expect(() => operated("instantKey", [held("2026-11-09T08:30:00")])).toThrow(
      "The value \"2026-11-09T08:30:00\" is outside the forms the DateTimeOffset is compared in.",
    );
  });

  test("an offset past 14 hours names no value, so it reads nothing", () => {
    expect(operated("instantKey", [held("2026-11-09T08:30:00+15:00")])).toStrictEqual(nothing);
  });

  test("an instant before the year 1 names no value, so it reads nothing", () => {
    expect(operated("instantKey", [held("0001-01-01T00:30:00+01:00")])).toStrictEqual(nothing);
  });
});

describe("when a DateOnly, a TimeOnly or a TimeSpan is compared, it reads as its key", () => {
  test("a day's text reads as itself", () => {
    expect(operated("dayKey", [held("2026-11-09")])).toStrictEqual(value("2026-11-09"));
  });

  test("a day is never a Date", () => {
    expect(() => operated("dayKey", [held(new Date(Date.UTC(2026, 10, 9)))])).toThrow(
      "The value the Date 2026-11-09T00:00:00.000Z is outside the forms the DateOnly is compared in.",
    );
  });

  test("a time of day written to the minute reads its seconds and fraction as zero", () => {
    expect(operated("timeKey", [held("08:30")])).toStrictEqual(value("08:30:00.0000000"));
  });

  test("a time of day at hour 24 names no value, so it reads nothing", () => {
    expect(operated("timeKey", [held("24:00")])).toStrictEqual(nothing);
  });

  test("a span reads its ticks past the least span, as 20 digits", () => {
    expect(operated("spanKey", [held("01:30")])).toStrictEqual(value("09223372090854775808"));
  });

  test("a negative span with days reads below the zero span", () => {
    expect(operated("spanKey", [held("-1.02:00:00")])).toStrictEqual(value("09223371100854775808"));
  });

  test("a negative span orders before zero as its key's text", () => {
    expect(
      operated("lt", [() => operated("spanKey", [held("-00:00:01")]), () => operated("spanKey", [held("00:00:00")])]),
    ).toStrictEqual(value(true));
  });

  test("a span at hour 24 is outside its forms, since the endpoint reads it as 24 days", () => {
    expect(() => operated("spanKey", [held("24:00:00")])).toThrow("The value \"24:00:00\" is outside the forms the TimeSpan is compared in.");
  });

  test("a span at minute 60 names no value, so it reads nothing", () => {
    expect(operated("spanKey", [held("00:60:00")])).toStrictEqual(nothing);
  });

  test("a span one tick past the greatest span names no value, so it reads nothing", () => {
    expect(operated("spanKey", [held("10675199.02:48:05.4775808")])).toStrictEqual(nothing);
  });

  test("the greatest span reads the greatest key", () => {
    expect(operated("spanKey", [held("10675199.02:48:05.4775807")])).toStrictEqual(value("18446744073709551615"));
  });
});
