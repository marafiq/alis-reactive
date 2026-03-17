# InputField Migration Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace all 97 `Html.Field()` usages with `Html.InputField().Component()`, delete old Field code, pass all tests.

**Architecture:** InputField infrastructure is in Core (generic `InputFieldSetup<THelper,TModel,TProp>`), closed in Native (`InputFieldSetup<TModel,TProp>`). Per-component extensions in vertical slices. Same HTML output — tests pass unchanged.

**Tech Stack:** C# 8, ASP.NET Core, NUnit, Playwright

---

## Existing Infrastructure (already done)

- `Alis.Reactive/Builders/InputFieldSetup.cs` — generic base, THelper open
- `Alis.Reactive/Builders/InputFieldOptions.cs` — POCO
- `Alis.Reactive/Builders/InputFieldBuilder.cs` — internal, BCL only
- `Alis.Reactive/Builders/InputFieldRenderScope.cs` — internal, BCL only
- `Alis.Reactive.Native/Extensions/InputFieldExtensions.cs` — closes THelper, factory, Render(IHtmlContent)
- `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxInputFieldExtensions.cs` — exists
- `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxInputFieldExtensions.cs` — exists
- `Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj` — Native reference added

## Component Extension Map (9 to create)

| Component | Extension Method | Builder Type | Vendor | .Render()? |
|---|---|---|---|---|
| NativeTextBox | `.NativeTextBox(b => ...)` | `NativeTextBoxBuilder<TModel,TProp>` | Native | No |
| **NativeCheckBox** | `.NativeCheckBox(b => ...)` | `NativeCheckBoxBuilder<TModel,bool>` | Native | No |
| **NativeDropDown** | `.NativeDropDown(b => ...)` | `NativeDropDownBuilder<TModel,TProp>` | Native | No |
| **NativeDatePicker** | `.NativeDatePicker(b => ...)` | `NativeDatePickerBuilder<TModel,TProp>` | Native | No |
| FusionNumericTextBox | `.NumericTextBox(b => ...)` | `NumericTextBoxBuilder` | Fusion | Yes |
| **FusionDatePicker** | `.FusionDatePicker(b => ...)` | `DatePickerBuilder` | Fusion | Yes |
| **FusionTimePicker** | `.FusionTimePicker(b => ...)` | `TimePickerBuilder` | Fusion | Yes |
| **FusionAutoComplete** | `.FusionAutoComplete(b => ...)` | `AutoCompleteBuilder` | Fusion | Yes |
| **FusionDropDownList** | `.FusionDropDownList(b => ...)` | `DropDownListBuilder` | Fusion | Yes |
| **FusionMultiSelect** | `.FusionMultiSelect(b => ...)` | `MultiSelectBuilder` | Fusion | Yes |
| **FusionMultiColumnComboBox** | `.FusionMultiColumnComboBox(b => ...)` | `MultiColumnComboBoxBuilder` | Fusion | Yes |

Bold = needs creating. Two already exist.

## View Migration Patterns

All transformations follow the same shape:

**Before (Native):**
```csharp
@{ Html.Field("LABEL", REQUIRED, m => EXPR, expr =>
    Html.NativeXxxFor(plan, expr)
        .BuilderChain()
); }
```

**After (Native):**
```csharp
@{ Html.InputField(plan, m => EXPR, o => o[.Required()].Label("LABEL"))
   .NativeXxx(b => b
       .BuilderChain()); }
```

**Before (Fusion):**
```csharp
@{ Html.Field("LABEL", REQUIRED, m => EXPR, expr =>
    Html.FusionXxxFor(plan, expr)
        .BuilderChain()
        .Render()
); }
```

**After (Fusion):**
```csharp
@{ Html.InputField(plan, m => EXPR, o => o[.Required()].Label("LABEL"))
   .FusionXxx(b => b
       .BuilderChain()); }
```

Key rules:
- `true` → add `.Required()` before `.Label()`
- `false` → just `.Label()`
- `expr =>` disappears (expression already captured by InputField)
- `Html.XxxFor(plan, expr)` → `.XxxMethod(b => b`
- `.Render()` removed (extension handles it internally)
- Closing changes from `); }` to `)); }`

## View Files (97 usages across 25 files)

