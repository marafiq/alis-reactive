---
name: http-pipeline-hardening
description: Next priority — HTTP pipeline error handling, immutability, retry-ready design. Current .catch() swallows errors that should reach developer's onError handlers.
type: project
---

# HTTP Pipeline Hardening

## The Problem
executeReaction() calls async HTTP functions fire-and-forget with .catch() safety net.
If gather throws, validation throws, or any pre-fetch error occurs — the developer's
onError status handlers NEVER fire. The error is swallowed into a log.

## Current Architecture
```
executeReaction (sync)
  → executeHttpReaction (async) .catch(log.error)  ← SAFETY NET, not proper handling
    → pipeline.ts: preFetch → validate → execRequest
      → http.ts: whileLoading → gather → fetch → routeHandlers
        → try/catch around fetch ONLY — pre-fetch errors escape
```

## What Must Change
1. **Error boundary must encompass the entire pipeline** — preFetch, validate, gather, fetch, response routing. Not just fetch.
2. **Pre-fetch errors must route through onError handlers** — if the developer registered `OnError(400, e => {...})`, they expect it to fire on ANY error, not just HTTP 400.
3. **ExecContext must be immutable** — currently `{ ...ctx, responseBody }` creates a new object but the original is still mutable. For retry, the original ctx must survive unchanged.
4. **Each request is a unit** — in parallel-http, one request failing doesn't affect others (already handled by Promise.allSettled). In chained, failure stops the chain (already correct).
5. **Retry is future** — but the design must not fight it. A retryable request needs: immutable original ctx, clean re-execution, and error handler that decides retry vs fail.

## Key Modules
- execution/execute.ts — lines 59-67: fire-and-forget .catch()
- execution/pipeline.ts — preFetch + validate + execRequest orchestration
- execution/http.ts — fetch lifecycle, response routing, chaining
- execution/gather.ts — form data collection (can throw on IncludeAll with no components)
- validation/orchestrator.ts — validate() (can throw on missing form container)

## Design Constraints
- Each request is a unit (parallel requests are independent)
- Chained requests stop on first failure
- onError handlers must fire for ALL errors, not just HTTP status errors
- ExecContext immutability (spread is shallow — nested objects share references)
- No retry implementation yet — but architecture must allow it
- The .catch() on executeReaction is a safety net, not the error handling strategy

## Related Files Changed This Session
- execution/execute.ts — added .catch() to 3 async call sites
- execution/pipeline.ts — passesValidation, preFetch commands (can throw)
- execution/http.ts — try/catch around fetch, routeHandlers
- core/coerce.ts — extracted, 64 tests
- validation/live-clear.ts — rewritten per-field
- validation/error-display.ts — ID-only lookups

## Session Stats
- 33 commits, 896 vitest + 481 Playwright = 1,377 all green
- Audit complete for all modules
- Sequence diagrams regenerated
