# HTTP Pipeline Redesign — Design Spec

## Problem

Three design flaws in the HTTP pipeline:

1. **Fire-and-forget async** — `executeReaction()` is `void`. HTTP reactions are dispatched with `.catch(log)` safety nets that swallow errors. Developer's `onError` handlers never fire for pre-fetch errors. Nested HTTP in status handlers is not awaited.

2. **Mutable ExecContext** — No `readonly` markers. Nothing prevents accidental mutation. Retry is impossible because gathered state isn't captured.

3. **Error boundary too narrow** — `http.ts` try/catch wraps only `fetch()`. Gather errors, whileLoading errors, and preFetch errors escape and become unhandled rejections.

## Design Decisions

### 1. Fully Async `executeReaction`

`executeReaction()` becomes `async` returning `Promise<void>`. Single code path for all reaction kinds.

**Rationale**: The dual sync/async paths (`executeReaction` + `dispatchAsync`) exist to avoid wrapping sync DOM mutations in promises. But modern JS engines optimize async functions that never hit `await` — they return already-resolved promises at near-zero cost. The dual path caused every bug: fire-and-forget HTTP, lost errors, 40 lines of duplicated switch logic. One correct path is always better than two paths where one is wrong.

**Void boundary**: `trigger.ts` is the ONLY place where async meets void (DOM event listeners are inherently void). A NEW `.catch()` is added here — the ONLY `.catch()` in the system. Today trigger.ts has no `.catch()` at all.

**Conditional guards**: `execute.ts` switches from `evaluateGuard` (sync) to `evaluateGuardAsync` for branch evaluation. The sync `evaluateGuard` is NOT deleted — it's still used by `commands.ts` for per-action `when` guards. Only `execute.ts` changes.

**Deleted from execute.ts**: `needsAsync()`, `dispatchAsync()`, three `.catch()` blocks (lines 28-30, 61-63, 68-70).

**Kept sync intentionally**: `executeCommands` in `commands.ts` remains sync. It's called from `whileLoading` (UI should update before network call), `preFetch`, and `onAllSettled` — all correctly sync.

### 2. ExecContext Readonly

```typescript
export interface ExecContext {
  readonly evt?: Record<string, unknown>;
  readonly responseBody?: unknown;
  readonly validationDesc?: ValidationDescriptor;
  readonly components?: Record<string, ComponentEntry>;
}
```

All existing code already creates new contexts via spread. This makes accidental mutation a compile error.

### 3. ResolvedFetch — Immutable Request Snapshot

After gather reads live DOM values, the result is frozen into a `ResolvedFetch`:

```typescript
interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}
```

Gather runs once, produces `ResolvedFetch`, fetch uses it. Future retry reuses the same `ResolvedFetch` — no DOM re-reading, exact same request over the wire.

### 4. One Request = One Unit = One Error Boundary

`execRequest` wraps its entire lifecycle in a single try/catch:

```typescript
async function execRequest(req, ctx): Promise<void> {
  try {
    whileLoading (sync)
    gather → ResolvedFetch (freeze)
    fetch(resolved.url, resolved.init)
    route response (await routeHandlers)
  } catch (err) {
    status = err instanceof TypeError ? 0 : -1
    await routeHandlers(req.onError, status, ctx)
    return  // no chained on error
  }
  if (req.chained) await execRequest(req.chained, ctx)
}
```

Any throw — whileLoading, gather, fetch, response parsing — same error lane. Developer's `onError` handlers always fire.

**Fail-fast guarantee**: If the developer's `onError` handler itself throws (bug in handler — e.g., missing element ID), the rejection propagates through `execRequest` → `executeHttpReaction` → `executeReaction` → trigger.ts `.catch()` → logged. No nested try/catch to silence it. For parallel requests, `Promise.all` rejects → `onAllSettled` doesn't run → visible symptom (spinner stuck) → developer notices the bug immediately. This is correct per project philosophy: no fallbacks, fail fast.

