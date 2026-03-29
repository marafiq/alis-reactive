# IdGenerator Default + Plan-Driven Gather — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make IdGenerator the default ID strategy for all expression-based components — no opt-in, no two paths. Simplify validation. Remove DOM scanning from gather.

**Architecture:** Three phases executed incrementally:
1. IdGenerator replaces all expression-based ID generation (rendering + targeting + gather)
2. Validation uses IdGenerator — remove WithPrefix + prefix overloads
3. IncludeAll becomes parameterless — plan-driven, no DOM scanning

**Tech Stack:** C# (.NET 8), TypeScript, NUnit, Vitest, Playwright

**Design doc:** `docs/plans/2026-03-11-idgenerator-plan-driven-gather.md`

---

## Phase 1: IdGenerator Default for Rendering + Gather

> All expression-based IDs switch to IdGenerator format: `{TypeScope}__{MemberPath}`.
> Rendering, targeting (`Component<T>(expr)`), and gather (`Include<T>(expr)`) all align.

---

### Task 1: NativeDropDownFor uses IdGenerator by default

**Files:**
- Modify: `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDownBuilder.cs`

**Step 1: Modify public constructor to use IdGenerator**

Change the public constructor (line 39-45):

```csharp
// Before:
public NativeDropDownBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
{
    _html = html;
    _expression = expression;
    _elementId = html.IdFor(expression).ToString();
    _bindingPath = html.NameFor(expression).ToString();
}

// After:
public NativeDropDownBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
{
    _html = html;
    _expression = expression;
    _elementId = IdGenerator.For<TModel, TProp>(expression);
    _bindingPath = html.NameFor(expression).ToString();
}
```

**Step 2: Remove internal constructor (lines 47-53)**

No longer needed — public constructor now does IdGenerator.

**Step 3: Remove AsNativeDropDownFor extension (lines 122-127)**

Delete the `AsNativeDropDownFor` method from `NativeDropDownHtmlExtensions`.
`NativeDropDownFor` now produces IdGenerator IDs by default.

**Step 4: Update IdGenerator sandbox view**

File: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/IdGenerator/Index.cshtml`

Change all `Html.AsNativeDropDownFor(expr)` → `Html.NativeDropDownFor(expr)`.

Also update the description text that mentions `AsNativeDropDownFor`.

**Step 5: Build and run unit tests**

```bash
npm run build:all && dotnet build
dotnet test tests/Alis.Reactive.UnitTests
```

Expected: Unit tests pass (or snapshot diffs — accept new snapshots).

**Step 6: Run Playwright tests**

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests
```

Expected: IdGenerator tests pass. Other tests may need selector updates (see Task 7).

**Step 7: Commit**

```bash
git add -A && git commit -m "feat: NativeDropDownFor uses IdGenerator by default — remove As prefix"
```

---

### Task 2: SF NumericTextBoxFor uses IdGenerator by default