| File | Count | Components |
|---|---|---|
| Validation/Index.cshtml | 17 | NativeTextBox, NativeCheckBox |
| ValidationContract/ConditionalHide.cshtml | 14 | NativeTextBox, NativeCheckBox, NativeDropDown, NumericTextBox |
| ValidationContract/Index.cshtml | 16 | NativeTextBox, NativeCheckBox, NativeDropDown, NumericTextBox |
| ValidationContract/ServerPartial.cshtml | 6 | NativeTextBox, NativeCheckBox, NativeDropDown |
| ValidationContract/AjaxPartial.cshtml | 4 | NativeTextBox, NativeDropDown |
| ValidationContract/_EmergencyPartial.cshtml | 4 | NativeTextBox, NativeCheckBox |
| ValidationContract/_AddressPartial.cshtml | 3 | NativeTextBox |
| PlaygroundSyntax/Index.cshtml | 5 | NativeTextBox, NativeDropDown, NumericTextBox |
| PlaygroundSyntax/ReactiveConditions.cshtml | 5 | NativeDropDown, NumericTextBox |
| IdGenerator/Index.cshtml | 6 | NativeTextBox, NativeDropDown, NumericTextBox |
| NumericTextBox/Index.cshtml | 3 | NumericTextBox |
| FusionDatePicker/Index.cshtml | 2 | DatePicker |
| TimePicker/Index.cshtml | 2 | TimePicker |
| CheckBox/Index.cshtml | 3 | NativeCheckBox |
| NativeTextBox/Index.cshtml | 2 | NativeTextBox |
| NativeDropDown/Index.cshtml | 2 | NativeDropDown |
| NativeDatePicker/Index.cshtml | 2 | NativeDatePicker |
| AutoComplete/Index.cshtml | 1 | AutoComplete |
| DropDownList/Index.cshtml | 1 | DropDownList |
| MultiSelect/Index.cshtml | 2 | MultiSelect |
| MultiColumnComboBox/Index.cshtml | 1 | MultiColumnComboBox |
| Cascading/Index.cshtml | 2 | DropDownList |
| Http/Index.cshtml | 3 | NativeTextBox |
| ContentType/_ContentTypePartial.cshtml | 2 | NativeTextBox, NumericTextBox |

## Deletions

| File | Action |
|---|---|
| `Alis.Reactive.Native/Extensions/FieldExtensions.cs` | DELETE |
| `Alis.Reactive.Native/Builders/FieldBuilder.cs` | DELETE |
| `Alis.Reactive.Native/Builders/HtmlRenderScope.cs` | DELETE (replaced by InputFieldRenderScope in Core) |

## Test Updates

| File | Change |
|---|---|
| `tests/Alis.Reactive.Native.UnitTests/Field/WhenRenderingAField.cs` | Update: `FieldBuilder` → `InputFieldBuilder`, namespace → `Alis.Reactive.Builders` |
| `Alis.Reactive/Alis.Reactive.csproj` | Add `InternalsVisibleTo` for `Alis.Reactive.Native.UnitTests` |

---

## Task 1: Create 9 Missing InputField Extensions

Each follows the exact same 3-line pattern. One file per component in its vertical slice folder.

**Files to create:**

### Native (3 files)

- [ ] **Step 1: Create NativeCheckBoxInputFieldExtensions.cs**

Create: `Alis.Reactive.Native/Components/NativeCheckBox/NativeCheckBoxInputFieldExtensions.cs`

```csharp
using System;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    public static class NativeCheckBoxInputFieldExtensions
    {
        public static void NativeCheckBox<TModel>(
            this InputFieldSetup<TModel, bool> setup,
            Action<NativeCheckBoxBuilder<TModel, bool>> configure)
            where TModel : class
        {
            var builder = setup.Helper.NativeCheckBoxFor(setup.Plan, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
```

Note: Constrained to `InputFieldSetup<TModel, bool>` — only compiles for bool properties.

- [ ] **Step 2: Create NativeDropDownInputFieldExtensions.cs**

Create: `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDownInputFieldExtensions.cs`

```csharp
using System;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    public static class NativeDropDownInputFieldExtensions
    {
        public static void NativeDropDown<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NativeDropDownBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            var builder = setup.Helper.NativeDropDownFor(setup.Plan, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
```

