# Known Issues — v1.0

Issues identified during quality audit. Deferred to post-1.0.

## Important

### 1. ElementBuilder.Show()/Hide() return type inconsistency
**File:** `Alis.Reactive/Builders/ElementBuilder.cs:161,171`

`Show()` and `Hide()` return `PipelineBuilder<TModel>` instead of `ElementBuilder<TModel>`. Breaks fluent chaining:
```csharp
p.Element("x").Show().AddClass("active")  // Fails — Show() exits element context
```
**Fix:** Change return type to `ElementBuilder<TModel>`, return `this`.

### 2. conditions.ts — silent fall-through when itemShape set on non-array
**File:** `Scripts/conditions/conditions.ts:122-127`

When `cond.itemShape` is present but source resolves to a scalar (not an array), the guard silently falls through — `items` becomes the scalar value, and the condition evaluates to `false` instead of throwing. Violates Rule 7 (fail-fast).

### 3. resolver.ts — error message missing componentId
**File:** `Scripts/resolution/resolver.ts:72`

`unknown vendor` error doesn't include which component failed. Hard to trace in large forms.

## Minor

### 4. Validation module needs refactor
Observed during ComponentGather testing — validation behaves differently from what the interactive pattern was designed for. Needs stricter module boundaries. Separate design session required.
