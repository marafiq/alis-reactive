/**
 * Scoped tracer factory and module-level configuration.
 *
 * Internal pipeline: `tracer("scope")` returns a `ScopedTracer`; each
 * method builds a `TraceEvent` at emit time and passes it to the active
 * sink. Trace-id and span-id are read from `interactions.ts` lazily at
 * every emission — not captured at tracer creation — so events carry
 * the trace context of the interaction currently running.
 *
 * Breadcrumbs at error or warn level are always captured, even when the
 * active level is `off`, so the ring buffer has enough history to attach
 * to a subsequent error event.
 */

import { BreadcrumbBuffer } from "./breadcrumbs";
import { getCurrentSpanId, getCurrentTraceId, setRootFromTraceparent } from "./interactions";
import { ConsoleSink } from "./sink";
import {
  LEVELS,
  SEVERITY,
  type Level,
  type ScopedTracer,
  type TraceConfig,
  type TraceEvent,
  type TraceSink,
} from "./types";

let breadcrumbs = new BreadcrumbBuffer(64);
let activeSink: TraceSink = new ConsoleSink();
let activeLevel: number = LEVELS.off;

/**
 * Apply runtime configuration. Call once at boot from `root.ts` after
 * parsing the plan. Resetting the level, sink, breadcrumb capacity, or
 * traceparent is idempotent — later calls replace earlier state entirely.
 */
export function configure(config: TraceConfig): void {
  activeLevel = LEVELS[config.level ?? "off"];
  activeSink = config.sink ?? new ConsoleSink();
  breadcrumbs = new BreadcrumbBuffer(config.breadcrumbCapacity ?? 64);
  setRootFromTraceparent(config.traceparent);
}

/**
 * Create a `ScopedTracer` bound to a named scope. The scope is the
 * logger identity (e.g. `"http"`, `"execute"`) and appears on every
 * event emitted through this tracer.
 *
 * Trace-id / span-id are read lazily inside `emit` — not closed over
 * at tracer creation — so a tracer created at module load picks up the
 * interaction that is actively running at emit time.
 */
export function tracer(scope: string): ScopedTracer {
  function emit(
    level: Exclude<Level, "off">,
    event: string,
    data?: Record<string, unknown>,
    err?: Error,
  ): void {
    const levelNum = LEVELS[level];

    if (levelNum <= LEVELS.warn || levelNum <= activeLevel) {
      breadcrumbs.push({ time: performance.now(), event, scope, level, data });
    }

    if (levelNum > activeLevel) return;

    const ev: TraceEvent = {
      time: performance.now(),
      event,
      scope,
      level,
      severityNumber: SEVERITY[level],
      data,
      error: err ? serializeError(err) : undefined,
      traceId: getCurrentTraceId(),
      spanId: getCurrentSpanId(),
      breadcrumbs: level === "error" ? breadcrumbs.snapshot() : undefined,
    };

    activeSink.emit(ev);
  }

  return {
    error: (event, data, err) => emit("error", event, data, err),
    warn: (event, data, err) => emit("warn", event, data, err),
    info: (event, data) => emit("info", event, data),
    debug: (event, data) => emit("debug", event, data),
    trace: (event, data) => emit("trace", event, data),
    enabled: (level) => level !== "off" && LEVELS[level] <= activeLevel,
  };
}

/** Serialize an Error for structured event carriage. Retains name, message, stack. */
function serializeError(err: Error): { name: string; message: string; stack?: string } {
  return { name: err.name, message: err.message, stack: err.stack };
}

/**
 * Test-only hook. Resets module-level state so vitest files can isolate
 * cases. Production code must not call this.
 */
export function resetForTests(): void {
  breadcrumbs = new BreadcrumbBuffer(64);
  activeSink = new ConsoleSink();
  activeLevel = LEVELS.off;
  setRootFromTraceparent(undefined);
}
