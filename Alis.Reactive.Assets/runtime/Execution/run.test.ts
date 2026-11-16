import { afterEach, describe, expect, test, vi } from "vitest";
import { cutsRunsInTheirStep, Lane } from "./index";

type AnOwner = ConstructorParameters<typeof Lane>[2];

function anOwner(): AnOwner {
  return {
    runs: () => undefined,
    mounts: () => undefined,
    listens: () => undefined,
    holds: () => undefined,
    registers: () => undefined,
  };
}

const aRoundsOwner = anOwner();
const theCard = anOwner();
const thePage = anOwner();

type Block = ConstructorParameters<typeof Lane>[1];
type Step = Block[number];
type Source = Extract<Step, { kind: "do" }>["expr"];
type Layer = Extract<Step, { kind: "request" }>["layers"][number];
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
  readonly signals: AbortSignal[] = [];

  set line(written: unknown) {
    this.lines.push(written);
  }

  set signal(held: AbortSignal) {
    this.signals.push(held);
  }
}

class Bell {
  readonly rungBy: string[] = [];
  readonly #rings: () => void;

  constructor(rings: () => void) {
    this.#rings = rings;
  }

  set rung(by: string) {
    this.rungBy.push(by);
    this.#rings();
  }
}

interface Gate {
  readonly held: Promise<unknown>;
  readonly opens: (answer: unknown) => void;
  readonly refuses: (error: Error) => void;
}

function gate(): Gate {
  let opens: Gate["opens"] = () => undefined;
  let refuses: Gate["refuses"] = () => undefined;
  const held = new Promise<unknown>((resolve, reject) => {
    opens = resolve;
    refuses = reject;
  });
  return {
    held,
    opens: (answer) => {
      opens(answer);
    },
    refuses: (error) => {
      refuses(error);
    },
  };
}

function raisedWith(bound: Readonly<Record<string, unknown>>): Raised {
  const scope = new Map<string, Held>();
  for (const [name, value] of Object.entries(bound)) scope.set(name, { kind: "value", value });
  return { plan: ward, scope, signal: new AbortController().signal, trace: untraced };
}

class Spans {
  readonly recorded: string[] = [];
  #started = 0;

  readonly root: Raised["trace"] = this.#traced("handler");

  #traced(name: string): Raised["trace"] {
    return {
      handler: () => this.root,
      run: () => this.#starts("alis.run", {}),
      span: (child, attributes) => this.#starts(child, attributes),
      send: () => untraced.send(),
      event: (event, attributes) => {
        this.recorded.push(name + " " + event + " " + JSON.stringify(attributes));
      },
      end: (outcome) => {
        this.recorded.push(name + " " + outcome);
      },
    };
  }

  #starts(name: string, attributes: Readonly<Record<string, unknown>>): Raised["trace"] {
    this.#started += 1;
    const numbered = name + "#" + String(this.#started);
    this.recorded.push(numbered + " starts " + JSON.stringify(attributes));
    return this.#traced(numbered);
  }
}

function tracedWith(spans: Spans, bound: Readonly<Record<string, unknown>>): Raised {
  return { ...raisedWith(bound), trace: spans.root };
}

function endsOn(chart: Chart, whose: string, bound: Readonly<Record<string, unknown>>): Raised {
  const ended: Raised["trace"] = {
    ...untraced,
    end: (outcome) => {
      chart.lines.push(whose + " ended " + outcome);
    },
  };
  return { ...raisedWith(bound), trace: { ...untraced, run: () => ended } };
}

function aSlotEnding(owner: AnOwner, chart: Chart, waited: boolean[]): { readonly unload: () => void } {
  return {
    unload: () => {
      waited.push(
        cutsRunsInTheirStep([owner], () => {
          chart.lines.push("the slot released");
        }),
      );
    },
  };
}

function everyPromiseJobRan(): Promise<void> {
  return new Promise((resolve) => {
    setTimeout(resolve, 0);
  });
}

function stopsLogged() {
  return vi.spyOn(console, "error").mockImplementation(() => undefined);
}

