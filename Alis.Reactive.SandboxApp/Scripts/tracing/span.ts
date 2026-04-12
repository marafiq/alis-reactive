import type { Span, TraceRoot, TraceSink } from "./types";
import { formatTraceparent } from "./context";

export function generateTraceId(): string {
  const bytes = new Uint8Array(16);
  crypto.getRandomValues(bytes);
  return Array.from(bytes, b => b.toString(16).padStart(2, "0")).join("");
}

export function generateSpanId(): string {
  const bytes = new Uint8Array(8);
  crypto.getRandomValues(bytes);
  return Array.from(bytes, b => b.toString(16).padStart(2, "0")).join("");
}

export const NOOP_SPAN: Span = Object.freeze({
  traceId: "0".repeat(32),
  spanId: "0".repeat(16),
  parentSpanId: undefined,
  name: "",
  startTime: 0,
  child: () => NOOP_SPAN,
  set: () => {},
  event: () => {},
  end: () => {},
  traceparent: () => "00-" + "0".repeat(32) + "-" + "0".repeat(16) + "-00",
});

export class ContextOnlySpan implements Span {
  readonly traceId: string;
  readonly spanId: string;
  readonly parentSpanId?: string;
  readonly name = "";
  readonly startTime = 0;
  private readonly root: TraceRoot;

  constructor(root: TraceRoot, parentSpanId?: string) {
    this.root = root;
    this.traceId = root.traceId;
    this.spanId = generateSpanId();
    this.parentSpanId = parentSpanId;
  }

  child(): Span { return new ContextOnlySpan(this.root, this.spanId); }
  set(): void {}
  event(): void {}
  end(): void {}
  traceparent(): string { return formatTraceparent(this.root.traceId, this.spanId, this.root.flags); }
}

export class ActiveSpan implements Span {
  readonly traceId: string;
  readonly spanId: string;
  readonly parentSpanId?: string;
  readonly name: string;
  readonly startTime: number;
  private readonly root: TraceRoot;
  private readonly scope: string;
  private readonly attributes: Record<string, string | number | boolean> = {};
  private readonly spanEvents: Array<{ name: string; time: number; attributes?: Record<string, unknown> }> = [];
  private readonly sink: TraceSink;

  constructor(
    name: string,
    scope: string,
    parent: ActiveSpan | undefined,
    sink: TraceSink,
    attrs?: Record<string, unknown>,
    inheritedRoot?: TraceRoot,
  ) {
    this.root = inheritedRoot ?? parent?.root ?? { traceId: generateTraceId(), flags: "01" };
    this.traceId = this.root.traceId;
    this.spanId = generateSpanId();
    this.parentSpanId = parent?.spanId;
    this.name = name;
    this.scope = scope;
    this.startTime = performance.now();
    this.sink = sink;
    if (attrs) {
      for (const [k, v] of Object.entries(attrs)) {
        if (typeof v === "string" || typeof v === "number" || typeof v === "boolean") {
          this.attributes[k] = v;
        }
      }
    }
  }

  child(name: string, attrs?: Record<string, unknown>): Span {
    return new ActiveSpan(name, this.scope, this, this.sink, attrs);
  }

  set(key: string, value: string | number | boolean): void {
    this.attributes[key] = value;
  }

  event(name: string, attrs?: Record<string, unknown>): void {
    this.spanEvents.push({ name, time: performance.now(), attributes: attrs });
  }

  end(status?: "ok" | "error"): void {
    const endTime = performance.now();
    this.sink.span({
      traceId: this.traceId,
      spanId: this.spanId,
      parentSpanId: this.parentSpanId,
      name: this.name,
      scope: this.scope,
      startTime: this.startTime,
      endTime,
      durationMs: endTime - this.startTime,
      status: status ?? "unset",
      attributes: { ...this.attributes },
      events: [...this.spanEvents],
    });
  }

  traceparent(): string {
    return formatTraceparent(this.root.traceId, this.spanId, this.root.flags);
  }
}
