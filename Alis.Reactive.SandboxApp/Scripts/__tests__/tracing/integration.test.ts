import { describe, it, expect, vi, beforeEach } from "vitest";
import { tracer, configure, flush } from "../../tracing";
import { resetForTests } from "../../tracing/trace";
import type { TraceEvent, TraceSink } from "../../tracing/types";

describe("integration: full pipeline", () => {
  beforeEach(() => resetForTests());

  it("configure -> tracer -> emit -> sink receives TraceEvent", () => {
    const events: TraceEvent[] = [];
    const sink: TraceSink = { emit: e => events.push(e), span: vi.fn(), flush: vi.fn() };
    configure({ level: "info", sink });

    const t = tracer("boot");
    t.info("boot.start", { planId: "test-plan", behaviors: 3 });

    expect(events).toHaveLength(1);
    expect(events[0].event).toBe("boot.start");
    expect(events[0].scope).toBe("boot");
    expect(events[0].severityNumber).toBe(9);
    expect(events[0].data).toEqual({ planId: "test-plan", behaviors: 3 });
  });

  it("error event includes breadcrumb trail", () => {
    const events: TraceEvent[] = [];
    const sink: TraceSink = { emit: e => events.push(e), span: vi.fn(), flush: vi.fn() };
    configure({ level: "debug", sink });

    const t = tracer("execute");
    t.debug("reaction.set", { component: "DDL" });
    t.debug("reaction.call", { component: "DDL" });
    t.error("reaction.fail", { trigger: "component-event:DDL.change" }, new Error("prop not found"));

    const errorEvent = events.find(e => e.level === "error")!;
    expect(errorEvent.breadcrumbs!.length).toBeGreaterThanOrEqual(2);
    expect(errorEvent.error!.message).toBe("prop not found");
  });

  it("span emits to sink on end()", () => {
    const spans: unknown[] = [];
    const sink: TraceSink = { emit: vi.fn(), span: s => spans.push(s), flush: vi.fn() };
    configure({ level: "debug", sink });

    const t = tracer("http");
    const span = t.span("http.request", { "http.method": "POST" });
    span.set("http.status", 200);
    span.end("ok");

    expect(spans).toHaveLength(1);
  });

  it("flush delegates to sink", () => {
    const flushFn = vi.fn();
    const sink: TraceSink = { emit: vi.fn(), span: vi.fn(), flush: flushFn };
    configure({ level: "off", sink });

    flush();

    expect(flushFn).toHaveBeenCalled();
  });
});
