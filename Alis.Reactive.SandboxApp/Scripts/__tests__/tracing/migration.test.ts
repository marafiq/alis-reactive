import { describe, it, expect, vi, beforeEach } from "vitest";
import { tracer, configure } from "../../tracing";
import { resetForTests } from "../../tracing/trace";
import type { TraceEvent, TraceSink } from "../../tracing/types";

describe("migration spot-check", () => {
  let events: TraceEvent[];
  let sink: TraceSink;

  beforeEach(() => {
    resetForTests();
    events = [];
    sink = { emit: e => events.push(e), span: vi.fn(), flush: vi.fn() };
    configure({ level: "trace", sink });
  });

  it("boot.start event has planId and behaviors fields", () => {
    const t = tracer("boot");
    t.info("boot.start", { planId: "test", behaviors: 5 });
    expect(events[0].event).toBe("boot.start");
    expect(events[0].scope).toBe("boot");
    expect(events[0].data).toEqual({ planId: "test", behaviors: 5 });
  });

  it("reaction.fail event includes trigger and planId", () => {
    const t = tracer("trigger");
    t.error("reaction.fail", {
      trigger: "component-event:DDL__Status.change",
      planId: "order-form",
    }, new Error("prop not found"));
    expect(events[0].event).toBe("reaction.fail");
    expect(events[0].data?.trigger).toBe("component-event:DDL__Status.change");
    expect(events[0].data?.planId).toBe("order-form");
    expect(events[0].error?.type).toBe("Error");
    expect(events[0].error?.message).toBe("prop not found");
    expect(events[0].breadcrumbs).toBeDefined();
  });

  it("http.request.send has method and url", () => {
    const t = tracer("http");
    t.debug("http.request.send", { method: "POST", url: "/api/orders" });
    expect(events[0].event).toBe("http.request.send");
    expect(events[0].data).toEqual({ method: "POST", url: "/api/orders" });
  });

  it("gather.serialize.fail has field and error", () => {
    const t = tracer("gather");
    t.warn("gather.serialize.fail", { field: "amount", error: "NaN" });
    expect(events[0].event).toBe("gather.serialize.fail");
    expect(events[0].data?.field).toBe("amount");
  });

  it("condition.eval uses trace level", () => {
    const t = tracer("conditions");
    if (t.enabled("trace")) {
      t.trace("condition.eval", { op: "eq", left: "a", right: "b" });
    }
    expect(events[0].event).toBe("condition.eval");
    expect(events[0].level).toBe("trace");
  });
});
