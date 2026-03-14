# DESIGN-017: execute.ts dispatchAsync Is a Near-Exact Copy of executeReaction (DRY Violation)

## Status: Design Issue — DRY Violation

## File
`Scripts/execute.ts:23-66` (sync) vs `Scripts/execute.ts:72-112` (async)

## How to Reproduce

1. Open `execute.ts` and compare `executeReaction` (lines 23-66) with `dispatchAsync` (lines 72-112).
2. Both functions have:
   - Identical `switch` on `reaction.kind` with cases for `sequential`, `conditional`, `http`, `parallel-http`
   - Identical logging (`log.debug(...)`)
   - Identical command execution loops (`for (const cmd of reaction.commands) executeCommand(cmd, ctx)`)
   - Identical HTTP dispatch (`executeHttpReaction(reaction, ctx)`)
3. The only differences:
   - `dispatchAsync` uses `await` before recursive calls and HTTP execution
   - `dispatchAsync` uses `evaluateGuardAsync` instead of `evaluateGuard`
   - `dispatchAsync` handles `branch.guard == null` separately (line 89) instead of combining it with guard evaluation (line 47)

## Deep Reasoning: Why This Is a Real Issue

This duplication means every bug fix, every new reaction kind, and every behavioral change must be made in two places. The async path was introduced for ConfirmGuard support — a single feature that required duplicating the entire execution engine.

The duplication has already caused a subtle inconsistency: the sync path (line 47) handles `branch.guard == null` and guard evaluation in a single expression: `if (branch.guard == null || evaluateGuard(...)`. The async path (lines 89-97) separates them into two `if` blocks. While functionally equivalent today, this divergence means future changes to branch evaluation logic could be applied to one path but not the other.

Additionally, the fire-and-forget pattern on line 27 is concerning:
```typescript
dispatchAsync(reaction, ctx);
return;
```
The `dispatchAsync` promise is neither awaited nor `.catch()`-ed. If an HTTP request in the async path throws (network error, server error), the exception is swallowed as an unhandled promise rejection.

## How Fixing This Improves the Codebase

1. **Single execution path**: Refactor to a single `async` execution function. The `needsAsync` check at the top can remain as an optimization hint, but the execution logic should not be duplicated.
2. **Error handling**: The promise returned by the execution function should be `.catch()`-ed at the call site in `trigger.ts` or `execute.ts`.
3. **Future-proof**: When new reaction kinds are added, they only need one handler.

## How This Fix Will Not Break Existing Features

- The sync path is already identical to the async path in behavior. Merging them into one `async` function that is `await`-ed when needed does not change behavior.
- The `needsAsync` optimization can be preserved: if no confirm guards exist, the execution function can run synchronously (by not hitting any `await` points — async functions that don't `await` execute synchronously up to the first `await`).
- All existing tests exercise both paths through `ConfirmGuard` tests and non-confirm tests. Both will continue to work with a unified function.
