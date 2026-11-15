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
type Source = Extract<Step, { kind: "do" }>["expr"];
type DataRead = Extract<Source, { kind: "data" }>;
type Walked = DataRead["path"][number];
type Absent = Walked["absent"];
type Raised = Parameters<Lane["raise"]>[0];
type Held = Raised["scope"] extends ReadonlyMap<string, infer Read> ? Read : never;

const untraced: Raised["trace"] = {
  handler: () => untraced,
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

const record: Source = { kind: "ref", name: "record" };
const charts = (value: Source): Step => ({ kind: "set", path: ["line"], members: "required", content: false, target: { kind: "ref", name: "chart" }, value });
const before: Step = charts({ kind: "lit", value: "before" });
const starts = (value: string): Absent => ({ kind: "start", value });
const readsNothing: Absent = { kind: "nothing" };
const unbound: Absent = { kind: "unbound" };
const step = (name: string, absent: Absent, requires: readonly string[] = []): Walked => ({ name, requires, absent });
const read = (...path: Walked[]): Source => ({ kind: "data", path, target: record });

function charted(held: unknown, ...path: Walked[]): unknown[] {
  const chart = new Chart();
  new Lane("overlap", [before, charts(read(...path))], aRoundsOwner).raise(raisedWith({ chart, record: held }));
  return chart.lines;
}

afterEach(() => {
  vi.restoreAllMocks();
});

describe("when a data read meets its record, it keeps Absence.tla's reads", () => {
  test("DataAbsentReadsItsStart: a member the record holds reads its value", () => {
    expect(charted({ shift: "day" }, step("shift", starts("night")))).toStrictEqual(["before", "day"]);
  });

  test("DataAbsentReadsItsStart: a member the record lacks reads the start its step carries", () => {
    expect(charted({}, step("shift", starts("night")))).toStrictEqual(["before", "night"]);
  });

  test("DataAbsentReadsItsStart: a member held as undefined is absent and reads its start", () => {
    expect(charted({ shift: undefined }, step("shift", starts("night")))).toStrictEqual(["before", "night"]);
  });

  test("DataAbsentReadsItsStart: a member the record lacks reads nothing where a holder on the rest of the path starts null", () => {
    expect(charted({}, step("lead", readsNothing), step("name", starts("none")))).toStrictEqual(["before", null]);
  });

  test("DataAbsentReadsItsStart: a member the record lacks stops the pipeline where its binding cannot make the holder", () => {
    const logged = stopsLogged();

    expect(charted({}, step("dose", unbound))).toStrictEqual(["before"]);
    expect(logged).toHaveBeenCalledOnce();
  });

  test("a member that holds null reads null", () => {
    expect(charted({ note: null }, step("note", starts("none")))).toStrictEqual(["before", null]);
  });

  test("NullHolderReadsNothing: a holder that holds null reads nothing, and so does every member past it", () => {
    expect(charted({ lead: null }, step("lead", starts("none")), step("name", starts("Ada Hart")))).toStrictEqual(["before", null]);
  });

  test("each step reads the member of the value the step before it read", () => {
    expect(charted({ lead: { name: "Ada Hart" } }, step("lead", readsNothing), step("name", starts("none")))).toStrictEqual(["before", "Ada Hart"]);
  });

  test("LackingRequiredStops: a holder that lacks a member its type requires stops the pipeline, though it holds the member read", () => {
    const logged = stopsLogged();

    expect(charted({ shift: "day" }, step("shift", starts("night"), ["ward"]))).toStrictEqual(["before"]);
    expect(logged).toHaveBeenCalledOnce();
  });

  test("a holder that holds every member its type requires reads on", () => {
    expect(charted({ shift: "day", ward: "North" }, step("shift", starts("night"), ["ward"]))).toStrictEqual(["before", "day"]);
  });

  test("a holder that is not an object stops the pipeline", () => {
    const logged = stopsLogged();

    expect(charted({ lead: "Ada Hart" }, step("lead", readsNothing), step("name", starts("none")))).toStrictEqual(["before"]);
    expect(logged).toHaveBeenCalledOnce();
  });

  test("PromiseWhereValueStops: a member that holds a promise stops the pipeline", () => {
    const logged = stopsLogged();

    expect(charted({ shift: Promise.resolve("day") }, step("shift", starts("night")))).toStrictEqual(["before"]);
    expect(logged).toHaveBeenCalledOnce();
  });

  test("what every object inherits is no member of the record, so it reads the start", () => {
    expect(charted({}, step("constructor", starts("none")))).toStrictEqual(["before", "none"]);
  });

  test("a member the record holds by the name of what every object inherits reads its value", () => {
    expect(charted(JSON.parse('{"constructor":"held"}'), step("constructor", starts("none")))).toStrictEqual(["before", "held"]);
  });
});

describe("when a container's data read uses the sent body", () => {
  test("AnUnsentMemberReadsWhatTheEndpointBinds: an absent number reads its literal start", () => {
    expect(charted({}, step("nights", { kind: "start", value: 5 }))).toStrictEqual(["before", 5]);
  });

  test("ReadIsWhatTheEndpointBinds and ReachBlankReadsItsStart: the binding's cleared number reads its live start", () => {
    const chart = new Chart();
    const blank: Source = {
      kind: "op", op: "coalesce", args: [
        { kind: "as", expr: record, shape: { kind: "nullable", inner: { kind: "number", of: "integer", text: "view", overflow: "nothing" } } },
        { kind: "lit", value: 5 },
      ],
    };
    new Lane("overlap", [before, charts(blank)], aRoundsOwner).raise(raisedWith({ chart, record: "   " }));
    expect(chart.lines).toStrictEqual(["before", 5]);
  });
});