const named = (name: string): Source => ({ kind: "ref", name });
const calls = (holder: string, method: string): Source => ({ kind: "call", path: [method], members: "required", target: named(holder), args: [] });
const text = (value: string): Source => ({ kind: "lit", value });
const charts = (value: Source): Step => ({ kind: "set", path: ["line"], members: "required", content: false, target: named("chart"), value });
const chartsItsSignal: Step = { kind: "set", path: ["signal"], members: "required", content: false, target: named("chart"), value: { kind: "signal" } };
const rings: Step = { kind: "set", path: ["rung"], members: "required", content: false, target: named("bell"), value: text("the handler") };
const awaits = (name: string): Step => ({ kind: "do", await: true, expr: named(name) });
const unloads: Step = { kind: "do", await: false, expr: calls("slot", "unload") };
const stops = (reason: string): Step => ({ kind: "stop", reason });
const when = (name: string, then: Block, otherwise: Block): Step => ({ kind: "if", when: named(name), then, else: otherwise });
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

afterEach(() => {
  vi.restoreAllMocks();
});

describe("when a handler's event is raised, its run keeps Run.tla's invariants", () => {
  test("SyncRunEndsInItsTurn: a run with no awaited step ends in its trigger's turn, its branch and the step after it included", () => {
    const chart = new Chart();
    const lane = new Lane("overlap", [charts(text("admitted")), when("flagged", [charts(text("flagged"))], []), charts(text("charted"))], aRoundsOwner);

    lane.raise(raisedWith({ chart, flagged: true }));

    expect(chart.lines).toStrictEqual(["admitted", "flagged", "charted"]);
  });

  test("SecondHandlerStartsInTheTurn: the second handler of an event runs to its end while the first waits, and the first goes on when its promise resolves", async () => {
    const chart = new Chart();
    const settle = gate();
    const first = new Lane("overlap", [charts(text("first writes")), awaits("settle"), charts(text("first goes on"))], aRoundsOwner);
    const second = new Lane("overlap", [charts(text("second writes"))], aRoundsOwner);
    const raised = raisedWith({ chart, settle: settle.held });

    first.raise(raised);
    second.raise(raised);

    expect(chart.lines).toStrictEqual(["first writes", "second writes"]);
    settle.opens(null);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual(["first writes", "second writes", "first goes on"]);
  });

  test("NothingRunsWhileItWaits: the step after an if runs after the whole of the branch it ran, its awaited step included", async () => {
    const chart = new Chart();
    const settle = gate();
    const lane = new Lane("overlap", [when("flagged", [awaits("settle"), charts(text("branch"))], []), charts(text("after the if"))], aRoundsOwner);

    lane.raise(raisedWith({ chart, flagged: true, settle: settle.held }));

    expect(chart.lines).toStrictEqual([]);
    settle.opens(null);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual(["branch", "after the if"]);
  });

  test("NothingRunsAfterAStop: a stop in a branch runs no later step of its branch or of its handler, and logs its reason", () => {
    const logged = stopsLogged();
    const chart = new Chart();
    const lane = new Lane("overlap", [
      charts(text("before")),
      when("flagged", [stops("The bed is taken."), charts(text("after the stop"))], []),
      charts(text("after the if")),
    ], aRoundsOwner);

    lane.raise(raisedWith({ chart, flagged: true }));

    expect(chart.lines).toStrictEqual(["before"]);
    expect(logged.mock.calls).toStrictEqual([["The bed is taken."]]);
  });

  test("NothingRunsAfterAnAbort: a step that raises its handler's own event under replace is the last step of its run", () => {
    const chart = new Chart();
    const lane = new Lane("replace", [charts(named("round")), rings, charts(named("after"))], aRoundsOwner);
    const quiet = new Bell(() => undefined);
    const bell = new Bell(() => {
      lane.raise(raisedWith({ chart, bell: quiet, round: "round 2", after: "after 2" }));
    });

    lane.raise(raisedWith({ chart, bell, round: "round 1", after: "after 1" }));

    expect(chart.lines).toStrictEqual(["round 1", "round 2", "after 2"]);
  });

  test("FinallyRunsWhenLoaded: a stop in a request's own WhileLoading runs the Finally of the defaults' layer, whose WhileLoading completed", () => {
    stopsLogged();
    const chart = new Chart();
    const lane = new Lane("overlap", [
      request(
        layer([charts(text("defaults loading"))], [charts(text("defaults finally"))]),
        layer([charts(text("loading")), stops("No bed is free.")], [charts(text("finally"))]),
      ),
      charts(text("after the request")),
    ], aRoundsOwner);

    lane.raise(raisedWith({ chart }));

    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "defaults finally"]);
  });

  test("FinallyOnlyWhenLoaded: a replace while a request's own WhileLoading waits runs the defaults' Finally and not its own, and no later step of that WhileLoading", async () => {
    const chart = new Chart();
    const first = gate();
    const second = gate();
    const lane = new Lane("replace", [
      request(layer([], [charts(named("defaultsFinally"))]), layer([awaits("settle"), charts(named("busy"))], [charts(named("finally"))])),
    ], aRoundsOwner);

    lane.raise(raisedWith({ chart, settle: first.held, defaultsFinally: "defaults finally 1", busy: "busy 1", finally: "finally 1" }));
    lane.raise(raisedWith({ chart, settle: second.held, defaultsFinally: "defaults finally 2", busy: "busy 2", finally: "finally 2" }));

    expect(chart.lines).toStrictEqual(["defaults finally 1"]);
    first.opens(null);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual(["defaults finally 1"]);
  });

  test("FinallyRunsOnce: a stop inside a request's Finally runs the defaults' Finally after it and never that Finally again", () => {
    stopsLogged();
    const chart = new Chart();
    const lane = new Lane("overlap", [
      request(
        layer([], [charts(text("defaults finally"))]),
        layer([], [charts(text("finally starts")), stops("The chart is locked."), charts(text("finally ends"))]),
        layer([stops("No bed is free.")], [charts(text("never loaded"))]),
      ),
    ], aRoundsOwner);

    lane.raise(raisedWith({ chart }));

    expect(chart.lines).toStrictEqual(["finally starts", "defaults finally"]);
  });

  test("InnerFinallyFirst: a stop unwinds the inner request's Finally and its defaults', then the outer request's own and its defaults', and no step after either", () => {
    stopsLogged();
    const chart = new Chart();
    const beds = request(
      layer([], [charts(text("inner defaults finally"))]),
      layer([], [charts(text("inner finally"))]),
      layer([stops("The picker lacks its text.")], []),
    );
    const lane = new Lane("overlap", [
      request(
        layer([], [charts(text("outer defaults finally"))]),
        layer([], [charts(text("outer finally"))]),
        layer([beds, charts(text("after beds"))], []),
      ),
      charts(text("after admission")),
    ], aRoundsOwner);

    lane.raise(raisedWith({ chart }));

    expect(chart.lines).toStrictEqual(["inner finally", "inner defaults finally", "outer finally", "outer defaults finally"]);
  });

  test("DisposedRunNeverWaits: a dispose runs the defaults' Finally to its first awaited step, and no settling runs a step after", async () => {
    const chart = new Chart();
    const settle = gate();
    const flush = gate();
    const lane = new Lane("overlap", [
      request(
        layer([], [charts(text("defaults finally")), awaits("flush"), charts(text("flushed"))]),
        layer([awaits("settle"), charts(text("busy"))], [charts(text("finally"))]),
      ),
      charts(text("after the request")),
    ], aRoundsOwner);
    lane.raise(raisedWith({ chart, settle: settle.held, flush: flush.held }));

    lane.dispose();

    expect(chart.lines).toStrictEqual(["defaults finally"]);
    flush.opens(null);
    settle.opens(null);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual(["defaults finally"]);
  });

  test("ReplacedFinallyGoesOn: a replace that lands while a Finally waits leaves it to run to its end, then the defaults' Finally, and no step after the request", async () => {
    stopsLogged();
    const chart = new Chart();
    const first = gate();
    const second = gate();
    const lane = new Lane("replace", [
      request(
        layer([], [charts(named("defaultsFinally"))]),
        layer([], [charts(named("finallyStarts")), awaits("settle"), charts(named("finallyEnds"))]),
        layer([stops("The roster is held.")], []),
      ),
      charts(named("after")),
    ], aRoundsOwner);

    lane.raise(
      raisedWith({ chart, settle: first.held, defaultsFinally: "defaults finally 1", finallyStarts: "finally start 1", finallyEnds: "finally end 1", after: "after 1" }),
    );
    lane.raise(
      raisedWith({ chart, settle: second.held, defaultsFinally: "defaults finally 2", finallyStarts: "finally start 2", finallyEnds: "finally end 2", after: "after 2" }),
    );
    first.opens(null);
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual(["finally start 1", "finally start 2", "finally end 1", "defaults finally 1"]);
  });

  test("TheOldFinallyRunsBeforeTheNewRun: a run its own step replaces runs its Finally before the new run's first step", () => {
    stopsLogged();
    const chart = new Chart();
    const lane = new Lane("replace", [
      charts(named("rang")),
      request(layer([], [charts(named("finally"))]), layer([rings, stops("The round ends at its send.")], [])),
      charts(named("after")),
    ], aRoundsOwner);
    const quiet = new Bell(() => undefined);
    const bell = new Bell(() => {
      lane.raise(raisedWith({ chart, bell: quiet, rang: "rang 2", finally: "finally 2", after: "after 2" }));
    });

    lane.raise(raisedWith({ chart, bell, rang: "rang 1", finally: "finally 1", after: "after 1" }));

    expect(chart.lines).toStrictEqual(["rang 1", "finally 1", "rang 2", "finally 2"]);
  });
});

