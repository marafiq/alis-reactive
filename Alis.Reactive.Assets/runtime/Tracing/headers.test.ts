import { describe, expect, test } from "vitest";
import { isThePagesOrigin, Port, randomIds, type EndedSpan, type SinkLookup } from "./index";

type PlanTrace = Parameters<Port["planTrace"]>[0];

const roundTrace: PlanTrace = {
  kind: "on",
  traceparent: "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01",
  tracestate: "rojo=00f067aa0ba902b7",
  console: false,
  sources: [{ view: "/Views/Care/Round.cshtml", line: 12 }],
};

const off: PlanTrace = { kind: "off" };

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

  readonly lookup = (): SinkLookup => ({ kind: "registered", sink: { ended: (span) => this.ended.push(span) } });
}

function failing(): SinkLookup {
  return {
    kind: "registered",
    sink: {
      ended: () => {
        throw new Error("The collector is down.");
      },
    },
  };
}

const quiet = { error: () => undefined, info: () => undefined };

function portOf(sinkOf: () => SinkLookup): Port {
  return new Port(sinkOf, new Mint(), () => 1000, quiet);
}

describe("when a traced send attempts, each attempt joins the trace (D20.3)", () => {
  test("AttemptsJoinTheTrace: each attempt to the page's origin sends a traceparent that names its own span, and the plan's tracestate", () => {
    const sink = new Collector();
    const send = portOf(sink.lookup).planTrace(roundTrace).handler(0, "click").run().send();

    const first = send.attempt(true, "GET", "/Care/Round/{ward}");
    first.answered(503);
    const second = send.attempt(true, "GET", "/Care/Round/{ward}");
    second.answered(200);

    expect([first.headers, second.headers, sink.ended.map((span) => span.spanId)]).toStrictEqual([
      { traceparent: "00-0af7651916cd43dd8448eb211c80319c-5a1e000000000003-01", tracestate: "rojo=00f067aa0ba902b7" },
      {
        traceparent: "00-0af7651916cd43dd8448eb211c80319c-5a1e000000000004-01",
        tracestate: "rojo=00f067aa0ba902b7",
        baggage: "http.request.resend_count=1",
      },
      ["5a1e000000000003", "5a1e000000000004"],
    ]);
  });

  test("an attempt's span is named by its method and route template under the send, and records the status and the resend count", () => {
    const sink = new Collector();
    const send = portOf(sink.lookup).planTrace(roundTrace).handler(0, "click").run().send();

    send.attempt(true, "PUT", "/Care/Round/{ward}/Doses").answered(503);
    send.attempt(true, "PUT", "/Care/Round/{ward}/Doses").answered(204);

    expect(sink.ended.map((span) => [span.name, span.kind, span.parentId, span.parentIsRemote, span.outcome, span.attributes])).toStrictEqual([
      ["PUT /Care/Round/{ward}/Doses", "client", "5a1e000000000002", false, "completed", { "http.request.method": "PUT", "url.template": "/Care/Round/{ward}/Doses", "http.response.status_code": 503 }],
      [
        "PUT /Care/Round/{ward}/Doses",
        "client",
        "5a1e000000000002",
        false,
        "completed",
        { "http.request.method": "PUT", "url.template": "/Care/Round/{ward}/Doses", "http.request.resend_count": 1, "http.response.status_code": 204 },
      ],
    ]);
  });

  test("a plan whose server span holds no tracestate sends traceparent alone", () => {
    const send = portOf(new Collector().lookup).planTrace({ ...roundTrace, tracestate: "" }).handler(0, "click").run().send();

    expect(send.attempt(true, "GET", "/Care/Round/{ward}").headers).toStrictEqual({ traceparent: "00-0af7651916cd43dd8448eb211c80319c-5a1e000000000003-01" });
  });

  test("SinkFailureKeepsHeaders: a page whose sink failed still sends each attempt the traceparent of its own span", () => {
    const traced = portOf(failing).planTrace(roundTrace);
    traced.handler(0, "click").run().end("completed");

    const attempt = traced.handler(0, "click").run().send().attempt(true, "GET", "/Care/Round/{ward}");

    expect(attempt.headers).toStrictEqual({ traceparent: "00-0af7651916cd43dd8448eb211c80319c-5a1e000000000004-01", tracestate: "rojo=00f067aa0ba902b7" });
  });
});

