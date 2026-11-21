import { afterEach, describe, expect, test, vi } from "vitest";
import { Lane } from "./index";

const aRoundsOwner: ConstructorParameters<typeof Lane>[2] = {
  runs: () => undefined,
  mounts: () => undefined,
  listens: () => undefined,
  holds: () => undefined,
  registers: () => undefined,
};

type Block = ConstructorParameters<typeof Lane>[1];
type Step = Block[number];
type Write = Extract<Step, { kind: "set" }>;
type Raised = Parameters<Lane["raise"]>[0];
type Held = Raised["scope"] extends ReadonlyMap<string, infer Read> ? Read : never;

const untraced: Raised["trace"] = {
  handler: () => untraced,
  heardOn: () => untraced,
  run: () => untraced,
  span: () => untraced,
  send: () => ({ attempt: () => ({ headers: {}, answered: () => undefined, end: () => undefined }), end: () => undefined }),
  event: () => undefined,
  end: () => undefined,
};

const ward: Raised["plan"] = {
  version: 7,
  planId: "ward",
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
};

class Chart {
  readonly lines: unknown[] = [];

  set line(written: unknown) {
    this.lines.push(written);
  }
}

function raisedWith(bound: Readonly<Record<string, unknown>>): Raised {
  const scope = new Map<string, Held>();
  for (const [name, value] of Object.entries(bound)) scope.set(name, { kind: "value", value });
  return { plan: ward, scope, signal: new AbortController().signal, trace: untraced };
}

function stopsLogged() {
  return vi.spyOn(console, "error").mockImplementation(() => undefined);
}

const sets = (path: Write["path"], members: Write["members"], value: string): Step => ({
  kind: "set",
  path,
  members,
  content: false,
  target: { kind: "ref", name: "station" },
  value: { kind: "lit", value },
});
const charts: Step = { kind: "set", path: ["line"], members: "required", content: false, target: { kind: "ref", name: "chart" }, value: { kind: "lit", value: "after" } };

function ran(station: unknown, write: Step): unknown[] {
  const chart = new Chart();
  new Lane("overlap", [write, charts], aRoundsOwner).raise(raisedWith({ chart, station }));
  return chart.lines;
}

afterEach(() => {
  vi.restoreAllMocks();
});

describe("when a set writes at a path below its target, it writes as its member's mark says", () => {
  test("a set at a path of two steps writes the member of the holder the first step reads", () => {
    const station = { pager: { number: "4" } };

    expect(ran(station, sets(["pager", "number"], "required", "7"))).toStrictEqual(["after"]);
    expect(station).toStrictEqual({ pager: { number: "7" } });
  });

  test("RequiredAbsentStops: a required member the holder lacks stops the pipeline and is not written", () => {
    const logged = stopsLogged();
    const station = { pager: { number: "4" } };

    expect(ran(station, sets(["pager", "extension"], "required", "7"))).toStrictEqual([]);
    expect(station).toStrictEqual({ pager: { number: "4" } });
    expect(logged).toHaveBeenCalledOnce();
  });

  test("a required holder on the path the target lacks stops the pipeline", () => {
    const logged = stopsLogged();

    expect(ran({ phone: "5" }, sets(["pager", "number"], "required", "7"))).toStrictEqual([]);
    expect(logged).toHaveBeenCalledOnce();
  });

  test("an optional member is written on a holder that lacks it", () => {
    const station = { pager: { number: "4" } };

    expect(ran(station, sets(["pager", "extension"], "optional", "12"))).toStrictEqual(["after"]);
    expect(station).toStrictEqual({ pager: { number: "4", extension: "12" } });
  });

  test("an optional member is written on a holder that holds it", () => {
    const station = { pager: { number: "4" } };

    expect(ran(station, sets(["pager", "number"], "optional", "7"))).toStrictEqual(["after"]);
    expect(station).toStrictEqual({ pager: { number: "7" } });
  });

  test("an optional member of a holder that holds nothing stops the pipeline", () => {
    const logged = stopsLogged();

    expect(ran({ pager: null }, sets(["pager", "number"], "optional", "7"))).toStrictEqual([]);
    expect(logged).toHaveBeenCalledOnce();
  });

  test("an optional member of a holder the path reads as absent stops the pipeline", () => {
    const logged = stopsLogged();

    expect(ran({ phone: "5" }, sets(["pager", "number"], "optional", "7"))).toStrictEqual([]);
    expect(logged).toHaveBeenCalledOnce();
  });
});
