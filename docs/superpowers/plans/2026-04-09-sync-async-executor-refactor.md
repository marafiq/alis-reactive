# Sync-First Executor Refactor

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the runtime executor run sync reaction kinds (set, call, dispatch, inject, branch) synchronously in the same tick as the event callback, so SF event arg mutations (args.cancel, args.preventDefaultAction) take effect before SF checks them.

**Architecture:** Change `executeReaction` from `async () => Promise<void>` to `() => void | Promise<void>`. Sync kinds return `void`. Async kinds (`request`, `parallel`) return `Promise<void>`. The sequence executor runs sync steps synchronously and only returns a Promise when it hits an async step. Trigger callbacks check `instanceof Promise` for error handling. No plan schema changes. No C# changes. Runtime stays dumb.

**Tech Stack:** TypeScript, esbuild, vitest (jsdom), Playwright

---

## Research Evidence

### Reaction Kind Classification (from deep code analysis)

| Kind | Handler | Inherently | Evidence |
|------|---------|-----------|----------|
| `set` | `executeSet()` | **sync** | Returns void, no promises |
| `call` | `executeCall()` | **sync** | Returns void, no promises |
| `dispatch` | `executeDispatch()` | **sync** | `document.dispatchEvent()` is sync |
| `inject` | `executeInject()` → `injectHtml()` | **sync** | DOM manipulation, `mergePlan()` — no promises |
| `show-validation-errors` | `showValidationErrors()` | **sync** | `validateContainer()` is sync. Only `async` for dynamic import |
| `branch` | condition eval + delegate | **sync** | Unless condition contains `confirm` (user dialog) |
| `sequence` | loop of steps | **mixed** | Sync until it hits an async step |
| `request` | `executeRequest()` → `fetch()` | **async** | HTTP fetch is inherently async |
| `parallel` | `Promise.allSettled()` | **async** | Parallel execution requires promises |

### The Bug

SF events like `popupOpen` call the handler synchronously and check `args.cancel` immediately after the callback returns. The current executor wraps everything in `async/await`, causing even `set event.cancel = true` to run in a microtask — after SF has already read `cancel` as `false`.

**Proven in session:** Sync execution of `set event.cancel` works. Async doesn't.

---

## Review Findings (incorporated)

### Critical: All executeReaction call sites

The reviewer identified **6 call sites** (not 4) that call `executeReaction` and need the `instanceof Promise` pattern:

| File | Lines | Pattern |
|------|-------|---------|
| `Scripts/execution/trigger.ts` | 23, 27, 35, 53 | `.catch()` on return value |
| `Scripts/execution/server-push.ts` | 91-95 | `.catch()` on return value |
| `Scripts/execution/signalr.ts` | 122-134 | `.catch()` on return value |
| `Scripts/components/native/native-action-link.ts` | 44 | `.catch()` on return value |

### http.ts internal calls — no changes needed

`http.ts` has 4 internal `await executeReaction(...)` calls (before handlers, complete handlers, route handlers). These work correctly because `await void` (i.e. `await undefined`) resolves immediately in JavaScript. The http context is already async, so the `await` is harmless and semantically correct.

### showValidationErrors — use direct static import

The validation module is already eagerly loaded by `boot.ts` via the enrichment pipeline. Replace the dynamic import with a direct static import:
```typescript
import { validateContainer, showServerErrors } from "../validation";
```
The `setValueEvaluator` pattern already breaks the circular dependency, so a static import is safe.

### inject → mergePlan — known reentrant path

`injectHtml()` calls `mergePlan()` which calls `wireBehaviors()` which may call `executeReaction` synchronously (for page-ready triggers on already-loaded documents). This is a reentrant call. It works because `executeReaction` is stateless. After the refactor it remains stateless — reentrant sync calls complete before the outer call resumes.

### Sequence async tail — error propagation

In `executeSequence`, if a sync step throws inside the `.then()` callback of a previous async step (the "remaining" loop), the error becomes a Promise rejection caught by the outer `.catch()` in `runReaction`. This is standard Promise behavior and correct.

### instanceof Promise correctness

`void` returns are `undefined` at runtime. `undefined instanceof Promise` is `false`. Native Promises from async functions satisfy `instanceof Promise`. The codebase uses only native Promises (no foreign thenables). TypeScript correctly narrows after the check.

---

## File Structure

