// Scripts/tracing/types.ts
// Type definitions for the OTel tracing module. Zero imports.

export type Level = "off" | "error" | "warn" | "info" | "debug" | "trace";

export const LEVELS: Record<Level, number> = {
  off: 0, error: 1, warn: 2, info: 3, debug: 4, trace: 5,
};

export const SEVERITY: Record<Exclude<Level, "off">, number> = {
  error: 17, warn: 13, info: 9, debug: 5, trace: 1,
};

export interface Breadcrumb {
  readonly time: number;
  readonly event: string;
  readonly scope: string;
  readonly level: Level;
  readonly data?: Record<string, unknown>;
}

export interface TraceEvent {
  readonly time: number;
  readonly event: string;
  readonly scope: string;
  readonly level: Level;
  readonly severityNumber: number;
  readonly data?: Record<string, unknown>;
  readonly error?: {
    readonly type: string;
    readonly message: string;
    readonly stack?: string;
    readonly cause?: string;
  };
  readonly traceId?: string;
  readonly spanId?: string;
  readonly breadcrumbs?: readonly Breadcrumb[];
}

export interface SpanData {
  readonly traceId: string;
  readonly spanId: string;
  readonly parentSpanId?: string;
  readonly name: string;
  readonly scope: string;
  readonly startTime: number;
  readonly endTime: number;
  readonly durationMs: number;
  readonly status: "ok" | "error" | "unset";
  readonly attributes: Record<string, string | number | boolean>;
  readonly events: ReadonlyArray<{
    readonly name: string;
    readonly time: number;
    readonly attributes?: Record<string, unknown>;
  }>;
}

export interface TraceSink {
  emit(event: TraceEvent): void;
  span(data: SpanData): void;
  flush(): void;
}

export interface TraceConfig {
  level?: Level;
  sink?: TraceSink;
  breadcrumbCapacity?: number;
  traceparent?: string;
}

export interface TraceRoot {
  readonly traceId: string;
  readonly flags: string;
}

export interface Span {
  readonly traceId: string;
  readonly spanId: string;
  readonly parentSpanId?: string;
  readonly name: string;
  readonly startTime: number;

  child(name: string, attrs?: Record<string, unknown>): Span;
  set(key: string, value: string | number | boolean): void;
  event(name: string, attrs?: Record<string, unknown>): void;
  end(status?: "ok" | "error"): void;
  traceparent(): string;
}

export interface ScopedTracer {
  error(event: string, data?: Record<string, unknown>, err?: Error): void;
  warn(event: string, data?: Record<string, unknown>, err?: Error): void;
  info(event: string, data?: Record<string, unknown>): void;
  debug(event: string, data?: Record<string, unknown>): void;
  trace(event: string, data?: Record<string, unknown>): void;
  span(name: string, attrs?: Record<string, unknown>): Span;
  enabled(level: Level): boolean;
  withSpan(span: Span | undefined): ScopedTracer;
}
