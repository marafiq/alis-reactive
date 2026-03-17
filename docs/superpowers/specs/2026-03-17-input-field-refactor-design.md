# InputField Refactor — Design Spec

**Date:** 2026-03-17
**Status:** Draft
**Scope:** Refactor `Html.Field()` into `Html.InputField()` with fluent two-step API

## Problem

Current syntax duplicates the property expression — once for Field, once for the builder:

```csharp
Html.Field("Name", true, m => m.Name, expr =>
    Html.NativeTextBoxFor(plan, expr).CssClass("...").Placeholder("..."));
```

The expression `m => m.Name` is passed to `Field`, which passes it as `expr` to the callback,
which passes it again to `NativeTextBoxFor`. This is redundant and noisy.

## Solution

Two-step fluent API: `Html.InputField()` captures expression + options, returns `InputFieldSetup<TModel, TProp>`.
Component-specific extension methods on `InputFieldSetup` create the builder and render the wrapper.

### Target Syntax

```csharp
// Native TextBox
Html.InputField(plan, m => m.Name, o => o.Required().Label("Name"))
   .NativeTextBox(b => b.CssClass("...").Placeholder("Resident name"));

// Native CheckBox with Reactive
Html.InputField(plan, m => m.IsVeteran, o => o.Label("Veteran"))
   .NativeCheckBox(b => b.CssClass("rounded border-border")
       .Reactive(plan, evt => evt.Changed, (args, p) => { ... }));

// Native DropDown with multiple Reactive chains
Html.InputField(plan, m => m.CareLevel, o => o.Required().Label("Care Level"))
   .NativeDropDown(b => b.Items(careLevels)
       .Placeholder("-- Select --")
       .CssClass("...")
       .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
       .Reactive(plan, evt => evt.Changed, (args, p) => { ... }));

// Fusion NumericTextBox
Html.InputField(plan, m => m.MemoryAssessmentScore, o => o.Label("Memory Assessment Score"))
   .NumericTextBox(b => b.Placeholder("Score").CssClass("..."));

// Fusion DatePicker
Html.InputField(plan, m => m.AdmissionDate, o => o.Required().Label("Admission Date"))
   .FusionDatePicker(b => b.Placeholder("Select date").CssClass("..."));

// Minimal (label is dev responsibility — omitting Label() means no label renders)
Html.InputField(plan, m => m.Phone)
   .NativeTextBox(b => b.CssClass("...").Placeholder("555-0123"));
```

## Architecture

### New Types

#### `InputFieldOptions` (Alis.Reactive.Native/Builders/)

```csharp
public class InputFieldOptions
{
    internal string? LabelText { get; private set; }
    internal bool IsRequired { get; private set; }

    public InputFieldOptions Required() { IsRequired = true; return this; }
    public InputFieldOptions Label(string label) { LabelText = label; return this; }
}
```

Fully explicit. No auto-derivation from property names. No magic. Dev calls `.Label()` or no label renders.

#### `InputFieldSetup<TModel, TProp>` (Alis.Reactive.Native/Builders/)

```csharp
public class InputFieldSetup<TModel, TProp> where TModel : class
{
    // Captured state — accessible to vertical slice extensions
    public IHtmlHelper<TModel> Html { get; }
    public IReactivePlan<TModel> Plan { get; }
    public Expression<Func<TModel, TProp>> Expression { get; }
    public InputFieldOptions Options { get; }

    internal InputFieldSetup(IHtmlHelper<TModel> html, IReactivePlan<TModel> plan,
        Expression<Func<TModel, TProp>> expression, InputFieldOptions options) { ... }

    /// <summary>
    /// Renders the field wrapper (label + validation slot) around the given content.
    /// Called by vertical slice extensions — they never access InputFieldBuilder directly.
    /// </summary>
    public void Render(IHtmlContent content)
    {
        var id = IdGenerator.For<TModel, TProp>(Expression);
        var name = Html.NameFor(Expression);
        var fb = new InputFieldBuilder(Html.ViewContext.Writer, name)
            .Label(Options.LabelText)
            .ForId(id);
        if (Options.IsRequired) fb.Required();
        using (fb.Begin()) { content.WriteTo(Html.ViewContext.Writer, HtmlEncoder.Default); }
    }
}
```

#### `InputFieldBuilder` (Alis.Reactive.Native/Builders/)

Renamed from `FieldBuilder`. Internal. Same rendering logic (wrapper div, label, required marker, validation placeholder).

#### `Html.InputField()` Extension (Alis.Reactive.Native/Extensions/)

```csharp
public static InputFieldSetup<TModel, TProp> InputField<TModel, TProp>(
    this IHtmlHelper<TModel> html,
    IReactivePlan<TModel> plan,
    Expression<Func<TModel, TProp>> expression,
    Action<InputFieldOptions>? options = null)
    where TModel : class
{
    var opts = new InputFieldOptions();
    options?.Invoke(opts);
    return new InputFieldSetup<TModel, TProp>(html, plan, expression, opts);
}
```

### Vertical Slice Extensions

Each component adds its own extension method on `InputFieldSetup<TModel, TProp>`.

#### Native Example (NativeTextBox)

```csharp
public static void NativeTextBox<TModel, TProp>(
    this InputFieldSetup<TModel, TProp> setup,
    Action<NativeTextBoxBuilder<TModel, TProp>> configure)
    where TModel : class
{
    var builder = setup.Html.NativeTextBoxFor(setup.Plan, setup.Expression);
    configure(builder);
    setup.Render(builder); // builder IS IHtmlContent
}
```