describe("when a step is one C# marked awaited (D11.2, D11.3, D4.3)", () => {
  test("a step C# did not mark awaited runs on in its turn, whatever its value holds", () => {
    const chart = new Chart();
    const lane = new Lane("overlap", [{ kind: "do", await: false, expr: named("settle") }, charts(text("next"))], aRoundsOwner);

    lane.raise(raisedWith({ chart, settle: gate().held }));

    expect(chart.lines).toStrictEqual(["next"]);
  });

  test("a let that awaits binds the value its promise resolves to", async () => {
    const chart = new Chart();
    const record = gate();
    const lane = new Lane("overlap", [{ kind: "let", name: "count", await: true, binds: "line", expr: named("record") }, charts(named("count"))], aRoundsOwner);
    lane.raise(raisedWith({ chart, record: record.held }));

    record.opens(125);
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual([125]);
  });

  test("a let that awaits a method's call binds the value the promise the method returns resolves to", async () => {
    const chart = new Chart();
    const record = gate();
    const lane = new Lane("overlap", [{ kind: "let", name: "count", await: true, binds: "line", expr: calls("record", "count") }, charts(named("count"))], aRoundsOwner);
    lane.raise(raisedWith({ chart, record: { count: () => record.held } }));

    record.opens(125);
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual([125]);
  });

  test("a do that awaits a method's call runs the step after it once the promise the method returns resolves", async () => {
    const chart = new Chart();
    const settle = gate();
    const lane = new Lane("overlap", [{ kind: "do", await: true, expr: calls("desk", "settle") }, charts(text("Settled"))], aRoundsOwner);
    lane.raise(raisedWith({ chart, desk: { settle: () => settle.held } }));

    expect(chart.lines).toStrictEqual([]);
    settle.opens(null);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual(["Settled"]);
  });

  test("PromiseWhereValueStops: a let that does not await a method's call that returns a promise stops the run, and the step after it never runs", async () => {
    const logged = stopsLogged();
    const chart = new Chart();
    const record = gate();
    const lane = new Lane("overlap", [{ kind: "let", name: "count", await: false, binds: "line", expr: calls("record", "count") }, charts(named("count"))], aRoundsOwner);
    lane.raise(raisedWith({ chart, record: { count: () => record.held } }));

    record.opens(125);
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual([]);
    expect(logged.mock.calls).toStrictEqual([["The method 'count' returned a promise where its declaration reads a value; a method that returns one is declared with MethodAsync."]]);
  });

  test("a value that is no promise is awaited as that value, so the step after it runs after the turn", async () => {
    const chart = new Chart();
    const lane = new Lane("overlap", [{ kind: "let", name: "count", await: true, binds: "line", expr: { kind: "lit", value: 90 } }, charts(named("count"))], aRoundsOwner);

    lane.raise(raisedWith({ chart }));

    expect(chart.lines).toStrictEqual([]);
    await everyPromiseJobRan();
    expect(chart.lines).toStrictEqual([90]);
  });

  test("a rejection stops the run with one logged error, and the step after it never runs", async () => {
    const logged = stopsLogged();
    const chart = new Chart();
    const settle = gate();
    const lane = new Lane("overlap", [charts(text("Testing")), awaits("settle"), charts(text("Self-test passed"))], aRoundsOwner);
    lane.raise(raisedWith({ chart, settle: settle.held }));

    settle.refuses(new Error("500"));
    await everyPromiseJobRan();

    expect(chart.lines).toStrictEqual(["Testing"]);
    expect(logged.mock.calls.length).toBe(1);
  });
});

