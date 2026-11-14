import { afterEach, describe, expect, test, vi } from "vitest";
import { Lane } from "./index";

type Block = ConstructorParameters<typeof Lane>[1];
type Step = Block[number];
type Source = Extract<Step, { kind: "do" }>["expr"];
type Layer = Extract<Step, { kind: "request" }>["layers"][number];
type Raised = Parameters<Lane["raise"]>[0];
type Held = Raised["scope"] extends ReadonlyMap<string, infer Read> ? Read : never;

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

interface Gate {
  readonly held: Promise<unknown>;
  readonly opens: (answer: unknown) => void;
}

function gate(): Gate {
  let opens: Gate["opens"] = () => undefined;
  const held = new Promise<unknown>((resolve) => {
    opens = resolve;
  });
  return {
    held,
    opens: (answer) => {
      opens(answer);
    },
  };
}

function raisedWith(bound: Readonly<Record<string, unknown>>): Raised {
  const scope = new Map<string, Held>();
  for (const [name, value] of Object.entries(bound)) scope.set(name, { kind: "value", value });
  return { plan: ward, scope, signal: new AbortController().signal };
}

function everyPromiseJobRan(): Promise<void> {
  return new Promise((resolve) => {
    setTimeout(resolve, 0);
  });
}

const named = (name: string): Source => ({ kind: "ref", name });
const text = (value: string): Source => ({ kind: "lit", value });
const charts = (value: Source): Step => ({ kind: "set", path: ["line"], members: "required", content: false, target: named("chart"), value });
const awaits = (name: string): Step => ({ kind: "do", await: true, expr: named(name) });
const stops = (reason: string): Step => ({ kind: "stop", reason });
const parallel = (...blocks: Block[]): Step => ({ kind: "all", blocks });
const layer = (whileLoading: Block, last: Block): Layer => ({ whileLoading, finally: last });
const request = (...layers: Layer[]): Step => ({
  kind: "request",
  argument: "sent",
  reply: "reply",
  send: { kind: "lit", value: null },
  check: [],
  layers,
  success: { kind: "none", do: [] },
  statuses: [],
  otherwise: { kind: "none", do: [] },
  chained: [],
});

const aRound: Block = [charts(named("starts")), awaits("settle"), charts(named("ends"))];

afterEach(() => {
  vi.restoreAllMocks();
});

describe("when a handler's event is raised while its last run is in flight, its lane keeps Lanes.tla's invariants", () => {
  test("overlap starts another run beside the first, and each goes on as its own promise resolves", async () => {
    const chart = new Chart();
    const first = gate();
    const second = gate();
    const lane = new Lane("overlap", aRound);

    lane.raise(raisedWith({ chart, settle: first.held, starts: "start 1", ends: "end 1" }));
    lane.raise(raisedWith({ chart, settle: second.held, starts: "start 2", ends: "end 2" }));
    second.opens(null);
    await everyPromiseJobRan();
    first.opens(null);
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual(["start 1", "start 2", "end 2", "end 1"]);
  });

  test("SkippedTriggerStartsNothing: under skip a raise starts nothing while a run is in flight, and the next raise after it ended starts one", async () => {
    const chart = new Chart();
    const first = gate();
    const lane = new Lane("skip", aRound);

    lane.raise(raisedWith({ chart, settle: first.held, starts: "start 1", ends: "end 1" }));
    lane.raise(raisedWith({ chart, settle: gate().held, starts: "start 2", ends: "end 2" }));
    first.opens(null);
    await everyPromiseJobRan();
    lane.raise(raisedWith({ chart, settle: gate().held, starts: "start 3", ends: "end 3" }));

    expect(chart.lines).toStrictEqual(["start 1", "end 1", "start 3"]);
  });

  test("ReplacedRunRunsNothingAfter: under replace the run in flight runs no later step, though its promise resolves after the new run's", async () => {
    const chart = new Chart();
    const first = gate();
    const second = gate();
    const lane = new Lane("replace", aRound);

    lane.raise(raisedWith({ chart, settle: first.held, starts: "start 1", ends: "end 1" }));
    lane.raise(raisedWith({ chart, settle: second.held, starts: "start 2", ends: "end 2" }));
    second.opens(null);
    await everyPromiseJobRan();
    first.opens(null);
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual(["start 1", "start 2", "end 2"]);
  });

  test("DisposedRunsNothingMore: after its owner's dispose a run in flight runs no later step, and a raise starts nothing", async () => {
    const chart = new Chart();
    const first = gate();
    const lane = new Lane("overlap", aRound);
    lane.raise(raisedWith({ chart, settle: first.held, starts: "start 1", ends: "end 1" }));

    lane.dispose();
    first.opens(null);
    await everyPromiseJobRan();
    lane.raise(raisedWith({ chart, settle: gate().held, starts: "start 2", ends: "end 2" }));

    expect(chart.lines).toStrictEqual(["start 1"]);
  });

  test("FinallyRunsBeforeRelease: a dispose returns after the Finally of each run in flight ran, so its owner releases after it", () => {
    const chart = new Chart();
    const lane = new Lane("overlap", [request(layer([charts(text("loading"))], [charts(text("finally"))]), layer([awaits("settle")], []))]);
    lane.raise(raisedWith({ chart, settle: gate().held }));

    lane.dispose();
    chart.line = "released";

    expect(chart.lines).toStrictEqual(["loading", "finally", "released"]);
  });
});

