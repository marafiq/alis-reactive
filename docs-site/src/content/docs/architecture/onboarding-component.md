---
title: Onboarding a Syncfusion Component
description: Step-by-step guide for adding a new Syncfusion EJ2 component to the framework — the 7-file vertical slice, mutations, events, and testing.
sidebar:
  order: 10
---

Adding a new Syncfusion component requires zero runtime changes and zero schema changes. The plan carries all behavior — the runtime is a dumb executor.

This guide covers two distinct patterns. Read the decision section first to know which one you need.

## Which pattern do I follow?

Ask one question: **Does this component have a form value that participates in validation and gather?**

| Answer | Pattern | Interface | Example components | Files |
|--------|---------|-----------|-------------------|-------|
| **Yes** — it has a `value`, `checked`, or similar readable property that users fill in | **Input component** | `IInputComponent` | ColorPicker, DatePicker, NumericTextBox, DropDown, AutoComplete, Switch | 7 files |
| **No** — it's a container, layout, or interaction component with no form value | **Non-input component** | `IComponent` only | Accordion, Tab, Toolbar, Sidebar, TreeView | 5-6 files |

**Input components** (ColorPicker, etc.):
- Wrap in `Html.InputField()` — get label + validation slot
- Register in `ComponentsMap` — participate in `IncludeAll()` gather and `.Validate<T>()`
- Have `ReadExpr` — runtime reads their value via `ej2[readExpr]`
- Have `Value()` — typed source for conditions

**Non-input components** (Accordion, Tab, etc.):
- Render directly via `Html.FusionXxx()` — no InputField wrapper
- NOT in ComponentsMap — not in validation, not in gather
- NO `ReadExpr` — nothing to read
- Have events + methods — `.Reactive()` for interaction, mutations for control
- Need an explicit element ID (not model-expression-derived)

---

## Before you start

### 1. Read the SF API docs

Find the component at `https://ej2.syncfusion.com/javascript/documentation/api/{component}/`. Identify:
- **Properties** you need to write (e.g., `value`, `text`, `enabled`, `dataSource`)
- **Methods** you need to call (e.g., `focusIn()`, `showPopup()`, `dataBind()`)
- **Events** you need to wire (e.g., `change`, `filtering`, `focus`, `blur`)
- **Event args** properties available on each event object

### 2. Experiment in the browser console

**Never onboard an API without verifying it works.** SF docs can be misleading.

```javascript
const el = document.getElementById('{componentId}');
const ej2 = el.ej2_instances[0];

ej2.enabled = false;     // Does it disable?
ej2.showPopup();         // Does popup appear?
typeof ej2.someMethod;   // "function" or "undefined"?
```

Document what works and what doesn't. Some methods exist but have no effect (e.g., `showSpinner` on AutoComplete). Omit those intentionally and comment why.

---

## The 7-file vertical slice

Every Syncfusion component is exactly 7 files (plus one per event type):

```
Alis.Reactive.Fusion/Components/FusionXxx/
├── FusionXxx.cs                      ← 1. Component type marker
├── FusionXxxExtensions.cs            ← 2. Mutations (SetValue, Focus, Value)
├── FusionXxxHtmlExtensions.cs        ← 3. Factory method (Html.Xxx())
├── FusionXxxEvents.cs                ← 4. Event descriptor registry
├── FusionXxxReactiveExtensions.cs    ← 5. .Reactive() wiring
└── Events/
    ├── FusionXxxOnChanged.cs         ← 6. Changed event args
    └── FusionXxxOnFiltering.cs       ← 7. (Optional) Filtering args + extensions
```

No other files are needed. The gather extension is shared across all components — `GatherExtensions.Include<TComponent, TModel>()` works for any `TComponent : IComponent, IInputComponent, new()`.

### Project and namespace

All files go in the `Alis.Reactive.Fusion` project under `Components/FusionXxx/`:

```
Alis.Reactive.Fusion/
└── Components/
    └── FusionXxx/
        ├── FusionXxx.cs
        ├── FusionXxxExtensions.cs
        ├── FusionXxxHtmlExtensions.cs
        ├── FusionXxxEvents.cs
        ├── FusionXxxReactiveExtensions.cs
        └── Events/
            └── FusionXxxOnChanged.cs
```

Namespace: `Alis.Reactive.Fusion.Components` (same as all other Fusion components).

Event args files go in an `Events/` subfolder and are named `FusionXxxOn{EventName}.cs` (e.g., `FusionColorPickerOnChanged.cs`). This matches the convention across all existing components.

### Required using statements

