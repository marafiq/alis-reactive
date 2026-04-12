# OTel Tracing Module Implementation Plan

> **For agentic workers:** REQUIRED SKILLS: Load `otel-tracing` + `modern-ts` skills before starting any task. Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Each task must pass Codex xhigh review before moving to the next.

**Goal:** Replace the 38-line `core/trace.ts` with a unified OTel-compatible tracing module under `Scripts/tracing/` — structured events, span lifecycle, breadcrumb ring buffer, pluggable sink, W3C traceparent propagation, and migration of all 66 log call sites.

**Architecture:** Bottom-up TDD. Build the tracing module first (types → breadcrumbs → context → span → sink → trace → barrel), test each file in isolation, then integrate into the runtime (Plan type → ExecContext → boot → migration → span instrumentation).

**Tech Stack:** TypeScript 5.8, Vitest 3.x + jsdom, esbuild ESM, `crypto.getRandomValues`, `performance.now()`

**Spec:** `docs/superpowers/specs/2026-04-12-otel-tracing-module-design.md`
**Skill:** `.claude/skills/otel-tracing/SKILL.md`

---

## File Structure

### New files (tracing module)

| File | Responsibility |
|------|---------------|
| `Scripts/tracing/types.ts` | Type definitions: Level, LEVELS, SEVERITY, TraceEvent, SpanData, Breadcrumb, TraceSink, TraceConfig, Span, ScopedTracer, TraceRoot |
| `Scripts/tracing/breadcrumbs.ts` | BreadcrumbBuffer ring buffer class |
| `Scripts/tracing/context.ts` | parseTraceparent, formatTraceparent, resolveLevel, isValidLevel |
| `Scripts/tracing/span.ts` | ActiveSpan, ContextOnlySpan, NOOP_SPAN, generateTraceId, generateSpanId |
| `Scripts/tracing/sink.ts` | TraceSink (re-export), ConsoleSink, serializeError |
| `Scripts/tracing/trace.ts` | createScopedTracer, configure, flush, global state |
| `Scripts/tracing/index.ts` | Barrel: tracer(), configure(), flush(), ConsoleSink |

### New files (tests)

| File | Tests for |
|------|-----------|
| `Scripts/__tests__/vitest.setup.ts` | Empty setup file (vitest.config.ts references it) |
| `Scripts/__tests__/tracing/types.test.ts` | LEVELS ordering, SEVERITY mapping |
| `Scripts/__tests__/tracing/breadcrumbs.test.ts` | Ring buffer behavior |
| `Scripts/__tests__/tracing/context.test.ts` | Traceparent parse/format, level resolution |
| `Scripts/__tests__/tracing/span.test.ts` | All 3 span types, ID generation |
| `Scripts/__tests__/tracing/sink.test.ts` | ConsoleSink routing |
| `Scripts/__tests__/tracing/trace.test.ts` | ScopedTracer, configure, breadcrumb auto-attach |
| `Scripts/__tests__/tracing/integration.test.ts` | Full pipeline |
| `Scripts/__tests__/tracing/twp.test.ts` | Tracing Without Performance mode |

### Modified files (runtime integration)

| File | Change |
|------|--------|
| `Scripts/types/plan.ts` | Add `traceparent?` and `traceLevel?` to Plan |
| `Scripts/types/context.ts` | Add `span?` to ExecContext |
| `Scripts/root.ts` | Replace trace import with configure(), flush wiring |
| `Scripts/lifecycle/boot.ts` | Replace scope/setLevel with tracer(), root span |
| `Scripts/lifecycle/merge-plan.ts` | Migrate 2 log calls |
| `Scripts/execution/trigger.ts` | Migrate 4 log calls, add span context to runReaction |
| `Scripts/execution/execute.ts` | Migrate 6 log calls, thread span through parallel |
| `Scripts/execution/http.ts` | Migrate 3 log calls, inject traceparent header |
| `Scripts/execution/gather.ts` | Migrate 3 log calls |
| `Scripts/execution/server-push.ts` | Migrate 9 log calls |
| `Scripts/execution/signalr.ts` | Migrate 15 log calls |
| `Scripts/execution/retry-indicator.ts` | Migrate 4 log calls |
| `Scripts/resolution/resolver.ts` | Migrate 1 log call |
| `Scripts/conditions/conditions.ts` | Migrate 3 log calls |
| `Scripts/validation/orchestrator.ts` | Migrate 9 log calls |
| `Scripts/components/fusion/confirm.ts` | Migrate 2 log calls |
| `Scripts/components/native/native-action-link.ts` | Migrate 2 log calls |

### Deleted files

| File | Reason |
|------|--------|
| `Scripts/core/trace.ts` | Replaced by `Scripts/tracing/` module |

All paths below are relative to `Alis.Reactive.SandboxApp/`.

---

### Task 0: Test Infrastructure

**Files:**
- Create: `Scripts/__tests__/vitest.setup.ts`

- [ ] **Step 1: Create empty vitest setup file**

```typescript
// Scripts/__tests__/vitest.setup.ts
// Vitest setup — jsdom environment configured in vitest.config.ts
```

- [ ] **Step 2: Verify vitest runs**

Run: `npm test`
Expected: 0 test files found (no error, clean exit)

- [ ] **Step 3: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/__tests__/vitest.setup.ts
git commit -m "chore: add vitest setup file for tracing module tests"
```

---

### Task 1: types.ts — Type Definitions

**Files:**
- Create: `Scripts/tracing/types.ts`
- Test: `Scripts/__tests__/tracing/types.test.ts`

- [ ] **Step 1: Write the failing test**

```typescript
// Scripts/__tests__/tracing/types.test.ts
import { describe, it, expect } from "vitest";
import { LEVELS, SEVERITY } from "../../tracing/types";

describe("LEVELS", () => {
  it("off is lowest (0)", () => {
    expect(LEVELS.off).toBe(0);
  });

  it("levels increase from error to trace", () => {
    expect(LEVELS.error).toBeLessThan(LEVELS.warn);
    expect(LEVELS.warn).toBeLessThan(LEVELS.info);
    expect(LEVELS.info).toBeLessThan(LEVELS.debug);
    expect(LEVELS.debug).toBeLessThan(LEVELS.trace);
  });

  it("has exactly 6 levels", () => {
    expect(Object.keys(LEVELS)).toHaveLength(6);
  });
});