Chained requests are independent units. Each `execRequest` call has its own try/catch. If B fails, A already completed successfully. C never starts.

### 5. Async routeHandlers + executeHandler

`routeHandlers` and `executeHandler` become async, awaiting `executeReaction`. This means nested HTTP reactions inside status handlers are properly awaited before chained requests fire.

### 6. Promise.all for Parallel

In the new design, `execRequest` never rejects — all errors are handled internally and routed to developer's handlers. `Promise.allSettled` solves a problem that no longer exists.

`Promise.all` is correct: wait for all request units to complete, then run `onAllSettled` commands.

### 7. Pipeline Error Boundary

`pipeline.ts` wraps preFetch + validation in try/catch. For single HTTP, routes to the request's `onError` — requires importing `routeHandlers` from `http.ts`. `execRequest` has its own boundary — never throws to caller.

For parallel: preFetch is shared across all requests. If it throws, log and return (can't route to individual request's `onError` — no single owner). `routeHandlers` is NOT used in the parallel preFetch catch path.

### 8. Error Status Convention

| Status | Meaning | Source |
|--------|---------|--------|
| -1 | Client error | gather/preFetch/whileLoading throw |
| 0 | Network error | fetch TypeError |
| 4xx/5xx | Server error | HTTP response |

No schema changes — `StatusHandler.statusCode` is already `number?`.

## Sequence Diagrams

### Happy Path — Single HTTP

```
trigger.ts              execute.ts              pipeline.ts              http.ts
    |                       |                       |                       |
    | executeReaction()     |                       |                       |
    |   .catch(log.error)   |                       |                       |
    |---------------------->|                       |                       |
    |                       | case "http":          |                       |
    |                       | await --------------->|                       |
    |                       |                       | preFetch (sync)       |
    |                       |                       | validation passes     |
    |                       |                       | await execRequest()   |
    |                       |                       |---------------------->|
    |                       |                       |                       | try {
    |                       |                       |                       |   whileLoading (sync)
    |                       |                       |                       |   gather -> ResolvedFetch
    |                       |                       |                       |   await fetch(resolved)
    |                       |                       |                       |   await routeHandlers(onSuccess)
    |                       |                       |                       |     -> await executeReaction(handler)
    |                       |                       |                       | }
    |                       |                       |                       | chained? await execRequest(chained)
    |                       |                       | <- resolved           |
    |                       | <- resolved           |                       |
    | <- void               |                       |                       |
```

### Gather Failure — Developer's onError Fires

```
http.ts
    |
    | try {
    |   whileLoading -> spinner shown
    |   resolveGather() -> THROW! (component not found)
    | } catch(err) {
    |   status = -1
    |   await routeHandlers(onError, -1, ctx)
    |   -> developer's handler fires -> can hide spinner, show message
    |   return (no chained)
    | }
```

### Nested HTTP in Success Handler — Properly Awaited

```
http.ts                                execute.ts
    |                                      |
    | fetch -> 200                         |
    | await routeHandlers(onSuccess)       |
    |   await executeHandler(h)            |
    |     await executeReaction(h.reaction)|
    |     -------------------------------->|
    |                                      | case "http":
    |                                      | await executeHttpReaction()
    |                                      |   -> await fetch (inner)
    |                                      |   -> await routeHandlers (inner)
    |                                      | <- resolved
    |     <- resolved                      |
    |   <- resolved                        |
    | <- resolved                          |
    |                                      |
    | chained? await execRequest(chained)  |
    | <- inner HTTP completed BEFORE chained starts
```

### Parallel — Each Request Owns Its Errors

```
pipeline.ts                    http.ts
    |
    | preFetch (sync)
    | validRequests = filter(passesValidation)
    |
    | Promise.all([
    |   execRequest(req1) -> try { gather, fetch, route } catch { onError } -> resolves
    |   execRequest(req2) -> try { gather, fetch, route } catch { onError } -> resolves
    | ])
    | <- all resolved (errors handled inside each unit)
    |
    | onAllSettled commands run
```