| File | Change | Responsibility |
|------|--------|---------------|
| `Scripts/execution/execute.ts` | **Major** | Change return type, sync-first dispatch, sequence/branch rewrite |
| `Scripts/execution/trigger.ts` | **Minor** | `runReaction` helper with `instanceof Promise` check |
| `Scripts/execution/server-push.ts` | **Minor** | Same `instanceof Promise` check |
| `Scripts/execution/signalr.ts` | **Minor** | Same `instanceof Promise` check |
| `Scripts/components/native/native-action-link.ts` | **Minor** | Same `instanceof Promise` check |

**No changes needed:**
- `conditions.ts` — sync `evaluateCondition` already exists
- `http.ts` — stays async internally; `await void` works correctly
- `inject.ts` — already sync
- `types/plan.ts` — no schema changes
- Any C# code

---

### Task 1: Change executeReaction signature and sync kinds

**Files:**
- Modify: `Scripts/execution/execute.ts`

- [ ] **Step 1: Change the function signature**

Remove `async`, change return type:

```typescript
// BEFORE (line 42):
export async function executeReaction(
  reaction: Reaction,
  plan?: Plan,
  ctx?: ExecContext,
): Promise<void> {

// AFTER:
export function executeReaction(
  reaction: Reaction,
  plan?: Plan,
  ctx?: ExecContext,
): void | Promise<void> {
```

- [ ] **Step 2: Add imports**

```typescript
// BEFORE (line 15):
import { evaluateConditionAsync } from "../conditions/conditions";

// AFTER:
import { evaluateCondition, evaluateConditionAsync } from "../conditions/conditions";
```

Add Condition to type imports:
```typescript
import type {
  Plan, Reaction, SequenceReaction, ParallelReaction, BranchReaction,
  SetReaction, CallReaction, RequestReaction, DispatchReaction,
  InjectReaction, ShowValidationErrorsReaction,
  ValueProducer, ExecContext, Source, Condition,
} from "../types";
```

Replace dynamic validation import with static:
```typescript
// BEFORE (anywhere in the file):
// const { validateContainer, showServerErrors } = await import("../validation");

// AFTER (top-level):
import { validateContainer, showServerErrors } from "../validation";
```

- [ ] **Step 3: Rewrite the switch body**

```typescript
export function executeReaction(
  reaction: Reaction,
  plan?: Plan,
  ctx?: ExecContext,
): void | Promise<void> {
  const p = requirePlan(plan);

  switch (reaction.kind) {
    // ── Sync kinds: return void ──────────────────────────
    case "set":
      executeSet(reaction, p, ctx);
      return;

    case "call":
      executeCall(reaction, p, ctx);
      return;

    case "dispatch":
      executeDispatch(reaction, p, ctx);
      return;

    case "inject":
      executeInject(reaction, p, ctx);
      return;

    case "show-validation-errors":
      showValidationErrors(reaction, p, ctx);
      return;

    // ── Mixed kinds: void or Promise ─────────────────────
    case "sequence":
      return executeSequence(reaction, p, ctx);

    case "branch":
      return executeBranch(reaction, p, ctx);

    // ── Async kinds: return Promise ──────────────────────
    case "request":
      return executeRequest(reaction.request, p, ctx);

    case "parallel":
      return executeParallel(reaction, p, ctx);

    default:
      assertNever(reaction, "reaction kind");
  }
}
```

- [ ] **Step 4: Extract sequence executor**

```typescript
/**
 * Runs sequence steps synchronously until hitting an async step.
 * Sync steps (set, call, dispatch, inject, branch) execute in the same tick.
 * When an async step is encountered, returns a Promise that awaits it
 * and continues the remaining steps.
 *
 * NOTE: If a sync step throws inside the async tail (.then callback),
 * the error becomes a Promise rejection — standard JS behavior.
 * The ctx closure is safe because ExecContext is immutable by convention.
 */
function executeSequence(
  reaction: SequenceReaction,
  plan: Plan,
  ctx?: ExecContext,
): void | Promise<void> {
  for (let i = 0; i < reaction.steps.length; i++) {
    const result = executeReaction(reaction.steps[i], plan, ctx);
    if (result instanceof Promise) {
      // Sync prefix done. Return Promise for async step + remaining.
      const remaining = reaction.steps.slice(i + 1);
      return result.then(async () => {
        for (const step of remaining) {
          const r = executeReaction(step, plan, ctx);
          if (r instanceof Promise) await r;
        }
      });
    }
  }
  // All steps were sync — return void (not a Promise)
}
```

