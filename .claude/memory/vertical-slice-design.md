---
name: vertical-slice-design
description: Component vertical slice architecture redesign — IInputComponent, ComponentsMap, vendor on commands, structured BindSource, TypedComponentSource for conditions
type: project
---

# Component Vertical Slice Architecture Redesign

## Design Doc
`docs/plans/2026-03-12-component-vertical-slice-design.md` — committed as `94927e4`

## Core Design Decisions (User-Approved)

### 1. Interface Contracts Replace Reflection (C# 8.0)
- `IComponent` gets instance `string Vendor { get; }`
- `IInputComponent : IComponent` gets instance `string ReadExpr { get; }`
- `ComponentRef<TComponent>` uses `where TComponent : IComponent, new()` + cached static instance
- DELETE: `ReadExprAttribute`, `ComponentHelper`, `IReadableComponent`
- Zero reflection for component metadata

### 2. ComponentsMap — Single Source of Truth
- Builder extensions take `plan` parameter: `Html.NumericTextBoxFor(plan, m => m.Amount)`
- Component registers with plan's ComponentsMap at CREATION TIME (not at .Reactive() time)
- Map keyed by bindingPath → { ComponentId, Vendor, BindingPath, ReadExpr }
- GatherResolver expands AllGather from map (already works, just different storage)
- ValidationResolver looks up vendor + readExpr from map (fixes hardcoded "native"/"value")

### 3. Vendor on MutateElementCommand
- ComponentRef carries vendor via `new()` + cached instance and passes to MutateElementCommand
- Runtime resolves vendor root BEFORE jsEmit: `el = cmd.vendor ? resolveRoot(domEl, vendor) : domEl`
- jsEmit operates on component root, NOT DOM element
- Plain elements (no vendor) → el = DOM element (unchanged)
- Fusion jsEmit becomes clean: `"el.value=Number(val)"` not `"var c=el.ej2_instances[0]; c.value=..."`

### 4. Structured BindSource (Unifies with Guards)
- MutateElementCommand.source changes from `string` (BindExpr) to `BindSource` (structured object)
- BindSource gains ComponentSource variant: `{ kind: "component", componentId, vendor, readExpr }`
- Matches how Guards already use BindSource — removes inconsistency
- Runtime element.ts calls `resolveSource()` (switch on kind) instead of `resolveEventPath()`

### 5. TypedComponentSource Preserves Typed Conditions
- `TypedComponentSource<TProp> : TypedSource<TProp>` — new class alongside EventArgSource
- `.Value()` returns `TypedComponentSource<decimal>` not `ComponentSource` — carries type info
- Plugs into existing `ConditionSourceBuilder<TModel, TProp>` unchanged
- Guard operators remain typed (Gt/Lt only on numbers, Contains only on strings)
- New PipelineBuilder overload: `When<TProp>(TypedSource<TProp> source)`
- Developer experience: `p.When(comp.Value(), g => g.Gt(0), ...)` — fully typed

### 6. Scope
- FusionNumericTextBox: expand JS API (events: change/focus/blur, methods: focusIn/focusOut/increment/decrement, properties: value/min read+write)
- FusionDropDownList: new vertical slice
- Sandbox pages at `/Sandbox/Components/Fusion/ComponentName.cshtml`
- All existing components updated to new architecture

### 7. What's Deleted
- `ReadExprAttribute`, `ComponentHelper`, `IReadableComponent`
- `ReadExprOverrides` in `RequestBuildContext`
- `.ReadExpr()` on `HttpRequestBuilder`
- `WithReadExpr()` on `ValidationDescriptor`
- `ExtractProperty()` reflection in Fusion reactive extensions
- Hardcoded "native"/"value" in FluentValidationAdapter

### 8. Breaking Changes
- Builder extensions gain `plan` parameter (plan-less overload kept for non-reactive)
- `.Value()`/`.Checked()` return `TypedComponentSource<T>` instead of `string`
- MutateElementCommand.source: string → BindSource (all snapshots regenerate)
- `IReadableComponent` → `IInputComponent` (rename)

## Status
- Design approved by user
- Implementation plan: WRITTEN — `docs/plans/2026-03-12-component-vertical-slice-plan.md` (committed `63633fa`)
- C# 8.0 constraint acknowledged — `static abstract` → instance properties + `new()` constraint
- Awaiting execution approach choice (subagent-driven vs parallel session)
