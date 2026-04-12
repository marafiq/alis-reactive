import { describe, it, expect, beforeEach } from "vitest";
import { tracer, configure } from "../../tracing";
import { resetForTests, getRootSpan } from "../../tracing/trace";
import { ContextOnlySpan } from "../../tracing/span";

describe("Tracing Without Performance (TwP)", () => {
  beforeEach(() => resetForTests());

  it("tracing off + traceparent -> ContextOnlySpan root", () => {
    configure({
      level: "off",
      traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    });
    const root = getRootSpan();
    expect(root).toBeInstanceOf(ContextOnlySpan);
    expect(root.traceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
  });

  it("t.span() returns ContextOnlySpan child in TwP mode", () => {
    configure({
      level: "off",
      traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    });
    const t = tracer("http");
    const span = t.span("http.request");
    expect(span).toBeInstanceOf(ContextOnlySpan);
    expect(span.traceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
  });

  it("ContextOnlySpan.traceparent() returns valid W3C header", () => {
    configure({
      level: "off",
      traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    });
    const t = tracer("http");
    const span = t.span("http.request");
    const tp = span.traceparent();
    expect(tp).toMatch(/^00-4bf92f3577b34da6a3ce929d0e0e4736-[0-9a-f]{16}-01$/);
    expect(tp).not.toContain("0".repeat(32));
  });

  it("tracing off + no traceparent -> NOOP_SPAN", () => {
    configure({ level: "off" });
    const t = tracer("http");
    const span = t.span("http.request");
    expect(span.traceId).toBe("0".repeat(32));
  });

  it("preserves flags from server traceparent", () => {
    configure({
      level: "off",
      traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-ff",
    });
    const t = tracer("http");
    const span = t.span("http.request");
    expect(span.traceparent()).toContain("-ff");
  });
});