describe("when a run is traced, each span it starts ends once with how it ended (Tracing.tla, D20.4, D20.6)", () => {
  test("EveryStartedSpanEnds: a run with no awaited step ends its span completed in its trigger's turn", () => {
    const spans = new Spans();
    const lane = new Lane("overlap", [charts(text("admitted"))], aRoundsOwner);

    lane.raise(tracedWith(spans, { chart: new Chart() }));

    expect(spans.recorded).toStrictEqual(["alis.run#1 starts {}", "alis.run#1 completed"]);
  });

  test("EveryStartedSpanEnds: a run its next raise replaces ends its span replaced, and the new run's span stays open while it waits", () => {
    const spans = new Spans();
    const lane = new Lane("replace", [{ kind: "do", await: true, expr: calls("pharmacy", "refresh") }], aRoundsOwner);
    const pharmacy = { refresh: () => gate().held };

    lane.raise(tracedWith(spans, { pharmacy }));
    lane.raise(tracedWith(spans, { pharmacy }));

    expect(spans.recorded).toStrictEqual([
      "alis.run#1 starts {}",
      "alis.method#2 starts {\"alis.method\":\"refresh\"}",
      "alis.run#1 replaced",
      "alis.run#3 starts {}",
      "alis.method#4 starts {\"alis.method\":\"refresh\"}",
    ]);
  });

  test("EveryStartedSpanEnds: a run disposed while it waits ends its span disposed", () => {
    const spans = new Spans();
    const lane = new Lane("overlap", [awaits("settle"), charts(text("after"))], aRoundsOwner);
    lane.raise(tracedWith(spans, { chart: new Chart(), settle: gate().held }));

    lane.dispose();

    expect(spans.recorded).toStrictEqual(["alis.run#1 starts {}", "alis.method#2 starts {}", "alis.run#1 disposed"]);
  });

  test("a stop statement records its reason, which C# wrote, on the run's span, and the run ends stopped", () => {
    stopsLogged();
    const spans = new Spans();
    const lane = new Lane("overlap", [stops("The bed is taken."), charts(text("after"))], aRoundsOwner);

    lane.raise(tracedWith(spans, { chart: new Chart() }));

    expect(spans.recorded).toStrictEqual([
      "alis.run#1 starts {}",
      "alis.run#1 alis.stop {\"alis.reason\":\"The bed is taken.\"}",
      "alis.run#1 stopped",
    ]);
  });

  test("a stop the page's values cause records no reason, since a value may be in it (D20.5), and the console keeps it", () => {
    const logged = stopsLogged();
    const spans = new Spans();
    const lane = new Lane("overlap", [{ kind: "do", await: false, expr: calls("resident", "name") }], aRoundsOwner);

    lane.raise(tracedWith(spans, { resident: { name: "Ruth" } }));

    expect([spans.recorded, logged.mock.calls]).toStrictEqual([
      ["alis.run#1 starts {}", "alis.run#1 alis.stop {}", "alis.run#1 stopped"],
      [["The member 'name' is no method."]],
    ]);
  });

  test("a rejected method ends its span stopped and records an error with no message on the run's span, which ends stopped", async () => {
    stopsLogged();
    const spans = new Spans();
    const settle = gate();
    const lane = new Lane("overlap", [{ kind: "do", await: true, expr: calls("pharmacy", "refresh") }], aRoundsOwner);
    lane.raise(tracedWith(spans, { pharmacy: { refresh: () => settle.held } }));

    settle.refuses(new Error("Ruth's dose is held."));
    await everyPromiseJobRan();

    expect(spans.recorded).toStrictEqual([
      "alis.run#1 starts {}",
      "alis.method#2 starts {\"alis.method\":\"refresh\"}",
      "alis.method#2 stopped",
      "alis.run#1 alis.error {}",
      "alis.run#1 stopped",
    ]);
  });

  test("a resolved method ends its span completed before the run goes on", async () => {
    const spans = new Spans();
    const settle = gate();
    const lane = new Lane("overlap", [{ kind: "let", name: "stock", await: true, binds: "line", expr: calls("pharmacy", "refresh") }], aRoundsOwner);
    lane.raise(tracedWith(spans, { pharmacy: { refresh: () => settle.held } }));

    settle.opens(12);
    await everyPromiseJobRan();

    expect(spans.recorded).toStrictEqual([
      "alis.run#1 starts {}",
      "alis.method#2 starts {\"alis.method\":\"refresh\"}",
      "alis.method#2 completed",
      "alis.run#1 completed",
    ]);
  });

  test("each block of an all is a span under the run, and each ends before the run", () => {
    const spans = new Spans();
    const lane = new Lane("overlap", [{ kind: "all", blocks: [[charts(text("north"))], [charts(text("south"))]] }], aRoundsOwner);

    lane.raise(tracedWith(spans, { chart: new Chart() }));

    expect(spans.recorded).toStrictEqual([
      "alis.run#1 starts {}",
      "alis.all#2 starts {}",
      "alis.all#2 completed",
      "alis.all#3 starts {}",
      "alis.all#3 completed",
      "alis.run#1 completed",
    ]);
  });
});

