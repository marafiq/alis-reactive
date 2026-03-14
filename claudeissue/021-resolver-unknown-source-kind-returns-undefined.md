# FAIL-FAST-021: resolver.ts Returns undefined for Unknown Source Kind

## Status: Fail-Fast Violation (Rule 8)

## File
`Scripts/resolver.ts:33-34`

## How to Reproduce

1. Add a new `BindSource` kind to the C# DSL (e.g., `"session"` for session storage binding).
2. Add the kind to `types.ts` and the JSON schema.
3. Forget to add the handler in `resolver.ts`.
4. At runtime, `resolveSource` hits the `default` case:
   ```typescript
   default:
     return undefined;
   ```
5. `val` becomes `undefined` in `element.ts:13`.
6. `new Function("el", "val", "el.textContent = val").call(null, el, undefined)` executes.
7. The element displays the text `"undefined"` — literally the word "undefined" rendered in the DOM.

## Deep Reasoning: Why This Is a Real Issue

The `resolver.ts` module is the bridge between `BindSource` descriptors in the plan and runtime values. It is called by:
- `element.ts` — for mutate-element commands with source binding
- `conditions.ts` — for guard evaluation (via `resolveSource` and `resolveSourceAs`)

When a source kind is unrecognized, returning `undefined` is a **silent failure** that propagates through the entire execution chain. The `undefined` value becomes `val` in jsEmit expressions, which then:
- Sets `el.textContent = undefined` → renders "undefined" in the DOM
- Sets `el.classList.add(undefined)` → adds CSS class literally named "undefined"
- Passes to `coerce(undefined, "number")` → returns `0` (valid-looking but wrong)
- Passes to `coerce(undefined, "string")` → returns `""` (empty, not obviously wrong)

Every downstream consumer gets a plausible-looking but incorrect value. None of them can detect that the source resolution failed.

Compare with `component.ts:26` which correctly throws for unknown vendors: `throw new Error('[alis] unknown vendor: "${vendor}"')`. The resolver should follow the same pattern.

## How Fixing This Improves the Codebase

1. **Immediate error on new source kinds**: Replace `return undefined` with `throw new Error('[alis] unknown source kind: "${source.kind}"')`.
2. **Development feedback**: The error message names the exact source kind that needs a handler.
3. **Consistent with component.ts**: Both modules dispatch on a `kind` discriminator — both should throw on unknown kinds.

## How This Fix Will Not Break Existing Features

- The two existing source kinds (`"event"` and `"component"`) are both handled. The `default` case only fires for unknown kinds.
- No existing plan produces an unknown source kind.
- The fix is purely additive — it adds error handling for a code path that currently silently fails.