Every file needs a subset of these (copy what you need):

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Native.Extensions;     // for InputBoundField<TModel, TProp>
using Syncfusion.EJ2;                       // for setup.Helper.EJS()
using Syncfusion.EJ2.DropDowns;             // SF component namespace (varies)
```

---

---

# Input Component Pattern (IInputComponent)

> ColorPicker, DatePicker, NumericTextBox, DropDown, AutoComplete, Switch, etc.

## File 1: Component type marker

```csharp
public sealed class FusionXxx : FusionComponent, IInputComponent
{
    public string ReadExpr => "value";
}
```

This file declares two things:
- **Vendor** — inherited from `FusionComponent` → `"fusion"`. The runtime uses this to resolve `el.ej2_instances[0]` instead of the raw DOM element.
- **ReadExpr** — the property path to read the component's current value. Common values:

| ReadExpr | Component types | Runtime reads |
|----------|----------------|---------------|
| `"value"` | TextBox, DropDown, DatePicker, NumericTextBox, AutoComplete | `ej2.value` |
| `"checked"` | Switch, CheckBox | `ej2.checked` |
| `"value"` | DateRangePicker | `ej2.value` (returns `[Date, Date]`; use `StartDate()`/`EndDate()` extensions for individual reads) |
| `"filesData"` | FileUpload | `ej2.filesData` |

The class is sealed, has no state, and a parameterless constructor. It exists only as a type parameter — `ComponentRef<FusionXxx, TModel>` uses it to resolve vendor and readExpr at compile time via a static `new FusionXxx()` instance.

---

## File 2: Mutation extensions

Extensions on `ComponentRef<FusionXxx, TModel>`. Each method emits one command into the plan.

### Property writes — `SetPropMutation`

Sets a property on the ej2 instance at runtime: `ej2[prop] = value`.

```csharp
private static readonly FusionXxx Component = new();

// String property
public static ComponentRef<FusionXxx, TModel> SetValue<TModel>(
    this ComponentRef<FusionXxx, TModel> self, string? value) where TModel : class
    => self.Emit(new SetPropMutation("value"), value: value);

// Boolean property (with coercion) — always accept a parameter for both directions
public static ComponentRef<FusionXxx, TModel> SetChecked<TModel>(
    this ComponentRef<FusionXxx, TModel> self, bool isChecked) where TModel : class
    => self.Emit(new SetPropMutation("checked", coerce: "boolean"),
        value: isChecked ? "true" : "false");

// Disable/Enable — same pattern, parameterized (never hardcode one direction)
public static ComponentRef<FusionXxx, TModel> Disable<TModel>(
    this ComponentRef<FusionXxx, TModel> self, bool disabled = true) where TModel : class
    => self.Emit(new SetPropMutation("disabled", coerce: "boolean"),
        value: disabled ? "true" : "false");

// Decimal property (with coercion)
public static ComponentRef<FusionXxx, TModel> SetValue<TModel>(
    this ComponentRef<FusionXxx, TModel> self, decimal value) where TModel : class
    => self.Emit(new SetPropMutation("value", coerce: "number"),
        value: value.ToString(CultureInfo.InvariantCulture));

// DateTime property
public static ComponentRef<FusionXxx, TModel> SetValue<TModel>(
    this ComponentRef<FusionXxx, TModel> self, DateTime value) where TModel : class
    => self.Emit(new SetPropMutation("value"), value: value.ToString("yyyy-MM-dd"));
```

**Key:** The `coerce` parameter tells the runtime to parse the string value before assignment. Without it, `ej2.value = "42"` sets a string. With `coerce: "number"`, the runtime does `ej2.value = Number("42")`.

### Property writes from source — `SetPropMutation` + `EventSource`

Sets a property from an HTTP response or event payload:

```csharp
// From HTTP response
public static ComponentRef<FusionXxx, TModel> SetDataSource<TModel, TResponse>(
    this ComponentRef<FusionXxx, TModel> self,
    ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
    where TModel : class where TResponse : class
{
    var sourcePath = ExpressionPathHelper.ToResponsePath(path);
    return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
}

// From event payload
public static ComponentRef<FusionXxx, TModel> SetDataSource<TModel, TSource>(
    this ComponentRef<FusionXxx, TModel> self,
    TSource source, Expression<Func<TSource, object?>> path)
    where TModel : class
{
    var sourcePath = ExpressionPathHelper.ToEventPath(path);
    return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
}
```

### Method calls — `CallMutation`

Calls a method on the ej2 instance: `ej2[method]()`.

```csharp
// Void method (no arguments)
public static ComponentRef<FusionXxx, TModel> DataBind<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => self.Emit(new CallMutation("dataBind"));

public static ComponentRef<FusionXxx, TModel> FocusIn<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => self.Emit(new CallMutation("focusIn"));

public static ComponentRef<FusionXxx, TModel> ShowPopup<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => self.Emit(new CallMutation("showPopup"));
```

### Value read — `TypedComponentSource`

Returns a typed source for use in conditions or `SetText`:

```csharp
public static TypedComponentSource<string> Value<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
```

The type parameter (`string`, `bool`, `decimal`, `DateTime`) matches the component's semantic type. This flows through `When()` conditions with full compile-time type safety:

```csharp
pipeline.When(comp.Value()).Eq("some-value")  // compiler enforces string
```

### Dual-property reads (DateRangePicker pattern)

When a component exposes multiple readable properties, create separate read methods with hardcoded readExpr:

```csharp
public static TypedComponentSource<DateTime> StartDate<TModel>(
    this ComponentRef<FusionDateRangePicker, TModel> self) where TModel : class
    => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "startDate");

