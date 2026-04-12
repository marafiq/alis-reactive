import { describe, it, expect, vi } from "vitest";
import { NOOP_SPAN, ActiveSpan, ContextOnlySpan, generateTraceId, generateSpanId } from "../../tracing/span";
import type { TraceSink, SpanData } from "../../tracing/types";

function mockSink(): TraceSink & { spans: SpanData[] } {
  const spans: SpanData[] = [];
  return {
    spans,
    emit: vi.fn(),
    span: (data: SpanData) => spans.push(data),
    flush: vi.fn(),
  };
}

describe("NOOP_SPAN", () => {
  it("child returns itself", () => {
    expect(NOOP_SPAN.child("x")).toBe(NOOP_SPAN);
  });

  it("set/event/end are no-ops", () => {
    NOOP_SPAN.set("k", "v");
    NOOP_SPAN.event("e");
    NOOP_SPAN.end();
  });

  it("traceparent returns zero string", () => {
    expect(NOOP_SPAN.traceparent()).toMatch(/^00-0{32}-0{16}-00$/);
  });

  it("has zero traceId and spanId", () => {
    expect(NOOP_SPAN.traceId).toBe("0".repeat(32));
    expect(NOOP_SPAN.spanId).toBe("0".repeat(16));
  });
});

describe("ActiveSpan", () => {
  it("generates unique spanId", () => {
    const sink = mockSink();
    const a = new ActiveSpan("a", "test", undefined, sink);
    const b = new ActiveSpan("b", "test", undefined, sink);
    expect(a.spanId).not.toBe(b.spanId);
  });

  it("inherits traceId from parent", () => {
    const sink = mockSink();
    const parent = new ActiveSpan("parent", "test", undefined, sink);
    const child = parent.child("child") as ActiveSpan;
    expect(child.traceId).toBe(parent.traceId);
    expect(child.parentSpanId).toBe(parent.spanId);
  });

  it("emits SpanData to sink on end()", () => {
    const sink = mockSink();
    const span = new ActiveSpan("test-span", "test", undefined, sink);
    span.set("key", "value");
    span.event("mid-point", { x: 1 });
    span.end("ok");
    expect(sink.spans).toHaveLength(1);
    expect(sink.spans[0].name).toBe("test-span");
    expect(sink.spans[0].status).toBe("ok");
    expect(sink.spans[0].attributes).toEqual({ key: "value" });
    expect(sink.spans[0].events).toHaveLength(1);
    expect(sink.spans[0].events[0].name).toBe("mid-point");
  });

  it("defaults status to unset", () => {
    const sink = mockSink();
    const span = new ActiveSpan("s", "test", undefined, sink);
    span.end();
    expect(sink.spans[0].status).toBe("unset");
  });

  it("traceparent format is W3C compliant", () => {
    const sink = mockSink();
    const span = new ActiveSpan("s", "test", undefined, sink);
    const tp = span.traceparent();
    expect(tp).toMatch(/^00-[0-9a-f]{32}-[0-9a-f]{16}-01$/);
  });

  it("accepts initial attributes via constructor", () => {
    const sink = mockSink();
    const span = new ActiveSpan("s", "test", undefined, sink, { "http.method": "POST", "http.url": "/api" });
    span.end();
    expect(sink.spans[0].attributes).toEqual({ "http.method": "POST", "http.url": "/api" });
  });

  it("filters non-primitive attributes", () => {
    const sink = mockSink();
    const span = new ActiveSpan("s", "test", undefined, sink, { good: "yes", bad: { nested: true } });
    span.end();
    expect(sink.spans[0].attributes).toEqual({ good: "yes" });
  });
});

describe("ContextOnlySpan", () => {
  it("propagates traceId from root", () => {
    const root = { traceId: "a".repeat(32), flags: "01" };
    const span = new ContextOnlySpan(root);
    expect(span.traceId).toBe("a".repeat(32));
  });

  it("child propagates same traceId", () => {
    const root = { traceId: "b".repeat(32), flags: "ff" };
    const span = new ContextOnlySpan(root);
    const child = span.child("x");
    expect(child.traceId).toBe("b".repeat(32));
    expect(child.parentSpanId).toBe(span.spanId);
  });

  it("preserves flags in traceparent", () => {
    const root = { traceId: "c".repeat(32), flags: "ff" };
    const span = new ContextOnlySpan(root);
    expect(span.traceparent()).toContain("-ff");
  });

  it("end is a no-op (does not emit)", () => {
    const root = { traceId: "d".repeat(32), flags: "01" };
    const span = new ContextOnlySpan(root);
    span.end();
  });
});

describe("ID generation", () => {
  it("generateTraceId produces 32 hex chars", () => {
    const id = generateTraceId();
    expect(id).toMatch(/^[0-9a-f]{32}$/);
  });

  it("generateSpanId produces 16 hex chars", () => {
    const id = generateSpanId();
    expect(id).toMatch(/^[0-9a-f]{16}$/);
  });

  it("generates unique IDs", () => {
    const ids = new Set(Array.from({ length: 100 }, () => generateSpanId()));
    expect(ids.size).toBe(100);
  });
});
