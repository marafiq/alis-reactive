# OTel Tracing Research for Alis.Reactive TS Runtime

**Date:** 2026-04-12
**Branch:** tracing-module-otel-standard-as-first-class-citizen-in-ts-runtime
**Scope:** Layer 3 (TS Runtime) with Layer 1 integration point (C# plan carries traceparent)

## Executive Summary

The current `core/trace.ts` (38 lines) is a level-gated console logger. The research
concludes: **do NOT import the full OTel SDK** (~60KB gzipped, "not optimized for browsers"
per the OTel Browser SIG). Instead, build a lightweight, W3C-compliant tracing module that
follows OTel data models in ~1-2KB of code with zero external dependencies.

## Current State Inventory

| Metric | Count |
|--------|-------|
| `log.*` calls across runtime | 61 |
| Throw statements with `[alis]` prefix | 38 |
| Files using trace.ts | 18 |
| Trace module size | 38 lines |
| Correlation IDs | 0 |
| Spans / context propagation | 0 |
| Performance metrics | 0 |
| W3C traceparent implementation | 0 (documented as aspiration) |

### Current trace.ts Architecture

```typescript
// 5 levels: off(0), error(1), warn(2), info(3), debug(4), trace(5)
// API: scope(name) returns Logger { error, warn, info, debug, trace }
// Output: [alis:scope] message {JSON.stringify(data)}
// Dispatch: error→console.error, warn→console.warn, else→console.log
// Guard: if (level > active) return  ← zero-cost when off
```

Level control: `data-trace` attribute on `<script data-reactive-plan>` element,
or `trace.setLevel(level)` at runtime.

## Research Findings

### Finding 1: No Full OTel SDK — Bundle Cost Prohibitive

| Approach | Bundle Impact (gzipped) | Verdict |
|----------|------------------------|---------|
| @opentelemetry/sdk-trace-web | ~60KB | REJECT — 65% increase on 92KB runtime |
| @opentelemetry/api only | ~20KB (uncompressed) | REJECT — designed for Node.js async hooks |
| Custom W3C-compliant | ~1-2KB | ADOPT — zero dependencies, OTel-compatible output |

**Evidence:**
- OTel Browser SIG: "Client instrumentation for the browser is experimental and mostly
  unspecified." The existing JS SDK "has not been optimized for the browser."
- OTel-JS Issue #4817: "Bundle size too large for js-web"
- OTel-JS Discussion #3714: Users report 300KB parse JS from OTel alone
- The `semver` dependency alone adds ~30KB

**Sources:**
- https://opentelemetry.io/blog/2025/otel-js-sdk-2-0/
- https://github.com/open-telemetry/opentelemetry-browser
- https://github.com/open-telemetry/opentelemetry-js/issues/4817
- https://signoz.io/blog/reduce-opentelemetry-bundle-size-for-browser-frontend/

### Finding 2: OTel Is Deprecating Span Events

OTel announced deprecation of the Span Events API. The community is converging on:
events are logs emitted via the Logs API, correlated with traces through context.

**Implication:** The existing `log.*` approach (structured log records with scope tags)
is the forward-looking pattern. Enhance it with trace context correlation rather than
building a span event tree.

**Source:** https://opentelemetry.io/blog/2026/deprecating-span-events/

### Finding 3: W3C Trace Context — The Wire Format

```
traceparent: {version}-{trace-id}-{span-id}-{trace-flags}
             00       - 32 hex   - 16 hex  - 2 hex

Example: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
```

- **trace-id**: 16 bytes (32 hex). Globally unique, randomly generated.
- **span-id**: 8 bytes (16 hex). Unique per span within the trace.
- **trace-flags**: bit 0 = sampled (01 = yes, 00 = no).
- Generated via `crypto.getRandomValues()` — supported in all modern browsers.

**Sources:**
- https://www.w3.org/TR/trace-context/
- https://www.w3.org/TR/trace-context-2/

### Finding 4: Server → Browser Trace Propagation

The established pattern (Sentry, Elastic RUM, Datadog):

1. C# server embeds `Activity.Current.Id` (W3C traceparent) in the plan JSON during `Render()`.
2. Browser runtime reads `plan.traceparent` at boot, extracts the trace-id.
3. All browser-side spans share the same trace-id, with new span-ids.
4. Outgoing `fetch()` requests carry `traceparent` header → server creates child spans.

**For this runtime:** The plan JSON already carries all information the runtime needs.
Adding `traceparent` to the plan JSON is the natural extension — no separate `<meta>` tag.

**Sources:**
- https://docs.sentry.io/platforms/javascript/tracing/distributed-tracing/
- https://tracetest.io/blog/propagating-the-opentelemetry-context-from-the-browser-to-the-backend
- https://www.elastic.co/docs/reference/apm/agents/rum-js/distributed-tracing

### Finding 5: ExecContext IS OTel Context Propagation

The existing `ExecContext` pattern (passing `ctx` through every function call) is exactly
how OTel context propagation works — minus the framework overhead. No Zone.js, no
AsyncContext (TC39 Stage 2, not shipping), no implicit propagation needed.

```typescript
// Current: ctx flows through the call chain
executeReaction(reaction, plan, ctx)
  → executeSequence(reaction, plan, ctx)
    → executeSet(reaction, plan, ctx)

// Enhancement: ctx carries the active span
interface ExecContext {
  readonly event?: unknown;
  readonly response?: unknown;
  readonly span?: Span;  // NEW: active span for this execution branch
}
```

**Why this works:**
- Event listener closures capture boot trace context
- Promise chains capture ctx in closures
- Explicit, debuggable, zero magic

### Finding 6: Noop Span Pattern — Zero Cost When Off

When tracing is disabled, return a frozen NOOP_SPAN object. V8 inlines noop functions.

```typescript
const NOOP_SPAN: Span = Object.freeze({
  traceId: "00000000000000000000000000000000",
  spanId: "0000000000000000",
  name: "",
  child: () => NOOP_SPAN,
  addEvent: () => {},
  setAttribute: () => {},
  end: () => {},
  traceparent: () => "00-00000000000000000000000000000000-0000000000000000-00",
});
```

Cost when tracing is off: one function call returning a frozen object. No allocations,
no string formatting, no timestamp capture.

**Source:** https://github.com/opentracing/opentracing-javascript (noop pattern)

### Finding 7: Sentry's "Tracing Without Performance" (TwP)

Sentry's TwP mode propagates trace context (traceparent headers on outgoing fetch)
WITHOUT collecting or sending span data. Minimum setup: < 20KB gzipped.

**Key insight:** Context propagation and span collection are independent concerns.
The runtime can propagate traceparent headers to the server even when browser-side
span collection is off. This gives server-side traces the browser correlation for free.

**Sources:**
- https://develop.sentry.dev/sdk/telemetry/traces/tracing-without-performance/
- https://blog.sentry.io/javascript-sdk-package-reduced/

### Finding 8: Breadcrumb Ring Buffer

Sentry's breadcrumb model: a fixed-size circular buffer (e.g., 50 entries) capturing
the last N execution steps. When an error occurs, the buffer contents attach to the error.

For this runtime, the plan execution IS the breadcrumb trail. Each step (resolve object,
evaluate value, execute action) is a natural breadcrumb. The gap is correlating these
breadcrumbs to a single error when one occurs.

**Source:** https://develop.sentry.dev/sdk/data-model/event-payloads/breadcrumbs/

### Finding 9: OTel Severity Levels

| Level | OTel Number Range | Console Mapping |
|-------|------------------|-----------------|
| TRACE | 1-4 | console.log |
| DEBUG | 5-8 | console.log |
| INFO | 9-12 | console.info |
| WARN | 13-16 | console.warn |
| ERROR | 17-20 | console.error |
| FATAL | 21-24 | console.error |

Current `trace.ts` maps correctly (minus FATAL, which is excluded by design).

### Finding 10: Span Hierarchy for Plan Execution

```
Root span: boot(planId)                     ← trace-id from server's traceparent
├── wire-behaviors                          ← span
│   ├── wire: page-ready → sequence
│   └── wire: component-event(DDL, change)
│
├── [trigger: page-ready]                   ← child span of boot
│   └── sequence                            ← parent span
│       ├── set                             ← child span 1
│       ├── call                            ← child span 2
│       └── request                         ← child span 3 (async)
│           ├── gather                      ← event on request span
│           ├── fetch                       ← event: traceparent injected
│           └── response                    ← event: status, duration
│
├── [trigger: DDL change]                   ← child span of boot
│   └── branch                              ← span, attribute: taken=case[0]
│       └── set                             ← child span (taken branch only)
│
└── [trigger: parallel]
    └── parallel                            ← parent span
        ├── request A                       ← sibling child
        └── request B                       ← sibling child
```

### Finding 11: Grafana Faro's Modular Architecture

Faro separates core observability from OTel tracing:
- `@grafana/faro-core`: Logs, errors, events, metrics — no OTel dependency
- `@grafana/faro-web-tracing`: OTel tracing — separate package due to bundle size

**Implication:** The tracing module should be designed as a layer ON TOP of the existing
trace.ts, not a replacement. Logs and spans are separate concerns that correlate through
shared trace context.

**Source:** https://grafana.com/docs/grafana-cloud/monitor-applications/frontend-observability/architecture/

### Finding 12: Runtime Control Patterns

| Pattern | Use Case | Current Support |
|---------|----------|-----------------|
| `data-trace` attribute | Server controls level per request | YES |
| URL query param `?trace=debug` | Developer debugging | NO |
| localStorage flag | Persistent across reloads | NO |
| Console API `window.__alis.trace.setLevel()` | DevTools toggling | Partial (boot.trace) |
| Plan JSON `traceLevel` field | Server-side per-plan control | NO |

## Design Principles (Derived from Research)

1. **Zero external dependencies.** Everything from `crypto.getRandomValues()` and
   `performance.now()`. The W3C spec is a wire format, not a library.

2. **Zero cost when off.** Noop span pattern + early-exit guard. No allocations,
   no string formatting when `level = off`.

3. **OTel-compatible, not OTel-dependent.** Follow the data model (trace-id, span-id,
   severity numbers, semantic conventions). Export in OTLP JSON if needed later.

4. **Context propagation through ExecContext.** No Zone.js. No implicit magic.
   The existing manual passing pattern is the correct architectural choice.

5. **Logs and spans are separate concerns.** Logs (the existing `log.*` calls) correlate
   with spans through shared trace context. They are not replaced by spans.

6. **Server → browser continuity.** C# embeds traceparent in plan JSON. Browser
   propagates the same trace-id in outgoing fetch requests. Full distributed trace.

7. **Breadcrumbs for error context.** A ring buffer captures the last N execution
   steps. When an error occurs, the full path is attached. Self-documenting for
   both developers and LLMs.

8. **Structured output, not string soup.** Every log/span emits structured JSON with
   consistent fields. Searchable, filterable, parseable by any log aggregator.

## What the Existing trace.ts Gets Right

- Zero-dependency, 38 lines, negligible bundle impact
- Level-gated with early-exit guard (zero-cost when off)
- Scoped loggers with `[alis:scope]` prefix
- Structured data as second argument to all log calls
- Console-level routing (error→console.error, warn→console.warn)

## What Needs Enhancement (Ordered by Impact)

1. **Span lifecycle** — start/end spans with trace-id, span-id, parent-child relationships
2. **Traceparent propagation** — read from plan JSON, inject into outgoing fetch headers
3. **Breadcrumb ring buffer** — capture last N steps, attach to errors on failure
4. **Structured error context** — errors carry trace-id, span-id, breadcrumbs, plan context
5. **performance.now() for timing** — microsecond precision for span durations
6. **Configurable sink** — console (default), OTLP JSON exporter, navigator.sendBeacon, noop
7. **Runtime control** — URL param, localStorage, console API in addition to data-trace
8. **OTel semantic conventions** — use standard attribute names for exceptions, HTTP, etc.