- [ ] **Step 5: Extract branch executor**

```typescript
/**
 * Evaluates branch conditions synchronously (for compare, all, any, not).
 * Falls back to async only when a condition contains a ConfirmCondition
 * (which requires user dialog interaction).
 */
function executeBranch(
  reaction: BranchReaction,
  plan: Plan,
  ctx?: ExecContext,
): void | Promise<void> {
  for (const c of reaction.cases) {
    // Confirm conditions require async — delegate entire branch to async path
    if (c.when && hasConfirm(c.when)) {
      return executeBranchAsync(reaction, plan, ctx);
    }
    // Sync condition evaluation
    if (!c.when || evaluateCondition(c.when, plan, ctx)) {
      return executeReaction(c.reaction, plan, ctx); // void or Promise
    }
  }
  log.trace("no-branch-taken");
}

/** Walk condition tree checking for ConfirmCondition nodes. */
function hasConfirm(condition: Condition): boolean {
  switch (condition.kind) {
    case "confirm": return true;
    case "all": case "any": return condition.terms.some(hasConfirm);
    case "not": return hasConfirm(condition.term);
    default: return false;
  }
}

/** Async branch fallback — only used when a condition contains ConfirmCondition. */
async function executeBranchAsync(
  reaction: BranchReaction,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  for (const c of reaction.cases) {
    if (!c.when || await evaluateConditionAsync(c.when, plan, ctx)) {
      const r = executeReaction(c.reaction, plan, ctx);
      if (r instanceof Promise) await r;
      return;
    }
  }
  log.trace("no-branch-taken");
}
```

- [ ] **Step 6: Extract parallel executor**

```typescript
async function executeParallel(
  reaction: ParallelReaction,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  const results = await Promise.allSettled(
    reaction.steps.map(s => {
      const r = executeReaction(s, plan, ctx);
      return r instanceof Promise ? r : Promise.resolve();
    })
  );
  for (const r of results) {
    if (r.status === "rejected") log.error("parallel step failed", { error: String(r.reason) });
  }
  if (reaction.onSettled) {
    const r = executeReaction(reaction.onSettled, plan, ctx);
    if (r instanceof Promise) await r;
  }
}
```

- [ ] **Step 7: Change showValidationErrors to sync**

```typescript
// BEFORE:
async function showValidationErrors(
  reaction: ShowValidationErrorsReaction,
  plan: Plan,
  ctx?: ExecContext,
): Promise<void> {
  const { validateContainer, showServerErrors } = await import("../validation");
  if (ctx?.response && typeof ctx.response === "object") {
    showServerErrors(plan, reaction.container, ctx.response);
  } else {
    validateContainer(plan, reaction.container, ctx);
  }
}

// AFTER:
function showValidationErrors(
  reaction: ShowValidationErrorsReaction,
  plan: Plan,
  ctx?: ExecContext,
): void {
  if (ctx?.response && typeof ctx.response === "object") {
    showServerErrors(plan, reaction.container, ctx.response);
  } else {
    validateContainer(plan, reaction.container, ctx);
  }
}
```

The `validateContainer` and `showServerErrors` are now imported statically at the top of the file (Step 2).

- [ ] **Step 8: Run `npm run typecheck`**

```bash
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/.codex-worktrees/final-reactive-schema && npm run typecheck
```

Expected: Clean (no errors).

- [ ] **Step 9: Run `npm run build`**

```bash
npm run build
```

Expected: Build succeeds, bundle size similar (~92kb).

---

### Task 2: Update all call sites — trigger.ts, server-push.ts, signalr.ts, native-action-link.ts

**Files:**
- Modify: `Scripts/execution/trigger.ts`
- Modify: `Scripts/execution/server-push.ts`
- Modify: `Scripts/execution/signalr.ts`
- Modify: `Scripts/components/native/native-action-link.ts`

- [ ] **Step 1: Add runReaction helper to trigger.ts**

```typescript
/**
 * Execute a reaction and handle errors for both sync and async paths.
 * Sync reactions: errors caught by try/catch.
 * Async reactions: errors caught by .catch() on the Promise.
 *
 * This is the critical fix: the callback is synchronous. For pure sync
 * reactions (set, call, branch with compare conditions), the entire
 * execution completes before this function returns — in the same tick
 * as the SF event callback. SF checks args.cancel AFTER this returns,
 * so the mutation is visible.
 */
function runReaction(reaction: Reaction, plan: Plan, ctx: ExecContext): void {
  try {
    const result = executeReaction(reaction, plan, ctx);
    if (result instanceof Promise) {
      result.catch(err => log.error("reaction failed", { error: String(err) }));
    }
  } catch (err) {
    log.error("reaction failed (sync)", { error: String(err) });
  }
}
```

