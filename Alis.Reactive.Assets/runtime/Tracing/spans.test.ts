import { describe, expect, test } from "vitest";
import { Port, type EndedSpan, type SinkLookup } from "./index";

type PlanTrace = Parameters<Port["planTrace"]>[0];

const roundTrace: PlanTrace = {
  kind: "on",
  traceparent: "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01",
  tracestate: "rojo=00f067aa0ba902b7",
  console: false,
  sources: [{ view: "/Views/Care/Round.cshtml", line: 12 }, { view: "/Views/Care/Round.cshtml", line: 19 }],
};

const printedRound: PlanTrace = { ...roundTrace, console: true };

const off: PlanTrace = { kind: "off" };

class Watch {
  now = 1000;

  readonly read = (): number => this.now;
}

class Mint {
  #spans = 0;
  #traces = 0;

  readonly traceId = (): string => {
    this.#traces += 1;
    return "4bf92f3577b34da6a3ce929d0e0e47" + String(this.#traces).padStart(2, "0");
  };

  readonly spanId = (): string => {
    this.#spans += 1;
    return "5a1e" + String(this.#spans).padStart(12, "0");
  };
}

class Collector {
  readonly ended: EndedSpan[] = [];
  lookups = 0;

  readonly lookup = (): SinkLookup => {
    this.lookups += 1;
    return { kind: "registered", sink: { ended: (span) => this.ended.push(span) } };
  };
}

class FailingSink {
  calls = 0;

  readonly lookup = (): SinkLookup => ({
    kind: "registered",
    sink: {
      ended: () => {
        this.calls += 1;
        throw new Error("The collector is down.");
      },
    },
  });
}

class Log {
  readonly errors: string[] = [];
  readonly infos: string[] = [];

  error(message: string): void {
    this.errors.push(message);
  }

  info(message: string): void {
    this.infos.push(message);
  }
}

function portOf(sinkOf: () => SinkLookup, log: Log = new Log(), clock: Watch = new Watch()): Port {
  return new Port(sinkOf, new Mint(), clock.read, log);
}

function noSink(): SinkLookup {
  return { kind: "none" };
}

const tree = (span: EndedSpan) => ({ name: span.name, spanId: span.spanId, parentId: span.parentId, outcome: span.outcome });

describe("when a plan's trace is off (D20.1)", () => {
  test("NothingRecordedWhenOff: a boot, a handler's run, its spans, its send and its events reach no sink and print nothing", () => {
    const sink = new Collector();
    const log = new Log();
    const traced = portOf(sink.lookup, log).planTrace(off);

    const boot = traced.span("alis.boot", { "alis.plan_id": "round" });
    const run = traced.handler(0, "click").run();
    const method = run.span("alis.method", { "alis.method": "refresh" });
    const send = run.send();
    send.attempt(true, "GET", "/Care/Round/{ward}").answered(200);
    run.event("alis.stop", { "alis.reason": "The bed is taken." });
    method.end("completed");
    send.end("completed");
    run.end("stopped");
    boot.end("completed");

    expect([sink.lookups, sink.ended, log.infos, log.errors]).toStrictEqual([0, [], [], []]);
  });
});

describe("when a plan's trace is on, one trace runs from the server's span (D20.3, D20.7)", () => {
  test("a handler's run is a child of the plan's traceparent and records its event and the view and line of its builder call", () => {
    const sink = new Collector();
    const run = portOf(sink.lookup).planTrace(roundTrace).handler(1, "click").run();

    run.end("completed");

    expect(sink.ended).toStrictEqual([
      {
        name: "alis.run",
        kind: "internal",
        traceId: "0af7651916cd43dd8448eb211c80319c",
        spanId: "5a1e000000000001",
        parentId: "b7ad6b7169203331",
        parentIsRemote: true,
        start: 1000,
        end: 1000,
        outcome: "completed",
        attributes: { "alis.event": "click", "code.file.path": "/Views/Care/Round.cshtml", "code.line.number": 19 },
        events: [],
      },
    ]);
  });

  test("a plan's boot is a child of the plan's traceparent and records the plan's id", () => {
    const sink = new Collector();
    const clock = new Watch();
    const boot = portOf(sink.lookup, new Log(), clock).planTrace(roundTrace).span("alis.boot", { "alis.plan_id": "round" });

    clock.now = 1004;
    boot.end("completed");

    expect(sink.ended.map((span) => [tree(span), span.traceId, span.start, span.end, span.attributes])).toStrictEqual([
      [{ name: "alis.boot", spanId: "5a1e000000000001", parentId: "b7ad6b7169203331", outcome: "completed" }, "0af7651916cd43dd8448eb211c80319c", 1000, 1004, { "alis.plan_id": "round" }],
    ]);
  });

  test("an event is recorded on the span that holds it, at its time, with its attributes", () => {
    const sink = new Collector();
    const clock = new Watch();
    const run = portOf(sink.lookup, new Log(), clock).planTrace(roundTrace).handler(0, "click").run();

    clock.now = 1007;
    run.event("alis.stop", { "alis.reason": "The bed is taken." });
    run.end("stopped");
    run.event("alis.error", {});

    expect(sink.ended.map((span) => [span.outcome, span.events])).toStrictEqual([
      ["stopped", [{ name: "alis.stop", time: 1007, attributes: { "alis.reason": "The bed is taken." } }]],
    ]);
  });
});

describe("when a run ends, every span it started ends once, after its children (D20.6)", () => {
  test("EveryStartedSpanEnds: a run that completes ends its send and its attempt, each once", () => {
    const sink = new Collector();
    const run = portOf(sink.lookup).planTrace(roundTrace).handler(0, "click").run();
    const send = run.send();
    send.attempt(true, "GET", "/Care/Round/{ward}").answered(200);

    send.end("completed");
    run.end("completed");
    run.end("completed");
    send.end("completed");

    expect(sink.ended.map(tree)).toStrictEqual([
      { name: "GET /Care/Round/{ward}", spanId: "5a1e000000000003", parentId: "5a1e000000000002", outcome: "completed" },
      { name: "alis.send", spanId: "5a1e000000000002", parentId: "5a1e000000000001", outcome: "completed" },
      { name: "alis.run", spanId: "5a1e000000000001", parentId: "b7ad6b7169203331", outcome: "completed" },
    ]);
  });

  test("EveryStartedSpanEnds: a run its next raise replaces ends its send and its attempt in flight as replaced", () => {
    const sink = new Collector();
    const run = portOf(sink.lookup).planTrace(roundTrace).handler(0, "click").run();
    run.send().attempt(true, "GET", "/Care/Round/{ward}");

    run.end("replaced");

    expect(sink.ended.map(tree)).toStrictEqual([
      { name: "GET /Care/Round/{ward}", spanId: "5a1e000000000003", parentId: "5a1e000000000002", outcome: "replaced" },
      { name: "alis.send", spanId: "5a1e000000000002", parentId: "5a1e000000000001", outcome: "replaced" },
      { name: "alis.run", spanId: "5a1e000000000001", parentId: "b7ad6b7169203331", outcome: "replaced" },
    ]);
  });

  test("ChildEndsBeforeParent: a disposed run ends a method its block of an all awaits, then the block, then itself", () => {
    const sink = new Collector();
    const run = portOf(sink.lookup).planTrace(roundTrace).handler(0, "click").run();
    run.span("alis.all", {}).span("alis.method", { "alis.method": "refresh" });

    run.end("disposed");

    expect(sink.ended.map(tree)).toStrictEqual([
      { name: "alis.method", spanId: "5a1e000000000003", parentId: "5a1e000000000002", outcome: "disposed" },
      { name: "alis.all", spanId: "5a1e000000000002", parentId: "5a1e000000000001", outcome: "disposed" },
      { name: "alis.run", spanId: "5a1e000000000001", parentId: "b7ad6b7169203331", outcome: "disposed" },
    ]);
  });

  test("ChildEndsBeforeParent: a method settling after its run ended records no span, and a span ended first keeps its own outcome", () => {
    const sink = new Collector();
    const run = portOf(sink.lookup).planTrace(roundTrace).handler(0, "click").run();
    run.span("alis.method", { "alis.method": "refresh" }).end("stopped");
    run.end("replaced");

    const late = run.span("alis.method", { "alis.method": "refresh" });
    late.end("completed");
    run.send().attempt(true, "GET", "/Care/Round/{ward}").answered(200);

    expect(sink.ended.map(tree)).toStrictEqual([
      { name: "alis.method", spanId: "5a1e000000000002", parentId: "5a1e000000000001", outcome: "stopped" },
      { name: "alis.run", spanId: "5a1e000000000001", parentId: "b7ad6b7169203331", outcome: "replaced" },
    ]);
  });
});

describe("when the sink throws (D20.6, M26.D1)", () => {
  test("SinkFailureChangesNothing: the sink's error is logged once, the spans after it reach it no more, and the run goes on", () => {
    const sink = new FailingSink();
    const log = new Log();
    const traced = portOf(sink.lookup, log).planTrace(roundTrace);
    const first = traced.handler(0, "click").run();
    const second = traced.handler(1, "click").run();

    first.end("completed");
    second.event("alis.stop", { "alis.reason": "The bed is taken." });
    second.end("stopped");

    expect([sink.calls, log.errors]).toStrictEqual([
      1,
      ["The trace sink threw at the span alis.run 5a1e000000000001, so this page calls it no more: The collector is down."],
    ]);
  });

  test("a page whose script registers no sink ends its spans with nothing logged", () => {
    const log = new Log();
    const run = portOf(noSink, log).planTrace(roundTrace).handler(0, "click").run();

    run.end("completed");

    expect([log.errors, log.infos]).toStrictEqual([[], []]);
  });
});

describe("when the plan's console is set (D20.2)", () => {
  test("each ended span is printed with its outcome, its time and its ids, and no attribute", () => {
    const log = new Log();
    const clock = new Watch();
    const run = portOf(noSink, log, clock).planTrace(printedRound).handler(0, "click").run();
    const attempt = run.send().attempt(true, "GET", "/Care/Round/{ward}");

    clock.now = 1042;
    attempt.answered(200);
    run.end("completed");

    expect(log.infos).toStrictEqual([
      "alis span GET /Care/Round/{ward} completed in 42 ms, trace 0af7651916cd43dd8448eb211c80319c, span 5a1e000000000003, parent 5a1e000000000002",
      "alis span alis.send completed in 42 ms, trace 0af7651916cd43dd8448eb211c80319c, span 5a1e000000000002, parent 5a1e000000000001",
      "alis span alis.run completed in 42 ms, trace 0af7651916cd43dd8448eb211c80319c, span 5a1e000000000001, parent b7ad6b7169203331",
    ]);
  });

  test("a plan whose console is not set prints no span", () => {
    const log = new Log();
    const run = portOf(new Collector().lookup, log).planTrace(roundTrace).handler(0, "click").run();

    run.end("completed");

    expect(log.infos).toStrictEqual([]);
  });

  test("SinkFailureChangesNothing: a set console prints every span after the sink failed", () => {
    const log = new Log();
    const traced = portOf(new FailingSink().lookup, log).planTrace(printedRound);

    traced.handler(0, "click").run().end("completed");
    traced.handler(1, "click").run().end("stopped");

    expect(log.infos).toStrictEqual([
      "alis span alis.run completed in 0 ms, trace 0af7651916cd43dd8448eb211c80319c, span 5a1e000000000001, parent b7ad6b7169203331",
      "alis span alis.run stopped in 0 ms, trace 0af7651916cd43dd8448eb211c80319c, span 5a1e000000000002, parent b7ad6b7169203331",
    ]);
  });
});