public static TypedComponentSource<DateTime> EndDate<TModel>(
    this ComponentRef<FusionDateRangePicker, TModel> self) where TModel : class
    => new TypedComponentSource<DateTime>(self.TargetId, Component.Vendor, "endDate");
```

---

## File 3: Factory method (Html extension)

Registers the component in the plan's `ComponentsMap` and renders the SF builder HTML.

```csharp
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns; // or the SF namespace for your component
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native.Extensions;

public static class FusionXxxHtmlExtensions
{
    private static readonly FusionXxx Component = new();

    public static void FusionXxx<TModel, TProp>(
        this InputBoundField<TModel, TProp> setup,
        Action<XxxBuilder> build)
        where TModel : class
    {
        // 1. Register in ComponentsMap
        setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
            setup.ElementId,
            Component.Vendor,                                // "fusion"
            setup.BindingPath,
            Component.ReadExpr,                              // "value" or "checked"
            "xxx",                                           // component type descriptor
            CoercionTypes.InferFromType(typeof(TProp))));    // auto-inferred

        // 2. Create the SF EJ2 builder — pass htmlAttributes as a PARAMETER to XxxFor()
        // CRITICAL: Do NOT use the fluent .HtmlAttributes() method — it does not override
        // the element ID on all SF components. Passing as a parameter to XxxFor() bakes the
        // custom ID into both the rendered HTML and the JS appendTo() target.
        var attrs = new Dictionary<string, object>
        {
            ["id"] = setup.ElementId,
            ["name"] = setup.BindingPath
        };
        var builder = setup.Helper.EJS().XxxFor(setup.Expression, attrs);

        // 3. Let the user build (DataSource, Placeholder, etc.)
        build(builder);

        // 4. Render — pass builder.Render() which returns IHtmlContent
        setup.Render(builder.Render());
    }
}
```

**How to find the SF builder type name:** The EJ2 tag helpers follow the pattern `setup.Helper.EJS().{ComponentName}For(expression)`. For DropDownList it's `DropDownListFor`, for DatePicker it's `DatePickerFor`, for ColorPicker it would be `ColorPickerFor`. The builder type is `{ComponentName}Builder` (e.g., `DropDownListBuilder`, `DatePickerBuilder`). Check the Syncfusion NuGet package for the exact type.

**ComponentRegistration fields explained:**

| Field | Source | Purpose |
|-------|--------|---------|
| `componentId` | `setup.ElementId` | DOM element ID for runtime lookup |
| `vendor` | `Component.Vendor` | `"fusion"` — runtime resolves `ej2_instances[0]` |
| `bindingPath` | `setup.BindingPath` | Model property name — key in `ComponentsMap` |
| `readExpr` | `Component.ReadExpr` | Property path for reading value |
| `componentType` | Literal string | Descriptive label (e.g., `"autocomplete"`, `"datepicker"`) |
| `coerceAs` | `CoercionTypes.InferFromType(typeof(TProp))` | Auto-inferred from model property type |

**Coercion inference:** `InferFromType` maps C# types to runtime coercion:

| C# type | Inferred coerceAs |
|---------|-------------------|
| `string` | `"string"` |
| `int`, `decimal`, `double`, `long` | `"number"` |
| `bool` | `"boolean"` |
| `DateTime`, `DateTimeOffset`, `DateOnly` | `"date"` |
| `string[]`, `List<string>` | `"array"` |

### Typed Fields helper (for list components)

List components (DropDown, AutoComplete, MultiSelect) need a `Fields<TItem>()` method that maps item properties to SF field settings:

```csharp
public static XxxBuilder Fields<TItem>(
    this XxxBuilder builder,
    Expression<Func<TItem, object?>> text,
    Expression<Func<TItem, object?>> value)
{
    builder.Fields = new XxxFieldSettings
    {
        Text = ToCamelCase(GetMemberName(text)),    // "Text" → "text"
        Value = ToCamelCase(GetMemberName(value))   // "Value" → "value"
    };
    return builder;
}
```

SF uses camelCase field names. The helper extracts the member name from the expression and converts it.

---

## File 4: Event descriptor registry

A singleton that maps event names to typed descriptors:

```csharp
public sealed class FusionXxxEvents
{
    public static readonly FusionXxxEvents Instance = new();
    private FusionXxxEvents() { }

