# HTTP Pipeline Redesign — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make every HTTP request a reliable unit where ALL errors route through developer's handlers, context is immutable, and the architecture supports future retry.

**Architecture:** Fully async `executeReaction` (single code path), one try/catch per request unit in `execRequest`, async `routeHandlers`/`executeHandler`, `Promise.all` for parallel, `readonly` ExecContext, `ResolvedFetch` immutable snapshot.

**Tech Stack:** TypeScript, Vitest + jsdom, esbuild ESM bundle

**Spec:** `docs/superpowers/specs/2026-03-19-http-pipeline-redesign.md`

**BDD tests already written:** 4 new test files, 14 failing tests define the contract, 16 pass on current code.

**Test baseline:** 912 existing tests pass. Zero must break.

---

### Task 1: ExecContext Readonly

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/types/context.ts`

- [ ] **Step 1: Add `readonly` to all ExecContext properties**

```typescript
export interface ExecContext {
  readonly evt?: Record<string, unknown>;
  readonly responseBody?: unknown;
  readonly validationDesc?: ValidationDescriptor;
  readonly components?: Record<string, ComponentEntry>;
}
```

- [ ] **Step 2: Run typecheck to find any direct assignment sites**

Run: `npm run typecheck`
Expected: PASS (all existing code uses spread to create new contexts)

- [ ] **Step 3: Run full test suite**

Run: `npx vitest run`
Expected: 912 pass, 14 fail (same as baseline — no behavior change)

- [ ] **Step 4: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/types/context.ts
git commit -m "refactor: make ExecContext readonly for immutability"
```

---

### Task 2: Make routeHandlers and executeHandler async, export routeHandlers

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/execution/http.ts`

- [ ] **Step 1: Change `routeHandlers` to async, export it**

```typescript
export async function routeHandlers(handlers: StatusHandler[] | undefined, status: number, ctx?: ExecContext): Promise<void> {
  if (!handlers || handlers.length === 0) return;

  // Try specific status match first
  for (const h of handlers) {
    if (h.statusCode != null && h.statusCode === status) {
      await executeHandler(h, ctx);
      return;
    }
  }

  // Fall through to catch-all (no statusCode)
  for (const h of handlers) {
    if (h.statusCode == null) {
      await executeHandler(h, ctx);
      return;
    }
  }
}
```

- [ ] **Step 2: Change `executeHandler` to async**

```typescript
async function executeHandler(h: StatusHandler, ctx?: ExecContext): Promise<void> {
  if (h.reaction) {
    await executeReaction(h.reaction, ctx);
  } else if (h.commands) {
    executeCommands(h.commands, ctx);
  }
}
```

- [ ] **Step 3: Add `await` to all `routeHandlers` calls in `execRequest`**

There are two call sites in `execRequest` (success and error paths) — add `await` to both. Also `await` the chained `execRequest` call (already awaited, verify).

- [ ] **Step 4: Run typecheck**

Run: `npm run typecheck`
Expected: PASS

- [ ] **Step 5: Run tests**

Run: `npx vitest run`
Expected: 912 pass, 14 fail (same baseline — async void is backward compatible)

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/execution/http.ts
git commit -m "refactor: make routeHandlers and executeHandler async"
```

---

### Task 3: Expand error boundary in execRequest + add ResolvedFetch

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/execution/http.ts`

This is the core change. The try/catch wraps the ENTIRE request lifecycle.

- [ ] **Step 1: Add `ResolvedFetch` interface and `buildFetch` helper**

```typescript
interface ResolvedFetch {
  readonly url: string;
  readonly init: RequestInit;
}