describe("when a send's attempts cross to another origin, the trace headers stay home (D20.3, X20.4)", () => {
  test("TraceHeadersStayHome: a traced attempt to another origin sends no trace header and still records its span", () => {
    const sink = new Collector();
    const send = portOf(sink.lookup).planTrace(roundTrace).handler(0, "click").run().send();

    const first = send.attempt(false, "GET", "https://pharmacy.example/stock/{drug}");
    first.answered(503);
    const second = send.attempt(false, "GET", "https://pharmacy.example/stock/{drug}");

    expect([first.headers, second.headers, sink.ended.map((span) => span.name)]).toStrictEqual([{}, {}, ["GET https://pharmacy.example/stock/{drug}"]]);
  });

  test("TraceHeadersStayHome: an untraced retry to another origin sends no trace header", () => {
    const send = portOf(new Collector().lookup).planTrace(off).handler(0, "click").run().send();

    send.attempt(false, "GET", "https://pharmacy.example/stock/{drug}");

    expect(send.attempt(false, "GET", "https://pharmacy.example/stock/{drug}").headers).toStrictEqual({});
  });

  test("a relative URL and an absolute one on the page's scheme, host and port are the page's origin", () => {
    expect([
      isThePagesOrigin("/Care/Round/7", "https://care.example/Care/Round"),
      isThePagesOrigin("Doses?ward=7", "https://care.example/Care/Round"),
      isThePagesOrigin("https://care.example:443/Care/Round/7", "https://care.example/Care/Round"),
    ]).toStrictEqual([true, true, true]);
  });

  test("another host, another scheme and another port are not the page's origin", () => {
    expect([
      isThePagesOrigin("https://pharmacy.example/stock", "https://care.example/Care/Round"),
      isThePagesOrigin("http://care.example/Care/Round/7", "https://care.example/Care/Round"),
      isThePagesOrigin("https://care.example:8443/Care/Round/7", "https://care.example/Care/Round"),
      isThePagesOrigin("//pharmacy.example/stock", "https://care.example/Care/Round"),
    ]).toStrictEqual([false, false, false, false]);
  });
});

describe("when a send retries, the retry is always traced (D20.11)", () => {
  test("RetriesAlwaysTraced: with the trace off, the first attempt carries nothing and each retry home carries one minted trace id with its own span", () => {
    const send = portOf(new Collector().lookup).planTrace(off).handler(0, "click").run().send();

    const attempts = [send.attempt(true, "GET", "/Care/Round/{ward}"), send.attempt(true, "GET", "/Care/Round/{ward}"), send.attempt(true, "GET", "/Care/Round/{ward}")];

    expect(attempts.map((attempt) => attempt.headers)).toStrictEqual([
      {},
      { traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4701-5a1e000000000001-01", baggage: "http.request.resend_count=1" },
      { traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4701-5a1e000000000002-01", baggage: "http.request.resend_count=2" },
    ]);
  });

  test("RetriesAlwaysTraced: two untraced sends mint a trace id each", () => {
    const run = portOf(new Collector().lookup).planTrace(off).handler(0, "click").run();
    const first = run.send();
    const second = run.send();
    first.attempt(true, "GET", "/Care/Round/{ward}");
    second.attempt(true, "GET", "/Care/Round/{ward}");

    expect([first.attempt(true, "GET", "/Care/Round/{ward}").headers, second.attempt(true, "GET", "/Care/Round/{ward}").headers]).toStrictEqual([
      { traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4701-5a1e000000000001-01", baggage: "http.request.resend_count=1" },
      { traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4702-5a1e000000000002-01", baggage: "http.request.resend_count=1" },
    ]);
  });

  test("ResendCountSent: a traced third attempt sends a resend count of two", () => {
    const send = portOf(new Collector().lookup).planTrace(roundTrace).handler(0, "click").run().send();
    send.attempt(true, "GET", "/Care/Round/{ward}").answered(503);
    send.attempt(true, "GET", "/Care/Round/{ward}").answered(503);

    expect(send.attempt(true, "GET", "/Care/Round/{ward}").headers).toStrictEqual({
      traceparent: "00-0af7651916cd43dd8448eb211c80319c-5a1e000000000005-01",
      tracestate: "rojo=00f067aa0ba902b7",
      baggage: "http.request.resend_count=2",
    });
  });
});

describe("when the page mints its ids (W3C Trace Context)", () => {
  test("a trace id is 32 and a span id 16 lowercase hex digits, and two minted ids differ", () => {
    const traces = [randomIds.traceId(), randomIds.traceId()];
    const spans = [randomIds.spanId(), randomIds.spanId()];

    expect([traces.map((id) => /^[0-9a-f]{32}$/.test(id)), spans.map((id) => /^[0-9a-f]{16}$/.test(id)), traces[0] === traces[1], spans[0] === spans[1]]).toStrictEqual([
      [true, true],
      [true, true],
      false,
      false,
    ]);
  });
});
