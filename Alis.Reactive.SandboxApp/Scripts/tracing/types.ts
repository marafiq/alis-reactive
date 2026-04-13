/**
 * Public types for the structured tracing module.
 *
 * Export contract: this file + `index.ts` expose exactly 3 runtime names
 * (`tracer`, `configure`, `ConsoleSink`) + 5 type names (`Level`,
 * `TraceEvent`, `TraceSink`, `TraceConfig`, `ScopedTracer`).
 *
 * Span primitives (`Span`, `SpanData`, `TraceRoot`, `NOOP_SPAN`,
 * `ActiveSpan`, `ContextOnlySpan`, `withSpan`) do not exist in this design.
 * Span lifecycle lives inside `interactions.ts` and is never exposed.
 */

/** Severity level for trace output. `off` suppresses all emission. */
export type Level = "off" | "error" | "warn" | "info" | "debug" | "trace";

/**
 * Numeric ordering of levels for gating. Higher numbers emit more.
 * An emission passes when its level's number is `<=` the active level.
 */
export const LEVELS: Record<Level, number> = {
  off: 0,
  error: 1,
  warn: 2,
  info: 3,
  debug: 4,
  trace: 5,
};

/**
 * OpenTelemetry severity numbers (per the OTel logs data model) for each
 * emittable level. `off` has no severity because it never emits.
 */
export const SEVERITY: Record<Exclude<Level, "off">, number> = {
  error: 17,
  warn: 13,
  info: 9,
  debug: 5,
  trace: 1,
};

/** A single breadcrumb captured in the ring buffer. */
export interface Breadcrumb {
  readonly time: number;
  readonly event: string;
  readonly scope: string;
  readonly level: Exclude<Level, "off">;
  readonly data?: Record<string, unknown>;
}

/**
 * One structured trace event delivered to a `TraceSink`.
 *
 * `traceId` and `spanId` reflect the interaction active at emit time
 * (read lazily from `interactions.ts`, not captured at tracer construction).
 * `breadcrumbs` is populated only for error-level events.
 */
export interface TraceEvent {
  readonly time: number;
  readonly event: string;
  readonly scope: string;
  readonly level: Exclude<Level, "off">;
  readonly severityNumber: number;
  readonly data?: Record<string, unknown>;
  readonly error?: {
    readonly name: string;
    readonly message: string;
    readonly stack?: string;
  };
  readonly traceId?: string;
  readonly spanId?: string;
  readonly breadcrumbs?: readonly Breadcrumb[];
}

/** Consumer of trace events. `ConsoleSink` is the default implementation. */
export interface TraceSink {
  emit(event: TraceEvent): void;
  flush(): void;
}

/** Runtime configuration for the tracing module, applied via `configure()`. */
export interface TraceConfig {
  readonly level?: Level;
  readonly sink?: TraceSink;
  readonly traceparent?: string;
  readonly breadcrumbCapacity?: number;
}

/**
 * The consumer-facing tracing API. Obtained via `tracer("scope")`.
 * Devs call these methods with dotted event names and structured data;
 * span lifecycle is handled automatically inside the framework.
 */
export interface ScopedTracer {
  error(event: string, data?: Record<string, unknown>, err?: Error): void;
  warn(event: string, data?: Record<string, unknown>, err?: Error): void;
  info(event: string, data?: Record<string, unknown>): void;
  debug(event: string, data?: Record<string, unknown>): void;
  trace(event: string, data?: Record<string, unknown>): void;
  enabled(level: Level): boolean;
}
