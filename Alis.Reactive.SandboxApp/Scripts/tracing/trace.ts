import type { Level, Span, ScopedTracer, TraceConfig, TraceEvent, TraceSink } from "./types";
import { LEVELS, SEVERITY } from "./types";
import { BreadcrumbBuffer } from "./breadcrumbs";
import { ConsoleSink, serializeError } from "./sink";
import { NOOP_SPAN, ActiveSpan, ContextOnlySpan } from "./span";
import { parseTraceparent } from "./context";

// ── Global state with safe defaults ──────────────────────────
// Safe defaults ensure tracer() calls before configure() work correctly.
// resolver.ts and confirm.ts create scoped tracers at module load time.

let breadcrumbs = new BreadcrumbBuffer(64);
let rootSpan: Span = NOOP_SPAN;
let activeSink: TraceSink = new ConsoleSink();
let activeLevel: number = LEVELS.off;

export function configure(config: TraceConfig): void {
  activeLevel = LEVELS[config.level ?? "off"];
  activeSink = config.sink ?? new ConsoleSink();
  breadcrumbs = new BreadcrumbBuffer(config.breadcrumbCapacity ?? 64);

  if (config.traceparent) {
    const parsed = parseTraceparent(config.traceparent);
    if (parsed) {
      const root = { traceId: parsed.traceId, flags: parsed.flags };
      rootSpan = activeLevel === LEVELS.off
        ? new ContextOnlySpan(root, parsed.spanId)
        : new ActiveSpan("boot", "boot", undefined, activeSink, undefined, root);
    } else {
      rootSpan = NOOP_SPAN;
    }
  } else {
    rootSpan = NOOP_SPAN;
  }
}

export function flush(): void {
  activeSink.flush();
}

export function getRootSpan(): Span {
  return rootSpan;
}

export function createTracer(scope: string): ScopedTracer {
  return buildScopedTracer(scope, undefined);
}

function buildScopedTracer(scope: string, boundSpan: Span | undefined): ScopedTracer {
  function emitEvent(level: Level, event: string, data?: Record<string, unknown>, err?: Error): void {
    const numLevel = LEVELS[level];

    // Breadcrumbs: always capture error/warn, plus anything at or above active level
    if (numLevel <= LEVELS.warn || numLevel <= activeLevel) {
      breadcrumbs.push({ time: performance.now(), event, scope, level, data });
    }

    // Gate: skip sink emission if below active level
    if (numLevel > activeLevel) return;

    const traceEvent: TraceEvent = {
      time: performance.now(),
      event,
      scope,
      level,
      severityNumber: SEVERITY[level as Exclude<Level, "off">],
      data,
      error: err ? serializeError(err) : undefined,
      traceId: boundSpan && boundSpan !== NOOP_SPAN ? boundSpan.traceId : undefined,
      spanId: boundSpan && boundSpan !== NOOP_SPAN ? boundSpan.spanId : undefined,
      breadcrumbs: level === "error" ? breadcrumbs.snapshot() : undefined,
    };

    activeSink.emit(traceEvent);
  }

  return {
    error: (event, data, err) => emitEvent("error", event, data, err),
    warn: (event, data, err) => emitEvent("warn", event, data, err),
    info: (event, data) => emitEvent("info", event, data),
    debug: (event, data) => emitEvent("debug", event, data),
    trace: (event, data) => emitEvent("trace", event, data),
    span: (name, attrs) => {
      const effectiveParent = boundSpan !== NOOP_SPAN ? boundSpan : undefined;
      // TwP: rootSpan is ContextOnlySpan — propagate trace context without collecting
      if (rootSpan instanceof ContextOnlySpan) {
        return (effectiveParent ?? rootSpan).child(name);
      }
      // Full tracing: create ActiveSpan for span collection
      if (activeLevel >= LEVELS.debug) {
        const parent = effectiveParent as ActiveSpan | undefined
          ?? (rootSpan !== NOOP_SPAN ? rootSpan as ActiveSpan : undefined);
        return new ActiveSpan(name, scope, parent, activeSink, attrs);
      }
      // No trace context, no tracing: noop
      return NOOP_SPAN;
    },
    enabled: (level) => LEVELS[level] <= activeLevel,
    withSpan: (span) => buildScopedTracer(scope, span),
  };
}

/** Reset all global state for testing. */
export function resetForTests(): void {
  breadcrumbs = new BreadcrumbBuffer(64);
  rootSpan = NOOP_SPAN;
  activeSink = new ConsoleSink();
  activeLevel = LEVELS.off;
}
