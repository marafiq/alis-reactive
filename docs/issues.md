# Known Issues — v1.0

Issues identified during quality audit. Deferred to post-1.0.

## Important

### 1. ElementBuilder.Show()/Hide() return type inconsistency
**File:** `Alis.Reactive/Builders/ElementBuilder.cs:124-134`

`Show()` and `Hide()` return `PipelineBuilder<TModel>` instead of `ElementBuilder<TModel>`. Breaks fluent chaining:
```csharp
p.Element("x").Show().AddClass("active")  // Fails — Show() exits element context
```
**Fix:** Change return type to `ElementBuilder<TModel>`, return `this`.

### 2. Schema doesn't constrain MutateElementCommand.value
**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json:344`

`value` field is unconstrained (`{}`). Should be `oneOf: [{"type": "string"}, {"type": "array", "items": {"type": "string"}}]` to match `types.ts` contract (`string | string[]`).

## Minor

### 3. conditions.ts — silent undefined when elementCoerceAs set on non-array
**File:** `Scripts/conditions.ts:105-106`

When `elementCoerceAs` is present but source resolves to scalar, `items` becomes `undefined`. Should throw early with diagnostic message.

### 4. component.ts — error message missing componentId
**File:** `Scripts/component.ts:16-27`

`unknown vendor` error doesn't include which component failed. Hard to trace in large forms.

### 5. Validation module needs refactor
Observed during ComponentGather testing — validation behaves differently from what the interactive pattern was designed for. Needs stricter module boundaries. Separate design session required.
