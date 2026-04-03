# IdGenerator Default + Plan-Driven Gather — Full Design Context

## The Problem
Expression-based components (`NativeDropDownFor(m => m.Status)`) generate element IDs from
the expression path (`"Status"`). When the same model appears twice on a page, IDs collide.
Manual `idPrefix` workarounds don't scale for 100+ component onboarding.

## The Solution: IdGenerator

### Format
`{FullName with dots→underscores}__{MemberPath}`

Example: `Alis_Reactive_SandboxApp_Areas_Sandbox_Models_IdGeneratorModel__Address_City`

- Double underscore `__` delimits scope from property path (splittable)
- `TypeScope(typeof(TModel))` = `type.FullName.Replace('.', '_').Replace('+', '_')`
- Guaranteed unique — FullName is unique per type in .NET
- All vendors (SF, native, future) produce the same ID for the same expression

### Implementation (already committed as `96f82c3`)
- `Alis.Reactive/IdGenerator.cs` — `For<TModel>(expr)`, `For<TModel,TProp>(expr)`, `TypeScope(type)`
- `ExpressionPathHelper.ToElementId<TModel,TProp>(expr)` — typed overload added
- `AsNativeDropDownFor(expr)` — native extension using IdGenerator (in NativeDropDownBuilder.cs)
- `AsNumericTextBoxFor(expr)` — SF extension using IdGenerator (FusionNumericTextBoxIdExtensions.cs)
- `NativeDropDownBuilder` — internal constructor accepting pre-computed elementId
- `NativeDropDownBuilder.WriteTo` — always passes `_elementId` as HTML `id` attribute
- Unit tests: `WhenGeneratingUniqueIds.cs` — 10 tests
- Playwright tests: `WhenUsingCollisionFreeIds.cs` — 6 tests (JSON POST, FormData POST, name attrs, DOM IDs, delimiter)
- Sandbox page: `/Sandbox/IdGenerator` — mixed model, nested props, SF + native

### What the `name` attribute does
- `name` = model binding path for MVC posting (`"Status"`, `"Address.City"`)
- NEVER affected by IdGenerator — only `id` changes
- `IncludeAll(formId)` gathers by `name` → posting works regardless of ID

## NEXT: Make IdGenerator the Default Everywhere

### Part 1: IdGenerator replaces all expression-based ID generation

Every call site that uses `ExpressionPathHelper.ToElementId(expr)` or `html.IdFor(expr)`
switches to `IdGenerator.For<TModel>(expr)`. No opt-in, no two paths.

| Call site | File | Today | After |
|-----------|------|-------|-------|
| `NativeDropDownFor(expr)` | `NativeDropDownBuilder.cs` | `html.IdFor(expr)` | `IdGenerator.For<TModel,TProp>(expr)` |
| `NumericTextBoxFor(expr).Reactive()` | `FusionNumericTextBoxReactiveExtensions.cs` | SF auto-ID via `ExtractProperty("ID")` | IdGenerator ID via HtmlAttributes |
| `Component<T>(expr)` | `PipelineBuilder.cs:56` | `ExpressionPathHelper.ToElementId(expr)` | `IdGenerator.For<TModel>(expr)` |
| `Include<T>(expr)` | `NativeGatherExtensions.cs:25` + `FusionGatherExtensions.cs` | `ExpressionPathHelper.ToElementId(expr)` | `IdGenerator.For<TModel>(expr)` |
| `FieldExtensions` | `FieldExtensions.cs` | `html.IdFor(expr)` for label `for=` | `IdGenerator.For<TModel,TProp>(expr)` |
| `FluentValidationAdapter` | `FluentValidationAdapter.cs` | `propertyPath.Replace(".", "_")` | `IdGenerator.For<TModel>(expr)` |
| `ValidationDescriptor.WithPrefix()` | `ValidationDescriptor.cs` | manual prefix remapping | REMOVED — IdGenerator makes it unnecessary |