describe("when a step's value is a conversion (D3.2)", () => {
  test("an as value converts under the plan's culture and time zone", () => {
    const chart = new Chart();
    const kolkata: Raised["plan"] = { ...ward, timeZone: { kind: "zone", id: "Asia/Kolkata" } };
    const lane = new Lane("overlap", [
      charts({ kind: "as", expr: text("1,234.5"), shape: { kind: "number", of: "fraction", text: "view", overflow: "nothing" } }),
      charts({ kind: "as", expr: text("2026-11-09T08:30"), shape: { kind: "instant", from: "none" } }),
    ], aRoundsOwner);

    lane.raise({ ...raisedWith({ chart }), plan: kolkata });

    expect(chart.lines).toStrictEqual([1234.5, new Date("2026-11-09T03:00:00.000Z")]);
  });
});

describe("when a block reads its abort signal (D11.12)", () => {
  test("a replace aborts the signal of the old run's blocks, and the new run's blocks hold one of their own", () => {
    const chart = new Chart();
    const lane = new Lane("replace", [when("flagged", [chartsItsSignal], []), awaits("settle")], aRoundsOwner);

    lane.raise(raisedWith({ chart, flagged: true, settle: gate().held }));
    lane.raise(raisedWith({ chart, flagged: true, settle: gate().held }));

    expect(chart.signals.map((signal) => signal.aborted)).toStrictEqual([true, false]);
  });

  test("a Finally layer holds a new signal, which a replace that lands in it leaves and a dispose aborts", () => {
    stopsLogged();
    const chart = new Chart();
    const lane = new Lane("replace", [request(layer([], [chartsItsSignal, awaits("settle")]), layer([stops("The roster is held.")], []))], aRoundsOwner);
    lane.raise(raisedWith({ chart, settle: gate().held }));

    lane.raise(raisedWith({ chart, settle: gate().held }));

    expect(chart.signals.map((signal) => signal.aborted)).toStrictEqual([false, false]);
    lane.dispose();
    expect(chart.signals.map((signal) => signal.aborted)).toStrictEqual([true, true]);
  });
});