    public TypedEventDescriptor<FusionXxxChangeArgs> Changed =>
        new("change", new FusionXxxChangeArgs());
}
```

Each property creates a `TypedEventDescriptor` with:
- **JS event name** — the string SF uses for `addEventListener` (e.g., `"change"`, `"filtering"`, `"focus"`, `"blur"`)
- **Phantom args instance** — used only for compile-time type inference, never read at runtime

Add one property per event. The JS event name comes from the SF API docs.

---

## File 5: Reactive extensions (.Reactive() wiring)

Thin bridge between the SF builder and the reactive plan:

```csharp
private static readonly FusionXxx Component = new();

public static XxxBuilder Reactive<TModel, TArgs>(
    this XxxBuilder builder,
    IReactivePlan<TModel> plan,
    Func<FusionXxxEvents, TypedEventDescriptor<TArgs>> eventSelector,
    Action<TArgs, PipelineBuilder<TModel>> pipeline)
    where TModel : class
{
    var descriptor = eventSelector(FusionXxxEvents.Instance);
    var pb = new PipelineBuilder<TModel>();
    pipeline(descriptor.Args, pb);

    // IMPORTANT: use builder.model.HtmlAttributes — NOT builder.HtmlAttributes
    // The .model property accesses the SF EJ2 control model where id/name are stored.
    var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
    var componentId = (string)attrs["id"];
    var bindingPath = (string)attrs["name"];

    var trigger = new ComponentEventTrigger(
        componentId,
        descriptor.JsEvent,      // "change", "filtering", etc.
        Component.Vendor,         // "fusion"
        bindingPath,
        Component.ReadExpr);      // "value", "checked", etc.

    foreach (var reaction in pb.BuildReactions())
        plan.AddEntry(new Entry(trigger, reaction));

    return builder;
}
```

**This file is identical across all components** except for the type names. The generic `TArgs` parameter makes it work with any event args class. You only need to change:
- Class name: `FusionXxxReactiveExtensions`
- Builder type: `XxxBuilder`
- Events type: `FusionXxxEvents`
- Component type: `FusionXxx`

---

## File 6: Event args (simple — no methods on args)

A plain class with properties matching the SF event object:

```csharp
public class FusionXxxChangeArgs
{
    public string? Value { get; set; }
    public bool IsInteracted { get; set; }
    public FusionXxxChangeArgs() { }
}
```

Properties become typed condition sources in the pipeline:

```csharp
.Reactive(plan, evt => evt.Changed, (args, p) =>
{
    p.When(args, x => x.Value).Eq("selected-value")
        .Then(t => t.Element("status").SetText("matched"));
})
```

The expression `x => x.Value` compiles to `"evt.value"` in the plan. At runtime, the SF event's `value` property is walked at that path.

**Common event args patterns:**

| Event | Properties | Notes |
|-------|-----------|-------|
| Changed | `Value` (string/bool/DateTime?), `IsInteracted` (bool) | Most common |
| Focus | (empty) | Marker class, no properties |
| Blur | (empty) | Marker class, no properties |
| Filtering | `Text` (string) | User's typed text |
| Selected | `FilesCount` (int), `IsInteracted` (bool) | File upload |

---

## File 7: Event args with extensions (Filtering pattern)

When the event args expose methods (like SF's `preventDefaultAction` or `updateData`), add extension methods in the same file:

```csharp
public class FusionXxxFilteringArgs
{
    public string Text { get; set; } = "";
    public FusionXxxFilteringArgs() { }
}

public static class FusionXxxFilteringArgsExtensions
{
    public static void PreventDefault(
        this FusionXxxFilteringArgs args,
        ICommandEmitter pipeline)
    {
        pipeline.AddCommand(new MutateEventCommand(
            new SetPropMutation("preventDefaultAction"), value: true));
    }