**Files:**
- Rename: `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxIdExtensions.cs`
- Modify: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/IdGenerator/Index.cshtml`

**Step 1: Rename class and method — remove "As" prefix**

```csharp
// Before:
public static class FusionNumericTextBoxIdExtensions
{
    public static NumericTextBoxBuilder AsNumericTextBoxFor<TModel, TProp>(...)

// After:
public static class FusionNumericTextBoxExtensions
{
    public static NumericTextBoxBuilder NumericTextBoxFor<TModel, TProp>(...)
```

**Step 2: Update IdGenerator sandbox view**

Change all `Html.AsNumericTextBoxFor(expr)` → `Html.NumericTextBoxFor(expr)`.

Also update the description text that mentions `AsNumericTextBoxFor`.

**Step 3: Build and run tests**

```bash
npm run build:all && dotnet build
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

**Step 4: Commit**

```bash
git add -A && git commit -m "feat: NumericTextBoxFor uses IdGenerator by default — remove As prefix"
```

---

### Task 3: Component<T>(expr) uses IdGenerator

**Files:**
- Modify: `Alis.Reactive/Builders/PipelineBuilder.cs:56`

**Step 1: Change Component<T>(expr) to use IdGenerator**

```csharp
// Before (line 56):
var elementId = ExpressionPathHelper.ToElementId(expr);

// After:
var elementId = IdGenerator.For<TModel>(expr);
```

**Step 2: Build and run tests**

```bash
dotnet build
dotnet test tests/Alis.Reactive.UnitTests
```

Expected: Snapshot files will diff — accept new snapshots showing IdGenerator IDs.

**Step 3: Commit**

```bash
git add -A && git commit -m "feat: Component<T>(expr) uses IdGenerator for element targeting"
```

---

### Task 4: Include<T>(expr) uses IdGenerator (Native + Fusion)

**Files:**
- Modify: `Alis.Reactive.Native/Extensions/NativeGatherExtensions.cs:25`
- Modify: `Alis.Reactive.Fusion/Extensions/FusionGatherExtensions.cs:26`

**Step 1: Change Native Include<T>(expr) (line 25)**

```csharp
// Before:
var elementId = ExpressionPathHelper.ToElementId(expr);

// After:
var elementId = IdGenerator.For<TModel>(expr);
```

**Step 2: Change Fusion Include<T>(expr) (line 26)**

Same change as Step 1.

**Step 3: Build and run tests**

```bash
dotnet build
dotnet test tests/Alis.Reactive.UnitTests
```

Expected: Snapshot files with gather items will diff — accept new snapshots.

**Step 4: Commit**

```bash
git add -A && git commit -m "feat: Include<T>(expr) uses IdGenerator for gather targeting"
```

---

### Task 5: FieldExtensions uses IdGenerator

**Files:**
- Modify: `Alis.Reactive.Native/Extensions/FieldExtensions.cs`
- Modify: ALL views using `Html.Field()` (6 files)

**Step 1: Modify primary Field() overload**

The callback gains an `id` parameter. FieldExtensions computes IdGenerator id
and passes it so callers can apply it to plain MVC helpers like `TextBoxFor`.

```csharp
// Before:
public static void Field<TModel, TProp>(
    this IHtmlHelper<TModel> html,
    string label, bool isRequired,
    Expression<Func<TModel, TProp>> expression,
    Func<Expression<Func<TModel, TProp>>, IHtmlContent> inputBuilder)
{
    var writer = html.ViewContext.Writer;
    var b = new FieldBuilder(writer, html.NameFor(expression).ToString())
        .Label(label)
        .ForId(html.IdFor(expression).ToString());
    if (isRequired) b.Required();
    using (b.Begin()) { inputBuilder(expression).WriteTo(writer, HtmlEncoder.Default); }
}

// After:
public static void Field<TModel, TProp>(
    this IHtmlHelper<TModel> html,
    string label, bool isRequired,
    Expression<Func<TModel, TProp>> expression,
    Func<Expression<Func<TModel, TProp>>, string, IHtmlContent> inputBuilder)
{
    var writer = html.ViewContext.Writer;
    var id = IdGenerator.For<TModel, TProp>(expression);
    var b = new FieldBuilder(writer, html.NameFor(expression).ToString())
        .Label(label)
        .ForId(id);
    if (isRequired) b.Required();
    using (b.Begin()) { inputBuilder(expression, id).WriteTo(writer, HtmlEncoder.Default); }
}
```

**Step 2: Remove idPrefix overload (lines 40-55)**

Delete the second `Field()` overload that accepts `string idPrefix`.

**Step 3: Update ALL Field() call sites in ALL views**

Views to update (6 files):
- `Views/Home/Index.cshtml`
- `Areas/Sandbox/Views/IdGenerator/Index.cshtml`
- `Areas/Sandbox/Views/Validation/Index.cshtml`
- `Areas/Sandbox/Views/PlaygroundSyntax/Index.cshtml`
- `Areas/Sandbox/Views/PlaygroundSyntax/ReactiveConditions.cshtml`
- `Areas/Sandbox/Views/ContentType/_ContentTypePartial.cshtml`

Pattern for plain TextBoxFor:
```csharp
// Before:
Html.Field("Name", true, m => m.Name, expr =>
    Html.TextBoxFor(expr, new { @class = "..." }))

// After:
Html.Field("Name", true, m => m.Name, (expr, id) =>
    Html.TextBoxFor(expr, new { id, @class = "..." }))
```

Pattern for NativeDropDownFor (id param ignored — builder generates its own):
```csharp
// Before:
Html.Field("Status", false, m => m.Status, expr =>
    Html.NativeDropDownFor(expr).Items(items).CssClass("..."))

// After:
Html.Field("Status", false, m => m.Status, (expr, id) =>
    Html.NativeDropDownFor(expr).Items(items).CssClass("..."))
```

Pattern for PasswordFor:
```csharp
// Before:
Html.Field("Password", false, m => m.Password, expr =>
    Html.PasswordFor(expr, new { @class = "..." }))

// After:
Html.Field("Password", false, m => m.Password, (expr, id) =>
    Html.PasswordFor(expr, new { id, @class = "..." }))
```

**For validation views with former idPrefix pattern** (sections 2, 5, 6, 7, 8):
The idPrefix overload is removed. These sections used prefixes to avoid ID collision
when the same `ValidationShowcaseModel` property appears in multiple forms.

With IdGenerator, all `Name` fields on the same model type get the same ID
(`...ValidationShowcaseModel__Name`). This causes duplicate IDs on the validation page.

**Resolution:** Sections that previously used prefixes should use `(expr, id)` like
all other sections. The `id` from IdGenerator will be the same across sections —
but validation scopes to `formId` and checks `container.contains(el)`, so only the
first matching element is validated. This is a known limitation that will be addressed
in Phase 2 (validation simplification) by introducing per-section model types or
form-scoped element resolution.

**For now in Phase 1:** Convert all prefix sections to standard `(expr, id)` pattern.
The validation showcase will have duplicate IDs (sections 2, 5, 6, 7, 8 reuse
properties from section 1). Only section 1's fields will be found by `getElementById`.
This is acceptable temporarily — Phase 2 fixes it properly.

**Step 4: Build and run tests**

```bash
npm run build:all && dotnet build
dotnet test tests/Alis.Reactive.UnitTests
```

**Step 5: Commit**

```bash
git add -A && git commit -m "feat: FieldExtensions uses IdGenerator — remove idPrefix overload"
```

---

### Task 6: Update all views using NativeDropDownFor / Component<T>(expr)

The PlaygroundSyntax views render `NativeDropDownFor` and use `Component<T>(m => m.X)`
to target those elements. Both now use IdGenerator, so they should match.

**Files to verify:**
- `Areas/Sandbox/Views/PlaygroundSyntax/Index.cshtml` — renders NativeDropDownFor + NumericTextBoxFor, targets via Component<T>(expr)
- `Areas/Sandbox/Views/PlaygroundSyntax/ReactiveConditions.cshtml` — same pattern
- `Areas/Sandbox/Views/ContentType/Index.cshtml` — uses NativeDropDownFor
- `Areas/Sandbox/Views/ContentType/_ContentTypePartial.cshtml` — uses NativeDropDownFor in Field()

**Step 1: Verify PlaygroundSyntax views compile**

Both rendering (NativeDropDownFor) and targeting (Component<T>(expr)) now use
IdGenerator. They should produce matching IDs automatically. No view changes needed
unless `AsNativeDropDownFor` or `AsNumericTextBoxFor` are used.

**Step 2: Verify ContentType views compile**

Update any `AsNativeDropDownFor` → `NativeDropDownFor` if present.
Update any `Field()` callbacks to `(expr, id)` pattern.

**Step 3: Build and run ALL tests**

```bash
npm run build:all && dotnet build
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

**Step 4: Fix any failing tests**

Common fixes:
- Snapshot `.verified.txt` files: accept new snapshots (IDs changed format)
- Playwright selectors: update `#Status` → `#...Model__Status` or use `[name='Status']`
- TS tests: update `getElementById("Name")` → use IdGenerator format or `querySelector("[name='Name']")`

**Step 5: Commit**

```bash
git add -A && git commit -m "feat: Phase 1 complete — IdGenerator default for all expression-based components"
```

---

## Phase 2: Validation Simplification

> FluentValidationAdapter uses IdGenerator for field IDs.
> WithPrefix infrastructure removed. Validation showcase refactored.

---

### Task 7: FluentValidationAdapter uses IdGenerator

**Files:**
- Modify: `Alis.Reactive.FluentValidator/FluentValidationAdapter.cs`

**Step 1: Add helper to extract model type from validator type**

```csharp
private static Type? GetModelType(Type validatorType)
{
    var type = validatorType;
    while (type != null)
    {
        if (type.IsGenericType &&
            type.GetGenericTypeDefinition().FullName == "FluentValidation.AbstractValidator`1")
            return type.GetGenericArguments()[0];
        type = type.BaseType;
    }
    return null;
}
```

**Step 2: Use IdGenerator for elementId in ExtractRules (line 38)**

```csharp
// Before:
var elementId = propertyPath.Replace(".", "_");

