import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ConsoleSink, safeStringify } from "../../tracing/sink";
import type { TraceEvent } from "../../tracing/types";

function makeEvent(overrides: Partial<TraceEvent> = {}): TraceEvent {
  return {
    time: 123.4,
    event: "test.event",
    scope: "test",
    level: "info",
    severityNumber: 9,
    data: undefined,
    error: undefined,
    traceId: undefined,
    spanId: undefined,
    breadcrumbs: undefined,
    ...overrides,
  };
}

let errSpy: ReturnType<typeof vi.spyOn>;
let warnSpy: ReturnType<typeof vi.spyOn>;
let infoSpy: ReturnType<typeof vi.spyOn>;
let logSpy: ReturnType<typeof vi.spyOn>;
let groupCollapsedSpy: ReturnType<typeof vi.spyOn>;
let groupEndSpy: ReturnType<typeof vi.spyOn>;
let tableSpy: ReturnType<typeof vi.spyOn>;

beforeEach(() => {
  errSpy = vi.spyOn(console, "error").mockImplementation(() => {});
  warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});
  infoSpy = vi.spyOn(console, "info").mockImplementation(() => {});
  logSpy = vi.spyOn(console, "log").mockImplementation(() => {});
  groupCollapsedSpy = vi.spyOn(console, "groupCollapsed").mockImplementation(() => {});
  groupEndSpy = vi.spyOn(console, "groupEnd").mockImplementation(() => {});
  tableSpy = vi.spyOn(console, "table").mockImplementation(() => {});
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe("ConsoleSink.emit level routing", () => {
  const sink = new ConsoleSink();

  it("routes error events to console.error", () => {
    sink.emit(makeEvent({ level: "error", severityNumber: 17 }));
    expect(errSpy).toHaveBeenCalledOnce();
    expect(warnSpy).not.toHaveBeenCalled();
  });

  it("routes warn events to console.warn", () => {
    sink.emit(makeEvent({ level: "warn", severityNumber: 13 }));
    expect(warnSpy).toHaveBeenCalledOnce();
    expect(errSpy).not.toHaveBeenCalled();
  });

  it("routes info events to console.info", () => {
    sink.emit(makeEvent({ level: "info", severityNumber: 9 }));
    expect(infoSpy).toHaveBeenCalledOnce();
    expect(logSpy).not.toHaveBeenCalled();
  });

  it("routes debug events to console.log", () => {
    sink.emit(makeEvent({ level: "debug", severityNumber: 5 }));
    expect(logSpy).toHaveBeenCalledOnce();
    expect(infoSpy).not.toHaveBeenCalled();
  });

  it("routes trace events to console.log", () => {
    sink.emit(makeEvent({ level: "trace", severityNumber: 1 }));
    expect(logSpy).toHaveBeenCalledOnce();
  });
});

describe("ConsoleSink.emit data handling", () => {
  const sink = new ConsoleSink();

  it("inlines data into the message — does not pass as a separate arg", () => {
    sink.emit(makeEvent({ data: { method: "GET", url: "/api/x" } }));
    const callArgs = infoSpy.mock.calls[0];
    const messageStr = callArgs[0] as string;
    // data must appear as JSON inside the message string
    expect(messageStr).toContain('{"method":"GET","url":"/api/x"}');
    // No additional object arg with the data — only the %c style strings and (optionally) error
    const extraArgs = callArgs.slice(1);
    for (const arg of extraArgs) {
      if (typeof arg === "object" && arg !== null && !Array.isArray(arg)) {
        // The only object arg allowed after styles is the serialized error
        expect(arg).toHaveProperty("message");
      }
    }
  });

  it("passes the error as a separate arg so DevTools can expand the stack", () => {
    const serialized = { name: "Error", message: "boom", stack: "stack..." };
    sink.emit(makeEvent({ level: "error", error: serialized }));
    const callArgs = errSpy.mock.calls[0];
    // Error arg should appear after the 3 style strings
    expect(callArgs).toContainEqual(serialized);
  });

  it("omits dataStr entirely when data is undefined", () => {
    sink.emit(makeEvent({ data: undefined }));
    const messageStr = infoSpy.mock.calls[0][0] as string;
    expect(messageStr).not.toContain("undefined");
    expect(messageStr).toMatch(/\[alis:test\].*test\.event.*INFO/);
  });
});

describe("ConsoleSink.emit breadcrumbs rendering", () => {
  const sink = new ConsoleSink();

  it("renders a console.table only on error events", () => {
    sink.emit(
      makeEvent({
        level: "error",
        severityNumber: 17,
        breadcrumbs: [
          { time: 1, event: "e1", scope: "s", level: "info" },
          { time: 2, event: "e2", scope: "s", level: "info" },
        ],
      }),
    );
    expect(groupCollapsedSpy).toHaveBeenCalledOnce();
    expect(tableSpy).toHaveBeenCalledOnce();
    expect(groupEndSpy).toHaveBeenCalledOnce();
  });

  it("does not render breadcrumbs on info events", () => {
    sink.emit(
      makeEvent({
        level: "info",
        breadcrumbs: [{ time: 1, event: "e1", scope: "s", level: "info" }],
      }),
    );
    expect(tableSpy).not.toHaveBeenCalled();
    expect(groupCollapsedSpy).not.toHaveBeenCalled();
  });

  it("does not render breadcrumbs when the breadcrumbs array is empty", () => {
    sink.emit(makeEvent({ level: "error", severityNumber: 17, breadcrumbs: [] }));
    expect(tableSpy).not.toHaveBeenCalled();
  });
});

