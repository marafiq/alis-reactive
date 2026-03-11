# IdGenerator Default + Plan-Driven Gather

## Problem

Expression-based components generate element IDs from property paths (`"Status"`, `"Address_City"`).
Same model on same page = ID collision. Manual `idPrefix` doesn't scale for 100+ components.

Gather uses DOM scanning (`IncludeAll(formId)` walks the DOM for `name` attributes).
The runtime decides what to gather — violates "plan carries all, runtime is dumb."

## Solution

### IdGenerator — collision-free IDs by default

Format: `{FullName}__{MemberPath}` (double underscore delimiter)

```
Alis_Reactive_SandboxApp_Models_OrderModel__Address_City
↑ scope (unique per type)                  ↑ property path
```

- `TypeScope(type)` = `type.FullName.Replace('.', '_').Replace('+', '_')`
- Guaranteed unique — .NET FullName is unique per type
- All vendors produce same ID for same expression
- Splittable on `__` for future parsing
- `name` attribute (MVC binding) NEVER affected

### All expression-based paths use IdGenerator

| Call site | Today | After |
|-----------|-------|-------|
| `NativeDropDownFor(expr)` | `html.IdFor(expr)` | `IdGenerator.For<TModel,TProp>(expr)` |
| `NumericTextBoxFor(expr).Reactive()` | SF auto-ID | IdGenerator via HtmlAttributes |
| `Component<T>(expr)` | `ToElementId(expr)` | `IdGenerator.For<TModel>(expr)` |
| `Include<T>(expr)` | `ToElementId(expr)` | `IdGenerator.For<TModel>(expr)` |
| `FieldExtensions` | `html.IdFor(expr)` | `IdGenerator.For<TModel,TProp>(expr)` |
| `FluentValidationAdapter` | `propertyPath.Replace(".", "_")` | `IdGenerator.For<TModel>(expr)` |

No opt-in. No `As` prefix. No two paths. String-ref overloads stay for non-input elements.

### Plan-driven gather (replaces DOM scanning)

The plan carries exact gather targets. Every component rendered through framework builders
is known at plan-build time.

```csharp
// Explicit — plan carries each target
p.Post("/save", g =>
{
    g.Include<NativeDropDown>(m => m.Status);
    g.Include<FusionNumericTextBox>(m => m.Amount);
})

// IncludeAll — plan-driven, gathers ALL component IDs in the plan for this request
// No form ID needed, no DOM scanning
p.Post("/save", g => g.IncludeAll())
```

Plan JSON:
```json
"gather": [
  { "kind": "component", "componentId": "...Model__Status", "vendor": "native", "name": "Status", "readExpr": "value" },
  { "kind": "component", "componentId": "...Model__Amount", "vendor": "fusion", "name": "Amount", "readExpr": "value" }
]
```

Runtime: reads the list, calls `evalRead(componentId, vendor, readExpr)` for each,
builds payload with `name` as key. No DOM scanning.

### Validation simplification

`FluentValidationAdapter` uses `IdGenerator.For<TModel>(expr)` — exact same ID as rendered.
`WithPrefix()` removed. `idPrefix` overload on `FieldExtensions` removed.

## Removals

- `AsNativeDropDownFor` / `AsNumericTextBoxFor` — become the default, `As` prefix gone
- `FieldExtensions.Field(label, required, expr, idPrefix, inputBuilder)` — removed
- `ValidationDescriptor.WithPrefix()` — removed
- `IncludeAll(formId)` with form ID param — replaced by parameterless `IncludeAll()`

## Files to Modify

### Core
- `PipelineBuilder.cs` — `Component<T>(expr)` uses IdGenerator

### Native
- `NativeDropDownBuilder.cs` — default constructor uses IdGenerator
- `NativeGatherExtensions.cs` — `Include<T>(expr)` uses IdGenerator
- `FieldExtensions.cs` — uses IdGenerator, remove idPrefix overload

### Fusion
- `FusionNumericTextBoxReactiveExtensions.cs` — HtmlAttributes with IdGenerator
- `FusionGatherExtensions.cs` — `Include<T>(expr)` uses IdGenerator

### Validation
- `FluentValidationAdapter.cs` — uses IdGenerator
- `ValidationDescriptor.cs` — remove WithPrefix

### Views
- All existing views — update any hardcoded ID references
- Validation views — remove all idPrefix usage

### Tests
- Update snapshot `.verified.txt` files
- Update Playwright selectors
- Add test: `Component<T>(expr)` target matches rendered element ID

## Principle

One ID strategy. Plan carries targets. Runtime is dumb. Every module becomes simple.
