import { describe, it, expect, vi, beforeEach } from "vitest";
import { createTracer, configure, resetForTests } from "../../tracing/trace";
import type { TraceEvent, TraceSink } from "../../tracing/types";

function captureSink(): TraceSink & { events: TraceEvent[] } {
  const events: TraceEvent[] = [];
  return {
    events,
    emit: (e: TraceEvent) => events.push(e),
    span: vi.fn(),
    flush: vi.fn(),
  };
}

describe("ScopedTracer", () => {
  beforeEach(() => resetForTests());

  it("emits event with correct scope and level", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("http");
    t.debug("http.request.send", { method: "POST" });
    expect(sink.events).toHaveLength(1);
    expect(sink.events[0].scope).toBe("http");
    expect(sink.events[0].event).toBe("http.request.send");
    expect(sink.events[0].level).toBe("debug");
    expect(sink.events[0].severityNumber).toBe(5);
    expect(sink.events[0].data).toEqual({ method: "POST" });
  });

  it("gates events below active level", () => {
    const sink = captureSink();
    configure({ level: "warn", sink });
    const t = createTracer("http");
    t.debug("http.request.send", {});
    t.info("boot.start", {});
    t.warn("gather.serialize.fail", {});
    expect(sink.events).toHaveLength(1);
    expect(sink.events[0].level).toBe("warn");
  });

  it("auto-attaches breadcrumbs on error events", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("test");
    t.debug("step.one", {});
    t.debug("step.two", {});
    t.error("step.fail", { reason: "bad" });
    const errorEvent = sink.events.find(e => e.level === "error");
    expect(errorEvent?.breadcrumbs).toBeDefined();
    expect(errorEvent!.breadcrumbs!.length).toBeGreaterThanOrEqual(2);
  });

  it("captures warn breadcrumbs even when below active level", () => {
    const sink = captureSink();
    configure({ level: "error", sink }); // only error emits to sink
    const t = createTracer("test");
    t.warn("step.warn", {}); // below error, but warn always captured as breadcrumb
    t.error("step.fail", {}); // emits to sink with breadcrumbs attached
    const errorEvent = sink.events.find(e => e.level === "error");
    expect(errorEvent?.breadcrumbs).toBeDefined();
    expect(errorEvent!.breadcrumbs!.some(b => b.event === "step.warn")).toBe(true);
  });

  it("does not capture info/debug breadcrumbs when level is error", () => {
    const sink = captureSink();
    configure({ level: "error", sink });
    const t = createTracer("test");
    t.info("step.info", {}); // info > warn threshold, not captured
    t.debug("step.debug", {}); // debug > warn threshold, not captured
    t.error("step.fail", {});
    const errorEvent = sink.events.find(e => e.level === "error");
    expect(errorEvent!.breadcrumbs!.some(b => b.event === "step.info")).toBe(false);
    expect(errorEvent!.breadcrumbs!.some(b => b.event === "step.debug")).toBe(false);
  });

  it("enabled() returns correct boolean", () => {
    configure({ level: "warn" });
    const t = createTracer("test");
    expect(t.enabled("error")).toBe(true);
    expect(t.enabled("warn")).toBe(true);
    expect(t.enabled("info")).toBe(false);
    expect(t.enabled("debug")).toBe(false);
  });

  it("withSpan binds traceId and spanId", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("test");
    const mockSpan = { traceId: "a".repeat(32), spanId: "b".repeat(16) };
    const scoped = t.withSpan(mockSpan as any);
    scoped.debug("test.event", {});
    expect(sink.events[0].traceId).toBe("a".repeat(32));
    expect(sink.events[0].spanId).toBe("b".repeat(16));
  });

  it("serializes Error on error()", () => {
    const sink = captureSink();
    configure({ level: "error", sink });
    const t = createTracer("test");
    t.error("test.fail", {}, new TypeError("bad"));
    expect(sink.events[0].error?.type).toBe("TypeError");
    expect(sink.events[0].error?.message).toBe("bad");
  });

  it("serializes Error on warn()", () => {
    const sink = captureSink();
    configure({ level: "warn", sink });
    const t = createTracer("test");
    t.warn("test.warn", {}, new Error("oops"));
    expect(sink.events[0].error?.type).toBe("Error");
  });

  it("withSpan(undefined) does not emit zero trace IDs", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("test");
    const scoped = t.withSpan(undefined);
    scoped.debug("test.event", {});
    expect(sink.events[0].traceId).toBeUndefined();
    expect(sink.events[0].spanId).toBeUndefined();
  });

  it("span without traceparent has no parentSpanId", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("test");
    const span = t.span("root.span");
    span.end();
    const spanCall = (sink.span as ReturnType<typeof vi.fn>).mock.calls[0][0];
    expect(spanCall.parentSpanId).toBeUndefined();
  });

  it("does not attach breadcrumbs to non-error events", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("test");
    t.debug("step.one", {});
    t.warn("step.warn", {});
    const warnEvent = sink.events.find(e => e.level === "warn");
    expect(warnEvent?.breadcrumbs).toBeUndefined();
  });
});
