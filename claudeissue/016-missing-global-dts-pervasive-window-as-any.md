# TYPE-SAFETY-016: Missing global.d.ts — Pervasive (window as any) Casts

## Status: Type Safety Gap

## Files (all `(window as any)` or `(el as any)` casts)
- `Scripts/conditions.ts:46` — `(window as any).alis?.confirm?.(guard.message)`
- `Scripts/confirm.ts:20,31,32` — `(window as any).alis`, `ej.popups.Dialog`, `ej.base.append`
- `Scripts/component.ts:16,37` — `resolveRoot(el: any, ...)`, `document.getElementById(id) as any`
- `Scripts/trigger.ts:39` — `(root as EventTarget).addEventListener`, `(e: any)`
- `Scripts/test-widget.ts:108,109` — `(el as any).ej2_instances`

## How to Reproduce

1. In any IDE, hover over `(window as any).alis.confirm` — no autocomplete, no type information.
2. Rename the `confirm` function to `showConfirm` in `confirm.ts`.
3. Build succeeds — TypeScript cannot detect the broken reference because `(window as any)` bypasses all checking.
4. At runtime, `window.alis.confirm` is `undefined`. The `?.` optional chain returns `undefined`, which is coerced to `Promise.resolve(false)`. All confirm dialogs silently deny.

## Deep Reasoning: Why This Is a Real Issue

The codebase has `"strict": true` in `tsconfig.json`, but then circumvents it with `as any` in every file that touches vendor APIs or globals. This creates a false sense of type safety — the compiler reports zero errors, but entire call chains are unchecked.

The `(window as any)` pattern is particularly dangerous because:
1. **No refactoring support**: Renaming a property on `window.alis` requires manual grep across all files. The IDE's "Find All References" cannot track `(window as any).alis.confirm`.
2. **No autocomplete**: Developers must know the exact API shape from memory or documentation.
3. **No null safety**: The `?.` optional chaining on line 46 looks like proper null handling, but it is actually papering over a potential initialization-order bug. If `confirm.ts` fails to initialize, the confirm function is truly missing, and the optional chain silently returns `false` — which is a deny-all behavior, not a "function is optional" behavior.

The `ej2_instances` pattern in `component.ts` and `test-widget.ts` has the same issue. The Syncfusion component instance interface (`{value: any, checked: boolean, dataBind(): void}`) is never declared, so any property access on the resolved root is unchecked.

## How Fixing This Improves the Codebase

A single `Scripts/global.d.ts` file eliminates all `as any` casts:

```typescript
declare global {
  interface Window {
    alis: { confirm: (message: string) => Promise<boolean> };
  }
  interface HTMLElement {
    ej2_instances?: unknown[];
  }
}
```

Benefits:
1. **Refactoring safety**: Renaming `window.alis.confirm` triggers compile errors at every call site.
2. **Autocomplete**: IDEs show the correct API shape when typing `window.alis.`.
3. **Null checks become meaningful**: `if (!window.alis?.confirm)` is now a typed check, not an `any`-chain.
4. **No runtime changes**: `declare global` only adds compile-time type information.

## How This Fix Will Not Break Existing Features

- `declare global` adds type information without generating any JavaScript code.
- Every `(window as any)` cast can be replaced with the direct typed access.
- The `as any` casts on `el` in `component.ts` can be replaced with `HTMLElement` + the `ej2_instances` augmentation.
- All existing runtime behavior is identical — only the compiler's understanding changes.
- All tests pass unchanged because the fix is compile-time only.