- [ ] **Step 3: Create NativeDatePickerInputFieldExtensions.cs**

Create: `Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerInputFieldExtensions.cs`

```csharp
using System;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    public static class NativeDatePickerInputFieldExtensions
    {
        public static void NativeDatePicker<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NativeDatePickerBuilder<TModel, TProp>> configure)
            where TModel : class
        {
            var builder = setup.Helper.NativeDatePickerFor(setup.Plan, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
```

### Fusion (6 files)

All follow identical pattern — call our factory, configure, `setup.Render(builder.Render())`.

- [ ] **Step 4: Create FusionDatePickerInputFieldExtensions.cs**

Create: `Alis.Reactive.Fusion/Components/FusionDatePicker/FusionDatePickerInputFieldExtensions.cs`

```csharp
using System;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionDatePickerInputFieldExtensions
    {
        public static void FusionDatePicker<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<DatePickerBuilder> configure)
            where TModel : class
        {
            var builder = setup.Helper.DatePickerFor(setup.Plan, setup.Expression);
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
```

- [ ] **Step 5: Create FusionTimePickerInputFieldExtensions.cs**

Create: `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerInputFieldExtensions.cs`

Same pattern, `TimePickerBuilder`, `setup.Helper.TimePickerFor(...)`, `Syncfusion.EJ2.Calendars`.

- [ ] **Step 6: Create FusionAutoCompleteInputFieldExtensions.cs**

Create: `Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteInputFieldExtensions.cs`

Same pattern, `AutoCompleteBuilder`, `setup.Helper.AutoCompleteFor(...)`, `Syncfusion.EJ2.DropDowns`.

- [ ] **Step 7: Create FusionDropDownListInputFieldExtensions.cs**

Create: `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListInputFieldExtensions.cs`

Same pattern, `DropDownListBuilder`, `setup.Helper.DropDownListFor(...)`, `Syncfusion.EJ2.DropDowns`.

- [ ] **Step 8: Create FusionMultiSelectInputFieldExtensions.cs**

Create: `Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelectInputFieldExtensions.cs`

Same pattern, `MultiSelectBuilder`, `setup.Helper.MultiSelectFor(...)`, `Syncfusion.EJ2.DropDowns`.

- [ ] **Step 9: Create FusionMultiColumnComboBoxInputFieldExtensions.cs**

Create: `Alis.Reactive.Fusion/Components/FusionMultiColumnComboBox/FusionMultiColumnComboBoxInputFieldExtensions.cs`

Same pattern, `MultiColumnComboBoxBuilder`, `setup.Helper.MultiColumnComboBoxFor(...)`, `Syncfusion.EJ2.MultiColumnComboBox`.

- [ ] **Step 10: Build to verify all extensions compile**

Run: `dotnet build`
Expected: 0 errors

- [ ] **Step 11: Commit extensions**

```bash
git add Alis.Reactive.Native/Components/*/\*InputFieldExtensions.cs \
        Alis.Reactive.Fusion/Components/*/\*InputFieldExtensions.cs
git commit -m "feat: add InputField extensions for all 11 component types"
```

---

## Task 2: Migrate All Views

Strategy: Per-file migration using structured find-replace. Each file is read, all `Html.Field(` blocks replaced, then the file is verified via build.

The transformation rules are mechanical:

### Regex Substitution Rules

**Rule 1 — Opening: required field**
```
Html.Field("LABEL", true, m => EXPR, expr =>
```
→
```
Html.InputField(plan, m => EXPR, o => o.Required().Label("LABEL"))
```

**Rule 2 — Opening: optional field**
```
Html.Field("LABEL", false, m => EXPR, expr =>
```
→
```
Html.InputField(plan, m => EXPR, o => o.Label("LABEL"))
```

**Rule 3 — Native factory call**
```
    Html.NativeTextBoxFor(plan, expr)
```
→
```
   .NativeTextBox(b => b
```
(Same for CheckBoxFor→NativeCheckBox, DropDownFor→NativeDropDown, DatePickerFor→NativeDatePicker)

**Rule 4 — Fusion factory call**
```
    Html.NumericTextBoxFor(plan, expr)
```
→
```
   .NumericTextBox(b => b
```
(Same for DatePickerFor→FusionDatePicker, TimePickerFor→FusionTimePicker, etc.)

