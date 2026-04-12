# Known Issues — v1.0

Issues identified during quality audit. Deferred to post-1.0.

## Important

### 1. ElementBuilder.Show()/Hide() return type inconsistency
**File:** `Alis.Reactive/Builders/ElementBuilder.cs:161,171`

`Show()` and `Hide()` return `PipelineBuilder<TModel>` instead of `ElementBuilder<TModel>`,
which exits the element context and breaks fluent chaining:
```csharp
p.Element("x").Show().AddClass("active")  // Fails — Show() returns PipelineBuilder, AddClass is on ElementBuilder
```
**Note:** AddClass, RemoveClass, ToggleClass, SetText(string), and SetHtml(string) also
return `PipelineBuilder<TModel>`. A design decision is needed on whether all ElementBuilder
methods should return `ElementBuilder<TModel>` for fluent element-level chaining, or whether
the current pattern (exit to pipeline after each mutation) is intentional.

### 2. conditions.ts — silent fall-through when itemShape set on non-array
**File:** `Alis.Reactive.SandboxApp/Scripts/conditions/conditions.ts:122-127`

In the `array-contains` case, when `cond.itemShape` is present but the source resolves to a
scalar (not an array), the ternary falls through to `items = shapedLeft`. Then
`Array.isArray(items)` returns `false`, and the entire expression evaluates to `false` silently.
No error is thrown. Violates Rule 7 (Fail Fast — Fallbacks Are Exceptions) in CLAUDE.md.

### 3. resolver.ts — unknown vendor error missing element ID
**File:** `Alis.Reactive.SandboxApp/Scripts/resolution/resolver.ts:72`

The `resolveVendorRoot` function's default branch throws `[alis] unknown vendor: "${_}"` but
does not include `el.id` (available in scope via the `el: HTMLElement` parameter). In a form
with 20+ components, the error is undiagnosable without the element ID or component key.