// After:
var modelType = GetModelType(validatorType);
var elementId = modelType != null
    ? IdGenerator.TypeScope(modelType) + "__" + propertyPath.Replace(".", "_")
    : propertyPath.Replace(".", "_");
```

**Step 3: Same change in FindOrCreateField (line 79)**

```csharp
// Before:
var elementId = propertyName.Replace(".", "_");

// After — needs model type passed down:
```

Note: `FindOrCreateField` doesn't have access to `validatorType`. Refactor to pass
the scope string (computed once in `ExtractRules`) to all helper methods.

```csharp
public ValidationDescriptor? ExtractRules(Type validatorType, string formId)
{
    var validator = ...;
    var modelType = GetModelType(validatorType);
    var scope = modelType != null ? IdGenerator.TypeScope(modelType) + "__" : "";

    // ... use scope + propertyPath.Replace(".", "_") everywhere
}
```

**Step 4: Build and run FluentValidator tests**

```bash
dotnet test tests/Alis.Reactive.FluentValidatorTests
```

Expected: Tests will fail — field IDs now include the full scope prefix.
Update test assertions to expect IdGenerator format.

**Step 5: Commit**

```bash
git add -A && git commit -m "feat: FluentValidationAdapter uses IdGenerator for field IDs"
```

---

### Task 8: Remove WithPrefix infrastructure

**Files:**
- Modify: `Alis.Reactive/Validation/ValidationDescriptor.cs` — remove WithPrefix()
- Modify: `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs` — remove Validate<T>(formId, prefix) overload
- Modify: `Alis.Reactive/Descriptors/Requests/RequestDescriptor.cs` — remove ValidationPrefix property
- Modify: `Alis.Reactive/IReactivePlan.cs` — remove prefix handling in ResolveRequest

**Step 1: Remove ValidationDescriptor.WithPrefix() (lines 27-43)**

Delete the entire `WithPrefix` method. Keep `WithReadExpr` — it's still needed.

**Step 2: Remove Validate<T>(formId, prefix) from HttpRequestBuilder (lines 103-110)**

Delete the overload that accepts `string prefix`. Keep `Validate<T>(formId)`.

**Step 3: Remove ValidationPrefix from RequestDescriptor (lines 64-65)**

Delete the `ValidationPrefix` property.

**Step 4: Remove prefix handling in IReactivePlan.ResolveRequest (lines 96-99)**

```csharp
// Delete these lines:
if (!string.IsNullOrEmpty(req.ValidationPrefix))
{
    extracted = extracted.WithPrefix(formId, req.ValidationPrefix);
}
```

**Step 5: Build — expect compile errors in validation views**

```bash
dotnet build
```

Expected: Compile errors in `Validation/Index.cshtml` where `.Validate<T>(formId, prefix)` is called.

**Step 6: Commit (compile errors will be fixed in Task 9)**

Do NOT commit yet — fix validation views first.

---

### Task 9: Refactor validation showcase — remove prefix usage

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Validation/Index.cshtml`