    public static void UpdateData<TResponse>(
        this FusionXxxFilteringArgs args,
        ICommandEmitter pipeline,
        ResponseBody<TResponse> source,
        Expression<Func<TResponse, object?>> path)
        where TResponse : class
    {
        var sourcePath = ExpressionPathHelper.ToResponsePath(path);
        pipeline.AddCommand(new MutateEventCommand(
            new CallMutation("updateData", args: new MethodArg[]
            {
                new SourceArg(new EventSource(sourcePath))
            })));
    }
}
```

**Key distinction — MutateEventCommand vs MutateElementCommand:**

| Command | Target | When to use |
|---------|--------|-------------|
| `MutateElementCommand` | DOM element / ej2 instance | Property sets, method calls on the component |
| `MutateEventCommand` | Event args object (`ctx.evt`) | Setting properties or calling methods on the triggering event |

`MutateEventCommand` is used for event args extensions because the target is the event object, not a DOM element. At runtime, `ctx.evt.preventDefaultAction = true` and `ctx.evt.updateData(data)` operate on the event args.

**Why `ICommandEmitter pipeline` parameter?** The `args` object is a phantom shared across the entire `.Reactive()` lambda. Unlike `ComponentRef` (created per-pipeline via `p.Component<T>()`), `args` has no pipeline binding. The pipeline must be passed explicitly so the extension can emit commands.

---

## What does NOT change

When onboarding any component — **none of these change:**

| Layer | Files | Why |
|-------|-------|-----|
| TS runtime | `trigger.ts`, `commands.ts`, `element.ts`, `gather.ts` | Plan carries vendor + readExpr, runtime resolves via bracket notation |
| JSON schema | `reactive-plan.schema.json` | Existing mutation kinds (set-prop, call) cover all component APIs |
| TS types | `types/*.ts` | No new command kinds or trigger kinds |
| Core descriptors | `Alis.Reactive/` project | Existing mutation algebra handles everything |

**If you find yourself modifying any of these, stop. You're doing it wrong.**

---

## Gather — no per-component file needed

The shared `GatherExtensions` works for any Fusion component:

```csharp
g.Include<FusionXxx, TModel>(m => m.Property)
```

It resolves the component from `ComponentsMap` using the binding path and reads via the registered `readExpr`. No per-component gather extension needed.

---

## Testing checklist

Every onboarded component needs tests at three layers. Test classes use BDD naming: `When{Scenario}`.

### C# unit tests (`Alis.Reactive.Fusion.UnitTests`)

Snapshot-verify each mutation. One test class per component:

```csharp
[TestFixture]
public class WhenMutatingAFusionXxx : FusionTestBase
{
    [Test]
    public Task SetValue_produces_correct_plan()
    {
        var plan = CreatePlan<TestModel>();
        Html.On(plan, t => t.DomReady(p =>
            p.Component<FusionXxx>(m => m.Property).SetValue("test")));
        return VerifyJson(plan.Render());
    }
}
```

`FusionTestBase` provides `CreatePlan<T>()` and `Html` (mocked `IHtmlHelper`). `VerifyJson` locks down exact JSON output via Verify.NUnit.

### Sandbox demo

Three files — model, controller, and view:

- **Model:** `Areas/Sandbox/Models/Xxx/XxxModel.cs` — property for each capability
- **Controller:** `Areas/Sandbox/Controllers/Xxx/XxxController.cs` — GET action returning the view, plus any HTTP endpoints for filtering/cascade
- **View:** `Areas/Sandbox/Views/Xxx/Index.cshtml` — numbered sections with `<span id="...">` echo elements for Playwright assertions

### Playwright tests

Navigate to the sandbox page, interact, assert DOM state:

```csharp
[TestFixture]
public class WhenUsingFusionXxx : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Xxx";

    [Test]
    public async Task selecting_a_value_updates_the_echo()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);

        // interact with component
        var input = Page.Locator("#Property");
        await input.ClickAsync();
        // ... select value ...

        // assert visible result
        await Expect(Page.Locator("#change-value"))
            .ToHaveTextAsync("expected-value");
        AssertNoConsoleErrors();
    }
}
```

### Run all tests

```bash
dotnet test tests/Alis.Reactive.Fusion.UnitTests     # C# mutations serialize correctly
dotnet test tests/Alis.Reactive.PlaywrightTests       # Browser behavior verified
```

---

## Variations and edge cases

Not every component follows the standard template. These patterns appear in the codebase and you may need them.

### Multiple events on one component

Some components have Changed + Focus + Blur. Add one `TypedEventDescriptor` per event:

```csharp
public TypedEventDescriptor<FusionXxxChangeArgs> Changed =>
    new("change", new FusionXxxChangeArgs());

public TypedEventDescriptor<FusionXxxFocusArgs> Focus =>
    new("focus", new FusionXxxFocusArgs());

public TypedEventDescriptor<FusionXxxBlurArgs> Blur =>
    new("blur", new FusionXxxBlurArgs());
```

Focus and Blur args are empty marker classes — no properties, just a parameterless constructor:

```csharp
public class FusionXxxFocusArgs
{
    public FusionXxxFocusArgs() { }
}
```

### Read-only components (no SetValue)

Some components are set by user interaction only (FileUpload, DateRangePicker). Provide `Value()` reads but intentionally omit write mutations. Document why:

```csharp
// No SetValue() — DateRangePicker is set by user interaction only.
// No SetStartDate()/SetEndDate() — SF has no API for setting individual dates.
```

### Components without `XxxFor()` factory

Some SF components (like Uploader) have no `For` helper. Use the ID-based constructor instead:

```csharp
// Standard (most components):
var builder = setup.Helper.EJS().DropDownListFor(setup.Expression)
    .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });

// Components without XxxFor() (e.g., Uploader, RichTextEditor):
var builder = setup.Helper.EJS().Uploader(setup.ElementId)
    .HtmlAttributes(new Dictionary<string, object> { ["name"] = setup.BindingPath });
```

When using the ID-based constructor, the reactive extension reads `componentId` from `builder.model.Id` instead of `HtmlAttributes["id"]`:

```csharp
// Standard: var componentId = (string)attrs["id"];
// ID-based: var componentId = builder.model.Id;
```

### Array-typed values (MultiSelect, CheckList)

For multi-select components, `SetValue` accepts `string[]?` and `Value()` returns `TypedComponentSource<string[]>`:

```csharp
public static ComponentRef<FusionMultiSelect, TModel> SetValue<TModel>(
    this ComponentRef<FusionMultiSelect, TModel> self, string[]? value)
    where TModel : class
    => self.Emit(new SetPropMutation("value"), value: value);

public static TypedComponentSource<string[]> Value<TModel>(
    this ComponentRef<FusionMultiSelect, TModel> self) where TModel : class
    => new TypedComponentSource<string[]>(self.TargetId, Component.Vendor, Component.ReadExpr);
```

### Fields with GroupBy (3-argument overload)

For grouped dropdown items, add a 3-argument `Fields<TItem>` overload:

```csharp
public static XxxBuilder Fields<TItem>(
    this XxxBuilder builder,
    Expression<Func<TItem, object?>> text,
    Expression<Func<TItem, object?>> value,
    Expression<Func<TItem, object?>> groupBy)
{
    return builder.Fields(new XxxFieldSettings
    {
        Text = ToCamelCase(GetMemberName(text)),
        Value = ToCamelCase(GetMemberName(value)),
        GroupBy = ToCamelCase(GetMemberName(groupBy))
    });
}
```

### AllowFiltering requirement for non-AutoComplete components

SF AutoComplete has filtering built-in. MultiSelect and DropDownList require `.AllowFiltering(true)` explicitly in the view — without it, the Filtering event **never fires** (silent failure, no errors):

```csharp
// AutoComplete — filtering works out of the box
.FusionAutoComplete(b => b.Reactive(plan, evt => evt.Filtering, ...))

// MultiSelect/DropDownList — MUST set AllowFiltering
.FusionMultiSelect(b => b.AllowFiltering(true).Reactive(plan, evt => evt.Filtering, ...))
```

### HtmlAttributes — parameter, not fluent method

SF components have two ways to set HtmlAttributes. **Only one works reliably for the `id` attribute:**

```csharp
// CORRECT — pass as parameter to XxxFor() (bakes ID into HTML + JS)
var attrs = new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath };
var builder = setup.Helper.EJS().XxxFor(setup.Expression, attrs);

// WRONG — fluent .HtmlAttributes() (does NOT override ID on all components)
var builder = setup.Helper.EJS().XxxFor(setup.Expression)
    .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId });
```

**Why:** The fluent `.HtmlAttributes()` method sets `model.HtmlAttributes` but does NOT update `model.Id` or regenerate the rendered HTML. Some SF JS components (like DropDownList) have a `setHTMLAttributes()` method that re-applies the ID at runtime, so it appears to work. Other components (like ColorPicker) lack this JS-side support, so the element renders with the wrong ID and the reactive runtime can't find it.

**The parameter overload** (`XxxFor(expression, htmlAttributes)`) calls `EJSUtil.GetHtmlId(htmlAttributes)` which correctly sets `model.Id` and bakes the custom ID into both the HTML output and the JS `appendTo()` target.

**Rule: Always pass HtmlAttributes as a parameter to `XxxFor()`, never as a fluent method.** This ensures the framework controls the element ID regardless of which SF component you're onboarding.

---

### Intentional API omissions

When an SF API doesn't work as expected (verified in browser), omit it and document why as a comment in the extensions file:

```csharp
// NOTE: showSpinner/hideSpinner have no visible effect on SF AutoComplete.
// refresh() causes focus loss mid-typing — not usable during filtering.
// Both verified manually via browser console. Omitted intentionally.
```

This prevents future developers from adding broken APIs.

---

## Verify in browser after onboarding — mandatory

`dotnet build` passing is necessary but NOT sufficient. You must run the sandbox page and verify every section works in a real browser. See the [Sandbox Demo guide](../sandbox-demo/#verify-in-the-browser--mandatory) for the full checklist.

Key things that only show up in the browser:
- **`[object Object]`** in echo spans → event arg property is an object, not a primitive
- **Element not found** errors in console → ID mismatch between plan and DOM (HtmlAttributes issue)
- **No change event firing** → SF component needs `.AllowFiltering(true)` or similar configuration
- **Value reads returning wrong type** → ReadExpr points to wrong property on ej2 instance

---

## Common mistakes

| Mistake | Why it's wrong | Correct approach |
|---------|---------------|-----------------|
| `Static("q", args.Text)` for event args | Resolves at C# compile time → always `""` | `FromEvent(args, x => x.Text, "q")` |
| `SetDataSource` for filtering events | SF lifecycle closes before async HTTP completes | `args.UpdateData(pipeline, json, path)` |
| `DataBind()` after `UpdateData` | `updateData` handles refresh internally | Only use `DataBind()` after `SetDataSource` in cascade patterns |
| Forgetting `PreventDefault` on filtering | SF flashes "No records found" during async HTTP | Call `args.PreventDefault(pipeline)` first |
| Modifying TS runtime for new component | Plan carries all behavior — runtime is a dumb executor | Zero runtime changes, always |
| Extensions on a builder class for args | Loses compile-time type safety | Extensions go directly on the args class |
| Using `showSpinner()`/`hideSpinner()` | Not built into dropdown components | Use DOM elements for loading indicators |

---

## Quick reference — mutation to plan JSON mapping

| C# extension | Mutation | Plan JSON | Runtime |
|--------------|----------|-----------|---------|
| `SetValue("x")` | `SetPropMutation("value")` | `{ kind: "set-prop", prop: "value" }` | `ej2.value = "x"` |
| `SetChecked(true)` | `SetPropMutation("checked", coerce: "boolean")` | `{ kind: "set-prop", prop: "checked", coerce: "boolean" }` | `ej2.checked = true` |
| `SetValue(42m)` | `SetPropMutation("value", coerce: "number")` | `{ kind: "set-prop", prop: "value", coerce: "number" }` | `ej2.value = 42` |
| `DataBind()` | `CallMutation("dataBind")` | `{ kind: "call", method: "dataBind" }` | `ej2.dataBind()` |
| `FocusIn()` | `CallMutation("focusIn")` | `{ kind: "call", method: "focusIn" }` | `ej2.focusIn()` |
| `ShowPopup()` | `CallMutation("showPopup")` | `{ kind: "call", method: "showPopup" }` | `ej2.showPopup()` |
| `Enable()` | `SetPropMutation("enabled")` | `{ kind: "set-prop", prop: "enabled" }` | `ej2.enabled = true` |
| `SetDataSource(json, x => x.Items)` | `SetPropMutation("dataSource")` + `EventSource` | `{ kind: "set-prop", prop: "dataSource", source: {...} }` | `ej2.dataSource = resolved` |
| `args.PreventDefault(p)` | `MutateEventCommand(SetPropMutation)` | `{ kind: "mutate-event", mutation: { kind: "set-prop", prop: "preventDefaultAction" } }` | `ctx.evt.preventDefaultAction = true` |
| `args.UpdateData(p, json, x => x.Items)` | `MutateEventCommand(CallMutation)` | `{ kind: "mutate-event", mutation: { kind: "call", method: "updateData" } }` | `ctx.evt.updateData(resolved)` |
| `comp.Value()` | Returns `TypedComponentSource<T>` | `{ kind: "component", componentId: "...", vendor: "fusion", readExpr: "value" }` | `ej2.value` (read) |

---

# Non-Input Component Pattern (IComponent only)

> Accordion, Tab, Toolbar, Sidebar, TreeView — components with events and methods but no form value.

Non-input components do NOT implement `IInputComponent`. They have no `ReadExpr`, no `Value()` read, no ComponentsMap registration, no validation, no gather participation.

## Differences from input components

| Aspect | Input component | Non-input component |
|--------|----------------|-------------------|
| Interface | `IInputComponent` (has `ReadExpr`) | `IComponent` only (no `ReadExpr`) |
| View factory | `Html.InputField(plan, m => m.Prop).Xxx(...)` | `Html.FusionXxx(plan, "elementId", ...)` |
| ComponentsMap | Registered (gather + validation) | NOT registered |
| Element ID | Derived from model expression | Explicit string parameter |
| `Value()` method | Yes — `TypedComponentSource<T>` | No — nothing to read |
| Wrapper HTML | Label + validation slot (InputField) | No wrapper — renders directly |

## Non-input file structure (5-6 files)

```
Alis.Reactive.Fusion/Components/FusionXxx/
├── FusionXxx.cs                      ← 1. Component type marker (IComponent, NOT IInputComponent)
├── FusionXxxExtensions.cs            ← 2. Mutations (methods + properties)
├── FusionXxxHtmlExtensions.cs        ← 3. Factory (NO InputField, NO ComponentsMap)
├── FusionXxxEvents.cs                ← 4. Event descriptor registry
├── FusionXxxReactiveExtensions.cs    ← 5. .Reactive() wiring
└── Events/
    └── FusionXxxOnSelected.cs        ← 6. Event args
```

## NI-File 1: Component type marker (NO IInputComponent)

```csharp
public sealed class FusionXxx : FusionComponent
{
    // NO ReadExpr — this component has no form value to read
}
```

Note: implements `FusionComponent` (which gives `IComponent` + `Vendor => "fusion"`) but does NOT implement `IInputComponent`. This means:
- `ComponentRef<FusionXxx, TModel>` works for mutations (SetPropMutation, CallMutation)
- `Value()` method is NOT available — nothing to read
- Component is NOT registered in ComponentsMap

## NI-File 2: Mutations (same patterns as input)

Extensions on `ComponentRef<FusionXxx, TModel>` work identically:

```csharp
private static readonly FusionXxx Component = new();

// Method call
public static ComponentRef<FusionXxx, TModel> Select<TModel>(
    this ComponentRef<FusionXxx, TModel> self, int index) where TModel : class
    => self.Emit(new CallMutation("select", args: new MethodArg[]
    {
        new LiteralArg(index)
    }));

// Void method
public static ComponentRef<FusionXxx, TModel> ExpandAll<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => self.Emit(new CallMutation("expandAll"));

// Property set
public static ComponentRef<FusionXxx, TModel> EnableTab<TModel>(
    this ComponentRef<FusionXxx, TModel> self, int index, bool enabled = true)
    where TModel : class
    => self.Emit(new CallMutation("enableTab", args: new MethodArg[]
    {
        new LiteralArg(index),
        new LiteralArg(enabled)
    }));
```

**No `Value()` method** — non-input components have nothing to read.

## NI-File 3: Factory (NO InputField, NO ComponentsMap)

Non-input components render directly — no InputField wrapper, no label, no validation slot, no ComponentsMap registration:

```csharp
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations; // or the SF namespace for your component

public static class FusionXxxHtmlExtensions
{
    public static FusionXxxBuilder<TModel> FusionXxx<TModel>(
        this IHtmlHelper<TModel> html,
        IReactivePlan<TModel> plan,
        string elementId,
        Action<XxxBuilder> build)
        where TModel : class
    {
        // NO ComponentsMap registration — this is NOT an input component

        var builder = html.EJS().Xxx(elementId);
        build(builder);

        return new FusionXxxBuilder<TModel>(plan, elementId, builder.Render());
    }
}
```

**Key differences from input component factory:**
- Takes `IHtmlHelper<TModel>` directly (not `InputBoundField`)
- Takes explicit `string elementId` (not model-expression-derived)
- Takes `IReactivePlan<TModel>` for passing to `.Reactive()`
- NO `plan.AddToComponentsMap()` call
- Returns a builder that wraps the SF content + allows `.Reactive()` chaining

## NI-File 4: Events (same pattern)

```csharp
public sealed class FusionXxxEvents
{
    public static readonly FusionXxxEvents Instance = new();
    private FusionXxxEvents() { }

    public TypedEventDescriptor<FusionXxxSelectedArgs> Selected =>
        new("selected", new FusionXxxSelectedArgs());
}
```

Same singleton pattern. The JS event name comes from SF docs.

## NI-File 5: Reactive extensions (same pattern)

```csharp
private static readonly FusionXxx Component = new();

public static FusionXxxBuilder<TModel> Reactive<TModel, TArgs>(
    this FusionXxxBuilder<TModel> builder,
    Func<FusionXxxEvents, TypedEventDescriptor<TArgs>> eventSelector,
    Action<TArgs, PipelineBuilder<TModel>> pipeline)
    where TModel : class
{
    var descriptor = eventSelector(FusionXxxEvents.Instance);
    var pb = new PipelineBuilder<TModel>();
    pipeline(descriptor.Args, pb);

    var trigger = new ComponentEventTrigger(
        builder.ElementId,
        descriptor.JsEvent,
        Component.Vendor,
        builder.ElementId,        // bindingPath = elementId for non-input
        "value");                  // readExpr placeholder (not used for non-input reads)

    foreach (var reaction in pb.BuildReactions())
        builder.Plan.AddEntry(new Entry(trigger, reaction));

    return builder;
}
```

**Key difference:** The plan and elementId come from the builder (not from `builder.model.HtmlAttributes`) since non-input components use a custom wrapper builder, not the raw SF builder.

## NI-File 6: Event args (same pattern)

```csharp
public class FusionXxxSelectedArgs
{
    public int SelectedIndex { get; set; }
    public bool IsInteracted { get; set; }
    public FusionXxxSelectedArgs() { }
}
```

Same pattern as input components — properties become condition sources in the pipeline.

## Non-input component usage in views

```csharp
@(Html.FusionXxx(plan, "my-accordion", b => b
    .DataSource(items)
    .Reactive(evt => evt.Selected, (args, p) =>
    {
        p.When(args, x => x.SelectedIndex).Eq(2)
            .Then(t => t.Element("status").SetText("Third panel selected"));
    })))
```

**No `Html.InputField()` wrapper.** No label. No validation slot. The component renders directly as `IHtmlContent`.

---

**Previous:** [Plan Composition](../plan-composition/) — how multiple plans merge and compose on a single page.