describe("when a release ends the runs of its owners that are in their own step (D12.4, D11.11)", () => {
  test("NothingRunsAfterAnAbort, DisposedRunNeverWaits: a run whose own step unloads its owner's slot runs no step after it, runs each loaded Finally layer once, innermost first and to its first awaited step, ends, and then the release runs", async () => {
    const chart = new Chart();
    const flush = gate();
    const waited: boolean[] = [];
    const beds = request(
      layer([], [charts(text("the inner defaults' Finally"))]),
      layer([], [charts(text("the inner Finally starts")), awaits("flush"), charts(text("the inner Finally flushed"))]),
      layer([unloads, charts(text("after the unload"))], []),
    );
    const lane = new Lane("overlap", [
      request(
        layer([], [charts(text("the outer defaults' Finally"))]),
        layer([], [charts(text("the outer Finally"))]),
        layer([beds, charts(text("after beds"))], []),
      ),
      charts(text("after admission")),
    ], theCard);

    lane.raise(endsOn(chart, "the card's run", { chart, flush: flush.held, slot: aSlotEnding(theCard, chart, waited) }));
    flush.opens(null);
    await everyPromiseJobRan();

    expect([chart.lines, waited]).toStrictEqual([
      ["the inner Finally starts", "the inner defaults' Finally", "the outer Finally", "the outer defaults' Finally", "the card's run ended disposed", "the slot released"],
      [true],
    ]);
  });

  test("FinallyRunsBeforeRelease: a release asked inside two runs of its owner, the inner one asking, waits until the outermost has run its Finally and ended", () => {
    const chart = new Chart();
    const waited: boolean[] = [];
    const inner = new Lane("overlap", [
      request(layer([], [charts(text("the inner Finally"))]), layer([unloads, charts(text("the inner run after its unload"))], [])),
    ], theCard);
    const bell = new Bell(() => {
      inner.raise(endsOn(chart, "the inner run", { chart, slot: aSlotEnding(theCard, chart, waited) }));
    });
    const outer = new Lane("overlap", [
      request(layer([], [charts(text("the outer Finally"))]), layer([rings, charts(text("the outer run after its ring"))], [])),
    ], theCard);

    outer.raise(endsOn(chart, "the outer run", { chart, bell }));

    expect([chart.lines, waited]).toStrictEqual([
      ["the inner Finally", "the inner run ended disposed", "the outer Finally", "the outer run ended disposed", "the slot released"],
      [true],
    ]);
  });

  test("a card's step raises an event the page's handler hears by unloading the card: the page's run is not cut and goes on, and the card's run ends at its step's return before the release", () => {
    const chart = new Chart();
    const waited: boolean[] = [];
    const pageHandler = new Lane("overlap", [unloads, charts(text("the page goes on"))], thePage);
    const bell = new Bell(() => {
      pageHandler.raise(raisedWith({ chart, slot: aSlotEnding(theCard, chart, waited) }));
    });
    const cardHandler = new Lane("overlap", [
      request(layer([], [charts(text("the card's Finally"))]), layer([rings, charts(text("the card after its ring"))], [])),
      charts(text("the card after its request")),
    ], theCard);

    cardHandler.raise(endsOn(chart, "the card's run", { chart, bell }));

    expect([chart.lines, waited]).toStrictEqual([["the page goes on", "the card's Finally", "the card's run ended disposed", "the slot released"], [true]]);
  });

  test("a release whose owners have no run in its own step is told so and calls nothing back, and a run of its owner that waits goes on", async () => {
    const chart = new Chart();
    const settle = gate();
    const waited: boolean[] = [];
    const cardHandler = new Lane("overlap", [awaits("settle"), charts(text("the card goes on"))], theCard);
    const pageHandler = new Lane("overlap", [unloads, charts(text("the page goes on"))], thePage);
    cardHandler.raise(raisedWith({ chart, settle: settle.held }));

    pageHandler.raise(raisedWith({ chart, slot: aSlotEnding(theCard, chart, waited) }));
    settle.opens(null);
    await everyPromiseJobRan();

    expect([chart.lines, waited]).toStrictEqual([["the page goes on", "the card goes on"], [false]]);
  });

  test("a run whose step threw past it is in its own step no more", () => {
    const chart = new Chart();
    const waited: boolean[] = [];
    const cardHandler = new Lane("overlap", [{ kind: "do", await: false, expr: calls("desk", "jam") }, charts(text("the card goes on"))], theCard);
    const pageHandler = new Lane("overlap", [unloads, charts(text("the page goes on"))], thePage);
    const desk = {
      jam: () => {
        throw new Error("The desk jammed.");
      },
    };

    expect(() => {
      cardHandler.raise(raisedWith({ chart, desk }));
    }).toThrow("The desk jammed.");
    pageHandler.raise(raisedWith({ chart, slot: aSlotEnding(theCard, chart, waited) }));

    expect([chart.lines, waited]).toStrictEqual([["the page goes on"], [false]]);
  });
});