The validation showcase uses the same `ValidationShowcaseModel` for 9 sections.
Sections 2, 5, 6, 7, 8 used prefixes to avoid ID collision. With IdGenerator,
different approaches are needed:

**Option A (recommended):** Use `querySelector` scoped to form instead of `getElementById`.
This requires a TS runtime change in `validation.ts` line 32:
```typescript
// Before:
const el = document.getElementById(f.fieldId);

// After:
const el = container.querySelector(`#${CSS.escape(f.fieldId)}`) ??
           document.getElementById(f.fieldId);
```

With form-scoped queries, duplicate IDs across forms work because each validation
call scopes to its own form container. The `container.contains(el)` check on line 33
already exists — form-scoped querySelector makes it redundant but harmless.

**Option B:** Create separate model types per section (e.g., `LiveValidationModel`,
`CombinedValidationModel`). IdGenerator produces unique IDs per model type.
More work but cleaner HTML (no duplicate IDs).

**Step 1: Update validation.ts for form-scoped field resolution (Option A)**

```typescript
// validation.ts line 32:
// Before:
const el = document.getElementById(f.fieldId);
// After:
const el = container.querySelector<HTMLElement>(`[id="${f.fieldId}"]`);
```

**Step 2: Update all Validate<T>(formId, prefix) → Validate<T>(formId)**

In `Validation/Index.cshtml`, remove the prefix parameter from all Validate calls:
- Line 216: `.Validate<ValidationShowcaseValidator>("live-form", "live_")` → `.Validate<ValidationShowcaseValidator>("live-form")`
- Line 257: `.Validate<ValidationShowcaseValidator>("combined-form", "cmb_")` → `.Validate<ValidationShowcaseValidator>("combined-form")`
- Line 320, 335: `"hidden-fields-form", "hf_"` → `"hidden-fields-form"`
- Line 384: `"db-form", "db_"` → `"db-form"`

**Step 3: Update all Field() calls that used idPrefix**

All sections already updated in Task 5 to use `(expr, id)` pattern.
No further changes needed.

**Step 4: Update showFieldErrors in validation.ts**

Server-side validation returns errors with property names like `"Name"`.
`showFieldErrors` needs to resolve these to IdGenerator IDs.
Current implementation may use `propertyPath.Replace(".", "_")` — must be updated
to use the same IdGenerator format.

Check `validation.ts` for `showFieldErrors` implementation and update accordingly.

**Step 5: Build and run ALL tests**

```bash
npm run build:all && dotnet build
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.FluentValidatorTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

