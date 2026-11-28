import { describe, expect, test } from "vitest";
import { readPlan } from "./index";

const anotherVersion = "This plan was written for plan version 6; this page runs version 7.";
const notJson = "This plan's text is not JSON.";
const noVersion = "This plan names no plan version; this page runs version 7.";

describe("when a plan script's text is read", () => {
  test("a plan of version 7 is the plan its text holds", () => {
    const text =
      '{"version":7,"planId":"rounds","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",",' +
      '"integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},' +
      '"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{},"handlers":[]}';

    expect(readPlan(text)).toStrictEqual({
      kind: "plan",
      plan: {
        version: 7,
        planId: "rounds",
        culture: {
          name: "en-US",
          collation: "en-US",
          decimal: ".",
          group: ",",
          integer: { group: false, exponent: false },
          fraction: { group: true, exponent: true },
        },
        timeZone: { kind: "clock" },
        trace: { kind: "off" },
        vendors: {},
        objects: {},
        handlers: [],
      },
    });
  });

  test("a plan of version 6 is refused with its version and the page's", () => {
    expect(readPlan('{"version":6,"planId":"rounds","handlers":[]}')).toStrictEqual({ kind: "refused", error: anotherVersion });
  });

  test("a text cut off inside an object is refused as no JSON", () => {
    expect(readPlan('{"version":7,"planId":"vitals","handlers":[{"on":"pulse"')).toStrictEqual({ kind: "refused", error: notJson });
  });

  test("an empty text is refused as no JSON", () => {
    expect(readPlan("")).toStrictEqual({ kind: "refused", error: notJson });
  });

  test("a list is refused as naming no version", () => {
    expect(readPlan("[]")).toStrictEqual({ kind: "refused", error: noVersion });
  });

  test("an object with no version is refused as naming no version", () => {
    expect(readPlan("{}")).toStrictEqual({ kind: "refused", error: noVersion });
  });

  test("a version written as text is refused as naming no version", () => {
    expect(readPlan('{"version":"7"}')).toStrictEqual({ kind: "refused", error: noVersion });
  });

  test("null is refused as naming no version", () => {
    expect(readPlan("null")).toStrictEqual({ kind: "refused", error: noVersion });
  });
});
