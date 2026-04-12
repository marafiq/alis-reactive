---
name: otel-tracing
description: Implements the OTel tracing module for the TS runtime. Use when writing, modifying, or reviewing any file under Scripts/tracing/, when migrating log.* call sites to the new tracer API, or when adding trace instrumentation to runtime modules.
---

# OTel Tracing Module

## Overview

Unified tracing module under `Scripts/tracing/` that replaces `core/trace.ts`. Zero external
dependencies. W3C traceparent propagation. OTel severity numbers. Pluggable sink. Breadcrumb
ring buffer. Near-zero cost when off.

**Spec:** `docs/superpowers/specs/2026-04-12-otel-tracing-module-design.md`
**Research:** `docs/superpowers/research/2026-04-12-otel-tracing-research.md`

## When to Use

- Writing any file under `Scripts/tracing/`
- Migrating a `log.*` call site to the new `tracer()` API
- Adding span instrumentation to execution paths
- Reviewing trace output quality (event names, data fields, breadcrumbs)
- Wiring traceparent into HTTP requests

## Module Structure

```
Scripts/tracing/
├── index.ts          ← barrel: tracer(), configure(), flush()
├── trace.ts          ← ScopedTracer factory, global state, emitEvent pipeline
├── span.ts           ← ActiveSpan, ContextOnlySpan, NoopSpan, TraceRoot, ID gen
├── breadcrumbs.ts    ← BreadcrumbBuffer ring buffer
├── sink.ts           ← TraceSink interface + ConsoleSink
├── context.ts        ← resolveLevel, parseTraceparent, formatTraceparent
└── types.ts          ← TraceEvent, SpanData, Breadcrumb, Level, Span, ScopedTracer, TraceSink
```

Each file has one responsibility. `types.ts` has zero imports. All others import from `types.ts`.

## Public API Contract

```typescript
import { tracer, configure, flush } from "../tracing";

// Configure once at boot
configure({ level: "debug", traceparent: plan.traceparent });

// Scoped tracer per module
const t = tracer("http");

// Structured events (base verb form, dotted, noun-first)
t.error("http.request.fail", { url, method, status: 500 }, err);
t.warn("gather.serialize.fail", { field: name, error: result.error });
t.info("boot.start", { planId, behaviors: plan.behaviors.length });
t.debug("http.request.send", { method: "POST", url });
t.trace("reaction.set", { component, property, value });

// Hot-path guard
if (t.enabled("trace")) {
  t.trace("condition.eval", { op, left, right });
}

// Spans
const span = t.span("http.request", { "http.method": "POST", "http.url": url });
span.set("http.status", 200);
span.end("ok");

// Bind active span for trace context correlation
const scoped = t.withSpan(ctx.span);
scoped.error("reaction.fail", { trigger, planId }, err);
```

## Event Naming Rules

| Rule | Example | Anti-pattern |
|------|---------|-------------|
| Dotted, noun-first | `boot.start`, `http.request.send` | `booting`, `sendingRequest` |
| Base verb form, no past tense | `reaction.fail`, `trigger.wire` | `reaction.failed`, `trigger.wired` |
| Compound adjective states OK | `validation.container.not-found` | (these describe state, not action) |
| Domain names for fields | `component`, `planId`, `field` | `target`, `id`, `name` |
| Error events include "which one?" | `{ component, availableComponents }` | `{ key }` |
| No `String(err)` | Pass `Error` as 3rd arg | `{ error: String(err) }` |

## Span Rules

1. **TraceRoot owns flags.** Flags come from server traceparent, inherited by all spans. Never
   passed as constructor parameter. Encapsulated in `TraceRoot`.

2. **Three span types:**
   - `ActiveSpan` — full tracing on. Creates span data, emits to sink on `end()`.
   - `ContextOnlySpan` — TwP mode. Tracing off but server traceparent present. Propagates
     trace-id in outgoing fetch, does NOT emit to sink.
   - `NOOP_SPAN` — frozen singleton. No trace context. Zero cost.

3. **Span creation via `t.span()`:**
   - If `rootSpan` is `ContextOnlySpan` → returns `ContextOnlySpan` child (TwP propagation)
   - If tracing level >= debug → returns `ActiveSpan` child
   - Otherwise → returns `NOOP_SPAN`

4. **Parallel branches:** Each step gets its own child span from the parent. Never share a
   span reference across parallel branches.