- [ ] **Step 2: Replace all 4 call sites in trigger.ts**

Replace every `executeReaction(reaction, plan, ctx).catch(...)` with `runReaction(reaction, plan, ctx)`:

1. page-ready (DOMContentLoaded) — line 23
2. page-ready (already loaded) — line 27
3. document-event — line 35
4. component-event — line 53

- [ ] **Step 3: Update server-push.ts**

Find `executeReaction(reaction, plan, { event: evt }).catch(...)` and replace with the same `instanceof Promise` pattern. Either import `runReaction` or inline the check:

```typescript
const result = executeReaction(reaction, plan, { event: evt });
if (result instanceof Promise) {
  result.catch(err => log.error("reaction failed", { error: String(err) }));
}
```

- [ ] **Step 4: Update signalr.ts**

Same treatment as server-push.ts.

- [ ] **Step 5: Update native-action-link.ts (line 44)**

Same treatment — replace `.catch()` with `instanceof Promise` check.

- [ ] **Step 6: Run `npm run typecheck` — expect clean**
- [ ] **Step 7: Run `npm run build` — expect clean**

---

### Task 3: Full verification

- [ ] **Step 1: npm test (vitest)**

```bash
npm test
```

Expected: Pass (or no test files exist — vitest config present but `__tests__` dir is empty).

- [ ] **Step 2: C# unit tests**

```bash
dotnet test tests/Alis.Reactive.UnitTests -nologo
dotnet test tests/Alis.Reactive.Native.UnitTests -nologo
dotnet test tests/Alis.Reactive.Fusion.UnitTests -nologo
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests -nologo
```

Expected: All pass (223 total). C# code didn't change.

- [ ] **Step 3: Playwright tests — THE critical gate**

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests -nologo
```

Expected: All 779 pass. If any test fails, the refactor broke something.

- [ ] **Step 4: Manual verification — Schedule popupOpen cancel with When condition**

1. Start SandboxApp: `dotnet run --project Alis.Reactive.SandboxApp --urls "http://localhost:5173"`
2. Go to `http://localhost:5173/Sandbox/Components/Schedule`
3. Double-click an event
4. Expected: SF editor does NOT open (cancelled). NativeDrawer opens with edit form.
5. Single-click an event: QuickInfo popup still works (When condition only matches "Editor").
6. Console trace: `[alis:conditions] eval {"op":"eq","left":"Editor","right":"Editor"}` → `[alis:execute] set {"target":"event","property":"cancel","value":true}`

- [ ] **Step 5: Manual verification — existing AutoComplete filtering**

1. Go to `http://localhost:5173/Sandbox/Components/AutoComplete`
2. Type in the server-filtered medication field
3. Expected: SF default filter cancelled via `preventDefaultAction`, server results appear

- [ ] **Step 6: Manual verification — Confirm dialog still works**

1. Go to `http://localhost:5173/Sandbox/Conditions/Guards`
2. Click "Trigger Confirm" button
3. Expected: Confirm dialog appears, OK proceeds, Cancel aborts (async branch path)

---

## Risk Assessment

**Low** — 7 of 9 reaction handlers are already synchronous functions. The refactor removes unnecessary `async/await` wrapping. The 2 async paths (`request`, `parallel`) stay exactly the same.

**The new logic:**
1. `executeSequence`: `instanceof Promise` check to detect sync→async boundary
2. `executeBranch`: `hasConfirm()` to decide sync vs async condition eval
3. `executeBranchAsync`: fallback for ConfirmCondition branches
4. `runReaction`: `try/catch` + `instanceof Promise` for error handling at trigger boundary

**Known safe paths:**
- `http.ts` internal `await executeReaction(...)` — `await void` resolves immediately, no behavior change
- `inject` → `mergePlan` → reentrant `executeReaction` — stateless executor, reentrant sync calls complete correctly
- Sequence async tail — sync errors in `.then()` become rejections, caught by `runReaction`

**779 Playwright tests** verify every existing behavior works after the change.
