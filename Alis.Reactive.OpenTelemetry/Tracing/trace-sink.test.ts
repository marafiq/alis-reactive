import { SpanKind, SpanStatusCode } from "@opentelemetry/api";
import { InMemorySpanExporter, type SpanExporter } from "@opentelemetry/sdk-trace";
import { describe, expect, test } from "vitest";
import { TraceSink, type EndedSpan } from "./trace-sink";

const attempt: EndedSpan = {
  name: "GET /Care/Round/{ward}",
  kind: "client",
  traceId: "0af7651916cd43dd8448eb211c80319c",
  spanId: "5a1e000000000003",
  parentId: "5a1e000000000002",
  parentIsRemote: false,
  start: 1_763_200_000_123.25,
  end: 1_763_200_000_165.5,
  outcome: "completed",
  attributes: { "http.request.method": "GET", "url.template": "/Care/Round/{ward}", "http.request.resend_count": 1, "http.response.status_code": 503 },
  events: [],
};

const run: EndedSpan = {
  name: "alis.run",
  kind: "internal",
  traceId: "0af7651916cd43dd8448eb211c80319c",
  spanId: "5a1e000000000001",
  parentId: "b7ad6b7169203331",
  parentIsRemote: true,
  start: 1_763_200_000_100,
  end: 1_763_200_000_200,
  outcome: "stopped",
  attributes: { "alis.event": "click", "code.file.path": "/Views/Care/Round.cshtml", "code.line.number": 12 },
  events: [{ name: "alis.stop", time: 1_763_200_000_150, attributes: { "alis.reason": "The bed is taken." } }],
};

function exported(...spans: EndedSpan[]) {
  const memory = new InMemorySpanExporter();
  const sink = new TraceSink(memory, "care-home");
  for (const span of spans) sink.ended(span);
  return memory.getFinishedSpans();
}

describe("when the page ends a span, the sink hands OpenTelemetry the span the page recorded (N20.2)", () => {
  test("an attempt keeps its ids, its parent, its times and its attributes, as a sampled client span", () => {
    const [span] = exported(attempt);

    expect([
      span?.name,
      span?.kind,
      span?.spanContext().traceId,
      span?.spanContext().spanId,
      span?.spanContext().traceFlags,
      span?.parentSpanContext,
      span?.startTime,
      span?.endTime,
      span?.duration,
      span?.attributes,
    ]).toStrictEqual([
      "GET /Care/Round/{ward}",
      SpanKind.CLIENT,
      "0af7651916cd43dd8448eb211c80319c",
      "5a1e000000000003",
      1,
      { traceId: "0af7651916cd43dd8448eb211c80319c", spanId: "5a1e000000000002", traceFlags: 1, isRemote: false },
      [1_763_200_000, 123_250_000],
      [1_763_200_000, 165_500_000],
      [0, 42_250_000],
      { "http.request.method": "GET", "url.template": "/Care/Round/{ward}", "http.request.resend_count": 1, "http.response.status_code": 503, "alis.outcome": "completed" },
    ]);
  });

  test("an attempt the server answered with a failure status is an error, and one it answered with success is not", () => {
    const spans = exported(attempt, { ...attempt, spanId: "5a1e000000000004", attributes: { ...attempt.attributes, "http.response.status_code": 200 } });

    expect(spans.map((span) => span.status)).toStrictEqual([{ code: SpanStatusCode.ERROR }, { code: SpanStatusCode.UNSET }]);
  });

  test("a run is an internal span under the server's remote span, carrying its outcome and its events at their times", () => {
    const [span] = exported(run);

    expect([span?.kind, span?.parentSpanContext?.isRemote, span?.status, span?.attributes["alis.outcome"], span?.events]).toStrictEqual([
      SpanKind.INTERNAL,
      true,
      { code: SpanStatusCode.UNSET },
      "stopped",
      [{ name: "alis.stop", time: [1_763_200_000, 150_000_000], attributes: { "alis.reason": "The bed is taken." } }],
    ]);
  });

  test("a span that held a boundary error is an error", () => {
    const [span] = exported({ ...run, events: [{ name: "alis.error", time: 1_763_200_000_150, attributes: {} }] });

    expect(span?.status).toStrictEqual({ code: SpanStatusCode.ERROR });
  });

  test("an error its exporter throws leaves the sink's call, so the page's port logs it and calls the sink no more (M26.D1)", () => {
    const refusing: SpanExporter = {
      export: () => {
        throw new Error("The collector refused the span.");
      },
      shutdown: () => Promise.resolve(),
    };
    const sink = new TraceSink(refusing, "care-home");

    expect(() => {
      sink.ended(run);
    }).toThrow("The collector refused the span.");
  });

  test("every span names the app's service and the framework's scope", () => {
    const [span] = exported(run);

    expect([span?.resource.attributes["service.name"], span?.instrumentationScope]).toStrictEqual(["care-home", { name: "Alis.Reactive" }]);
  });
});
