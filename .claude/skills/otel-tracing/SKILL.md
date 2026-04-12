---
name: otel-tracing
description: Implements the OTel tracing module for the TS runtime. Use when writing, modifying, or reviewing any file under Scripts/tracing/, when migrating log.* call sites to the new tracer API, or when adding trace instrumentation to runtime modules.
---

# OTel Tracing Module

## Overview

Unified tracing module under `Scripts/tracing/` that replaces `core/trace.ts`. Zero external
dependencies. W3C traceparent propagation. OTel severity numbers. Pluggable sink. Breadcrumb
ring buffer. Near-zero cost when off.

**Spec (authoritative):** `docs/superpowers/specs/2026-04-12-otel-tracing-module-design.md`
**Research:** `docs/superpowers/research/2026-04-12-otel-tracing-research.md`
**Full 66-call migration map:** spec section "Complete Migration Map"

## When to Use

- Writing any file under `Scripts/tracing/`
- Migrating a `log.*` call site to the new `tracer()` API
- Adding span instrumentation to execution paths
- Reviewing trace output quality (event names, data fields, breadcrumbs)
- Wiring traceparent into HTTP requests
- Modifying `ExecContext`, `Plan` types, `boot.ts`, `trigger.ts`, `execute.ts`, or `http.ts`

## Module Structure

```
Scripts/tracing/
├── index.ts          ← barrel: tracer(), configure(), flush()
├── trace.ts          ← ScopedTracer factory, global state, emitEvent pipeline
├── span.ts           ← ActiveSpan, ContextOnlySpan, NoopSpan, TraceRoot, ID gen
├── breadcrumbs.ts    ← BreadcrumbBuffer ring buffer
├── sink.ts           ← TraceSink interface + ConsoleSink
├── context.ts        ← resolveLevel, parseTraceparent, formatTraceparent
└── types.ts          ← all type definitions (zero imports)
```

Each file has one responsibility. `types.ts` has zero imports. All others import from `types.ts`.

## Exact Type Definitions

These types are the contract. Implementation must match exactly.

```typescript
// ─── types.ts ───────────────────────────────────────────────

type Level = "off" | "error" | "warn" | "info" | "debug" | "trace";

const LEVELS: Record<Level, number> = {
  off: 0, error: 1, warn: 2, info: 3, debug: 4, trace: 5,
};

const SEVERITY: Record<Exclude<Level, "off">, number> = {
  error: 17, warn: 13, info: 9, debug: 5, trace: 1,
};

interface TraceEvent {
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

interface SpanData {
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
  breadcrumbCapacity?: number;
  traceparent?: string;
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

interface TraceRoot {
  readonly traceId: string;
  readonly flags: string;
}
```

## Public API Usage

```typescript
import { tracer, configure, flush } from "../tracing";

// Configure once at boot
configure({ level: "debug", traceparent: plan.traceparent });

// Scoped tracer per module
const t = tracer("http");

// Structured events
t.error("http.request.fail", { url, method, status: 500 }, err);
t.debug("http.request.send", { method: "POST", url });

// Hot-path guard
if (t.enabled("trace")) {
  t.trace("condition.eval", { op, left, right });
}

// Spans with bulk attributes
const span = t.span("http.request", { "http.method": "POST", "http.url": url });
span.set("http.status", 200);
span.end("ok");

// Bind active span for trace context correlation
const scoped = t.withSpan(ctx.span);
scoped.error("reaction.fail", { trigger, planId }, err);
```

## Runtime Integration Points

Beyond `Scripts/tracing/`, these existing files must be modified:

### 1. Plan type extension (`Scripts/types/plan.ts`)

Add optional fields to `Plan`:

```typescript
interface Plan {
  // existing fields...
  traceparent?: string;   // W3C traceparent from server Activity.Current.Id
  traceLevel?: string;    // server-controlled trace level
}
```

### 2. ExecContext extension (`Scripts/types/context.ts`)

Add span field:

```typescript
interface ExecContext {
  readonly event?: unknown;
  readonly response?: unknown;
  readonly request?: unknown;
  readonly local?: Record<string, unknown>;
  readonly span?: Span;   // active span for this execution branch
}
```

### 3. Boot integration (`Scripts/root.ts` + `Scripts/lifecycle/boot.ts`)

- `root.ts`: after parsing plan JSON, call `configure()` with `plan.traceparent` and resolved level
- `boot.ts`: create root span from configured traceparent, pass through `wireBehaviors`

### 4. Trigger integration (`Scripts/execution/trigger.ts`)

- `wireBehavior`: create child span per event trigger, pass via `ctx`
- `runReaction`: use `t.withSpan(ctx.span)` for error correlation

### 5. Execution integration (`Scripts/execution/execute.ts`)

