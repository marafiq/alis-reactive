# OTel Tracing Module Design Spec

**Date:** 2026-04-12
**Branch:** tracing-module-otel-standard-as-first-class-citizen-in-ts-runtime
**Scope:** Layer 3 (TS Runtime), integration point in Layer 1 (C# plan carries traceparent)
**Research:** `docs/superpowers/research/2026-04-12-otel-tracing-research.md`

## Problem

The TS runtime has 38 lines of level-gated console logging (`core/trace.ts`). There are 66
`log.*` calls across 14 files and 47 throw statements with `[alis]` prefix. There is no
correlation between events, no span hierarchy, no breadcrumb trail, no structured error
context, and no distributed trace continuity between server and browser.

When something fails in production, a developer or LLM sees:
```
[alis:trigger] reaction failed {"error":"[alis] property \"status\" not found on type"}
```
No context on what happened before, which plan, which component, which trigger, or how to
reproduce. Debugging requires guessing.

## Goals

1. **Standard OTel severity levels** mapped to console methods, with OTel severity numbers
   in every trace event for backend ingestion.
2. **Clear, actionable errors** with structured context: which component, which plan, what was
   expected, what was available. Event-name + typed payload format. Both catch-point
   enrichment and throw-site message enhancement.
3. **Zero runtime pollution.** Self-contained module under `Scripts/tracing/`. Clean API.
   No global state leakage.
4. **Production-safe.** Configurable levels. Near-zero cost when `off` (noop span, early-exit
   guard, one small object allocation per call). Breadcrumbs still capture `error`/`warn`
   even when tracing is off.
5. **Full execution path on failure.** Breadcrumb ring buffer attached to every error event
   automatically. Developer and LLM see the complete chain from boot to failure.
6. **Structured tracing.** Event-name + typed payload. Machine-parseable, filterable,
   aggregatable. Console output uses rich formatting (groups, tables, CSS labels).
7. **OTel-compatible.** W3C traceparent propagation (including TwP mode for context
   propagation without span collection). OTel severity numbers. Semantic convention
   attribute names. Pluggable sink for any backend (Sentry, Datadog, OTLP).

## Non-Goals

- Importing the full OTel SDK (~60KB gzipped). Bundle cost prohibitive for a 93KB runtime.
- Replacing the sync-first execution model. Spans adapt to it.
- Sampling strategy (deferred to sink configuration).
- Server-side C# tracing changes beyond adding `traceparent` and `traceLevel` to plan JSON.

## Architecture

### Module Structure

```
Scripts/tracing/
├── index.ts          ← barrel export: tracer(), configure(), flush()
├── trace.ts          ← ScopedTracer factory, global state (level, sink, breadcrumbs, rootSpan)
├── span.ts           ← Span, NoopSpan, ID generation, traceparent format/parse
├── breadcrumbs.ts    ← ring buffer (fixed-size circular)
├── sink.ts           ← TraceSink interface + ConsoleSink (rich formatting)
├── context.ts        ← level resolution from plan/element/URL/localStorage
└── types.ts          ← TraceEvent, SpanData, Breadcrumb, Level, TraceConfig, TraceSink
```

**Dependency direction:** `types.ts` has zero imports. All other files import from `types.ts`.
`trace.ts` imports from all siblings. `index.ts` re-exports from `trace.ts`.

### Public API

```typescript
import { tracer, configure, flush } from "../tracing";

// ─── Configuration (called once at boot) ────────────────────
configure({
  level: "debug",                    // TraceLevel
  sink: new ConsoleSink(),           // TraceSink (default)
  breadcrumbCapacity: 64,            // ring buffer size
  traceparent: plan.traceparent,     // from server, optional
});

// ─── Scoped tracer (one per module) ─────────────────────────
const t = tracer("http");

// ─── Structured events (replaces log.*) ─────────────────────
t.error("request.fail", { url, method, status: 500 }, err);
t.warn("gather.serialize.fail", { field: name, error: result.error });
t.info("boot.start", { planId: plan.planId, behaviors: plan.behaviors.length });
t.debug("http.request.send", { method: "POST", url: "/api/orders" });
t.trace("reaction.set", { component: "OrderModel__Status", property: "value", value: 42 });

// ─── Level guard for hot paths ──────────────────────────────
if (t.enabled("trace")) {
  t.trace("condition.eval", { op: cond.op, left: shapedLeft, right: shapedRight });
}

// ─── Spans (new capability) ─────────────────────────────────
const span = t.span("http.request", { "http.method": "POST", "http.url": url });
span.set("http.status", 200);
span.event("response.parse", { bodyLength: 1234 });
span.end("ok");                // or "error"

// ─── Bind active span to scoped tracer ──────────────────────
const scopedWithSpan = t.withSpan(ctx.span);
scopedWithSpan.debug("http.request.send", { method, url });
// ↑ traceId/spanId from ctx.span automatically attached to the event

// ─── Flush (page unload) ────────────────────────────────────
window.addEventListener("beforeunload", () => flush());
```

### Type Definitions

```typescript
// ─── types.ts ───────────────────────────────────────────────

type Level = "off" | "error" | "warn" | "info" | "debug" | "trace";

/** OTel severity number mapping. */
const SEVERITY: Record<Exclude<Level, "off">, number> = {
  error: 17, warn: 13, info: 9, debug: 5, trace: 1,
};

interface TraceEvent {
  readonly time: number;           // performance.now()
  readonly event: string;          // dotted event name
  readonly scope: string;          // tracer scope
  readonly level: Level;
  readonly severityNumber: number; // OTel Logs Data Model severity (1-24)
  readonly data?: Record<string, unknown>;
  readonly error?: {
    readonly type: string;
    readonly message: string;
    readonly stack?: string;
    readonly cause?: string;       // Error.cause chain serialized
  };
  readonly traceId?: string;
  readonly spanId?: string;
  readonly breadcrumbs?: readonly Breadcrumb[];  // attached on error events
}

interface SpanData {
  readonly traceId: string;
  readonly spanId: string;
  readonly parentSpanId?: string;
  readonly name: string;
  readonly scope: string;
  readonly startTime: number;      // performance.now()
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

interface Breadcrumb {
  readonly time: number;
  readonly event: string;
  readonly scope: string;
  readonly level: Level;
  readonly data?: Record<string, unknown>;
}

interface TraceSink {
  emit(event: TraceEvent): void;
  span(data: SpanData): void;
  flush(): void;
}

interface TraceConfig {
  level?: Level;
  sink?: TraceSink;
  breadcrumbCapacity?: number;     // default 64
  traceparent?: string;            // W3C traceparent from server plan
}

interface Span {
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

interface ScopedTracer {
  error(event: string, data?: Record<string, unknown>, err?: Error): void;
  warn(event: string, data?: Record<string, unknown>, err?: Error): void;
  info(event: string, data?: Record<string, unknown>): void;
  debug(event: string, data?: Record<string, unknown>): void;
  trace(event: string, data?: Record<string, unknown>): void;
  span(name: string, attrs?: Record<string, unknown>): Span;
  enabled(level: Level): boolean;
  withSpan(span: Span | undefined): ScopedTracer;
}
```

### How Breadcrumbs Flow Through the Pipeline

This section addresses the full contract from breadcrumb capture to error output.

**1. Global state in `trace.ts` with safe defaults:**

The module initializes with safe defaults so that `tracer()` calls before `configure()` work
correctly (they emit to console at `off` level — effectively silent, but breadcrumbs still
capture errors). This is critical because `resolver.ts` and `confirm.ts` create scoped tracers
at module load time, before `boot()` calls `configure()`.

```typescript
// trace.ts — module-level state with safe defaults
let breadcrumbs = new BreadcrumbBuffer(64);
let rootSpan: Span = NOOP_SPAN;
let activeSink: TraceSink = new ConsoleSink();
let activeLevel: number = LEVELS.off;

function configure(config: TraceConfig): void {
  activeLevel = LEVELS[config.level ?? "off"];
  activeSink = config.sink ?? new ConsoleSink();
  breadcrumbs = new BreadcrumbBuffer(config.breadcrumbCapacity ?? 64);
  rootSpan = config.traceparent
    ? spanFromTraceparent(config.traceparent)
    : NOOP_SPAN;
}
```

**2. Every ScopedTracer method pushes to breadcrumbs AND emits to sink:**

```typescript
function createScopedTracer(scope: string, boundSpan?: Span): ScopedTracer {
  function emitEvent(level: Level, event: string, data?: Record<string, unknown>, err?: Error): void {
    // Always push to breadcrumbs for error/warn (even when tracing is off)
    const crumb: Breadcrumb = { time: performance.now(), event, scope, level, data };
    if (LEVELS[level] <= LEVELS.warn || LEVELS[level] <= activeLevel) {
      breadcrumbs.push(crumb);
    }

    // Early exit if level is below active threshold
    if (LEVELS[level] > activeLevel) return;

    const traceEvent: TraceEvent = {
      time: performance.now(),
      event,
      scope,
      level,
      severityNumber: SEVERITY[level],
      data,
      error: err ? serializeError(err) : undefined,
      traceId: boundSpan?.traceId,
      spanId: boundSpan?.spanId,
      // Breadcrumbs attached ONLY on error events
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
      // TwP: rootSpan is ContextOnlySpan — propagate trace context without collecting
      if (rootSpan instanceof ContextOnlySpan) {
        return (boundSpan ?? rootSpan).child(name);
      }
      // Full tracing: create ActiveSpan for span collection
      if (activeLevel >= LEVELS.debug) {
        return new ActiveSpan(name, scope, (boundSpan ?? rootSpan) as ActiveSpan, activeSink, attrs);
      }
      // No trace context, no tracing: noop
      return NOOP_SPAN;
    },
    enabled: (level) => LEVELS[level] <= activeLevel,
    withSpan: (span) => createScopedTracer(scope, span ?? NOOP_SPAN),
  };
}
```

**3. ScopedTracer.withSpan() binds trace context:**

Call sites that have access to `ctx.span` bind it to their tracer:

```typescript
// In trigger.ts:
const t = tracer("trigger");

function runReaction(reaction: Reaction, plan: Plan, ctx: ExecContext): void {
  const scoped = t.withSpan(ctx.span);  // binds traceId/spanId to all events
  try {
    const result = executeReaction(reaction, plan, ctx);
    if (result instanceof Promise) {
      result.catch(err => scoped.error("reaction.fail", {
        trigger: describeTrigger(trigger),
        planId: plan.planId,
      }, err as Error));
    }
  } catch (err) {
    scoped.error("reaction.fail", {
      trigger: describeTrigger(trigger),
      planId: plan.planId,
    }, err as Error);
  }
}
```

**4. ConsoleSink.emit() renders traceId, spanId, and breadcrumbs:**

See ConsoleSink section below.

### Span Implementation

**ID generation** via `crypto.getRandomValues`:

```typescript
function generateTraceId(): string {
  const bytes = new Uint8Array(16);
  crypto.getRandomValues(bytes);
  return Array.from(bytes, b => b.toString(16).padStart(2, "0")).join("");
}

function generateSpanId(): string {
  const bytes = new Uint8Array(8);
  crypto.getRandomValues(bytes);
  return Array.from(bytes, b => b.toString(16).padStart(2, "0")).join("");
}
```

**Traceparent format** per W3C spec:

```typescript
function formatTraceparent(traceId: string, spanId: string, flags: string = "01"): string {
  return `00-${traceId}-${spanId}-${flags}`;
}

function parseTraceparent(header: string): { traceId: string; spanId: string; flags: string } | undefined {
  const parts = header.split("-");
  if (parts.length !== 4 || parts[0] !== "00") return undefined;
  if (parts[1].length !== 32 || parts[2].length !== 16 || parts[3].length !== 2) return undefined;
  return { traceId: parts[1], spanId: parts[2], flags: parts[3] };
}
```

Flags are preserved as the raw 2-hex-char string from the incoming traceparent. This avoids
lossy boolean collapse — the W3C spec reserves bits 1-7 for future use. The runtime passes
the original flags through to outgoing requests unchanged.

**NoopSpan** (returned when tracing is off or level threshold not met):

```typescript
const NOOP_SPAN: Span = Object.freeze({
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
```

**TraceRoot** (immutable trace context, created once per trace):

Trace flags are an inherited property of the trace root, not a per-span concern.
Every span in a trace shares the same TraceRoot. Flags are invisible to consumers.

```typescript
interface TraceRoot {
  readonly traceId: string;
  readonly flags: string;   // 2-hex-char W3C trace-flags, preserved from server
}
```

**ContextOnlySpan** (for TwP mode: propagates traceparent without collecting spans):

When tracing level is `off` but a server `traceparent` was provided, the runtime still needs
to propagate the trace-id in outgoing fetch requests. `ContextOnlySpan` carries the trace
root and generates span-ids for propagation, but does NOT emit to the sink:

```typescript
class ContextOnlySpan implements Span {
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
  end(): void {}  // Does NOT emit to sink
  traceparent(): string { return formatTraceparent(this.root.traceId, this.spanId, this.root.flags); }
}
```

**spanFromTraceparent** (creates appropriate span type based on tracing state):

```typescript
function spanFromTraceparent(header: string): Span {
  const parsed = parseTraceparent(header);
  if (!parsed) return NOOP_SPAN;

  const root: TraceRoot = { traceId: parsed.traceId, flags: parsed.flags };

  // TwP: even when tracing is off, propagate the server's trace context
  if (activeLevel === LEVELS.off) {
    return new ContextOnlySpan(root, parsed.spanId);
  }

  // Full tracing: create an ActiveSpan rooted at the server's trace
  return new ActiveSpan("boot", "boot", undefined, activeSink, undefined, root);
}
```

**ActiveSpan** (real implementation when tracing is on):

```typescript
class ActiveSpan implements Span {
  readonly traceId: string;
  readonly spanId: string;
  readonly parentSpanId?: string;
  readonly name: string;
  readonly startTime: number;
  private readonly scope: string;
  private readonly root: TraceRoot;
  private readonly attributes: Record<string, string | number | boolean> = {};
  private readonly events: Array<{ name: string; time: number; attributes?: Record<string, unknown> }> = [];
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
    this.events.push({ name, time: performance.now(), attributes: attrs });
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
      events: [...this.events],
    });
  }

  traceparent(): string {
    return formatTraceparent(this.root.traceId, this.spanId, this.root.flags);
  }
}
```

### Breadcrumb Ring Buffer

```typescript
class BreadcrumbBuffer {
  private readonly buffer: Breadcrumb[];
  private readonly capacity: number;
  private head = 0;
  private count = 0;

  constructor(capacity: number = 64) {
    this.capacity = capacity;
    this.buffer = new Array(capacity);
  }

  push(crumb: Breadcrumb): void {
    this.buffer[this.head] = crumb;
    this.head = (this.head + 1) % this.capacity;
    if (this.count < this.capacity) this.count++;
  }

  snapshot(): readonly Breadcrumb[] {
    if (this.count < this.capacity) {
      return this.buffer.slice(0, this.count);
    }
    return [...this.buffer.slice(this.head), ...this.buffer.slice(0, this.head)];
  }

  clear(): void {
    this.head = 0;
    this.count = 0;
  }
}
```

**Capture rules:**
- When level is `off`: breadcrumbs still capture `error` and `warn` events. These are too
  valuable to lose in production for a senior living framework.
- When level is `error` or higher: breadcrumbs capture everything at or above the active level.
- Buffer is lightweight: 64 entries at ~128 bytes each = ~8KB worst case.

### ConsoleSink (Rich Formatting)

```typescript
class ConsoleSink implements TraceSink {
  emit(event: TraceEvent): void {
    const tag = `%c[alis:${event.scope}]%c ${event.event} %c${event.level.toUpperCase()}`;
    const styles = [
      "color:#6366f1;font-weight:bold",
      "color:inherit",
      this.levelColor(event.level),
    ];

    // Build args: tag + styles + data + error
    const args: unknown[] = [tag, ...styles];
    if (event.data) args.push(event.data);
    if (event.error) args.push(event.error);

    // Route to correct console method
    switch (event.level) {
      case "error": console.error(...args); break;
      case "warn":  console.warn(...args); break;
      case "info":  console.info(...args); break;
      default:      console.log(...args); break;
    }

    // Trace context (when present)
    if (event.traceId) {
      console.log(`  trace: ${event.traceId}  span: ${event.spanId}`);
    }

    // Breadcrumbs on error — displayed as console.table
    if (event.level === "error" && event.breadcrumbs && event.breadcrumbs.length > 0) {
      console.groupCollapsed(`  breadcrumbs (${event.breadcrumbs.length})`);
      console.table(event.breadcrumbs.map(b => ({
        time: b.time.toFixed(1),
        event: b.event,
        scope: b.scope,
        level: b.level,
      })));
      console.groupEnd();
    }
  }

  span(data: SpanData): void {
    const status = data.status === "error" ? " ERROR" : "";
    const label = `[alis:${data.scope}] ${data.name}  ${data.durationMs.toFixed(1)}ms${status}`;
    console.groupCollapsed(label);
    if (Object.keys(data.attributes).length > 0) {
      console.table(data.attributes);
    }
    if (data.events.length > 0) {
      console.table(data.events.map(e => ({
        event: e.name,
        offset_ms: (e.time - data.startTime).toFixed(1),
        ...e.attributes,
      })));
    }
    console.groupEnd();
  }

  flush(): void { /* Console sink has nothing to flush */ }

  private levelColor(level: Level): string {
    switch (level) {
      case "error": return "color:#ef4444;font-weight:bold";
      case "warn":  return "color:#f59e0b";
      case "info":  return "color:#3b82f6";
      case "debug": return "color:#6b7280";
      case "trace": return "color:#9ca3af";
      default:      return "color:inherit";
    }
  }
}
```

### Level Resolution

Checked once at boot, in priority order:

```typescript
function resolveLevel(plan: { traceLevel?: string }, el: HTMLElement): Level {
  const sources = [
    plan.traceLevel,
    el.dataset.trace,
    new URLSearchParams(location.search).get("trace"),
    typeof localStorage !== "undefined" ? localStorage.getItem("alis.trace") : null,
  ];
  for (const s of sources) {
    if (s && isValidLevel(s)) return s as Level;
  }
  return "off";
}

function isValidLevel(s: string): s is Level {
  return s === "off" || s === "error" || s === "warn" || s === "info" || s === "debug" || s === "trace";
}
```

### Plan Type Extension

The `Plan` type in `Scripts/types/plan.ts` gets two optional fields:

```typescript
interface Plan {
  version: 3;
  planId: string;
  partId?: string;
  traceparent?: string;     // NEW: W3C traceparent from server's Activity.Current.Id
  traceLevel?: string;      // NEW: server-controlled trace level for this plan
  types: Record<string, JsType>;
  components: Record<string, Component>;
  behaviors: Behavior[];
}
```

These are **optional** fields. The JSON schema (`reactive-plan.schema.json`) must be updated
to include them. This is a Layer 1→2 boundary crossing driven by a failing `AssertSchemaValid()`
test. The C# `Plan.Render()` method serializes `Activity.Current?.Id` when available.

### Context Propagation Through ExecContext

```typescript
// Enhanced ExecContext:
interface ExecContext {
  readonly event?: unknown;
  readonly response?: unknown;
  readonly request?: unknown;
  readonly local?: Record<string, unknown>;
  readonly span?: Span;            // NEW: active span for this execution branch
}
```

**Flow through execution tree:**

1. `boot()` creates root span from `plan.traceparent` (via `spanFromTraceparent`).
   If traceparent exists but level is `off`, creates `ContextOnlySpan` for TwP propagation.
   If no traceparent and level is `off`, uses `NOOP_SPAN`.
2. `wireBehavior()` captures root span in closure.
3. Event trigger creates child span: `ctx = { ...ctx, span: rootSpan.child("trigger.page-ready") }`.
4. `executeReaction()` receives ctx with span, creates children for sub-reactions.
5. `executeParallel()` creates sibling child spans (one per step, from parent).
6. `executeRequest()` injects `traceparent` header in outgoing fetch.
7. Span ends when reaction completes (or errors).

### HTTP Request Correlation (Including TwP)

```typescript
// In http.ts, before fetch():
const requestSpan = ctx?.span?.child("http.request", {
  "http.method": req.method,
  "http.url": resolved.url,
});

const headers: Record<string, string> = { ...(init.headers as Record<string, string> ?? {}) };

// TwP: inject traceparent even when span is ContextOnlySpan (tracing off but server
// trace context present). Only skip if span is NOOP_SPAN (no trace context at all).
const tp = requestSpan?.traceparent();
if (tp && !tp.startsWith("00-" + "0".repeat(32))) {
  headers["traceparent"] = tp;
}

const response = await fetch(url, { ...init, headers });
requestSpan?.set("http.status", response.status);
requestSpan?.end(response.ok ? "ok" : "error");
```

**TwP behavior:** When the server provides `traceparent` in the plan JSON but tracing level
is `off`, the runtime still propagates trace context in outgoing fetch headers via
`ContextOnlySpan`. The server receives the traceparent, creates child spans, and the
distributed trace is complete — without any browser-side span collection or console output.
This matches Sentry's Tracing Without Performance pattern (research Finding 7).

### Error Handling Strategy

**Two levels of error context enhancement:**

**Level 1: Throw-site message quality.** The 47 existing `throw new Error("[alis] ...")`
statements are fail-fast assertions at the point of failure. They stay as throw statements
(not rewritten to use the tracing module). However, their messages are enhanced to include
"what's available" context where the information is readily accessible:

| Current throw message | Enhanced throw message |
|----------------------|----------------------|
| `[alis] component not found: ${key}` | `[alis] component not found: "${key}" (available: ${Object.keys(plan.components).join(", ")})` |
| `[alis] property "${prop}" not found on type` | `[alis] property "${prop}" not found on type "${jsType.name}" (available: ${Object.keys(jsType.properties ?? {}).join(", ")})` |
| `[alis] method "${method}" not found on type` | `[alis] method "${method}" not found on type "${jsType.name}" (available: ${Object.keys(jsType.methods ?? {}).join(", ")})` |
| `[alis] element not found: ${comp.id}` | `[alis] element not found: "${comp.id}" (component: ${key}, vendor: ${comp.vendor})` |
| `[alis] trigger component not found: ${trigger.component}` | `[alis] trigger component not found: "${trigger.component}" (available: ${Object.keys(plan.components).join(", ")})` |

**Why throw-sites stay as throws (not tracer calls):** These are framework fail-fast
assertions. They must throw synchronously to halt execution immediately. Coupling them to
the tracing module would make the tracing module a runtime dependency for error control flow,
violating SRP. The throws surface to the catch points where they get enriched with trace
context. The throw message itself is the "local" context; the catch point adds the "global"
context (breadcrumbs, trace-id, trigger, plan).

**Level 2: Catch-point enrichment.** The 5 catch points where errors surface to the
developer are enhanced with the tracing module. They emit structured error events with
breadcrumbs, trace context, and plan context automatically attached.

| Location | Event Name | Required Context Fields |
|----------|-----------|------------------------|
| `trigger.ts:19-26` (runReaction) | `reaction.fail` | trigger kind, trigger event/component, planId, error + stack, breadcrumbs (auto) |
| `execute.ts:187-188` (parallel) | `parallel.step.fail` | step index, parent reaction kind, error + stack, breadcrumbs (auto) |
| `http.ts:101-106` (fetch) | `http.request.fail` | method, url, status, error + stack, breadcrumbs (auto) |
| `root.ts:36-38` (parse) | `plan.parse.fail` | element id, content length, parse error + stack |
| `server-push.ts:96` (SSE reaction) | `sse.reaction.fail` | url, event, error + stack, breadcrumbs (auto) |
| `signalr.ts:134` (SignalR reaction) | `signalr.reaction.fail` | hubUrl, method, error + stack, breadcrumbs (auto) |
| `native-action-link.ts:46` (action link) | `action-link.fail` | anchor id, error + stack, breadcrumbs (auto) |

(Note: this is 7 catch points, not 5. The initial review correctly identified 5 in execute/trigger/http/root.
The full audit found 2 more in server-push.ts:96 and signalr.ts:134.)

### Helper: describeTrigger

```typescript
function describeTrigger(trigger: StartsWhen): string {
  switch (trigger.kind) {
    case "page-ready":       return "page-ready";
    case "document-event":   return `document-event:${trigger.event}`;
    case "component-event":  return `component-event:${trigger.component}.${trigger.event}`;
    case "server-push":      return `server-push:${trigger.url}/${trigger.event}`;
    case "signalr":          return `signalr:${trigger.hubUrl}/${trigger.method}`;
  }
}
```

### Message Quality Standards

Every trace event follows these rules:

1. **Event names are dotted, noun-first:** `boot.start`, `http.response`, `reaction.fail`,
   `component.resolve`, `gather.serialize.fail`. Not verbs, not past tense.

2. **Data fields use domain names:** `component` not `target`, `planId` not `id`, `field` not
   `name`, `method` not `m`.

3. **Error events include "which one?" identifiers:** Component not found? Include
   `component` (the missing key) AND `availableComponents` (what IS available). Property not
   found? Include `property`, `type`, and `availableProperties`.

4. **No `String(err)`.** Pass the `Error` object as third argument. The tracing module
   extracts `type`, `message`, `stack`, and `cause` chain via `serializeError()`.

5. **Breadcrumbs attached to every error event automatically.** The `emitEvent` function
   in `trace.ts` attaches `breadcrumbs.snapshot()` to every event with `level === "error"`.
   Call sites do not need to handle this.

6. **Structured data as expandable objects in console.** Pass objects as separate `console.*`
   arguments, not `JSON.stringify`. Chrome DevTools renders them as expandable trees.

7. **Event name style guide:**

   | Pattern | Example | When |
   |---------|---------|------|
   | `module.noun` | `boot.start`, `boot.complete` | Lifecycle events |
   | `module.noun.qualifier` | `http.request.send`, `http.request.fail` | Specific outcomes |
   | `module.noun.not-found` | `validation.container.not-found` | Missing resource |
   | `module.noun.qualifier` | `gather.serialize.fail` | Operation outcome |

   Avoid: past tense (`activated`, `initialized`), abbreviations (`req`, `res`), generic
   names (`error`, `data`, `info`).

### Complete Migration Map (66 Call Sites)

Old `core/trace.ts` is deleted. Every `import { scope } from "../core/trace"` becomes
`import { tracer } from "../tracing"`. Every `const log = scope("x")` becomes `const t = tracer("x")`.

#### lifecycle/boot.ts (3 sites)

| Line | Old | New |
|------|-----|-----|
| 25 | `log.info("booting", { behaviors: plan.behaviors.length })` | `t.info("boot.start", { planId: plan.planId, behaviors: plan.behaviors.length })` |
| 36 | `log.info("booted")` | `t.info("boot.complete", { planId: plan.planId })` |
| 74 | `log.info("merge", { planId, newComponents })` | `t.info("plan.merge", { planId, newComponents })` |

#### lifecycle/merge-plan.ts (2 sites)

| Line | Old | New |
|------|-----|-----|
| 59 | `log.error("cross-source type collision", { key, owner, incoming: partId })` | `t.error("merge.type.collision", { key, owner, incomingPartId: partId })` |
| 70 | `log.error("cross-source component collision", { key, owner, incoming: partId })` | `t.error("merge.component.collision", { key, owner, incomingPartId: partId })` |

#### execution/trigger.ts (4 sites)

| Line | Old | New |
|------|-----|-----|
| 22 | `log.error("reaction failed", { error: String(err) })` | `scoped.error("reaction.fail", { trigger: describeTrigger(trigger), planId: plan.planId }, err)` |
| 25 | `log.error("reaction failed (sync)", { error: String(err) })` | `scoped.error("reaction.fail", { trigger: describeTrigger(trigger), planId: plan.planId }, err)` |
| 49 | `log.debug("document-event: listening", { event })` | `t.debug("trigger.wired", { kind: "document-event", event: trigger.event })` |
| 64 | `log.debug("component-event", { component, event, channel })` | `t.debug("trigger.wired", { kind: "component-event", component: trigger.component, event: trigger.event, channel })` |

#### execution/execute.ts (6 sites)

| Line | Old | New |
|------|-----|-----|
| 147 | `log.trace("no-branch-taken")` | `t.trace("branch.no-match", { caseCount: reaction.cases.length })` |
| 171 | `log.trace("no-branch-taken")` | `t.trace("branch.no-match", { caseCount: reaction.cases.length })` |
| 188 | `log.error("parallel step failed", { error: String(r.reason) })` | `t.error("parallel.step.fail", { stepIndex: i }, r.reason as Error)` |
| 205 | `log.trace("set", { target, property, value })` | `t.trace("reaction.set", { component: target, property: reaction.property, value })` |
| 232 | `log.trace("call", { target, method, args })` | `t.trace("reaction.call", { component: target, method: reaction.method, args })` |
| 256 | `log.trace("dispatch", { event, detail })` | `t.trace("reaction.dispatch", { event: reaction.event, detail })` |

#### execution/http.ts (3 sites)

| Line | Old | New |
|------|-----|-----|
| 65 | `log.debug("validation failed, aborting request")` | `t.debug("http.validation.fail", { container: req.container })` |
| 85 | `log.debug("fetch", { method, url })` | `t.debug("http.request.send", { method: req.method, url: resolved.url })` |
| 103 | `log.error(status === 0 ? "network error" : "client error", ...)` | `t.error("http.request.fail", { url: req.url, method: req.method, status }, err)` |

#### execution/gather.ts (3 sites)

| Line | Old | New |
|------|-----|-----|
| 37 | `log.warn("gather serialize failed, using empty", { name, error })` | `t.warn("gather.serialize.fail", { field: name, error: result.error })` |
| 111 | `log.trace("file", { name, count })` | `t.trace("gather.file", { field: name, count: raw.length })` |
| 120 | `log.trace("gathered", { name, value })` | `t.trace("gather.value", { field: name, value: raw })` |

#### execution/server-push.ts (9 sites)

| Line | Old | New |
|------|-----|-----|
| 29 | `log.info("manual retry", { url })` | `t.info("sse.retry", { url })` |
| 45 | `log.debug("connected", { url })` | `t.debug("sse.connection.open", { url })` |
| 54 | `log.error("connection closed permanently", { url })` | `t.error("sse.connection.close", { url, permanent: true })` |
| 61 | `log.warn("connection error (reconnecting)", { url })` | `t.warn("sse.reconnect", { url })` |
| 73 | `log.debug("closed", { url })` | `t.debug("sse.connection.close", { url, permanent: false })` |
| 77 | `log.debug("created", { url })` | `t.debug("sse.connection.new", { url })` |
| 93 | `log.debug("message", { url, event })` | `t.debug("sse.message", { url: trigger.url, event: trigger.event })` |
| 96 | `result.catch(err => log.error("reaction failed", ...))` | `result.catch(err => scoped.error("sse.reaction.fail", { url: trigger.url, event: trigger.event }, err))` |
| 102 | `log.debug("listening", { url, event })` | `t.debug("sse.listen", { url: trigger.url, event: eventName })` |

#### execution/signalr.ts (15 sites)

| Line | Old | New |
|------|-----|-----|
| 29 | `log.info("connected", { hubUrl })` | `t.info("signalr.connection.open", { hubUrl })` |
| 33 | `log.warn("start failed, retrying", { hubUrl, attempt, delay, error })` | `t.warn("signalr.start.retry", { hubUrl, attempt: attempt + 1, delay }, err)` |
| 38 | `log.error("start failed after all retries", { hubUrl, attempts })` | `t.error("signalr.start.fail", { hubUrl, attempts: maxAttempts })` |
| 46 | `log.warn("retry requested but no connection found", { hubUrl })` | `t.warn("signalr.retry.no-connection", { hubUrl })` |
| 53 | `log.debug("retry skipped — not disconnected", { hubUrl, state })` | `t.debug("signalr.retry.skip", { hubUrl, state: connection.state })` |
| 57 | `log.info("manual retry", { hubUrl })` | `t.info("signalr.retry", { hubUrl })` |
| 71 | `if (level >= Warning) log.warn("lib", { message })` | `t.warn("signalr.lib", { message })` |
| 72 | `else if (level >= Info) log.debug("lib", { message })` | `t.debug("signalr.lib", { message })` |
| 80 | `log.warn("reconnecting", { hubUrl, error })` | `t.warn("signalr.reconnect", { hubUrl }, err ?? undefined)` |
| 84 | `log.info("reconnected", { hubUrl, connectionId })` | `t.info("signalr.connection.restore", { hubUrl, connectionId })` |
| 90 | `log.debug("stopped", { hubUrl })` | `t.debug("signalr.connection.stop", { hubUrl })` |
| 93 | `log.warn("disconnected", { hubUrl, error })` | `t.warn("signalr.connection.lost", { hubUrl }, err ?? undefined)` |
| 131 | `log.debug("method", { hubUrl, method })` | `t.debug("signalr.method", { hubUrl: trigger.hubUrl, method: trigger.method })` |
| 134 | `result.catch(err => log.error("reaction failed", ...))` | `result.catch(err => scoped.error("signalr.reaction.fail", { hubUrl: trigger.hubUrl, method: trigger.method }, err))` |
| 138 | `log.debug("listening", { hubUrl, method })` | `t.debug("signalr.listen", { hubUrl: trigger.hubUrl, method: trigger.method })` |

#### execution/retry-indicator.ts (4 sites)

| Line | Old | New |
|------|-----|-----|
| 22 | `log.warn("target not found", { key, id })` | `t.warn("retry.target.not-found", { key, id })` |
| 46 | `log.info("shown", { key, placed })` | `t.info("retry.indicator.show", { key, placed: anchored.size })` |
| 48 | `log.error("no indicators placed", { key, targets })` | `t.error("retry.placement.fail", { key, targets: [...targetIds] })` |
| 55 | `if (icons.length > 0) log.debug("removed", { key })` | `if (icons.length > 0) t.debug("retry.indicator.clear", { key })` |

#### resolution/resolver.ts (1 site)

| Line | Old | New |
|------|-----|-----|
| 195 | `log.debug("loaded")` | `t.debug("resolver.ready", {})` |

#### conditions/conditions.ts (3 sites)

| Line | Old | New |
|------|-----|-----|
| 39 | `log.warn("ConfirmCondition in sync context — denying")` | `t.warn("condition.confirm.sync-denied", {})` |
| 102 | `log.trace("eval", { op, left, right })` | `t.trace("condition.eval", { op: cond.op, left: shapedLeft, right: shapedRight })` |
| 151 | `log.warn("invalid condition regex", { operand })` | `t.warn("condition.regex.invalid", { operand: shapedRight })` |

#### validation/orchestrator.ts (9 sites)

| Line | Old | New |
|------|-----|-----|
| 30 | `log.warn("validate: container component not found", { containerKey })` | `t.warn("validation.container.not-found", { container: containerKey })` |
| 36 | `log.warn("validate: component has no container scope", { containerKey })` | `t.warn("validation.container.no-scope", { container: containerKey })` |
| 47 | `log.warn("validate: form container missing, blocking", { containerId })` | `t.warn("validation.form.not-found", { containerId })` |
| 74 | `log.debug("validate", { containerId, valid })` | `t.debug("validation.complete", { containerId, valid })` |
| 111 | `log.debug("showServerErrors", { containerId, fieldCount })` | `t.debug("validation.server-errors", { containerId, fieldCount: Object.keys(errors).length })` |
| 166 | `log.trace("component-not-found", { component })` | `t.trace("validation.component.not-found", { component: cv.component })` |
| 187 | `log.trace("field outside form, skipping", { component, containerId })` | `t.trace("validation.field.outside-form", { component: cv.component, containerId })` |
| 227 | `log.trace("rule-fail", { component, rule, value, message })` | `t.trace("validation.rule.fail", { component: cv.component, rule: rule.name, value, message: rule.message })` |
| 312 | `log.warn("showServerErrors: response is not ProblemDetails shape")` | `t.warn("validation.server-errors.invalid-shape", {})` |

#### components/fusion/confirm.ts (2 sites)

| Line | Old | New |
|------|-----|-----|
| 41 | `log.warn("confirm element not found", { id })` | `t.warn("confirm.element.not-found", { id: ELEMENT_ID })` |
| 63 | `log.info("initialized")` | `t.info("confirm.ready", {})` |

#### components/native/native-action-link.ts (2 sites)

| Line | Old | New |
|------|-----|-----|
| 43 | `log.debug("activate", { id, href })` | `t.debug("action-link.start", { id: anchor.id, href: anchor.href })` |
| 46 | `result.catch(err => log.error("reaction failed", ...))` | `result.catch(err => t.error("action-link.fail", { id: anchor.id }, err))` |

**Total: 66 call sites across 14 files.**

### Performance Budget

| Component | Size Estimate (minified, uncompressed) |
|-----------|---------------------------------------|
| types.ts | ~300 bytes (type-only erased, SEVERITY const retained) |
| span.ts (ActiveSpan + ContextOnlySpan + NoopSpan + ID gen) | ~1000 bytes |
| trace.ts (ScopedTracer + configure) | ~500 bytes |
| breadcrumbs.ts (ring buffer) | ~300 bytes |
| sink.ts (ConsoleSink) | ~600 bytes |
| context.ts (traceparent parse/format + level resolve) | ~300 bytes |
| index.ts (barrel) | ~50 bytes |
| **Total** | **~3.0KB** |

Current runtime: 93KB. Addition: ~3.0KB (3.2% increase). Acceptable.

### Near-Zero-Cost-When-Off Analysis

**Honest assessment of cost when `level = off`:**

| Operation | Cost | Explanation |
|-----------|------|-------------|
| `t.debug("event", { ... })` | ~0.001ms | JS evaluates function arguments before the call. One small object allocation + one integer comparison + return. The early-exit guard prevents: string formatting, console output, sink dispatch, JSON serialization. |
| `t.span("name", { ... })` | ~0.0001ms | Returns `NOOP_SPAN` (frozen singleton). `attrs` object still allocated. |
| `NOOP_SPAN.child(...)` | ~0 | Returns `NOOP_SPAN`. V8 inlines. |
| `NOOP_SPAN.set/event/end(...)` | ~0 | No-op. V8 inlines. |
| `NOOP_SPAN.traceparent()` | ~0 | Returns constant string. |
| `t.enabled("debug")` | ~0 | Integer comparison only. |
| Breadcrumb push (error/warn) | ~0.001ms | One Breadcrumb object per error/warn. Deliberate: production errors are rare and valuable. |
| TwP traceparent propagation | ~0.01ms | `ContextOnlySpan` generates one span-id per outgoing fetch. No sink emission. |

**Hot-path guard:** For call sites inside tight loops (e.g., condition evaluation that runs
per-field), callers use `t.enabled()` to avoid the object allocation entirely:

```typescript
if (t.enabled("trace")) {
  t.trace("condition.eval", { op: cond.op, left: shapedLeft, right: shapedRight });
}
```

`t.enabled()` is a single integer comparison. When it returns `false`, no argument object
is allocated, no function is called. This is the standard pattern used by Java's SLF4J
(`log.isDebugEnabled()`), Go's slog (`slog.Debug` with lazy evaluation), and Rust's tracing
(`tracing::enabled!` macro).

### Test Strategy

| Test Type | What It Covers |
|-----------|---------------|
| vitest: trace.test.ts | ScopedTracer emits correct events, level gating, event-name format, breadcrumb auto-attach on error |
| vitest: span.test.ts | ActiveSpan lifecycle, ContextOnlySpan propagation, NoopSpan returns self, traceparent format, ID uniqueness, child span hierarchy |
| vitest: breadcrumbs.test.ts | Ring buffer overflow, snapshot ordering, capacity limits, clear |
| vitest: sink.test.ts | ConsoleSink routes to correct console methods (error/warn/info/log), CSS styling, groupCollapsed for spans, table for breadcrumbs |
| vitest: context.test.ts | parseTraceparent valid/invalid/malformed, resolveLevel priority order, isValidLevel |
| vitest: integration.test.ts | Full flow: configure → tracer → emit → sink receives correct TraceEvent with severityNumber, traceId, breadcrumbs |
| vitest: twp.test.ts | TwP mode: tracing off + traceparent present → ContextOnlySpan ��� traceparent() returns valid non-zero header |
| vitest: migration.test.ts | Spot-check: representative migrated call sites emit expected event name + data shape |
| Playwright (optional) | Console output visible in browser for a sandbox page with tracing on |

### Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| Scope creep in unified module | SOLID TS audit skill during implementation. Each file has one responsibility. |
| Parallel branches sharing span references | `executeParallel` creates one child span per step from parent. Each step gets `{ ...ctx, span: childSpan }`. |
| Breadcrumb memory in long-running SSE/SignalR pages | Fixed-size ring buffer (64). Old entries overwritten. Configurable capacity. Entries are lightweight (~128 bytes). |
| `performance.now()` reduced resolution (Spectre mitigation) | 5us precision is sufficient for reaction-level spans. Not sub-microsecond. |
| Console CSS styling differs across browsers | CSS styles are progressive enhancement. Plain text fallback is always readable. |
| Breaking change in ExecContext | `span` field is optional. Existing code unaffected. `ctx?.span` evaluates to `undefined`. |
| Plan type extension | `traceparent` and `traceLevel` are optional fields. Existing plans without them work unchanged. Schema update driven by failing `AssertSchemaValid()` test. |
| Object allocation on trace calls when off | Documented honestly. ~0.001ms per call. Mitigated by `t.enabled()` guard for hot paths. Same tradeoff as SLF4J, slog, sentry. |

