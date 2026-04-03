import type { TraceContext } from "../types";

export type TraceLevel = "off" | "error" | "warn" | "info" | "debug" | "trace";
export type SpanKind = "internal" | "client" | "consumer";
export type SpanStatusCode = "unset" | "ok" | "error";

export interface TraceSpan {
  readonly name: string;
  readonly kind: SpanKind;
  readonly context: TraceContext;
  readonly startedAt: number;
  readonly attributes: Record<string, unknown>;
  statusCode: SpanStatusCode;
  statusMessage?: string;
}

export interface StartSpanOptions {
  readonly parent?: TraceContext;
  readonly kind?: SpanKind;
  readonly attributes?: Record<string, unknown>;
}

const LEVELS: Record<TraceLevel, number> = {
  off: 0, error: 1, warn: 2, info: 3, debug: 4, trace: 5,
};

let active = LEVELS.off;
const eventContexts = new WeakMap<Event, TraceContext>();

export function setLevel(level: TraceLevel): void {
  active = LEVELS[level];
}

export interface Logger {
  error(msg: string, data?: unknown): void;
  warn(msg: string, data?: unknown): void;
  info(msg: string, data?: unknown): void;
  debug(msg: string, data?: unknown): void;
  trace(msg: string, data?: unknown): void;
}

export function formatTraceParent(context: TraceContext): string {
  return `00-${context.traceId}-${context.spanId}-${context.traceFlags}`;
}

export function applyTraceContext(
  headersInit: HeadersInit | undefined,
  context: TraceContext
): Headers {
  const headers = new Headers(headersInit);
  headers.set("traceparent", formatTraceParent(context));
  return headers;
}

export function scope(name: string): Logger {
  const tag = `[alis:${name}]`;
  return {
    error: (msg, data) => emit(LEVELS.error, tag, msg, data),
    warn: (msg, data) => emit(LEVELS.warn, tag, msg, data),
    info: (msg, data) => emit(LEVELS.info, tag, msg, data),
    debug: (msg, data) => emit(LEVELS.debug, tag, msg, data),
    trace: (msg, data) => emit(LEVELS.trace, tag, msg, data),
  };
}

export function startSpan(name: string, options: StartSpanOptions = {}): TraceSpan {
  const parent = options.parent;
  const context: TraceContext = {
    traceId: parent?.traceId ?? randomHex(32),
    spanId: randomHex(16),
    traceFlags: parent?.traceFlags ?? "01",
    parentSpanId: parent?.spanId,
  };

  const span: TraceSpan = {
    name,
    kind: options.kind ?? "internal",
    context,
    startedAt: Date.now(),
    attributes: options.attributes ? { ...options.attributes } : {},
    statusCode: "unset",
  };

  emitStructured(LEVELS.debug, {
    body: "span.start",
    span_name: span.name,
    span_kind: span.kind,
    trace_id: context.traceId,
    span_id: context.spanId,
    parent_span_id: context.parentSpanId,
    trace_flags: context.traceFlags,
    attributes: span.attributes,
  });

  return span;
}

export function addSpanEvent(span: TraceSpan, name: string, attributes?: Record<string, unknown>): void {
  emitStructured(LEVELS.trace, {
    body: "span.event",
    span_name: span.name,
    event_name: name,
    trace_id: span.context.traceId,
    span_id: span.context.spanId,
    parent_span_id: span.context.parentSpanId,
    trace_flags: span.context.traceFlags,
    attributes: attributes ?? {},
  });
}

export function setSpanStatus(span: TraceSpan, code: SpanStatusCode, message?: string): void {
  span.statusCode = code;
  span.statusMessage = message;
}

export function recordException(span: TraceSpan, error: unknown): void {
  const err = error instanceof Error ? error : new Error(String(error));
  addSpanEvent(span, "exception", {
    "exception.type": err.name,
    "exception.message": err.message,
    "exception.stacktrace": err.stack,
  });
}

export function endSpan(span: TraceSpan, attributes?: Record<string, unknown>): void {
  emitStructured(LEVELS.debug, {
    body: "span.end",
    span_name: span.name,
    span_kind: span.kind,
    trace_id: span.context.traceId,
    span_id: span.context.spanId,
    parent_span_id: span.context.parentSpanId,
    trace_flags: span.context.traceFlags,
    duration_ms: Date.now() - span.startedAt,
    status: {
      code: span.statusCode,
      message: span.statusMessage,
    },
    attributes: attributes ?? {},
  });
}

export function attachEventTraceContext(event: Event, context: TraceContext): void {
  eventContexts.set(event, context);
}

export function getEventTraceContext(event: Event): TraceContext | undefined {
  return eventContexts.get(event);
}

function emit(level: number, tag: string, msg: string, data?: unknown): void {
  if (level > active) return;
  const line = data !== undefined ? `${tag} ${msg} ${safeStringify(data)}` : `${tag} ${msg}`;
  if (level <= LEVELS.error) console.error(line);
  else if (level <= LEVELS.warn) console.warn(line);
  else console.log(line);
}

function emitStructured(level: number, record: Record<string, unknown>): void {
  if (level > active) return;
  const payload = {
    timestamp: new Date().toISOString(),
    severity_number: levelToSeverityNumber(level),
    severity_text: levelToSeverityText(level),
    ...record,
  };
  const line = safeStringify(payload);
  if (level <= LEVELS.error) console.error(line);
  else if (level <= LEVELS.warn) console.warn(line);
  else console.log(line);
}

function safeStringify(value: unknown): string {
  const seen = new WeakSet<object>();
  return JSON.stringify(value, (_key, current) => {
    if (current instanceof Date) return current.toISOString();
    if (current instanceof Error) {
      return {
        name: current.name,
        message: current.message,
        stack: current.stack,
      };
    }
    if (current instanceof File) {
      return {
        name: current.name,
        size: current.size,
        type: current.type,
      };
    }
    if (current instanceof Element) {
      return {
        tagName: current.tagName.toLowerCase(),
        id: current.id || undefined,
      };
    }
    if (typeof current === "function") return "[Function]";
    if (typeof current === "object" && current !== null) {
      if (seen.has(current)) return "[Circular]";
      seen.add(current);
    }
    return current;
  });
}

function randomHex(length: number): string {
  const bytes = new Uint8Array(length / 2);
  crypto.getRandomValues(bytes);
  return Array.from(bytes, byte => byte.toString(16).padStart(2, "0")).join("");
}

function levelToSeverityText(level: number): "ERROR" | "WARN" | "INFO" | "DEBUG" | "TRACE" {
  if (level <= LEVELS.error) return "ERROR";
  if (level <= LEVELS.warn) return "WARN";
  if (level <= LEVELS.info) return "INFO";
  if (level <= LEVELS.debug) return "DEBUG";
  return "TRACE";
}

function levelToSeverityNumber(level: number): number {
  if (level <= LEVELS.error) return 17;
  if (level <= LEVELS.warn) return 13;
  if (level <= LEVELS.info) return 9;
  if (level <= LEVELS.debug) return 5;
  return 1;
}