- `executeSequence`: pass `ctx.span` through each step
- `executeParallel`: create one child span per step from parent:

```typescript
reaction.steps.map((s, i) => {
  const childSpan = ctx?.span?.child(`parallel.step[${i}]`);
  const childCtx = { ...ctx, span: childSpan };
  return executeReaction(s, plan, childCtx);
});
```

- `executeBranch`: record which case was taken as span attribute

### 6. HTTP integration (`Scripts/execution/http.ts`)

- Create child span for request
- Inject traceparent header (TwP: inject even when tracing is off):

```typescript
const tp = requestSpan?.traceparent();
if (tp && !tp.startsWith("00-" + "0".repeat(32))) {
  headers["traceparent"] = tp;
}
```

- Record status on span, end with ok/error

### 7. Flush wiring

```typescript
// In root.ts or boot.ts:
window.addEventListener("beforeunload", () => flush());
```

## Event Naming Rules

| Rule | Example | Anti-pattern |
|------|---------|-------------|
| Dotted, noun-first | `boot.start`, `http.request.send` | `booting`, `sendingRequest` |
| Base verb form, no past tense | `reaction.fail`, `trigger.wire` | `reaction.failed`, `trigger.wired` |
| Compound adjective states OK | `validation.container.not-found` | (describes state, not action) |
| Domain names for fields | `component`, `planId`, `field` | `target`, `id`, `name` |
| Error events: include "which one?" | `{ component, availableComponents }` | `{ key }` |
| No `String(err)` | Pass `Error` as 3rd arg | `{ error: String(err) }` |

**Full 66-call migration map** with exact old→new mappings: see spec section "Complete Migration Map".
Every event name and data payload is defined there. Do not invent names — look them up.

## Span Rules

1. **TraceRoot owns flags.** Flags come from server traceparent, inherited by all spans via
   parent chain. Never passed as constructor parameter.

2. **Three span types:**

   | Type | When | Emits to sink | Propagates traceparent |
   |------|------|--------------|----------------------|
   | `ActiveSpan` | Tracing on (level >= debug) | Yes, on `end()` | Yes |
   | `ContextOnlySpan` | Tracing off + server traceparent present | No | Yes (TwP) |
   | `NOOP_SPAN` | No trace context, no tracing | No | No (zero string) |

3. **`t.span()` logic:**
   - If `rootSpan instanceof ContextOnlySpan` → return `ContextOnlySpan` child (TwP)
   - If `activeLevel >= LEVELS.debug` → return `ActiveSpan` child
   - Otherwise → return `NOOP_SPAN`

4. **Parallel branches:** Each step gets its own child span. Never share a span across branches.

## Breadcrumb Rules

1. **Always capture error/warn** even when tracing is off.
2. **Auto-attach to error events.** `emitEvent` in `trace.ts` attaches `breadcrumbs.snapshot()`
   to every `TraceEvent` with `level === "error"`. Call sites never handle this.
3. **Ring buffer** with configurable capacity (default 64). Old entries overwritten.
4. **Lightweight entries:** time + event name + scope + level + optional data.

## Error Handling — Two Levels

### Level 1: Throw-site message enhancement (47 throws)

Stay as `throw new Error("[alis] ...")`. Enhance messages with "what's available":

```typescript
// Before:
throw new Error(`[alis] component not found: ${key}`);
// After:
throw new Error(`[alis] component not found: "${key}" (available: ${Object.keys(plan.components).join(", ")})`);
```

### Level 2: Catch-point enrichment (7 sites)

Each catch point emits a structured error event with specific required context fields:

| Location | Event | Required fields |
|----------|-------|----------------|
| `trigger.ts` runReaction async | `reaction.fail` | trigger (via describeTrigger), planId, error+stack, breadcrumbs (auto) |
| `trigger.ts` runReaction sync | `reaction.fail` | trigger (via describeTrigger), planId, error+stack, breadcrumbs (auto) |
| `execute.ts` parallel | `parallel.step.fail` | stepIndex, parent reaction kind, error+stack, breadcrumbs (auto) |
| `http.ts` fetch | `http.request.fail` | method, url, status, error+stack, breadcrumbs (auto) |
| `root.ts` parse | `plan.parse.fail` | element id, content length, parse error+stack |
| `server-push.ts` SSE | `sse.reaction.fail` | url, event, error+stack, breadcrumbs (auto) |
| `signalr.ts` SignalR | `signalr.reaction.fail` | hubUrl, method, error+stack, breadcrumbs (auto) |
| `native-action-link.ts` action link | `action-link.fail` | anchor id, error+stack, breadcrumbs (auto) |

**Helper:**
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

## ConsoleSink Formatting