describe("when a block runs Parallel, its join keeps Lanes.tla's invariants", () => {
  test("AllSettledAfterEveryRequest: each branch starts in authored order in one turn, and the step after Parallel runs once, after every branch settled", async () => {
    const chart = new Chart();
    const residents = gate();
    const staff = gate();
    const lane = new Lane("overlap", [
      parallel([charts(text("residents sent")), awaits("residents"), charts(text("residents shown"))], [charts(text("staff sent")), awaits("staff"), charts(text("staff shown"))]),
      charts(text("Board ready")),
    ]);

    lane.raise(raisedWith({ chart, residents: residents.held, staff: staff.held }));

    expect(chart.lines).toStrictEqual(["residents sent", "staff sent"]);
    staff.opens(null);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual(["residents sent", "staff sent", "staff shown"]);
    residents.opens(null);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual(["residents sent", "staff sent", "staff shown", "residents shown", "Board ready"]);
  });

  test("BranchStopEndsOnlyItself: a stop in one branch ends that branch only, and the step after Parallel runs once the other settled", async () => {
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    const chart = new Chart();
    const staff = gate();
    const lane = new Lane("overlap", [
      parallel([stops("No resident is admitted."), charts(text("residents shown"))], [awaits("staff"), charts(text("staff shown"))]),
      charts(text("Board ready")),
    ]);

    lane.raise(raisedWith({ chart, staff: staff.held }));
    staff.opens(null);
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual(["staff shown", "Board ready"]);
    expect(logged.mock.calls).toStrictEqual([["No resident is admitted."]]);
  });

  test("JoinAfterEveryBranchTail: the step after Parallel waits for a branch's second awaited step, the tail of its block", async () => {
    const chart = new Chart();
    const residents = gate();
    const allergies = gate();
    const staff = gate();
    const lane = new Lane("overlap", [
      parallel([awaits("residents"), charts(text("residents shown")), awaits("allergies"), charts(text("allergies shown"))], [awaits("staff"), charts(text("staff shown"))]),
      charts(text("Board ready")),
    ]);

    lane.raise(raisedWith({ chart, residents: residents.held, allergies: allergies.held, staff: staff.held }));
    residents.opens(null);
    staff.opens(null);
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual(["residents shown", "staff shown"]);
    allergies.opens(null);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual(["residents shown", "staff shown", "allergies shown", "Board ready"]);
  });
});