function buildFetch(req: RequestDescriptor, gatherResult: GatherResult): ResolvedFetch {
  let url = req.url;
  const init: RequestInit = { method: req.verb };

  if (gatherResult.urlParams.length > 0) {
    const sep = url.includes("?") ? "&" : "?";
    url = url + sep + gatherResult.urlParams.join("&");
  }

  if (req.verb !== "GET") {
    if (gatherResult.body instanceof FormData) {
      init.body = gatherResult.body;
    } else if (Object.keys(gatherResult.body).length > 0) {
      init.headers = { "Content-Type": "application/json" };
      init.body = JSON.stringify(gatherResult.body);
    }
  }

  return { url, init };
}
```

- [ ] **Step 2: Rewrite `execRequest` with expanded error boundary**

```typescript
export async function execRequest(req: RequestDescriptor, ctx?: ExecContext): Promise<void> {
  try {
    // 1. WhileLoading
    if (req.whileLoading) {
      executeCommands(req.whileLoading, ctx);
    }

    // 2. Gather → freeze
    const gatherResult = resolveGather(req.gather ?? [], req.verb, ctx?.components ?? {}, req.contentType);
    const resolved = buildFetch(req, gatherResult);

    log.debug("fetch", { verb: req.verb, url: resolved.url });

    // 3. Fetch
    const response = await fetch(resolved.url, resolved.init);

    // 4. Route response
    const body = await readResponseBody(response);
    if (response.ok) {
      const successCtx: ExecContext = body != null ? { ...ctx, responseBody: body } : ctx ?? {};
      await routeHandlers(req.onSuccess, response.status, successCtx);
    } else {
      const errorCtx: ExecContext = {
        ...ctx,
        responseBody: body ?? undefined,
        validationDesc: req.validation,
      };
      await routeHandlers(req.onError, response.status, errorCtx);
      return; // no chained on error
    }
  } catch (err) {
    const status = err instanceof TypeError ? 0 : -1;
    log.error(status === 0 ? "network error" : "client error", { url: req.url, error: String(err) });
    await routeHandlers(req.onError, status, ctx);
    return; // no chained on error
  }

  // 5. Chained — only after success
  if (req.chained) {
    await execRequest(req.chained, ctx);
  }
}
```

- [ ] **Step 3: Remove old inline URL/body building code** (replaced by `buildFetch`)

Keep `readResponseBody` function unchanged — it is NOT part of the old URL/body building code being replaced. Only the inline URL construction and `RequestInit` building inside `execRequest` is replaced by `buildFetch`.

- [ ] **Step 4: Run the failing BDD tests for request unit failures**

Run: `npx vitest run Scripts/__tests__/when-http-request-unit-fails.test.ts`
Expected: ALL PASS (gather throws → onError(-1), whileLoading throws → onError(-1), network → onError(0), status routing)

- [ ] **Step 5: Run the nested reactions tests**

Run: `npx vitest run Scripts/__tests__/when-http-handlers-contain-nested-reactions.test.ts`
Expected: ALL PASS (inner HTTP awaited before chained fires)

- [ ] **Step 6: Run full test suite**

Run: `npx vitest run`
Expected: 926 pass, remaining failures only in pipeline tests (Task 4)

- [ ] **Step 7: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/execution/http.ts
git commit -m "fix: expand error boundary to cover entire request lifecycle"
```

---

### Task 4: Pipeline error boundary + Promise.all

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/execution/pipeline.ts`

- [ ] **Step 1: Import `routeHandlers` from http.ts**

```typescript
import { execRequest, routeHandlers } from "./http";
```

- [ ] **Step 2: Wrap preFetch + validation in try/catch for single HTTP**

```typescript
export async function executeHttpReaction(reaction: HttpReaction, ctx?: ExecContext): Promise<void> {
  try {
    if (reaction.preFetch) {
      executeCommands(reaction.preFetch, ctx);
    }
    if (!passesValidation(reaction.request)) return;
  } catch (err) {
    log.error("pre-request error", { error: String(err) });
    await routeHandlers(reaction.request.onError, -1, ctx);
    return;
  }
  await execRequest(reaction.request, ctx);
}
```

- [ ] **Step 3: Wrap preFetch in try/catch for parallel + switch to Promise.all**

```typescript
export async function executeParallelHttpReaction(reaction: ParallelHttpReaction, ctx?: ExecContext): Promise<void> {
  try {
    if (reaction.preFetch) {
      executeCommands(reaction.preFetch, ctx);
    }
  } catch (err) {
    log.error("pre-request error", { error: String(err) });
    return;
  }

  const validRequests = reaction.requests.filter(req => passesValidation(req));

  log.debug("parallel", { count: validRequests.length });

  await Promise.all(validRequests.map(req => execRequest(req, ctx)));

  if (reaction.onAllSettled) {
    executeCommands(reaction.onAllSettled, ctx);
  }
}
```

- [ ] **Step 4: Run the pipeline BDD tests**

Run: `npx vitest run Scripts/__tests__/when-http-pipeline-catches-prefetch-errors.test.ts`
Expected: ALL PASS

- [ ] **Step 5: Run full test suite**

Run: `npx vitest run`
Expected: 926 pass, 0 fail (only executeReaction async change remaining — but existing tests may already pass via setTimeout)

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/execution/pipeline.ts
git commit -m "fix: add pipeline error boundary, switch to Promise.all"
```

