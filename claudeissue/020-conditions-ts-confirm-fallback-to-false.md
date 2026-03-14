# FAIL-FAST-020: conditions.ts Confirm Guard Silently Returns false When window.alis Missing

## Status: Fail-Fast Violation (Rule 8)

## File
`Scripts/conditions.ts:46`

## How to Reproduce

1. Configure a confirm guard in a plan:
   ```csharp
   p.Confirm("Are you sure you want to delete?")
    .Then(t => t.Delete("/api/item/123"));
   ```
2. Remove or break the confirm initialization (e.g., the `#alis-confirm-dialog` element is missing from the layout).
3. `confirm.ts:init()` silently returns without setting `window.alis.confirm` (line 15: `if (!el) return`).
4. User clicks the delete button.
5. `evaluateGuardAsync` reaches the confirm case (line 46):
   ```typescript
   return (window as any).alis?.confirm?.(guard.message) ?? Promise.resolve(false);
   ```
6. `window.alis` is `undefined` → `?.confirm` is `undefined` → `?.(message)` is `undefined` → `?? Promise.resolve(false)` → returns `false`.
7. The guard returns `false`. The branch is not taken. The delete does not execute.
8. The user clicks the button and nothing happens. No dialog, no action, no error message.

## Deep Reasoning: Why This Is a Real Bug

The `??` fallback converts a **broken confirmation system** into a **silent denial**. The developer configured a confirm dialog because the action is destructive (delete). The expectation is:
- Dialog shows → user confirms → action executes
- Dialog shows → user cancels → action blocked

What actually happens with a broken confirm:
- No dialog → action silently blocked → user confused

This is a fail-safe default (deny when uncertain), which seems reasonable for destructive actions. But it violates the framework's Rule 8 because the developer has no way to know the confirm system is broken. They might spend hours debugging why the delete button "doesn't work" without ever discovering that the confirm dialog element is missing from the layout.

The correct behavior is to throw an error: "Confirm dialog not initialized. Ensure #alis-confirm-dialog exists in the layout." This immediately tells the developer what is wrong and how to fix it.

## How Fixing This Improves the Codebase

1. **Fail-fast on missing infrastructure**: If `window.alis.confirm` is not set, throw instead of returning `false`.
2. **Clear diagnostics**: The error message points to the exact fix (add the confirm dialog element).
3. **Consistent with other missing-element errors**: `trigger.ts:36` throws for missing component elements. Missing confirm infrastructure should throw similarly.

## How This Fix Will Not Break Existing Features

- The `confirm.ts:init()` function runs at module load time in `auto-boot.ts:6`. If the `#alis-confirm-dialog` element exists in the layout (which it does in the sandbox app), `window.alis.confirm` is set, and the fix never triggers.
- The fix only triggers when `window.alis.confirm` is truly missing, which is always a configuration error.
- Applications that don't use confirm guards never reach this code path — `needsAsync()` returns `false`, and the sync `executeReaction` path is used.