**Step 6: Commit**

```bash
git add -A && git commit -m "feat: remove WithPrefix — validation uses IdGenerator, form-scoped resolution"
```

---

## Phase 3: Plan-Driven IncludeAll (Future)

> IncludeAll() becomes parameterless. The plan carries all component IDs.
> Runtime reads the plan's gather list instead of scanning the DOM.
> This phase requires TS runtime changes and is designed separately.

---

### Task 10: Design plan-driven IncludeAll (design only)

**Problem:** Current `IncludeAll(formId)` → runtime calls `new FormData(form)` to scan
the DOM for all `name` attributes. The runtime decides what to gather.

**After:** `IncludeAll()` (no param) → plan carries all component gather targets.
Runtime reads the list and calls `evalRead(componentId, vendor, readExpr)` for each.

**Design decisions needed:**
1. How does the plan know which components exist? Options:
   a. Each component builder registers itself with the plan at render time
   b. `IncludeAll()` at build time enumerates all `ComponentGather` items already in the gather list
   c. A new `ComponentRegistry` tracks rendered components

2. How does `IncludeAll()` know which components belong to "this request"?
   All components rendered on the page? Only those within a specific form?

3. What about non-framework inputs (plain `<input>` rendered by MVC `TextBoxFor`)?
   These don't go through framework builders — they won't be in the plan.

**Recommendation:** Option (a) — each framework builder registers with the plan.
`IncludeAll()` gathers all registered components. For non-framework inputs,
explicit `Include` is required.

This task is design-only. Implementation follows after Phase 1 + 2 are stable.

---

### Task 11: Implement plan-driven IncludeAll (implementation)

Deferred. Depends on Task 10 design decisions.

**Files to modify:**
- `Alis.Reactive/Builders/Requests/GatherBuilder.cs` — `IncludeAll()` parameterless
- `Alis.Reactive/Descriptors/Requests/GatherItem.cs` — remove `AllGather` or repurpose
- `Alis.Reactive.SandboxApp/Scripts/gather.ts` — remove DOM scanning case
- All views using `IncludeAll("formId")` → `IncludeAll()`
- All tests referencing `IncludeAll` behavior

---

## Test Update Summary

### Snapshot files (auto-updated)
All `.verified.txt` files will diff when IDs change from `Status` to
`...Model__Status`. Run tests, review diffs, accept new snapshots.

### TS unit tests
Tests using `getElementById("Name")` need updating to IdGenerator format.
Key files:
- `Scripts/__tests__/when-validating-form-fields.test.ts` — many `getElementById("Name")` calls
- `Scripts/__tests__/when-executing-http-verbs-and-error-routing.test.ts` — IncludeAll test

### Playwright tests
Selectors referencing element IDs need updating:
- `#Status` → use `[name='Status']` or full IdGenerator ID
- `WhenUsingCollisionFreeIds.cs` — already uses IdGenerator IDs, should pass
- `WhenValidatingFormFields.cs` — needs IdGenerator IDs in selectors
- Other tests targeting form elements by ID

### FluentValidator tests
All tests expecting `fieldId: "Name"` need updating to expect IdGenerator format:
`fieldId: "...ValidationShowcaseModel__Name"`.

---

## Execution Order

```
Phase 1 (rendering + gather):
  Task 1 → Task 2 → Task 3 → Task 4 → Task 5 → Task 6
  ↓
Phase 2 (validation):
  Task 7 → Task 8 + Task 9
  ↓
Phase 3 (plan-driven IncludeAll):
  Task 10 → Task 11
```

Each task is independently committable. Phase 1 can ship without Phase 2.
Phase 2 can ship without Phase 3.