**Rule 5 — Remove Fusion .Render()**
```
        .Render()
```
→ (delete line)

**Rule 6 — Closing**
```
); }
```
→
```
)); }
```
(Only for Html.Field blocks — add extra closing paren)

### Migration Steps

- [ ] **Step 1: Migrate ConditionalHide.cshtml** (14 remaining fields — 2 already done as proof)

- [ ] **Step 2: Migrate ValidationContract/Index.cshtml** (16 fields)

- [ ] **Step 3: Migrate Validation/Index.cshtml** (17 fields)

- [ ] **Step 4: Migrate ValidationContract/ServerPartial.cshtml** (6 fields)

- [ ] **Step 5: Migrate ValidationContract/AjaxPartial.cshtml** (4 fields)

- [ ] **Step 6: Migrate ValidationContract/_EmergencyPartial.cshtml** (4 fields)

- [ ] **Step 7: Migrate ValidationContract/_AddressPartial.cshtml** (3 fields)

- [ ] **Step 8: Migrate PlaygroundSyntax/Index.cshtml** (5 fields)

- [ ] **Step 9: Migrate PlaygroundSyntax/ReactiveConditions.cshtml** (5 fields)

- [ ] **Step 10: Migrate IdGenerator/Index.cshtml** (6 fields)

- [ ] **Step 11: Migrate component demo pages** (NumericTextBox, FusionDatePicker, TimePicker, CheckBox, NativeTextBox, NativeDropDown, NativeDatePicker, AutoComplete, DropDownList, MultiSelect, MultiColumnComboBox, Cascading, Http, ContentType — 16 files)

- [ ] **Step 12: Update Home/Index.cshtml** — change descriptive text "Html.Field()" → "Html.InputField()"

- [ ] **Step 13: Build to verify all views compile**

Run: `dotnet build`
Expected: 0 errors

- [ ] **Step 14: Commit view migrations**

```bash
git add Alis.Reactive.SandboxApp/
git commit -m "refactor: migrate all Html.Field() to Html.InputField().Component()"
```

---

## Task 3: Delete Old Field Code + Update Tests

- [ ] **Step 1: Add InternalsVisibleTo for Native.UnitTests in Core**

Modify: `Alis.Reactive/Alis.Reactive.csproj`

Add: `<InternalsVisibleTo Include="Alis.Reactive.Native.UnitTests" />`

- [ ] **Step 2: Update WhenRenderingAField.cs**

Modify: `tests/Alis.Reactive.Native.UnitTests/Field/WhenRenderingAField.cs`

Replace:
- `using Alis.Reactive.Native.Builders;` → `using Alis.Reactive.Builders;`
- `new FieldBuilder(` → `new InputFieldBuilder(`

All test assertions stay identical — same HTML output.

- [ ] **Step 3: Delete old Field code**

Delete:
- `Alis.Reactive.Native/Extensions/FieldExtensions.cs`
- `Alis.Reactive.Native/Builders/FieldBuilder.cs`
- `Alis.Reactive.Native/Builders/HtmlRenderScope.cs`

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: 0 errors

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor: delete old Html.Field, FieldBuilder, FieldExtensions"
```

---

## Task 4: Run All Tests

- [ ] **Step 1: TS unit tests**

Run: `npm test`
Expected: All pass (no TS changes)

- [ ] **Step 2: C# unit tests**

Run: `dotnet test tests/Alis.Reactive.UnitTests`
Expected: All pass (no plan/descriptor changes)

- [ ] **Step 3: Native unit tests**

Run: `dotnet test tests/Alis.Reactive.Native.UnitTests`
Expected: All pass (InputFieldBuilder produces same HTML as FieldBuilder)

- [ ] **Step 4: Fusion unit tests**

Run: `dotnet test tests/Alis.Reactive.Fusion.UnitTests`
Expected: All pass

- [ ] **Step 5: FluentValidator unit tests**

Run: `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests`
Expected: All pass

- [ ] **Step 6: Playwright browser tests**

Run: `dotnet test tests/Alis.Reactive.PlaywrightTests`
Expected: All pass — same HTML rendered, same IDs, same validation, same behavior

- [ ] **Step 7: Final commit if any fixes needed**
