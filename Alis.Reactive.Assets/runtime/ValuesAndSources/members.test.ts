import { describe, expect, test } from "vitest";
import { called, got, heldOrNull, nothing, Stop, value } from "./index";

class Pump {
  readonly #rate = 40;

  get rate(): number {
    return this.#rate;
  }
}

class Bell {
  readonly rang: unknown[][] = [];

  readonly room: string;

  constructor(room: string) {
    this.room = room;
  }

  ring(...said: unknown[]): string {
    this.rang.push(said);
    return "rang " + this.room;
  }

  hush(): undefined {
    return undefined;
  }

  later(): Promise<string> {
    return Promise.reject(new Error("the bell is out of order"));
  }
}

const unread = (): readonly unknown[] => {
  throw new Error("arguments no call should read");
};

function holderOfItsThis(this: unknown): boolean {
  return this === undefined;
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

describe("when a call runs a declared method, it calls it on its holder as the method's mark says", () => {
  test("a required method the holder holds is called on the holder with its arguments, and reads what it returns", () => {
    const bell = new Bell("Room 4");

    expect(called(value(bell), ["ring"], "required", () => ["now", null], "value")).toStrictEqual(value("rang Room 4"));
    expect(bell.rang).toStrictEqual([["now", null]]);
  });

  test("a method that returns nothing reads nothing", () => {
    expect(called(value(new Bell("Room 4")), ["hush"], "required", () => [], "value")).toStrictEqual(nothing);
  });

  test("each step before the method reads the member of the value the step before it read, and the last is the method's holder", () => {
    const bell = new Bell("Room 9");

    expect(called(value({ ward: { bell } }), ["ward", "bell", "ring"], "required", () => [], "value")).toStrictEqual(value("rang Room 9"));
  });

  test("a method of a text runs on the text", () => {
    expect(called(value("North"), ["toLowerCase"], "required", () => [], "value")).toStrictEqual(value("north"));
  });

  test("RequiredAbsentStops: a required method the holder lacks stops the pipeline, its arguments unread", () => {
    expect(() => called(value(new Bell("Room 4")), ["silence"], "required", unread, "value")).toThrow(new Stop("The member 'silence' is not on the object that should hold it."));
  });

  test("a required method the holder holds as undefined stops the pipeline", () => {
    expect(() => called(value({ ring: undefined }), ["ring"], "required", unread, "value")).toThrow(new Stop("The member 'ring' is no method."));
  });

  test("a required member that holds no function stops the pipeline", () => {
    expect(() => called(value({ ring: "loud" }), ["ring"], "required", unread, "value")).toThrow(new Stop("The member 'ring' is no method."));
  });

  test("a required method of a holder that reads nothing or holds null stops the pipeline", () => {
    expect(() => called(nothing, ["ring"], "required", unread, "value")).toThrow(new Stop("Nothing holds the member 'ring'."));
    expect(() => called(value(null), ["ring"], "required", unread, "value")).toThrow(new Stop("Nothing holds the member 'ring'."));
  });

  test("a method required if held reads nothing on a holder that reads nothing, its arguments unread", () => {
    expect(called(nothing, ["filter"], "requiredIfHeld", unread, "value")).toStrictEqual(nothing);
  });

  test("a method required if held stops on a holder that lacks it, as a paged result lacks a list's filter", () => {
    expect(() => called(value({ result: [], count: 0 }), ["filter"], "requiredIfHeld", unread, "value")).toThrow(new Stop("The member 'filter' is not on the object that should hold it."));
  });

  test("an optional method the holder lacks, or holds as undefined, runs nothing and reads nothing, its arguments unread", () => {
    expect(called(value(new Bell("Room 4")), ["silence"], "optional", unread, "value")).toStrictEqual(nothing);
    expect(called(value({ ring: undefined }), ["ring"], "optional", unread, "value")).toStrictEqual(nothing);
  });

  test("an optional method of a holder that reads nothing runs nothing", () => {
    expect(called(nothing, ["ring"], "optional", unread, "value")).toStrictEqual(nothing);
  });

  test("an optional method of a required holder runs nothing on a holder that lacks it and stops on a holder that reads nothing", () => {
    expect(called(value(new Bell("Room 4")), ["silence"], "optionalMember", unread, "value")).toStrictEqual(nothing);
    expect(() => called(nothing, ["ring"], "optionalMember", unread, "value")).toThrow(new Stop("Nothing holds the member 'ring'."));
  });

  test("PromiseWhereValueStops: a method that returns a promise where a value is declared stops the pipeline, and the promise's own failure is held", () => {
    expect(() => called(value(new Bell("Room 4")), ["later"], "required", () => [], "value")).toThrow(
      new Stop("The method 'later' returned a promise where its declaration reads a value; a method that returns one is declared with MethodAsync."),
    );
  });

  test("a method an awaited step calls hands its promise on as returned, for the step to await", async () => {
    const read = called(value(new Bell("Room 4")), ["later"], "required", () => [], "awaited");

    await expect(heldOrNull(read)).rejects.toThrow("the bell is out of order");
  });

  test("an empty path calls the target itself, with no holder as its this", () => {
    expect(called(value(holderOfItsThis), [], "required", () => [], "value")).toStrictEqual(value(true));
  });

  test("an empty path stops the pipeline on a target that is no function or reads nothing", () => {
    expect(() => called(value({ ring: "loud" }), [], "required", unread, "value")).toThrow(new Stop("The object called as a function is no function."));
    expect(() => called(nothing, [], "required", unread, "value")).toThrow(new Stop("The object called as a function is no function."));
  });
});