describe("ConsoleSink.emit trace-id line", () => {
  const sink = new ConsoleSink();

  it("renders a trace/span line when traceId is set", () => {
    sink.emit(
      makeEvent({
        traceId: "4bf92f3577b34da6a3ce929d0e0e4736",
        spanId: "00f067aa0ba902b7",
      }),
    );
    const logCalls = logSpy.mock.calls.map((c) => c[0] as string);
    expect(logCalls.some((c) => c.includes("trace: 4bf92f3577b34da6a3ce929d0e0e4736"))).toBe(true);
  });

  it("omits the trace line when traceId is undefined", () => {
    sink.emit(makeEvent({ traceId: undefined }));
    const logCalls = logSpy.mock.calls.map((c) => c[0] as string);
    expect(logCalls.some((c) => c.includes("trace:"))).toBe(false);
  });
});

describe("ConsoleSink.flush", () => {
  it("is a no-op", () => {
    const sink = new ConsoleSink();
    expect(() => sink.flush()).not.toThrow();
  });
});

describe("safeStringify — non-JSON-safe value handling", () => {
  // Regression for Codex adversarial round 2 finding #2: a sink that
  // JSON.stringifies raw runtime values will throw on circular refs,
  // BigInt, Symbol, or functions — and that throw must not escape the
  // tracer. ConsoleSink uses safeStringify for precisely this reason;
  // these tests lock the contract so no future refactor regresses it.

  it("encodes circular references as [Circular] instead of throwing", () => {
    const circular: Record<string, unknown> = { name: "parent" };
    circular.self = circular;
    expect(() => safeStringify(circular)).not.toThrow();
    const result = safeStringify(circular);
    expect(result).toContain("[Circular]");
    expect(result).toContain("parent");
  });

  it("encodes deeply nested circular references", () => {
    const a: Record<string, unknown> = { name: "a" };
    const b: Record<string, unknown> = { name: "b", a };
    a.b = b;
    expect(() => safeStringify(a)).not.toThrow();
    expect(safeStringify(a)).toContain("[Circular]");
  });

  it("encodes BigInt as a string with a trailing n suffix", () => {
    const result = safeStringify({ count: 9007199254740993n });
    expect(result).toBe('{"count":"9007199254740993n"}');
  });

  it("encodes Symbol values as their description string", () => {
    const sym = Symbol("my-symbol");
    const result = safeStringify({ tag: sym });
    expect(result).toContain("Symbol(my-symbol)");
  });

  it("encodes functions with a [Function name] placeholder", () => {
    function myHandler(): void {}
    const result = safeStringify({ handler: myHandler });
    expect(result).toContain("[Function myHandler]");
  });

  it("encodes anonymous functions as [Function anonymous]", () => {
    const result = safeStringify({ handler: () => undefined });
    expect(result).toMatch(/\[Function (handler|anonymous)\]/);
  });

  it("encodes Error instances as a { name, message } object", () => {
    const err = new TypeError("bad input");
    const result = safeStringify({ cause: err });
    expect(result).toContain('"name":"TypeError"');
    expect(result).toContain('"message":"bad input"');
  });

  it("encodes Map as a plain object", () => {
    const m = new Map<string, number>([["a", 1], ["b", 2]]);
    const result = safeStringify({ m });
    expect(result).toContain('"a":1');
    expect(result).toContain('"b":2');
  });

  it("encodes Set as an array", () => {
    const s = new Set([1, 2, 3]);
    const result = safeStringify({ s });
    expect(result).toContain("[1,2,3]");
  });

  it("encodes DOM Node as a compact [Node tag#id] placeholder", () => {
    const div = document.createElement("div");
    div.id = "my-div";
    const result = safeStringify({ el: div });
    expect(result).toContain("[Node DIV#my-div]");
  });

  it("handles undefined input", () => {
    expect(safeStringify(undefined)).toBe("undefined");
  });

  it("handles null input", () => {
    expect(safeStringify(null)).toBe("null");
  });

  it("handles primitive numbers, strings, booleans", () => {
    expect(safeStringify(42)).toBe("42");
    expect(safeStringify("hello")).toBe('"hello"');
    expect(safeStringify(true)).toBe("true");
  });
});

describe("ConsoleSink resilience to non-JSON-safe event data", () => {
  it("does not throw when event.data contains a circular reference", () => {
    const sink = new ConsoleSink();
    const circular: Record<string, unknown> = { name: "n" };
    circular.self = circular;
    expect(() =>
      sink.emit(makeEvent({ data: circular as Record<string, unknown> })),
    ).not.toThrow();
  });

  it("does not throw when event.data contains a BigInt", () => {
    const sink = new ConsoleSink();
    expect(() =>
      sink.emit(makeEvent({ data: { big: 123n } as Record<string, unknown> })),
    ).not.toThrow();
  });

  it("does not throw when event.data contains a DOM node", () => {
    const sink = new ConsoleSink();
    const node = document.createElement("span");
    expect(() =>
      sink.emit(makeEvent({ data: { node } as Record<string, unknown> })),
    ).not.toThrow();
  });
});