### Chained A -> B -> C — Independent Units

```
execRequest(A)                execRequest(B)               execRequest(C)
+------------------+         +------------------+         +------------------+
| try {            |         | try {            |         | try {            |
|   whileLoading   |         |   whileLoading   |         |   whileLoading   |
|   gather->freeze |         |   gather->freeze |         |   gather->freeze |
|   fetch          |         |   fetch          |         |   fetch          |
|   routeHandlers  |         |   routeHandlers  |         |   routeHandlers  |
| } catch->onError |         | } catch->onError |         | } catch->onError |
+--------+---------+         +--------+---------+         +------------------+
         | ok?                         | ok?
         +-> await execRequest(B)      +-> await execRequest(C)
```

### Sync Reactions — Zero Overhead

```
trigger.ts                   execute.ts
    |                            |
    | executeReaction()          |
    |   .catch(log.error)        |
    |--------------------------->|
    |                            | case "sequential":
    |                            |   executeCommand(addClass)  <- sync
    |                            |   executeCommand(setText)   <- sync
    |                            |   return <- resolved promise (no await hit)
    |                            |
    | <- resolved                |
    |                            |
    | V8 optimization: async function that never hits await
    | returns already-resolved promise. Nanosecond cost.
    | All mutations happen synchronously before browser renders.
```

## Signature Changes

| Function | Before | After |
|----------|--------|-------|
| `executeReaction` | `void` | `Promise<void>` |
| `routeHandlers` | `void` | `Promise<void>` |
| `executeHandler` | `void` | `Promise<void>` |
| `executeCommands` | `void` (sync) | `void` (sync) — unchanged |
| `executeCommand` | `void` (sync) | `void` (sync) — unchanged |

## Test Migration

Making `executeReaction` async affects tests:
- Tests calling `executeReaction()` directly need `await`
- Tests using `boot()` with `setTimeout` can switch to `await` for deterministic assertions
- Tests calling `execRequest()` directly already use `await` — no change
- Tests calling `executeHttpReaction()` / `executeParallelHttpReaction()` already use `await` — no change

## Files Modified

| File | Change |
|------|--------|
| `types/context.ts` | `readonly` on all properties |
| `execution/execute.ts` | `async`, delete `needsAsync` + `dispatchAsync`, always `evaluateGuardAsync`, delete 3x `.catch()` |
| `execution/trigger.ts` | `.catch()` at void boundary (the ONLY `.catch`) |
| `execution/http.ts` | Single try/catch for entire lifecycle, `ResolvedFetch`, async `routeHandlers` + `executeHandler`, export `routeHandlers` |
| `execution/pipeline.ts` | try/catch for preFetch+validation, `Promise.all`, import `routeHandlers` |

## Files NOT Modified

| File | Why |
|------|-----|
| `execution/gather.ts` | Pure function, throws on invalid input — correct |
| `execution/commands.ts` | Sync, no async calls — correct |
| `execution/element.ts` | Sync DOM mutations — correct |
| `types/http.ts` | `StatusHandler` already supports `statusCode?: number` |
| `lifecycle/boot.ts` | `trigger.ts` handles the void boundary |
| `conditions/conditions.ts` | Both `evaluateGuard` and `evaluateGuardAsync` already exist. `evaluateGuard` still used by `commands.ts` for per-action `when` guards. `isConfirmGuard` still used by `commands.ts`. No change. |

## What This Enables (Future, Not Implemented Now)

- **Retry**: Immutable `ResolvedFetch` + clean error boundary = retryable unit. Re-call `fetch(resolved.url, resolved.init)`.
- **Status -1 in C# DSL**: `OnError(-1, e => {...})` for client-side error feedback.
- **Nested HTTP awaiting**: Response handlers with HTTP reactions complete before outer pipeline returns.
- **Proper test assertions**: Tests can `await executeReaction()` instead of `setTimeout`.
