import { afterEach, beforeEach, describe, expect, it } from "vitest";
import * as tracing from "../../tracing";
import { resetForTests as resetInteractions, run } from "../../tracing/interactions";
import { resetForTests as resetTrace } from "../../tracing/trace";
import type { TraceEvent, TraceSink } from "../../tracing/types";

class RecordingSink implements TraceSink {
  readonly events: TraceEvent[] = [];
  emit(event: TraceEvent): void {
    this.events.push(event);
  }
  flush(): void {}
}

beforeEach(() => {
  resetInteractions();
  resetTrace();
});

afterEach(() => {
  resetInteractions();
  resetTrace();
});

describe("public barrel", () => {
  it("exports exactly 3 runtime names", () => {
    const runtimeKeys = Object.keys(tracing)
      .filter((k) => typeof (tracing as Record<string, unknown>)[k] !== "undefined");
    expect(runtimeKeys.sort()).toEqual(["ConsoleSink", "configure", "tracer"]);
  });

  it("does not expose Span, NOOP_SPAN, getRootSpan, flush, withSpan", () => {
    const bag = tracing as Record<string, unknown>;
    expect(bag.Span).toBeUndefined();
    expect(bag.NOOP_SPAN).toBeUndefined();
    expect(bag.getRootSpan).toBeUndefined();
    expect(bag.flush).toBeUndefined();
    expect(bag.withSpan).toBeUndefined();
    expect(bag.SpanData).toBeUndefined();
    expect(bag.TraceRoot).toBeUndefined();
  });
});

describe("configure → tracer → run end-to-end", () => {
  it("propagates trace-id through the sink during an interaction", () => {
    const sink = new RecordingSink();
    tracing.configure({ level: "trace", sink });
    const t = tracing.tracer("http");
    run("document-event", { event: "click" }, () => {
      t.debug("http.request.send", { url: "/api" });
    });
    // interaction.start, http.request.send, interaction.end all carry same trace-id
    const traceIds = sink.events.map((e) => e.traceId);
    expect(traceIds[0]).toBeDefined();
    expect(traceIds[0]).toMatch(/^[0-9a-f]{32}$/);
    expect(new Set(traceIds).size).toBe(1);
  });

  it("server-side traceparent becomes the default root for new interactions", () => {
    const sink = new RecordingSink();
    tracing.configure({
      level: "trace",
      sink,
      traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    });
    run("test", {}, () => {
      const evt = sink.events[sink.events.length - 1];
      expect(evt.traceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
    });
  });

  it("configure replaces state idempotently", () => {
    const sinkA = new RecordingSink();
    const sinkB = new RecordingSink();
    tracing.configure({ level: "info", sink: sinkA });
    tracing.tracer("s").info("a");
    tracing.configure({ level: "info", sink: sinkB });
    tracing.tracer("s").info("b");
    expect(sinkA.events.map((e) => e.event)).toEqual(["a"]);
    expect(sinkB.events.map((e) => e.event)).toEqual(["b"]);
  });
});