| Level | Console method | CSS color |
|-------|---------------|-----------|
| error | `console.error` | #ef4444 bold |
| warn | `console.warn` | #f59e0b |
| info | `console.info` | #3b82f6 |
| debug | `console.log` | #6b7280 |
| trace | `console.log` | #9ca3af |

- Spans: `console.groupCollapsed` with duration
- Span attributes: `console.table`
- Breadcrumbs on error: `console.table` inside `console.groupCollapsed`
- Data objects: pass as separate args (expandable in DevTools), never `JSON.stringify`

## Safe Defaults

Module initializes with safe defaults before `configure()`:

```typescript
let breadcrumbs = new BreadcrumbBuffer(64);
let rootSpan: Span = NOOP_SPAN;
let activeSink: TraceSink = new ConsoleSink();
let activeLevel: number = LEVELS.off;
```

Critical: `resolver.ts` and `confirm.ts` create scoped tracers at module load time,
before `boot()` calls `configure()`.

## Level Resolution (priority order)

1. `plan.traceLevel` — server controls per-request
2. `el.dataset.trace` — `data-trace` attribute on script element
3. URL query param `?trace=debug`
4. `localStorage.getItem("alis.trace")`
5. Default: `"off"`

## Test Requirements

Vitest config: `vitest.config.ts` in repo root. Include pattern:
`Alis.Reactive.SandboxApp/Scripts/__tests__/**/*.test.ts`. Tests go in
`Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/`.

| Source | Test file | Key assertions |
|--------|-----------|----------------|
| types.ts | types.test.ts | LEVELS ordering, SEVERITY mapping, type narrowing |
| trace.ts | trace.test.ts | Level gating, event format, breadcrumb auto-attach on error, withSpan binding, enabled() guard |
| span.ts | span.test.ts | ActiveSpan lifecycle + end() emits to sink, ContextOnlySpan TwP propagation, NoopSpan self-return on child/set/event/end, ID uniqueness (100 IDs, all distinct), traceparent format W3C-compliant, TraceRoot flag inheritance |
| breadcrumbs.ts | breadcrumbs.test.ts | Push/snapshot ordering, overflow wraps correctly, capacity limit, clear resets, snapshot returns chronological order |
| sink.ts | sink.test.ts | ConsoleSink routes error→console.error, warn→console.warn, info→console.info, debug/trace→console.log. Span uses groupCollapsed. Error events render breadcrumbs via table. |
| context.ts | context.test.ts | parseTraceparent valid/invalid/malformed, flags preserved as raw hex, isValidLevel accepts valid levels and rejects invalid, resolveLevel priority order (plan > data-trace > URL > localStorage > off) |
| integration | integration.test.ts | Full configure → tracer → emit → sink receives correct TraceEvent with severityNumber + traceId + breadcrumbs |
| TwP | twp.test.ts | Tracing off + traceparent present → ContextOnlySpan → traceparent() returns valid non-zero W3C header |
| migration | migration.test.ts | Spot-check: representative migrated call sites emit expected event name + data shape |

## Migration Checklist (per file)

When migrating a file from `core/trace.ts` to `tracing/`:

1. Replace `import { scope } from "../core/trace"` with `import { tracer } from "../tracing"`
2. Replace `const log = scope("x")` with `const t = tracer("x")`
3. Look up the exact event name and data fields for each call site in the spec migration map
4. Replace each `log.level("msg", { data })` with `t.level("event.name", { data })` using the spec mapping
5. Replace `String(err)` with Error object as 3rd argument
6. At catch points (7 sites): use `t.withSpan(ctx.span)` and include ALL required context fields from the catch-point table above
7. For `execute.ts` and `trigger.ts`: thread `ctx.span` through, create child spans per spec flow
8. Verify: `npm run typecheck` passes
9. Verify: `npm test` passes (all vitest)
10. Verify: console output in browser DevTools looks correct

## Review Checklist

Before submitting any tracing code for review:

- [ ] Event names match spec migration map exactly (look them up, do not invent)
- [ ] Error events include ALL required context fields from catch-point table
- [ ] No `String(err)` — Error objects passed as 3rd arg
- [ ] Breadcrumbs auto-attach on error (not manually)
- [ ] `t.enabled()` guard on hot-path trace calls (condition.eval, gather.value)
- [ ] Parallel branches create own child spans
- [ ] TwP: traceparent injected in fetch even when tracing is off
- [ ] `Plan` type has `traceparent?` and `traceLevel?` fields
- [ ] `ExecContext` has `span?` field
- [ ] Safe defaults initialize before `configure()`
- [ ] `npm run typecheck` passes
- [ ] `npm test` passes (all vitest)
- [ ] ConsoleSink output verified in browser DevTools