describe("SEVERITY", () => {
  it("maps to OTel severity numbers", () => {
    expect(SEVERITY.error).toBe(17);
    expect(SEVERITY.warn).toBe(13);
    expect(SEVERITY.info).toBe(9);
    expect(SEVERITY.debug).toBe(5);
    expect(SEVERITY.trace).toBe(1);
  });

  it("does not include off", () => {
    expect("off" in SEVERITY).toBe(false);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- --reporter verbose`
Expected: FAIL — cannot find module `../../tracing/types`

- [ ] **Step 3: Write types.ts**

```typescript
// Scripts/tracing/types.ts
// ── Type definitions for the OTel tracing module ─────────────
// Zero imports. All other tracing files import from here.

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
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- --reporter verbose`
Expected: 3 tests PASS

- [ ] **Step 5: Typecheck**

Run: `npm run typecheck`
Expected: Clean (no errors related to tracing/)

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/tracing/types.ts Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/types.test.ts
git commit -m "feat(tracing): add type definitions — Level, LEVELS, SEVERITY, TraceEvent, Span, ScopedTracer"
```

---

### Task 2: breadcrumbs.ts — Ring Buffer

**Files:**
- Create: `Scripts/tracing/breadcrumbs.ts`
- Test: `Scripts/__tests__/tracing/breadcrumbs.test.ts`

- [ ] **Step 1: Write the failing test**

```typescript
// Scripts/__tests__/tracing/breadcrumbs.test.ts
import { describe, it, expect } from "vitest";
import { BreadcrumbBuffer } from "../../tracing/breadcrumbs";
import type { Breadcrumb } from "../../tracing/types";

function crumb(event: string): Breadcrumb {
  return { time: performance.now(), event, scope: "test", level: "info" };
}

describe("BreadcrumbBuffer", () => {
  it("returns empty snapshot when no items pushed", () => {
    const buf = new BreadcrumbBuffer(4);
    expect(buf.snapshot()).toEqual([]);
  });

  it("returns items in push order", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    const snap = buf.snapshot();
    expect(snap.map(b => b.event)).toEqual(["a", "b", "c"]);
  });

  it("overwrites oldest when capacity exceeded", () => {
    const buf = new BreadcrumbBuffer(3);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    buf.push(crumb("d")); // overwrites "a"
    const snap = buf.snapshot();
    expect(snap.map(b => b.event)).toEqual(["b", "c", "d"]);
  });

  it("wraps around multiple times", () => {
    const buf = new BreadcrumbBuffer(2);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.push(crumb("c"));
    buf.push(crumb("d"));
    buf.push(crumb("e"));
    const snap = buf.snapshot();
    expect(snap.map(b => b.event)).toEqual(["d", "e"]);
  });

  it("clear resets buffer", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.push(crumb("b"));
    buf.clear();
    expect(buf.snapshot()).toEqual([]);
  });

  it("works after clear and re-push", () => {
    const buf = new BreadcrumbBuffer(4);
    buf.push(crumb("a"));
    buf.clear();
    buf.push(crumb("b"));
    expect(buf.snapshot().map(b => b.event)).toEqual(["b"]);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- --reporter verbose`
Expected: FAIL — cannot find module `../../tracing/breadcrumbs`

- [ ] **Step 3: Write breadcrumbs.ts**

```typescript
// Scripts/tracing/breadcrumbs.ts
import type { Breadcrumb } from "./types";

export class BreadcrumbBuffer {
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
    if (this.count === 0) return [];
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

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- --reporter verbose`
Expected: 6 tests PASS (breadcrumbs) + 3 prior (types) = 9 total

- [ ] **Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/tracing/breadcrumbs.ts Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/breadcrumbs.test.ts
git commit -m "feat(tracing): add BreadcrumbBuffer ring buffer"
```

---

### Task 3: context.ts — Traceparent & Level Resolution

**Files:**
- Create: `Scripts/tracing/context.ts`
- Test: `Scripts/__tests__/tracing/context.test.ts`

- [ ] **Step 1: Write the failing test**

```typescript
// Scripts/__tests__/tracing/context.test.ts
import { describe, it, expect } from "vitest";
import { parseTraceparent, formatTraceparent, isValidLevel } from "../../tracing/context";

describe("parseTraceparent", () => {
  it("parses valid traceparent", () => {
    const result = parseTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01");
    expect(result).toEqual({
      traceId: "4bf92f3577b34da6a3ce929d0e0e4736",
      spanId: "00f067aa0ba902b7",
      flags: "01",
    });
  });

  it("preserves flags as raw hex", () => {
    const result = parseTraceparent("00-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa-bbbbbbbbbbbbbbbb-ff");
    expect(result?.flags).toBe("ff");
  });

  it("returns undefined for wrong version", () => {
    expect(parseTraceparent("01-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01")).toBeUndefined();
  });

  it("returns undefined for wrong trace-id length", () => {
    expect(parseTraceparent("00-4bf92f-00f067aa0ba902b7-01")).toBeUndefined();
  });

  it("returns undefined for wrong span-id length", () => {
    expect(parseTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f0-01")).toBeUndefined();
  });

  it("returns undefined for wrong flags length", () => {
    expect(parseTraceparent("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-1")).toBeUndefined();
  });

  it("returns undefined for empty string", () => {
    expect(parseTraceparent("")).toBeUndefined();
  });
});

describe("formatTraceparent", () => {
  it("formats with default flags", () => {
    expect(formatTraceparent("aaa", "bbb")).toBe("00-aaa-bbb-01");
  });

  it("formats with custom flags", () => {
    expect(formatTraceparent("aaa", "bbb", "ff")).toBe("00-aaa-bbb-ff");
  });
});

describe("isValidLevel", () => {
  it("accepts valid levels", () => {
    for (const level of ["off", "error", "warn", "info", "debug", "trace"]) {
      expect(isValidLevel(level)).toBe(true);
    }
  });

  it("rejects invalid levels", () => {
    expect(isValidLevel("verbose")).toBe(false);
    expect(isValidLevel("")).toBe(false);
    expect(isValidLevel("DEBUG")).toBe(false);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- --reporter verbose`
Expected: FAIL — cannot find module `../../tracing/context`

- [ ] **Step 3: Write context.ts**

```typescript
// Scripts/tracing/context.ts
import type { Level } from "./types";

const VALID_LEVELS: ReadonlySet<string> = new Set(["off", "error", "warn", "info", "debug", "trace"]);

export function isValidLevel(s: string): s is Level {
  return VALID_LEVELS.has(s);
}

export function parseTraceparent(header: string): { traceId: string; spanId: string; flags: string } | undefined {
  const parts = header.split("-");
  if (parts.length !== 4 || parts[0] !== "00") return undefined;
  if (parts[1].length !== 32 || parts[2].length !== 16 || parts[3].length !== 2) return undefined;
  return { traceId: parts[1], spanId: parts[2], flags: parts[3] };
}

export function formatTraceparent(traceId: string, spanId: string, flags: string = "01"): string {
  return `00-${traceId}-${spanId}-${flags}`;
}

export function resolveLevel(
  planTraceLevel: string | undefined,
  elDataTrace: string | undefined,
): Level {
  const sources = [
    planTraceLevel,
    elDataTrace,
    typeof location !== "undefined" ? new URLSearchParams(location.search).get("trace") : null,
    typeof localStorage !== "undefined" ? localStorage.getItem("alis.trace") : null,
  ];
  for (const s of sources) {
    if (s && isValidLevel(s)) return s;
  }
  return "off";
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- --reporter verbose`
Expected: All context tests PASS

- [ ] **Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/tracing/context.ts Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/context.test.ts
git commit -m "feat(tracing): add traceparent parse/format and level resolution"
```

---

### Task 4: span.ts — Span Lifecycle

**Files:**
- Create: `Scripts/tracing/span.ts`
- Test: `Scripts/__tests__/tracing/span.test.ts`

- [ ] **Step 1: Write the failing test**

```typescript
// Scripts/__tests__/tracing/span.test.ts
import { describe, it, expect, vi } from "vitest";
import { NOOP_SPAN, ActiveSpan, ContextOnlySpan, generateTraceId, generateSpanId } from "../../tracing/span";
import type { TraceSink, SpanData } from "../../tracing/types";

function mockSink(): TraceSink & { spans: SpanData[] } {
  const spans: SpanData[] = [];
  return {
    spans,
    emit: vi.fn(),
    span: (data: SpanData) => spans.push(data),
    flush: vi.fn(),
  };
}

describe("NOOP_SPAN", () => {
  it("child returns itself", () => {
    expect(NOOP_SPAN.child("x")).toBe(NOOP_SPAN);
  });

  it("set/event/end are no-ops", () => {
    NOOP_SPAN.set("k", "v");
    NOOP_SPAN.event("e");
    NOOP_SPAN.end();
    // no error thrown
  });

  it("traceparent returns zero string", () => {
    expect(NOOP_SPAN.traceparent()).toMatch(/^00-0{32}-0{16}-00$/);
  });

  it("has zero traceId and spanId", () => {
    expect(NOOP_SPAN.traceId).toBe("0".repeat(32));
    expect(NOOP_SPAN.spanId).toBe("0".repeat(16));
  });
});

describe("ActiveSpan", () => {
  it("generates unique spanId", () => {
    const sink = mockSink();
    const a = new ActiveSpan("a", "test", undefined, sink);
    const b = new ActiveSpan("b", "test", undefined, sink);
    expect(a.spanId).not.toBe(b.spanId);
  });

  it("inherits traceId from parent", () => {
    const sink = mockSink();
    const parent = new ActiveSpan("parent", "test", undefined, sink);
    const child = parent.child("child") as ActiveSpan;
    expect(child.traceId).toBe(parent.traceId);
    expect(child.parentSpanId).toBe(parent.spanId);
  });

  it("emits SpanData to sink on end()", () => {
    const sink = mockSink();
    const span = new ActiveSpan("test-span", "test", undefined, sink);
    span.set("key", "value");
    span.event("mid-point", { x: 1 });
    span.end("ok");
    expect(sink.spans).toHaveLength(1);
    expect(sink.spans[0].name).toBe("test-span");
    expect(sink.spans[0].status).toBe("ok");
    expect(sink.spans[0].attributes).toEqual({ key: "value" });
    expect(sink.spans[0].events).toHaveLength(1);
    expect(sink.spans[0].events[0].name).toBe("mid-point");
  });

  it("defaults status to unset", () => {
    const sink = mockSink();
    const span = new ActiveSpan("s", "test", undefined, sink);
    span.end();
    expect(sink.spans[0].status).toBe("unset");
  });

  it("traceparent format is W3C compliant", () => {
    const sink = mockSink();
    const span = new ActiveSpan("s", "test", undefined, sink);
    const tp = span.traceparent();
    expect(tp).toMatch(/^00-[0-9a-f]{32}-[0-9a-f]{16}-01$/);
  });
});

describe("ContextOnlySpan", () => {
  it("propagates traceId from root", () => {
    const root = { traceId: "a".repeat(32), flags: "01" };
    const span = new ContextOnlySpan(root);
    expect(span.traceId).toBe("a".repeat(32));
  });

  it("child propagates same traceId", () => {
    const root = { traceId: "b".repeat(32), flags: "ff" };
    const span = new ContextOnlySpan(root);
    const child = span.child("x");
    expect(child.traceId).toBe("b".repeat(32));
    expect(child.parentSpanId).toBe(span.spanId);
  });

  it("preserves flags in traceparent", () => {
    const root = { traceId: "c".repeat(32), flags: "ff" };
    const span = new ContextOnlySpan(root);
    expect(span.traceparent()).toContain("-ff");
  });

  it("end is a no-op (does not emit)", () => {
    const root = { traceId: "d".repeat(32), flags: "01" };
    const span = new ContextOnlySpan(root);
    span.end(); // should not throw
  });
});

describe("ID generation", () => {
  it("generateTraceId produces 32 hex chars", () => {
    const id = generateTraceId();
    expect(id).toMatch(/^[0-9a-f]{32}$/);
  });

  it("generateSpanId produces 16 hex chars", () => {
    const id = generateSpanId();
    expect(id).toMatch(/^[0-9a-f]{16}$/);
  });

  it("generates unique IDs", () => {
    const ids = new Set(Array.from({ length: 100 }, () => generateSpanId()));
    expect(ids.size).toBe(100);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- --reporter verbose`
Expected: FAIL — cannot find module `../../tracing/span`

- [ ] **Step 3: Write span.ts**

```typescript
// Scripts/tracing/span.ts
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
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- --reporter verbose`
Expected: All span tests PASS

- [ ] **Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/tracing/span.ts Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/span.test.ts
git commit -m "feat(tracing): add ActiveSpan, ContextOnlySpan, NOOP_SPAN, ID generation"
```

---

### Task 5: sink.ts — ConsoleSink

**Files:**
- Create: `Scripts/tracing/sink.ts`
- Test: `Scripts/__tests__/tracing/sink.test.ts`

- [ ] **Step 1: Write the failing test**

```typescript
// Scripts/__tests__/tracing/sink.test.ts
import { describe, it, expect, vi, beforeEach } from "vitest";
import { ConsoleSink, serializeError } from "../../tracing/sink";
import type { TraceEvent, SpanData } from "../../tracing/types";

describe("ConsoleSink", () => {
  const sink = new ConsoleSink();

  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("routes error events to console.error", () => {
    const spy = vi.spyOn(console, "error").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.fail", scope: "test", level: "error", severityNumber: 17,
    };
    sink.emit(event);
    expect(spy).toHaveBeenCalled();
  });

  it("routes warn events to console.warn", () => {
    const spy = vi.spyOn(console, "warn").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.warn", scope: "test", level: "warn", severityNumber: 13,
    };
    sink.emit(event);
    expect(spy).toHaveBeenCalled();
  });

  it("routes info events to console.info", () => {
    const spy = vi.spyOn(console, "info").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.info", scope: "test", level: "info", severityNumber: 9,
    };
    sink.emit(event);
    expect(spy).toHaveBeenCalled();
  });

  it("routes debug events to console.log", () => {
    const spy = vi.spyOn(console, "log").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.debug", scope: "test", level: "debug", severityNumber: 5,
    };
    sink.emit(event);
    expect(spy).toHaveBeenCalled();
  });

  it("renders breadcrumbs on error via console.table", () => {
    vi.spyOn(console, "error").mockImplementation(() => {});
    vi.spyOn(console, "groupCollapsed").mockImplementation(() => {});
    vi.spyOn(console, "groupEnd").mockImplementation(() => {});
    const tableSpy = vi.spyOn(console, "table").mockImplementation(() => {});
    const event: TraceEvent = {
      time: 0, event: "test.fail", scope: "test", level: "error", severityNumber: 17,
      breadcrumbs: [
        { time: 1, event: "a", scope: "s", level: "info" },
        { time: 2, event: "b", scope: "s", level: "debug" },
      ],
    };
    sink.emit(event);
    expect(tableSpy).toHaveBeenCalled();
  });

  it("uses groupCollapsed for spans", () => {
    const groupSpy = vi.spyOn(console, "groupCollapsed").mockImplementation(() => {});
    vi.spyOn(console, "groupEnd").mockImplementation(() => {});
    const spanData: SpanData = {
      traceId: "a".repeat(32), spanId: "b".repeat(16), name: "test",
      scope: "test", startTime: 0, endTime: 10, durationMs: 10,
      status: "ok", attributes: {}, events: [],
    };
    sink.span(spanData);
    expect(groupSpy).toHaveBeenCalled();
  });
});

describe("serializeError", () => {
  it("extracts type, message, stack", () => {
    const err = new TypeError("bad input");
    const result = serializeError(err);
    expect(result.type).toBe("TypeError");
    expect(result.message).toBe("bad input");
    expect(result.stack).toBeDefined();
  });

  it("serializes cause chain", () => {
    const cause = new Error("root cause");
    const err = new Error("wrapper", { cause });
    const result = serializeError(err);
    expect(result.cause).toContain("root cause");
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- --reporter verbose`
Expected: FAIL — cannot find module `../../tracing/sink`

- [ ] **Step 3: Write sink.ts**

```typescript
// Scripts/tracing/sink.ts
import type { TraceEvent, SpanData, Level } from "./types";

export function serializeError(err: Error): { type: string; message: string; stack?: string; cause?: string } {
  const result: { type: string; message: string; stack?: string; cause?: string } = {
    type: err.constructor.name,
    message: err.message,
    stack: err.stack,
  };
  if (err.cause instanceof Error) {
    result.cause = `${err.cause.constructor.name}: ${err.cause.message}`;
  }
  return result;
}

export class ConsoleSink {
  emit(event: TraceEvent): void {
    const tag = `%c[alis:${event.scope}]%c ${event.event} %c${event.level.toUpperCase()}`;
    const styles = [
      "color:#6366f1;font-weight:bold",
      "color:inherit",
      levelColor(event.level),
    ];

    const args: unknown[] = [tag, ...styles];
    if (event.data) args.push(event.data);
    if (event.error) args.push(event.error);

    switch (event.level) {
      case "error": console.error(...args); break;
      case "warn":  console.warn(...args); break;
      case "info":  console.info(...args); break;
      default:      console.log(...args); break;
    }

    if (event.traceId) {
      console.log(`  trace: ${event.traceId}  span: ${event.spanId}`);
    }

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

  flush(): void {}
}

function levelColor(level: Level): string {
  switch (level) {
    case "error": return "color:#ef4444;font-weight:bold";
    case "warn":  return "color:#f59e0b";
    case "info":  return "color:#3b82f6";
    case "debug": return "color:#6b7280";
    case "trace": return "color:#9ca3af";
    default:      return "color:inherit";
  }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- --reporter verbose`
Expected: All sink tests PASS

- [ ] **Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/tracing/sink.ts Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/sink.test.ts
git commit -m "feat(tracing): add ConsoleSink with rich formatting and serializeError"
```

---

### Task 6: trace.ts — ScopedTracer & Configure

**Files:**
- Create: `Scripts/tracing/trace.ts`
- Test: `Scripts/__tests__/tracing/trace.test.ts`

- [ ] **Step 1: Write the failing test**

```typescript
// Scripts/__tests__/tracing/trace.test.ts
import { describe, it, expect, vi, beforeEach } from "vitest";
import { createTracer, configure, resetForTests } from "../../tracing/trace";
import type { TraceEvent, TraceSink } from "../../tracing/types";

function captureSink(): TraceSink & { events: TraceEvent[] } {
  const events: TraceEvent[] = [];
  return {
    events,
    emit: (e: TraceEvent) => events.push(e),
    span: vi.fn(),
    flush: vi.fn(),
  };
}

describe("ScopedTracer", () => {
  beforeEach(() => resetForTests());

  it("emits event with correct scope and level", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("http");
    t.debug("http.request.send", { method: "POST" });
    expect(sink.events).toHaveLength(1);
    expect(sink.events[0].scope).toBe("http");
    expect(sink.events[0].event).toBe("http.request.send");
    expect(sink.events[0].level).toBe("debug");
    expect(sink.events[0].severityNumber).toBe(5);
    expect(sink.events[0].data).toEqual({ method: "POST" });
  });

  it("gates events below active level", () => {
    const sink = captureSink();
    configure({ level: "warn", sink });
    const t = createTracer("http");
    t.debug("http.request.send", {});
    t.info("boot.start", {});
    t.warn("gather.serialize.fail", {});
    expect(sink.events).toHaveLength(1);
    expect(sink.events[0].level).toBe("warn");
  });

  it("auto-attaches breadcrumbs on error events", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("test");
    t.debug("step.one", {});
    t.debug("step.two", {});
    t.error("step.fail", { reason: "bad" });
    const errorEvent = sink.events.find(e => e.level === "error");
    expect(errorEvent?.breadcrumbs).toBeDefined();
    expect(errorEvent!.breadcrumbs!.length).toBeGreaterThanOrEqual(2);
  });

  it("captures error/warn breadcrumbs even when level is off", () => {
    const sink = captureSink();
    configure({ level: "off", sink });
    const t = createTracer("test");
    t.error("first.fail", {});
    // When level is off, error events are not emitted to sink,
    // but breadcrumbs are still captured internally.
    // Verify by switching level on and triggering another error.
    configure({ level: "error", sink });
    const t2 = createTracer("test");
    t2.error("second.fail", {});
    const errorEvent = sink.events.find(e => e.event === "second.fail");
    expect(errorEvent?.breadcrumbs).toBeDefined();
    expect(errorEvent!.breadcrumbs!.some(b => b.event === "first.fail")).toBe(true);
  });

  it("enabled() returns correct boolean", () => {
    configure({ level: "warn" });
    const t = createTracer("test");
    expect(t.enabled("error")).toBe(true);
    expect(t.enabled("warn")).toBe(true);
    expect(t.enabled("info")).toBe(false);
    expect(t.enabled("debug")).toBe(false);
  });

  it("withSpan binds traceId and spanId", () => {
    const sink = captureSink();
    configure({ level: "debug", sink });
    const t = createTracer("test");
    const mockSpan = { traceId: "a".repeat(32), spanId: "b".repeat(16) };
    const scoped = t.withSpan(mockSpan as any);
    scoped.debug("test.event", {});
    expect(sink.events[0].traceId).toBe("a".repeat(32));
    expect(sink.events[0].spanId).toBe("b".repeat(16));
  });

  it("serializes Error on error()", () => {
    const sink = captureSink();
    configure({ level: "error", sink });
    const t = createTracer("test");
    t.error("test.fail", {}, new TypeError("bad"));
    expect(sink.events[0].error?.type).toBe("TypeError");
    expect(sink.events[0].error?.message).toBe("bad");
  });

  it("serializes Error on warn()", () => {
    const sink = captureSink();
    configure({ level: "warn", sink });
    const t = createTracer("test");
    t.warn("test.warn", {}, new Error("oops"));
    expect(sink.events[0].error?.type).toBe("Error");
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- --reporter verbose`
Expected: FAIL — cannot find module `../../tracing/trace`

- [ ] **Step 3: Write trace.ts**

```typescript
// Scripts/tracing/trace.ts
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
      traceId: boundSpan?.traceId,
      spanId: boundSpan?.spanId,
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
      if (rootSpan instanceof ContextOnlySpan) {
        return (boundSpan ?? rootSpan).child(name);
      }
      if (activeLevel >= LEVELS.debug) {
        return new ActiveSpan(name, scope, (boundSpan ?? rootSpan) as ActiveSpan | undefined, activeSink, attrs);
      }
      return NOOP_SPAN;
    },
    enabled: (level) => LEVELS[level] <= activeLevel,
    withSpan: (span) => buildScopedTracer(scope, span ?? NOOP_SPAN),
  };
}

/** Reset all global state for testing. */
export function resetForTests(): void {
  breadcrumbs = new BreadcrumbBuffer(64);
  rootSpan = NOOP_SPAN;
  activeSink = new ConsoleSink();
  activeLevel = LEVELS.off;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- --reporter verbose`
Expected: All trace tests PASS

- [ ] **Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/tracing/trace.ts Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/trace.test.ts
git commit -m "feat(tracing): add ScopedTracer, configure, emitEvent pipeline with breadcrumb auto-attach"
```

---

### Task 7: index.ts + Integration + TwP Tests

**Files:**
- Create: `Scripts/tracing/index.ts`
- Create: `Scripts/__tests__/tracing/integration.test.ts`
- Create: `Scripts/__tests__/tracing/twp.test.ts`

- [ ] **Step 1: Write index.ts barrel**

```typescript
// Scripts/tracing/index.ts
export { createTracer as tracer, configure, flush, getRootSpan } from "./trace";
export { ConsoleSink } from "./sink";
export { NOOP_SPAN } from "./span";
export type {
  Level, TraceEvent, SpanData, Breadcrumb, TraceSink,
  TraceConfig, Span, ScopedTracer, TraceRoot,
} from "./types";
export { LEVELS, SEVERITY } from "./types";
```

- [ ] **Step 2: Write integration test**

```typescript
// Scripts/__tests__/tracing/integration.test.ts
import { describe, it, expect, vi, beforeEach } from "vitest";
import { tracer, configure } from "../../tracing";
import { resetForTests } from "../../tracing/trace";
import type { TraceEvent, TraceSink } from "../../tracing/types";

describe("integration: full pipeline", () => {
  beforeEach(() => resetForTests());

  it("configure → tracer → emit → sink receives TraceEvent", () => {
    const events: TraceEvent[] = [];
    const sink: TraceSink = { emit: e => events.push(e), span: vi.fn(), flush: vi.fn() };
    configure({ level: "info", sink });

    const t = tracer("boot");
    t.info("boot.start", { planId: "test-plan", behaviors: 3 });

    expect(events).toHaveLength(1);
    expect(events[0].event).toBe("boot.start");
    expect(events[0].severityNumber).toBe(9);
    expect(events[0].data).toEqual({ planId: "test-plan", behaviors: 3 });
  });

  it("error event includes breadcrumb trail", () => {
    const events: TraceEvent[] = [];
    const sink: TraceSink = { emit: e => events.push(e), span: vi.fn(), flush: vi.fn() };
    configure({ level: "debug", sink });

    const t = tracer("execute");
    t.debug("reaction.set", { component: "DDL" });
    t.debug("reaction.call", { component: "DDL" });
    t.error("reaction.fail", { trigger: "component-event:DDL.change" }, new Error("prop not found"));

    const errorEvent = events.find(e => e.level === "error")!;
    expect(errorEvent.breadcrumbs!.length).toBeGreaterThanOrEqual(2);
    expect(errorEvent.error!.message).toBe("prop not found");
  });
});
```

- [ ] **Step 3: Write TwP test**

```typescript
// Scripts/__tests__/tracing/twp.test.ts
import { describe, it, expect, beforeEach } from "vitest";
import { tracer, configure } from "../../tracing";
import { resetForTests, getRootSpan } from "../../tracing/trace";
import { ContextOnlySpan } from "../../tracing/span";

describe("Tracing Without Performance (TwP)", () => {
  beforeEach(() => resetForTests());

  it("tracing off + traceparent → ContextOnlySpan root", () => {
    configure({
      level: "off",
      traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    });
    const root = getRootSpan();
    expect(root).toBeInstanceOf(ContextOnlySpan);
    expect(root.traceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
  });

  it("t.span() returns ContextOnlySpan child in TwP mode", () => {
    configure({
      level: "off",
      traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    });
    const t = tracer("http");
    const span = t.span("http.request");
    expect(span).toBeInstanceOf(ContextOnlySpan);
    expect(span.traceId).toBe("4bf92f3577b34da6a3ce929d0e0e4736");
  });

  it("ContextOnlySpan.traceparent() returns valid W3C header", () => {
    configure({
      level: "off",
      traceparent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    });
    const t = tracer("http");
    const span = t.span("http.request");
    const tp = span.traceparent();
    expect(tp).toMatch(/^00-4bf92f3577b34da6a3ce929d0e0e4736-[0-9a-f]{16}-01$/);
    expect(tp).not.toContain("0".repeat(32)); // not the noop zero string
  });

  it("tracing off + no traceparent → NOOP_SPAN", () => {
    configure({ level: "off" });
    const t = tracer("http");
    const span = t.span("http.request");
    expect(span.traceId).toBe("0".repeat(32));
  });
});
```

- [ ] **Step 4: Run all tests**

Run: `npm test -- --reporter verbose`
Expected: ALL tests PASS (types + breadcrumbs + context + span + sink + trace + integration + twp)

- [ ] **Step 5: Typecheck**

Run: `npm run typecheck`
Expected: Clean

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/tracing/index.ts Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/integration.test.ts Alis.Reactive.SandboxApp/Scripts/__tests__/tracing/twp.test.ts
git commit -m "feat(tracing): add barrel export, integration test, and TwP test"
```

---

### Task 8: Runtime Integration — Plan, ExecContext, Boot

**Files:**
- Modify: `Scripts/types/plan.ts:8-15`
- Modify: `Scripts/types/context.ts:4-26`
- Modify: `Scripts/root.ts` (full rewrite of trace wiring)
- Modify: `Scripts/lifecycle/boot.ts` (replace scope/setLevel with tracer)
- Delete: `Scripts/core/trace.ts`

This task wires the tracing module into the runtime boot sequence. After this task, the
old `core/trace.ts` is deleted and the runtime uses `Scripts/tracing/` exclusively.

- [ ] **Step 1: Add traceparent and traceLevel to Plan type**

In `Scripts/types/plan.ts`, add after `partId?`:
```typescript
  traceparent?: string;
  traceLevel?: string;
```

- [ ] **Step 2: Add span to ExecContext**

In `Scripts/types/context.ts`, add after `local?`:
```typescript
  /** Active trace span for this execution branch. */
  readonly span?: import("../tracing/types").Span;
```

- [ ] **Step 3: Rewrite root.ts trace wiring**

Replace the trace import and wiring in `Scripts/root.ts`:

```typescript
// Replace:
import { boot, trace } from "./lifecycle/boot";
import type { TraceLevel } from "./core/trace";

// With:
import { boot } from "./lifecycle/boot";
import { configure, flush } from "./tracing";
import { resolveLevel } from "./tracing/context";
```

Replace the trace level setting:
```typescript
// Replace:
const traceLevel = el.dataset.trace as TraceLevel | undefined;
if (traceLevel) trace.setLevel(traceLevel);

// With:
// (moved to after plan parsing — configure() needs plan.traceparent)
```

After plan parsing, before boot loop:
```typescript
// Configure tracing from first plan's traceparent and resolved level
if (plans.length > 0) {
  const firstPlan = plans[0];
  const firstEl = planEls[0];
  configure({
    level: resolveLevel(firstPlan.traceLevel, firstEl?.dataset.trace),
    traceparent: firstPlan.traceparent,
  });
}

window.addEventListener("beforeunload", () => flush());
```

- [ ] **Step 4: Rewrite boot.ts — replace scope/setLevel with tracer**

```typescript
// Replace:
import { setLevel } from "../core/trace";
import { scope } from "../core/trace";
const log = scope("boot");

// With:
import { tracer } from "../tracing";
const t = tracer("boot");
```

Replace all `log.*` calls per the spec migration map (3 sites):
- `log.info("booting", ...)` → `t.info("boot.start", { planId: plan.planId, behaviors: plan.behaviors.length })`
- `log.info("booted")` → `t.info("boot.complete", { planId: plan.planId })`
- `log.info("merge", ...)` → `t.info("plan.merge", { planId: merged.planId, newComponents: Object.keys(incoming.components).length })`

Remove the `export const trace = { setLevel };` line.

- [ ] **Step 5: Delete core/trace.ts**

```bash
rm Alis.Reactive.SandboxApp/Scripts/core/trace.ts
```

- [ ] **Step 6: Typecheck**

Run: `npm run typecheck`
Expected: Errors in files still importing from `core/trace` — these are migrated in Task 9.

Note: typecheck will fail until all 14 files are migrated. This is expected. Continue to
Task 9 immediately.

- [ ] **Step 7: Commit (WIP — migration incomplete)**

```bash
git add -A
git commit -m "feat(tracing): wire tracing module into boot — Plan, ExecContext, root.ts, boot.ts

WIP: 14 files still import from deleted core/trace.ts. Migration continues in next task."
```

---

### Task 9: Migrate All 66 Log Call Sites

**Files:** All 14 files listed in the migration map (see spec section "Complete Migration Map")

This task migrates each file from `import { scope } from "../core/trace"` to
`import { tracer } from "../tracing"` and rewrites all `log.*` calls per the spec.

**The spec migration map is the authoritative reference.** Do not invent event names.
Look up each call site in the spec.

- [ ] **Step 1: Migrate lifecycle/merge-plan.ts (2 sites)**

Replace import and `const log = scope("merge")` with `const t = tracer("merge")`.
Migrate per spec:
- Line 59: `t.error("merge.type.collision", { key, owner, incomingPartId: partId })`
- Line 70: `t.error("merge.component.collision", { key, owner, incomingPartId: partId })`

- [ ] **Step 2: Migrate execution/trigger.ts (4 sites)**

Replace import. Migrate per spec. At `runReaction`, use `t.withSpan(ctx?.span)` for
error correlation (catch-point enrichment):
- Line 22: `scoped.error("reaction.fail", { trigger: describeTrigger(trigger), planId: plan.planId }, err as Error)`
- Line 25: same as above
- Line 49: `t.debug("trigger.wire", { kind: "document-event", event: trigger.event })`
- Line 64: `t.debug("trigger.wire", { kind: "component-event", component: trigger.component, event: trigger.event, channel })`

Add the `describeTrigger` helper function (from spec).

- [ ] **Step 3: Migrate execution/execute.ts (6 sites)**

Replace import. Migrate per spec:
- Line 147: `t.trace("branch.no-match", { caseCount: reaction.cases.length })`
- Line 171: same
- Line 188: `t.error("parallel.step.fail", { stepIndex: i }, r.reason as Error)`
- Line 205: `t.trace("reaction.set", { component: target, property: reaction.property, value })`
- Line 232: `t.trace("reaction.call", { component: target, method: reaction.method, args })`
- Line 256: `t.trace("reaction.dispatch", { event: reaction.event, detail })`

- [ ] **Step 4: Migrate execution/http.ts (3 sites)**

Replace import. Migrate per spec:
- Line 65: `t.debug("http.validation.fail", { container: req.container })`
- Line 85: `t.debug("http.request.send", { method: req.method, url: resolved.url })`
- Line 103: `t.error("http.request.fail", { url: req.url, method: req.method, status }, err as Error)`

- [ ] **Step 5: Migrate execution/gather.ts (3 sites)**

Replace import. Migrate per spec:
- Line 37: `t.warn("gather.serialize.fail", { field: name, error: result.error })`
- Line 111: `t.trace("gather.file", { field: name, count: raw.length })`
- Line 120: `t.trace("gather.value", { field: name, value: raw })`

- [ ] **Step 6: Migrate execution/server-push.ts (9 sites)**

Replace import. Migrate all 9 per spec. Use `t.withSpan()` at line 96 catch point.

- [ ] **Step 7: Migrate execution/signalr.ts (15 sites)**

Replace import. Migrate all 15 per spec. Use `t.withSpan()` at line 134 catch point.

- [ ] **Step 8: Migrate execution/retry-indicator.ts (4 sites)**

Replace import. Migrate per spec.

- [ ] **Step 9: Migrate resolution/resolver.ts (1 site)**

Replace import. `t.debug("resolver.ready", {})`

- [ ] **Step 10: Migrate conditions/conditions.ts (3 sites)**

Replace import. Use `t.enabled("trace")` guard on the condition eval call (hot path).

- [ ] **Step 11: Migrate validation/orchestrator.ts (9 sites)**

Replace import. Migrate all 9 per spec.

- [ ] **Step 12: Migrate components/fusion/confirm.ts (2 sites)**

Replace import. Migrate per spec.

- [ ] **Step 13: Migrate components/native/native-action-link.ts (2 sites)**

Replace import. Use catch-point enrichment at line 46.

- [ ] **Step 14: Typecheck**

Run: `npm run typecheck`
Expected: Clean — zero errors

- [ ] **Step 15: Run all tests**

Run: `npm test -- --reporter verbose`
Expected: ALL vitest pass

- [ ] **Step 16: Build bundle**

Run: `npm run build:all`
Expected: Bundle builds successfully

- [ ] **Step 17: Commit**

```bash
git add -A
git commit -m "feat(tracing): migrate all 66 log call sites to tracer API

Complete migration from core/trace.ts to Scripts/tracing/. All 14 files
updated with event-name + structured payload format per spec migration map."
```

---

### Task 10: Span Instrumentation — Trigger, Execute, HTTP

**Files:**
- Modify: `Scripts/execution/trigger.ts` (create child span per trigger)
- Modify: `Scripts/execution/execute.ts` (thread span, parallel child spans)
- Modify: `Scripts/execution/http.ts` (inject traceparent header)

- [ ] **Step 1: Add span to trigger.ts wireBehavior**

In `wireBehavior`, when creating the ExecContext for each trigger type, add a child span:

```typescript
// In document-event listener:
const triggerSpan = rootSpan?.child("trigger.document-event", { event: trigger.event });
const ctx: ExecContext = { event: (e as CustomEvent).detail ?? e, span: triggerSpan };

// In component-event wireEvent callback:
const triggerSpan = rootSpan?.child("trigger.component-event", { component: trigger.component, event: trigger.event });
const ctx: ExecContext = { event: eventData, span: triggerSpan };
```

Note: `rootSpan` is obtained from `getRootSpan()` imported from tracing.

- [ ] **Step 2: Thread span in execute.ts**

In `executeParallel`, create child spans per step:

```typescript
const results = await Promise.allSettled(
  reaction.steps.map((s, i) => {
    const childSpan = ctx?.span?.child(`parallel.step[${i}]`);
    const childCtx = { ...ctx, span: childSpan };
    const r = executeReaction(s, plan, childCtx);
    return r instanceof Promise ? r : Promise.resolve();
  })
);
```

- [ ] **Step 3: Inject traceparent in http.ts**

Before the `fetch()` call:

```typescript
import { NOOP_SPAN } from "../tracing";

// Create request span
const requestSpan = ctx?.span?.child("http.request", {
  "http.method": req.method,
  "http.url": resolved.url,
});

// Inject traceparent (TwP: inject even when tracing is off)
const tp = requestSpan?.traceparent();
if (tp && !tp.startsWith("00-" + "0".repeat(32))) {
  (init.headers as Record<string, string>)["traceparent"] = tp;
}

// After fetch:
requestSpan?.set("http.status", response.status);
requestSpan?.end(response.ok ? "ok" : "error");
```

- [ ] **Step 4: Typecheck**

Run: `npm run typecheck`
Expected: Clean

- [ ] **Step 5: Run all tests**

Run: `npm test -- --reporter verbose`
Expected: ALL pass

- [ ] **Step 6: Build bundle**

Run: `npm run build:all`
Expected: Bundle builds

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat(tracing): add span instrumentation — trigger, parallel, HTTP traceparent"
```

---

### Task 11: Throw-Site Message Enhancement

**Files:**
- Modify: `Scripts/resolution/resolver.ts` (enhance component/property/method not-found messages)
- Modify: `Scripts/execution/execute.ts` (enhance property/method not-found messages)
- Modify: `Scripts/execution/trigger.ts` (enhance trigger component not-found)

- [ ] **Step 1: Enhance resolver.ts throw messages**

Add "available" context to key throw sites:

```typescript
// Component not found:
throw new Error(`[alis] component not found: "${componentKey}" (available: ${Object.keys(plan.components).join(", ")})`);

// Element not found:
throw new Error(`[alis] element not found: "${comp.id}" (component: ${key}, vendor: ${comp.vendor})`);
```

- [ ] **Step 2: Enhance execute.ts throw messages**

```typescript
// Property not found:
throw new Error(`[alis] property "${reaction.property}" not found on type (available: ${Object.keys(jsType.properties ?? {}).join(", ")})`);

// Method not found:
throw new Error(`[alis] method "${reaction.method}" not found on type (available: ${Object.keys(jsType.methods ?? {}).join(", ")})`);
```

- [ ] **Step 3: Enhance trigger.ts throw message**

```typescript
throw new Error(`[alis] trigger component not found: "${trigger.component}" (available: ${Object.keys(plan.components).join(", ")})`);
```

- [ ] **Step 4: Typecheck + tests**

Run: `npm run typecheck && npm test -- --reporter verbose`
Expected: Clean typecheck, all tests pass

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(tracing): enhance throw-site messages with 'available' context"
```

---

### Task 12: Browser Verification + Final Tests

**Files:**
- Create: `Scripts/__tests__/tracing/migration.test.ts`

- [ ] **Step 1: Write migration spot-check test**

```typescript
// Scripts/__tests__/tracing/migration.test.ts
import { describe, it, expect, vi, beforeEach } from "vitest";
import { configure } from "../../tracing";
import { resetForTests } from "../../tracing/trace";
import type { TraceEvent, TraceSink } from "../../tracing/types";

describe("migration spot-check", () => {
  let events: TraceEvent[];
  let sink: TraceSink;

  beforeEach(() => {
    resetForTests();
    events = [];
    sink = { emit: e => events.push(e), span: vi.fn(), flush: vi.fn() };
    configure({ level: "trace", sink });
  });

  it("boot.start event has planId and behaviors fields", () => {
    // This test verifies the migration pattern — the actual boot
    // function is tested via Playwright. Here we verify the tracer
    // API usage pattern is correct.
    const { tracer } = require("../../tracing");
    const t = tracer("boot");
    t.info("boot.start", { planId: "test", behaviors: 5 });
    expect(events[0].event).toBe("boot.start");
    expect(events[0].data).toEqual({ planId: "test", behaviors: 5 });
  });

  it("reaction.fail event includes trigger and planId", () => {
    const { tracer } = require("../../tracing");
    const t = tracer("trigger");
    t.error("reaction.fail", {
      trigger: "component-event:DDL__Status.change",
      planId: "order-form",
    }, new Error("test"));
    expect(events[0].event).toBe("reaction.fail");
    expect(events[0].data?.trigger).toBe("component-event:DDL__Status.change");
    expect(events[0].error?.type).toBe("Error");
    expect(events[0].breadcrumbs).toBeDefined();
  });
});
```

- [ ] **Step 2: Run all tests**

Run: `npm test -- --reporter verbose`
Expected: ALL pass

- [ ] **Step 3: Build and start SandboxApp**

```bash
npm run build:all
dotnet build Alis.Reactive.slnx -nologo
lsof -ti:5220 | xargs kill -9 2>/dev/null
dotnet run --project Alis.Reactive.SandboxApp &
```

- [ ] **Step 4: Verify in browser**

Open `http://localhost:5220/Sandbox` with Chrome DevTools console open.
Set trace level by adding `?trace=debug` to URL.
Navigate to a form page, interact with components, verify:
- CSS-colored `[alis:scope]` tags in console
- `console.info` for info events (blue in Chrome)
- Expandable data objects (not stringified)
- Spans as `groupCollapsed` entries with duration

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat(tracing): add migration spot-check tests, verify in browser"
```

- [ ] **Step 6: Run full C# test suite**

```bash
dotnet test tests/Alis.Reactive.UnitTests/Alis.Reactive.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.Fusion.UnitTests/Alis.Reactive.Fusion.UnitTests.csproj -nologo
dotnet test tests/Alis.Reactive.Native.UnitTests/Alis.Reactive.Native.UnitTests.csproj -nologo
```

Expected: All pass (C# tests don't depend on TS runtime structure)

---

## Execution Summary

| Task | Files | Tests | Commit |
|------|-------|-------|--------|
| 0 | vitest.setup.ts | 0 | chore: vitest setup |
| 1 | types.ts | 3 | feat: type definitions |
| 2 | breadcrumbs.ts | 6 | feat: ring buffer |
| 3 | context.ts | ~10 | feat: traceparent + level |
| 4 | span.ts | ~15 | feat: span lifecycle |
| 5 | sink.ts | ~8 | feat: ConsoleSink |
| 6 | trace.ts | ~8 | feat: ScopedTracer + configure |
| 7 | index.ts | ~6 | feat: barrel + integration + TwP |
| 8 | plan.ts, context.ts, root.ts, boot.ts, -core/trace.ts | 0 (WIP) | feat: boot integration |
| 9 | 14 runtime files | 0 (migration) | feat: migrate 66 call sites |
| 10 | trigger.ts, execute.ts, http.ts | 0 | feat: span instrumentation |
| 11 | resolver.ts, execute.ts, trigger.ts | 0 | feat: throw-site enhancement |
| 12 | migration.test.ts + browser | ~2 | feat: migration tests + browser verify |