---

### Task 5: Unified async executeReaction

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/execution/execute.ts`

- [ ] **Step 1: Delete `needsAsync()` and `dispatchAsync()`**

Remove lines 15-22 (`needsAsync`) and lines 82-125 (`dispatchAsync`).

- [ ] **Step 2: Make `executeReaction` async with single code path**

```typescript
export async function executeReaction(reaction: Reaction, ctx?: ExecContext): Promise<void> {
  switch (reaction.kind) {
    case "sequential":
      log.debug("sequential", { commands: reaction.commands.length });
      for (const cmd of reaction.commands) {
        executeCommand(cmd, ctx);
      }
      return;

    case "conditional":
      log.debug("conditional", { commands: reaction.commands?.length ?? 0, branches: reaction.branches.length });
      if (reaction.commands) {
        for (const cmd of reaction.commands) {
          executeCommand(cmd, ctx);
        }
      }
      for (const branch of reaction.branches) {
        if (branch.guard == null || await evaluateGuardAsync(branch.guard, ctx)) {
          log.trace("branch-taken", { guard: branch.guard?.kind ?? "else" });
          await executeReaction(branch.reaction, ctx);
          return;
        }
      }
      log.trace("no-branch-taken");
      return;

    case "http":
      log.debug("http", { url: reaction.request.url });
      await executeHttpReaction(reaction, ctx);
      return;

    case "parallel-http":
      log.debug("parallel-http", { count: reaction.requests.length });
      await executeParallelHttpReaction(reaction, ctx);
      return;

    default:
      assertNever(reaction, "reaction kind");
  }
}
```

- [ ] **Step 3: Remove unused import `evaluateGuard` (keep `evaluateGuardAsync`, `isConfirmGuard`)**

`evaluateGuard` is no longer used in execute.ts (still used by commands.ts — its own import).
`isConfirmGuard` is no longer used in execute.ts (still used by commands.ts — its own import).

Current import (line 4): `import { evaluateGuard, evaluateGuardAsync, isConfirmGuard } from "../conditions/conditions";`

Change to: `import { evaluateGuardAsync } from "../conditions/conditions";`

Remove `evaluateGuard` and `isConfirmGuard` from this import — they are no longer used in execute.ts. Both are still used by `commands.ts` which has its own import.

- [ ] **Step 4: Run typecheck**

Run: `npm run typecheck`
Expected: PASS

- [ ] **Step 5: Run full test suite**

Run: `npx vitest run`
Expected: 926 pass, 0 fail

- [ ] **Step 6: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/execution/execute.ts
git commit -m "refactor: unify executeReaction as single async path"
```

---

### Task 6: Add .catch() at all void boundaries

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts`
- Modify: `Alis.Reactive.SandboxApp/Scripts/components/native/native-action-link.ts`

`executeReaction` now returns `Promise<void>`. Every call site in a void context (DOM event listener) needs `.catch()` to prevent floating promises.

- [ ] **Step 1: Add `.catch()` to all `executeReaction` call sites in trigger.ts**

4 call sites in `wireTrigger`:

```typescript
case "dom-ready":
  if (document.readyState === "complete" || document.readyState === "interactive") {
    executeReaction(reaction, { components }).catch(err =>
      log.error("reaction failed", { error: String(err) })
    );
  } else {
    document.addEventListener("DOMContentLoaded", () =>
      executeReaction(reaction, { components }).catch(err =>
        log.error("reaction failed", { error: String(err) })
      ), opts);
  }
  break;

