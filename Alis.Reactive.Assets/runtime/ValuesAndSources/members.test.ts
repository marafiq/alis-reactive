import { describe, expect, test } from "vitest";
import { got, nothing, Stop, value } from "./index";

class Pump {
  readonly #rate = 40;

  get rate(): number {
    return this.#rate;
  }
}

describe("when a get reads a declared member, it reads as the member's mark says", () => {
  test("RequiredAbsentStops: a required member the holder lacks stops the pipeline", () => {
    expect(() => got(value({ ward: "North" }), ["bed"], "required")).toThrow(Stop);
  });

  test("a required member the holder holds reads its value", () => {
    expect(got(value({ ward: "North" }), ["ward"], "required")).toStrictEqual(value("North"));
  });

  test("a required member of a holder that reads nothing stops the pipeline", () => {
    expect(() => got(nothing, ["ward"], "required")).toThrow(Stop);
  });

  test("a required member of a holder that holds null stops the pipeline", () => {
    expect(() => got(value(null), ["ward"], "required")).toThrow(Stop);
  });

  test("a member a class declares by a getter is a member of its instance", () => {
    expect(got(value(new Pump()), ["rate"], "required")).toStrictEqual(value(40));
  });

  test("a member held as undefined reads nothing", () => {
    expect(got(value({ ward: undefined }), ["ward"], "required")).toStrictEqual(nothing);
  });

  test("PromiseWhereValueStops: a member that holds a promise where a value is declared stops the pipeline", () => {
    expect(() => got(value({ ward: Promise.resolve("North") }), ["ward"], "required")).toThrow(Stop);
  });

  test("an optional member the holder lacks reads nothing", () => {
    expect(got(value({ ward: "North" }), ["bed"], "optional")).toStrictEqual(nothing);
  });

  test("an optional member of a holder that reads nothing reads nothing", () => {
    expect(got(nothing, ["ward"], "optional")).toStrictEqual(nothing);
  });

  test("a member required if held reads nothing on a holder that reads nothing", () => {
    expect(got(nothing, ["length"], "requiredIfHeld")).toStrictEqual(nothing);
  });

  test("a member required if held stops on a holder that lacks it", () => {
    expect(() => got(value({ ward: "North" }), ["length"], "requiredIfHeld")).toThrow(Stop);
  });

  test("a member required if held reads its value on a holder that holds it", () => {
    expect(got(value(["North", "East"]), ["length"], "requiredIfHeld")).toStrictEqual(value(2));
  });

  test("an optional member of a required holder reads nothing on a holder that lacks it", () => {
    expect(got(value({ ward: "North" }), ["bed"], "optionalMember")).toStrictEqual(nothing);
  });

  test("an optional member of a required holder stops on a holder that reads nothing", () => {
    expect(() => got(nothing, ["bed"], "optionalMember")).toThrow(Stop);
  });

  test("each step of a path reads the member of the value the step before it read", () => {
    expect(got(value({ ward: { lead: { name: "Ada Hart" } } }), ["ward", "lead", "name"], "required")).toStrictEqual(value("Ada Hart"));
  });

  test("an index step reads the item of a list", () => {
    expect(got(value({ beds: ["B-1", "B-2"] }), ["beds", 1], "required")).toStrictEqual(value("B-2"));
  });
});