#### Native CheckBox (bool-constrained)

```csharp
public static void NativeCheckBox<TModel>(
    this InputFieldSetup<TModel, bool> setup,
    Action<NativeCheckBoxBuilder<TModel, bool>> configure)
    where TModel : class
{
    var builder = setup.Html.NativeCheckBoxFor(setup.Plan, setup.Expression);
    configure(builder);
    setup.Render(builder);
}
```

Type safety: `.NativeCheckBox()` only compiles when `TProp` is `bool`.

#### Fusion Example (NumericTextBox)

```csharp
public static void NumericTextBox<TModel, TProp>(
    this InputFieldSetup<TModel, TProp> setup,
    Action<NumericTextBoxBuilder> configure)
    where TModel : class
{
    var builder = setup.Html.NumericTextBoxFor(setup.Plan, setup.Expression);
    configure(builder);
    setup.Render(builder.Render()); // SF .Render() returns IHtmlContent
}
```

### Project Dependency Change

```
Alis.Reactive.Fusion.csproj:
  + <ProjectReference Include="..\Alis.Reactive.Native\Alis.Reactive.Native.csproj" />
```

Fusion adds Native as a dependency. This is natural because:
- Views using Fusion always use Native (`Html.On`, `Html.ReactivePlan`, `Html.RenderPlan`)
- `InputFieldSetup` and `InputFieldBuilder` live in Native
- Fusion's vertical slice extensions need `InputFieldSetup.Render()`

### Removals

- `Html.Field()` — removed entirely (no backward compat)
- `FieldExtensions.cs` — replaced by `InputFieldExtensions.cs`
- `FieldBuilder` — renamed to `InputFieldBuilder`

### Files Changed

| File | Change |
|------|--------|
| `Alis.Reactive.Native/Builders/InputFieldOptions.cs` | NEW |
| `Alis.Reactive.Native/Builders/InputFieldSetup.cs` | NEW |
| `Alis.Reactive.Native/Builders/InputFieldBuilder.cs` | RENAME from FieldBuilder.cs |
| `Alis.Reactive.Native/Extensions/InputFieldExtensions.cs` | NEW (Html.InputField()) |
| `Alis.Reactive.Native/Extensions/FieldExtensions.cs` | DELETE |
| `Alis.Reactive.Native/Builders/FieldBuilder.cs` | DELETE |
| `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Native/Components/NativeCheckBox/NativeCheckBoxInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDownInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Fusion/Components/FusionDatePicker/FusionDatePickerInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Fusion/Components/FusionTimePicker/FusionTimePickerInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelectInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Fusion/Components/FusionMultiColumnComboBox/FusionMultiColumnComboBoxInputFieldExtensions.cs` | NEW |
| `Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj` | ADD Native project reference |
| All `.cshtml` views using `Html.Field()` | MIGRATE to `Html.InputField().XxxComponent()` |
| `tests/Alis.Reactive.Native.UnitTests/Field/WhenRenderingAField.cs` | RENAME + update to InputField |

### What Does NOT Change

- Builder classes (NativeTextBoxBuilder, etc.) — unchanged
- `Html.XxxFor()` factory methods — still exist, still work standalone
- Component registration / ComponentsMap — unchanged
- FieldBuilder rendering logic (div wrapper, label, validation span) — same HTML output
- JSON plan, TS runtime, schema — zero changes
- `.Reactive()` extensions — unchanged

### Type Safety Matrix

| `TProp` | Available extensions |
|---------|---------------------|
| `string` | NativeTextBox, NativeDropDown, NativeDatePicker, FusionAutoComplete, FusionDropDownList, FusionMultiColumnComboBox |
| `bool` | NativeCheckBox |
| `int`, `decimal`, `double` | NativeTextBox (.Type("number")), NumericTextBox |
| `DateTime` | NativeDatePicker, FusionDatePicker |
| `TimeSpan` | FusionTimePicker |
| `IEnumerable<string>` | FusionMultiSelect |

Note: Exact TProp constraints depend on each component's existing factory method signature.
Components that accept `Expression<Func<TModel, TProp>>` (open generic) remain available for any TProp.
Components with specific TProp constraints (like CheckBox requiring `bool`) are naturally constrained.

## Migration Example

**Before:**
```csharp
@{ Html.Field("Name", true, m => m.Name, expr =>
    Html.NativeTextBoxFor(plan, expr)
        .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
        .Placeholder("Resident name")
); }
```

**After:**
```csharp
@{ Html.InputField(plan, m => m.Name, o => o.Required().Label("Name"))
   .NativeTextBox(b => b
       .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
       .Placeholder("Resident name")); }
```

**Before (Fusion + Reactive):**
```csharp
@{ Html.Field("Memory Assessment Score", false, m => m.MemoryAssessmentScore, expr =>
    Html.NumericTextBoxFor(plan, expr)
        .Placeholder("Score")
        .CssClass("rounded-md border border-border")
        .Render()
); }
```

**After:**
```csharp
@{ Html.InputField(plan, m => m.MemoryAssessmentScore, o => o.Label("Memory Assessment Score"))
   .NumericTextBox(b => b
       .Placeholder("Score")
       .CssClass("rounded-md border border-border")); }
```

Note: `.Render()` is no longer called by the developer — the vertical slice extension handles it internally.