case "custom-event":
  log.debug("custom-event: listening", { event: trigger.event });
  document.addEventListener(trigger.event, (e) => {
    const detail = (e as CustomEvent).detail;
    executeReaction(reaction, { evt: detail ?? {}, components }).catch(err =>
      log.error("reaction failed", { error: String(err) })
    );
  }, opts);
  break;

case "component-event": {
  const el = document.getElementById(trigger.componentId);
  if (!el) throw new Error(`[alis] element not found: ${trigger.componentId}`);
  const root = resolveRoot(el, trigger.vendor);
  const expr = trigger.readExpr;
  log.debug("component-event", { componentId: trigger.componentId, jsEvent: trigger.jsEvent, vendor: trigger.vendor });
  (root as EventTarget).addEventListener(trigger.jsEvent, (e: any) => {
    const detail = trigger.vendor === "native"
      ? (expr ? { [expr]: walk(el, expr), event: e } : { event: e })
      : (e ?? {});
    executeReaction(reaction, { evt: detail, components }).catch(err =>
      log.error("reaction failed", { error: String(err) })
    );
  }, opts);
  break;
}
```

- [ ] **Step 2: Add `.catch()` to `executeReaction` in native-action-link.ts**

In `handleClick` function (line 44), change:

```typescript
// Before:
executeReaction(payload.reaction);

// After:
executeReaction(payload.reaction).catch(err =>
  log.error("reaction failed", { error: String(err) })
);
```

- [ ] **Step 3: Run full test suite**

Run: `npx vitest run`
Expected: 926 pass, 0 fail

- [ ] **Step 4: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/execution/trigger.ts Alis.Reactive.SandboxApp/Scripts/components/native/native-action-link.ts
git commit -m "fix: add .catch() at all void boundaries (trigger.ts, native-action-link.ts)"
```

---

### Task 7: Build + typecheck + full verification

**Files:**
- No new files — verification only

- [ ] **Step 1: TypeScript typecheck**

Run: `npm run typecheck`
Expected: PASS (zero errors)

- [ ] **Step 2: Vitest — all TS tests**

Run: `npx vitest run`
Expected: 926 pass, 0 fail

- [ ] **Step 3: Build JS bundle**

Run: `npm run build:all`
Expected: Success, `wwwroot/js/alis-reactive.js` updated

- [ ] **Step 4: Build C# projects**

Run: `dotnet build`
Expected: Success

- [ ] **Step 5: Run C# unit tests**

Run: `dotnet test tests/Alis.Reactive.UnitTests && dotnet test tests/Alis.Reactive.Native.UnitTests && dotnet test tests/Alis.Reactive.Fusion.UnitTests && dotnet test tests/Alis.Reactive.FluentValidator.UnitTests`
Expected: All pass (C# layer unchanged)

- [ ] **Step 6: Restart app and run Playwright tests**

Run:
```bash
# Terminal 1: restart the app
dotnet run --project Alis.Reactive.SandboxApp &

# Terminal 2: run Playwright
dotnet test tests/Alis.Reactive.PlaywrightTests
```
Expected: 481 Playwright tests pass

- [ ] **Step 7: Final commit (if any fixes needed)**

```bash
git commit -m "chore: verify all three test layers pass after HTTP pipeline redesign"
```

---

## Summary

| Task | What | Makes pass |
|------|------|-----------|
| 1 | ExecContext readonly | Compile-time safety |
| 2 | Async routeHandlers/executeHandler | Nested HTTP awaiting |
| 3 | Expanded error boundary + ResolvedFetch | 10 failing tests (gather, whileLoading, network, status routing) |
| 4 | Pipeline error boundary + Promise.all | 4 failing tests (preFetch errors, parallel gather errors) |
| 5 | Unified async executeReaction | Delete duplicate code, single path |
| 6 | Trigger.ts .catch() | Void boundary safety |
| 7 | Full verification | All 3 test layers green |

**Total new code:** ~60 lines (ResolvedFetch + buildFetch + expanded try/catch + pipeline try/catch)
**Total deleted code:** ~50 lines (needsAsync + dispatchAsync + 3x .catch + allSettled rejected loop)
**Net change:** ~+10 lines for correctness across the entire HTTP pipeline.