The `As` prefix variants (`AsNativeDropDownFor`, `AsNumericTextBoxFor`) become the default
behavior of `NativeDropDownFor` and the SF reactive extension. The `As` variants are removed.

String-ref overloads stay unchanged: `Component<T>("my-btn")`, `Include<T>(refId, name)`.
The `idPrefix` overload on `FieldExtensions` is removed.

### Part 2: Plan-driven gather (replaces DOM scanning)

**Today:** `IncludeAll(formId)` → runtime scans DOM for all elements with `name` attributes.
The runtime figures out what's in the form. Plan just says "scan this form."

**After:** Expression-based `Include<T>(expr)` is the primary gather path. The plan carries
the exact gather targets — componentId (from IdGenerator), vendor, name, readExpr.

```csharp
// Plan-driven (primary) — plan carries exact targets
p.Post("/save", g =>
{
    g.Include<NativeDropDown>(m => m.Status);
    g.Include<FusionNumericTextBox>(m => m.Amount);
    g.Include<NativeDropDown>(m => m.Address.City);
})
```

Plan JSON:
```json
"gather": [
  { "kind": "component", "componentId": "...Model__Status", "vendor": "native", "name": "Status", "readExpr": "value" },
  { "kind": "component", "componentId": "...Model__Amount", "vendor": "fusion", "name": "Amount", "readExpr": "value" }
]
```

Runtime: reads the list, calls `evalRead(componentId, vendor, readExpr)` for each, builds
JSON payload with `name` as key. No DOM scanning.

**`IncludeAll()` becomes plan-driven too** — no form ID needed. The plan already knows
every component rendered for that request. `IncludeAll()` just gathers ALL component IDs
in the plan's gather list. No form ID, no DOM scanning. Every module becomes dumb.

### Part 3: Validation simplification

`FluentValidationAdapter` no longer guesses IDs with `propertyPath.Replace(".", "_")`.
It uses `IdGenerator.For<TModel>(expr)` to get the exact same ID the element was rendered with.
`WithPrefix()` is removed entirely — IdGenerator makes it unnecessary.

## Key Files to Modify

### Core
- `Alis.Reactive/IdGenerator.cs` — already exists, no changes needed
- `Alis.Reactive/ExpressionPathHelper.cs` — typed overload already exists
- `Alis.Reactive/Builders/PipelineBuilder.cs` — line 56, `Component<T>(expr)`

### Native
- `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDownBuilder.cs` — default constructor uses IdGenerator
- `Alis.Reactive.Native/Extensions/NativeGatherExtensions.cs` — `Include<T>(expr)` uses IdGenerator
- `Alis.Reactive.Native/Extensions/FieldExtensions.cs` — remove `idPrefix` overload, use IdGenerator

### Fusion
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxReactiveExtensions.cs` — HtmlAttributes with IdGenerator ID
- `Alis.Reactive.Fusion/Extensions/FusionGatherExtensions.cs` — `Include<T>(expr)` uses IdGenerator

### Validation
- `Alis.Reactive.FluentValidator/FluentValidationRuleExtractor.cs` — use IdGenerator for fieldId
- `Alis.Reactive/Validation/FormValidation.cs` — keep validation field identity aligned with generated element ids

### Views (update all existing)
- All views using `NativeDropDownFor`, `NumericTextBoxFor`, `Component<T>(expr)`, `Include<T>(expr)`
- Remove all `idPrefix` usage from Validation views

### Tests (update all existing + add new)
- Update snapshot `.verified.txt` files (IDs change format)
- Update Playwright selectors that reference element IDs
- Add tests proving `Component<T>(expr)` targets match rendered IDs

## Design Principles
1. **One ID strategy** — IdGenerator everywhere, no fallback, no opt-in
2. **Plan carries targets** — gather by ID from plan, not DOM scanning
3. **name untouched** — MVC model binding path never affected
4. **Vendor-agnostic** — same ID for same expression regardless of vendor
5. **Splittable** — `__` delimiter allows future parsing of scope vs property
