import { describe, it, expect, vi, beforeEach } from "vitest";
import { ConsoleSink, serializeError } from "../../tracing/sink";
import type { TraceEvent, SpanData } from "../../tracing/types";

describe("ConsoleSink", () => {
  const sink = new ConsoleSink();

  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("routes error events to console.error", () => {
    const spy = vi.spyOn(console, "error").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.fail", scope: "test", level: "error", severityNumber: 17,
    };
    sink.emit(event);
    expect(spy).toHaveBeenCalled();
  });

  it("routes warn events to console.warn", () => {
    const spy = vi.spyOn(console, "warn").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.warn", scope: "test", level: "warn", severityNumber: 13,
    };
    sink.emit(event);
    expect(spy).toHaveBeenCalled();
  });

  it("routes info events to console.info", () => {
    const spy = vi.spyOn(console, "info").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.info", scope: "test", level: "info", severityNumber: 9,
    };
    sink.emit(event);
    expect(spy).toHaveBeenCalled();
  });

  it("routes debug events to console.log", () => {
    const spy = vi.spyOn(console, "log").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.debug", scope: "test", level: "debug", severityNumber: 5,
    };
    sink.emit(event);
    expect(spy).toHaveBeenCalled();
  });

  it("renders breadcrumbs on error via console.table", () => {
    vi.spyOn(console, "error").mockImplementation(() => {});
    vi.spyOn(console, "groupCollapsed").mockImplementation(() => {});
    vi.spyOn(console, "groupEnd").mockImplementation(() => {});
    const tableSpy = vi.spyOn(console, "table").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.fail", scope: "test", level: "error", severityNumber: 17,
      breadcrumbs: [
        { time: 1, event: "a", scope: "s", level: "info" },
        { time: 2, event: "b", scope: "s", level: "debug" },
      ],
    };
    sink.emit(event);
    expect(tableSpy).toHaveBeenCalled();
  });

  it("uses groupCollapsed for spans", () => {
    const groupSpy = vi.spyOn(console, "groupCollapsed").mockImplementation(() => {});
    vi.spyOn(console, "groupEnd").mockImplementation(() => {});
    const spanData: SpanData = {
      traceId: "a".repeat(32), spanId: "b".repeat(16), name: "test",
      scope: "test", startTime: 0, endTime: 10, durationMs: 10,
      status: "ok", attributes: {}, events: [],
    };
    sink.span(spanData);
    expect(groupSpy).toHaveBeenCalled();
  });

  it("shows attributes via console.table when present", () => {
    vi.spyOn(console, "groupCollapsed").mockImplementation(() => {});
    vi.spyOn(console, "groupEnd").mockImplementation(() => {});
    const tableSpy = vi.spyOn(console, "table").mockImplementation(() => {});
    const spanData: SpanData = {
      traceId: "a".repeat(32), spanId: "b".repeat(16), name: "test",
      scope: "test", startTime: 0, endTime: 10, durationMs: 10,
      status: "ok", attributes: { "http.method": "POST" }, events: [],
    };
    sink.span(spanData);
    expect(tableSpy).toHaveBeenCalled();
  });
});

describe("serializeError", () => {
  it("extracts type, message, stack", () => {
    const err = new TypeError("bad input");
    const result = serializeError(err);
    expect(result.type).toBe("TypeError");
    expect(result.message).toBe("bad input");
    expect(result.stack).toBeDefined();
  });

  it("serializes cause chain", () => {
    const cause = new Error("root cause");
    const err = new Error("wrapper", { cause });
    const result = serializeError(err);
    expect(result.cause).toContain("root cause");
  });

  it("handles error without cause", () => {
    const err = new Error("simple");
    const result = serializeError(err);
    expect(result.cause).toBeUndefined();
  });
});