```typescript
// In executeParallel:
reaction.steps.map((s, i) => {
  const childSpan = parentSpan?.child(`parallel.step[${i}]`);
  const childCtx = { ...ctx, span: childSpan };
  return executeReaction(s, plan, childCtx);
});
```

## Breadcrumb Rules

1. **Always capture error/warn** even when tracing is off. Production errors are rare and
   the breadcrumb trail is too valuable to lose.
2. **Auto-attach to error events.** `emitEvent` in `trace.ts` attaches `breadcrumbs.snapshot()`
   to every `TraceEvent` with `level === "error"`. Call sites never handle this.
3. **Ring buffer** with configurable capacity (default 64). Old entries overwritten.
4. **Lightweight entries:** time + event name + scope + level + optional data.

## Migration Checklist (per file)

When migrating a file from `core/trace.ts` to `tracing/`:

1. Replace `import { scope } from "../core/trace"` with `import { tracer } from "../tracing"`
2. Replace `const log = scope("x")` with `const t = tracer("x")`
3. Replace each `log.level("msg", { data })` with `t.level("event.name", { data })`
4. Apply event naming rules (base verb, dotted, noun-first)
5. Replace `String(err)` with Error object as 3rd argument
6. Add domain-specific context fields (component, planId, containerId, etc.)
7. At catch points: use `t.withSpan(ctx.span)` to bind trace context
8. Verify: `npm run typecheck` passes after migration

## Error Handling Strategy

**Two levels:**

**Level 1 — Throw sites (47 throws):** Stay as `throw new Error("[alis] ...")`. Enhance
messages to include "what's available" context where accessible:
```typescript
// Before: throw new Error(`[alis] component not found: ${key}`);
// After:  throw new Error(`[alis] component not found: "${key}" (available: ${Object.keys(plan.components).join(", ")})`);
```

**Level 2 — Catch points (7 sites):** Emit structured error events via tracer with
breadcrumbs auto-attached:

| Location | Event |
|----------|-------|
| trigger.ts runReaction (async + sync) | `reaction.fail` |
| execute.ts parallel | `parallel.step.fail` |
| http.ts fetch | `http.request.fail` |
| root.ts parse | `plan.parse.fail` |
| server-push.ts | `sse.reaction.fail` |
| signalr.ts | `signalr.reaction.fail` |
| native-action-link.ts | `action-link.fail` |

## HTTP Traceparent Injection

```typescript
// In http.ts before fetch():
const tp = requestSpan?.traceparent();
if (tp && !tp.startsWith("00-" + "0".repeat(32))) {
  headers["traceparent"] = tp;
}
```

TwP: inject traceparent even when tracing is off (ContextOnlySpan). Only skip for NOOP_SPAN.

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

This is critical: `resolver.ts` and `confirm.ts` create scoped tracers at module load time,
before `boot()` calls `configure()`.

## Level Resolution (priority order)

1. `plan.traceLevel` — server controls per-request
2. `el.dataset.trace` — `data-trace` attribute on script element
3. URL query param `?trace=debug`
4. `localStorage.getItem("alis.trace")`
5. Default: `"off"`

## Test Requirements

Every tracing module file gets its own vitest file:

| Source | Test | Key assertions |
|--------|------|----------------|
| trace.ts | trace.test.ts | Level gating, event format, breadcrumb auto-attach, withSpan binding |
| span.ts | span.test.ts | ActiveSpan lifecycle, ContextOnlySpan TwP, NoopSpan self-return, ID uniqueness, traceparent format |
| breadcrumbs.ts | breadcrumbs.test.ts | Overflow, snapshot order, capacity, clear |
| sink.ts | sink.test.ts | Console method routing, CSS, groupCollapsed, table |
| context.ts | context.test.ts | parseTraceparent valid/invalid, resolveLevel priority |
| integration | integration.test.ts | Full configure -> tracer -> emit -> sink pipeline |
| twp | twp.test.ts | Tracing off + traceparent -> ContextOnlySpan -> valid traceparent() |

## Review Checklist

Before submitting any tracing code for review:

- [ ] Event names follow naming rules (base verb, dotted, no past tense)
- [ ] Error events include "which one?" identifiers
- [ ] No `String(err)` — Error objects passed as 3rd arg
- [ ] Breadcrumbs auto-attached on error (not manually)
- [ ] `t.enabled()` guard on hot-path trace calls
- [ ] Parallel branches create own child spans
- [ ] `npm run typecheck` passes
- [ ] `npm test` passes (all vitest)
- [ ] ConsoleSink output verified in browser DevTools
