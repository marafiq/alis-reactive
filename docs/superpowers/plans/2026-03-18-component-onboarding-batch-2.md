# Component Onboarding Batch 2 — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Onboard 6 new input components as pure-additive vertical slices — zero changes to existing code or runtime.

**Architecture:** Each component is a self-contained vertical slice (7 files + sandbox + tests). Native components render raw HTML; Fusion components delegate to Syncfusion EJ2 builders via `EJS()`. All components register in `ComponentsMap` for gather/validation. Each gets its own sandbox page demonstrating component-specific API (property write/read, events, conditions). HTTP gather integration is tested by adding ALL new components to a single dedicated "ComponentGather" page that POSTs every type together — proving they all work in one form.

**Tech Stack:** C# (ASP.NET Core MVC + Syncfusion EJ2 v32), TypeScript (vitest + jsdom), Playwright (.NET)

---

## Vertical Slice Constraints (INVIOLABLE)

1. **No changes to existing files** except:
   - `NativeTestBase.cs` — add properties to `NativeTestModel` for new native components
   - `FusionTestBase.cs` — add properties to `FusionTestModel` for new fusion components
   - `_Layout.cshtml` — add nav links for new sandbox pages
2. **No changes to TS runtime** — no edits to `resolver.ts`, `types.ts`, `conditions.ts`, `gather.ts`, `element.ts`, `component.ts`, `walk.ts`, `boot.ts`, `auto-boot.ts`, `execute.ts`, `trigger.ts`
3. **No changes to JSON schema** — existing coercion types and mutation kinds cover all 6 components
4. **No changes to core C# descriptors** — `MutateElementCommand`, `CoercionTypes`, `Guard*`, `Mutation*` are untouched
5. **Each component is 100% self-contained** — 7 files in its own folder under `Components/`
6. **Every sandbox page uses the design system** — `native-vstack`, `native-card`, `native-heading`, `native-text` tag helpers
7. **HTTP gather tested on a shared ComponentGather page** — one form with ALL 6 new components + existing components, POST endpoint echoes payload. Each sandbox page demos component-specific behavior only (events, conditions, property read/write) — NOT its own POST endpoint.
8. **Builder constructors are internal** — devs use `Html.InputField().ComponentName()` factories only

---

## Components to Onboard

| # | Component | Project | Vendor | ReadExpr | C# Type | JS Type | Coercion | SF Builder |
|---|-----------|---------|--------|----------|---------|---------|----------|------------|
| 1 | NativeTextArea | Native | native | value | `string` | string | string | N/A (raw HTML) |
| 2 | FusionDateTimePicker | Fusion | fusion | value | `DateTime` | Date | date | `DateTimePickerFor` |
| 3 | FusionDateRangePicker | Fusion | fusion | startDate | `DateTime` | Date | date | `DateRangePickerFor` |
| 4 | FusionInputMask | Fusion | fusion | value | `string` | string | string | `MaskedTextBoxFor` |
| 5 | FusionRichTextEditor | Fusion | fusion | value | `string` | string | string | `RichTextEditorFor` |
| 6 | FusionSwitch | Fusion | fusion | checked | `bool` | boolean | boolean | `SwitchFor` |

---

## Chunk 1: NativeTextArea

### Task 1: NativeTextArea vertical slice (7 files)

**Files to create:**
- `Alis.Reactive.Native/Components/NativeTextArea/NativeTextArea.cs`
- `Alis.Reactive.Native/Components/NativeTextArea/NativeTextAreaBuilder.cs`
- `Alis.Reactive.Native/Components/NativeTextArea/NativeTextAreaEvents.cs`
- `Alis.Reactive.Native/Components/NativeTextArea/Events/NativeTextAreaOnChanged.cs`
- `Alis.Reactive.Native/Components/NativeTextArea/NativeTextAreaExtensions.cs`
- `Alis.Reactive.Native/Components/NativeTextArea/NativeTextAreaHtmlExtensions.cs`
- `Alis.Reactive.Native/Components/NativeTextArea/NativeTextAreaReactiveExtensions.cs`

**File to modify:**
- `tests/Alis.Reactive.Native.UnitTests/NativeTestBase.cs` — add `public string? CareNotes { get; set; }` to `NativeTestModel`

**Pattern:** Clone NativeTextBox, change `<input>` to `<textarea>`, add `Rows()` builder method.

- [ ] **Step 1:** Create `NativeTextArea.cs` — sealed phantom type, `IInputComponent`, `ReadExpr => "value"`

```csharp
namespace Alis.Reactive.Native.Components
{
    public sealed class NativeTextArea : NativeComponent, IInputComponent
    {
        public string ReadExpr => "value";
    }
}
```

- [ ] **Step 2:** Create `Events/NativeTextAreaOnChanged.cs` — event args with `string? Value`

```csharp
namespace Alis.Reactive.Native.Components
{
    public class NativeTextAreaChangeArgs
    {
        public string? Value { get; set; }
        public NativeTextAreaChangeArgs() { }
    }
}
```

- [ ] **Step 3:** Create `NativeTextAreaEvents.cs` — singleton with `Changed` descriptor

```csharp
namespace Alis.Reactive.Native.Components
{
    public sealed class NativeTextAreaEvents
    {
        public static readonly NativeTextAreaEvents Instance = new NativeTextAreaEvents();
        private NativeTextAreaEvents() { }

        public TypedEventDescriptor<NativeTextAreaChangeArgs> Changed =>
            new TypedEventDescriptor<NativeTextAreaChangeArgs>("change", new NativeTextAreaChangeArgs());
    }
}
```

- [ ] **Step 4:** Create `NativeTextAreaBuilder.cs` — renders `<textarea>`, fluent `Rows()`, `Placeholder()`, `CssClass()`

```csharp
using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    public class NativeTextAreaBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;
        private int _rows = 4;
        private string? _cssClass;
        private string? _placeholder;

        internal NativeTextAreaBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        internal string ElementId => _elementId;
        internal string BindingPath => _bindingPath;

        public NativeTextAreaBuilder<TModel, TProp> Rows(int rows) { _rows = rows; return this; }
        public NativeTextAreaBuilder<TModel, TProp> CssClass(string css) { _cssClass = css; return this; }
        public NativeTextAreaBuilder<TModel, TProp> Placeholder(string placeholder) { _placeholder = placeholder; return this; }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["rows"] = _rows
            };
            if (_cssClass != null) attrs["class"] = _cssClass;
            if (_placeholder != null) attrs["placeholder"] = _placeholder;
            var result = _html.TextAreaFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }
}
```

- [ ] **Step 5:** Create `NativeTextAreaExtensions.cs` — `SetValue(string)`, `FocusIn()`, `Value()` returning `TypedComponentSource<string>`

```csharp
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Native.Components
{
    public static class NativeTextAreaExtensions
    {
        private static readonly NativeTextArea _component = new NativeTextArea();

        public static ComponentRef<NativeTextArea, TModel> SetValue<TModel>(
            this ComponentRef<NativeTextArea, TModel> self, string value) where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        public static ComponentRef<NativeTextArea, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextArea, TModel> self) where TModel : class
            => self.Emit(new CallMutation("focus"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeTextArea, TModel> self) where TModel : class
            => new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
    }
}
```

- [ ] **Step 6:** Create `NativeTextAreaHtmlExtensions.cs` — factory on `InputFieldSetup`, registers in `ComponentsMap`

```csharp
using System;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.Native.Components
{
    public static class NativeTextAreaHtmlExtensions
    {
        private static readonly NativeTextArea _component = new NativeTextArea();

        public static void NativeTextArea<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<NativeTextAreaBuilder<TModel, TProp>> configure) where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, _component.Vendor, setup.BindingPath, _component.ReadExpr));
            var builder = new NativeTextAreaBuilder<TModel, TProp>(setup.Helper, setup.Expression);
            configure(builder);
            setup.Render(builder);
        }
    }
}
```

- [ ] **Step 7:** Create `NativeTextAreaReactiveExtensions.cs` — `.Reactive()` on builder

```csharp
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    public static class NativeTextAreaReactiveExtensions
    {
        private static readonly NativeTextArea _component = new NativeTextArea();

        public static NativeTextAreaBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeTextAreaBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeTextAreaEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline) where TModel : class
        {
            var descriptor = eventSelector(NativeTextAreaEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);
            var trigger = new ComponentEventTrigger(
                builder.ElementId, descriptor.JsEvent, _component.Vendor,
                builder.BindingPath, _component.ReadExpr);
            plan.AddEntry(new Entry(trigger, pb.BuildReaction()));
            return builder;
        }
    }
}
```

- [ ] **Step 8:** Add `CareNotes` property to `NativeTestModel` in `NativeTestBase.cs`

```csharp
public string? CareNotes { get; set; }
```

- [ ] **Step 9:** Build and verify compilation: `dotnet build Alis.Reactive.Native && dotnet build tests/Alis.Reactive.Native.UnitTests`

### Task 2: NativeTextArea unit tests

**Files to create:**
- `tests/Alis.Reactive.Native.UnitTests/Components/NativeTextArea/WhenMutatingANativeTextArea.cs`
- `tests/Alis.Reactive.Native.UnitTests/Components/NativeTextArea/WhenDescribingNativeTextAreaEvents.cs`

- [ ] **Step 1:** Create `WhenMutatingANativeTextArea.cs` — SetValue snapshot + schema, FocusIn, Value source type

```csharp
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenMutatingANativeTextArea : NativeTestBase
{
    [Test]
    public Task SetValue_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeTextArea>(m => m.CareNotes).SetValue("Patient stable"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_dom_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<NativeTextArea>(m => m.CareNotes).FocusIn());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Value_returns_typed_component_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<NativeTextArea>(m => m.CareNotes).Value();
            Assert.That(source, Is.InstanceOf<Alis.Reactive.Builders.Conditions.TypedComponentSource<string>>());
        });
    }
}
```

- [ ] **Step 2:** Create `WhenDescribingNativeTextAreaEvents.cs` — singleton, jsEvent, args type

```csharp
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

[TestFixture]
public class WhenDescribingNativeTextAreaEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        Assert.That(NativeTextAreaEvents.Instance, Is.SameAs(NativeTextAreaEvents.Instance));
    }

    [Test]
    public void Changed_descriptor_has_correct_js_event()
    {
        Assert.That(NativeTextAreaEvents.Instance.Changed.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var args = NativeTextAreaEvents.Instance.Changed.Args;
        Assert.That(args, Is.Not.Null);
        Assert.That(args, Is.TypeOf<NativeTextAreaChangeArgs>());
    }
}
```

- [ ] **Step 3:** Run tests, accept snapshots: `dotnet test tests/Alis.Reactive.Native.UnitTests --filter "NativeTextArea"`
- [ ] **Step 4:** Re-run to verify snapshots pass: `dotnet test tests/Alis.Reactive.Native.UnitTests --filter "NativeTextArea"`

### Task 3: NativeTextArea sandbox page

**Files to create:**
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/NativeTextAreaModel.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/NativeTextAreaController.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/NativeTextArea/Index.cshtml`

**Domain:** Care notes and incident descriptions for a resident.

- [ ] **Step 1:** Create `NativeTextAreaModel.cs`

```csharp
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NativeTextAreaModel
    {
        public string? ResidentName { get; set; }
        public string? CareNotes { get; set; }
        public string? IncidentDescription { get; set; }
    }
}
```

- [ ] **Step 2:** Create `NativeTextAreaController.cs` with Index only (no POST — HTTP gather tested on shared page)

```csharp
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class NativeTextAreaController : Controller
    {
        public IActionResult Index()
        {
            return View(new NativeTextAreaModel { CareNotes = "Patient admitted. Initial assessment completed." });
        }
    }
}
```

- [ ] **Step 3:** Create `Views/NativeTextArea/Index.cshtml` — 4 sections: property write, property read, event+condition, plan JSON. Uses `native-vstack`, `native-card`, `native-heading`, `native-text` design system tag helpers. NO HTTP POST section — that is on the shared ComponentGather page.

- [ ] **Step 4:** Build and verify: `dotnet build Alis.Reactive.SandboxApp`
- [ ] **Step 5:** Add nav link in `_Layout.cshtml` after the MultiSelect link

### Task 4: NativeTextArea Playwright tests

**File to create:**
- `tests/Alis.Reactive.PlaywrightTests/Components/Native/WhenUsingNativeTextArea.cs`

- [ ] **Step 1:** Create Playwright test with sections: page loads, property write sets initial value, property read echoes value, changed event with condition, component value condition. NO HTTP POST test — that goes on shared ComponentGather page.
- [ ] **Step 2:** Run: `dotnet test tests/Alis.Reactive.PlaywrightTests --filter "NativeTextArea"`
- [ ] **Step 3:** Commit: `git commit -m "feat: NativeTextArea vertical slice with sandbox + tests"`

---

## Chunk 2: FusionDateTimePicker

### Task 5: FusionDateTimePicker vertical slice (7 files)

**Files to create:**
- `Alis.Reactive.Fusion/Components/FusionDateTimePicker/FusionDateTimePicker.cs`
- `Alis.Reactive.Fusion/Components/FusionDateTimePicker/FusionDateTimePickerBuilder.cs` — NOT needed (SF builder)
- `Alis.Reactive.Fusion/Components/FusionDateTimePicker/FusionDateTimePickerEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionDateTimePicker/Events/FusionDateTimePickerOnChanged.cs`
- `Alis.Reactive.Fusion/Components/FusionDateTimePicker/FusionDateTimePickerExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionDateTimePicker/FusionDateTimePickerHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionDateTimePicker/FusionDateTimePickerReactiveExtensions.cs`

**File to modify:**
- `tests/Alis.Reactive.Fusion.UnitTests/FusionTestBase.cs` — add `public DateTime? AppointmentTime { get; set; }` to `FusionTestModel`

**Pattern:** Clone FusionDatePicker. Change `DatePickerFor` → `DateTimePickerFor`, use `Syncfusion.EJ2.Calendars.DateTimePickerBuilder`.

- [ ] **Step 1:** Create `FusionDateTimePicker.cs` — sealed, `FusionComponent`, `IInputComponent`, `ReadExpr => "value"`
- [ ] **Step 2:** Create `Events/FusionDateTimePickerOnChanged.cs` — `DateTime? Value`, `bool IsInteracted`
- [ ] **Step 3:** Create `FusionDateTimePickerEvents.cs` — singleton, `Changed` → `"change"`
- [ ] **Step 4:** Create `FusionDateTimePickerExtensions.cs` — `SetValue(DateTime)`, `Value()` → `TypedComponentSource<DateTime>`, coercion = `"date"`
- [ ] **Step 5:** Create `FusionDateTimePickerHtmlExtensions.cs` — factory using `setup.Helper.EJS().DateTimePickerFor(setup.Expression)`, register in ComponentsMap
- [ ] **Step 6:** Create `FusionDateTimePickerReactiveExtensions.cs` — `.Reactive()` on SF `DateTimePickerBuilder`
- [ ] **Step 7:** Add `AppointmentTime` to `FusionTestModel`
- [ ] **Step 8:** Build: `dotnet build Alis.Reactive.Fusion`

### Task 6: FusionDateTimePicker tests + sandbox

- [ ] **Step 1:** Create unit tests: `WhenMutatingAFusionDateTimePicker.cs`, `WhenDescribingFusionDateTimePickerEvents.cs`
- [ ] **Step 2:** Run and accept snapshots: `dotnet test tests/Alis.Reactive.Fusion.UnitTests --filter "DateTimePicker"`
- [ ] **Step 3:** Create sandbox model `DateTimePickerModel.cs` — `DateTime? MedicationTime`, `string? ResidentName`
- [ ] **Step 4:** Create controller `DateTimePickerController.cs` — Index only (no POST)
- [ ] **Step 5:** Create view `Views/DateTimePicker/Index.cshtml` — property write, read, event+condition, plan JSON (no HTTP POST section)
- [ ] **Step 6:** Add nav link in `_Layout.cshtml`
- [ ] **Step 7:** Create Playwright test `WhenUsingFusionDateTimePicker.cs`
- [ ] **Step 8:** Run all: `dotnet test tests/Alis.Reactive.Fusion.UnitTests --filter "DateTimePicker" && dotnet test tests/Alis.Reactive.PlaywrightTests --filter "DateTimePicker"`
- [ ] **Step 9:** Commit: `git commit -m "feat: FusionDateTimePicker vertical slice with sandbox + tests"`

---

## Chunk 3: FusionDateRangePicker

### Task 7: FusionDateRangePicker vertical slice (7 files)

**Design decision:** SF DateRangePicker exposes `ej2.startDate` and `ej2.endDate` as separate `Date` properties on the instance. We use `ReadExpr => "startDate"` as the primary read (for gather — binds to the start date). The change event args expose both `StartDate` and `EndDate` as typed `DateTime?` properties for condition expressions. This avoids the array coercion lie — a date range is NOT semantically an array, it's a structured from/to pair.

**The model binds to TWO separate `DateTime?` properties** (not a `DateTime[]`). The gather sends `StartDate` and `EndDate` as two separate scalar date values.

**Files to create:**
- `Alis.Reactive.Fusion/Components/FusionDateRangePicker/FusionDateRangePicker.cs`
- `Alis.Reactive.Fusion/Components/FusionDateRangePicker/FusionDateRangePickerEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionDateRangePicker/Events/FusionDateRangePickerOnChanged.cs`
- `Alis.Reactive.Fusion/Components/FusionDateRangePicker/FusionDateRangePickerExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionDateRangePicker/FusionDateRangePickerHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionDateRangePicker/FusionDateRangePickerReactiveExtensions.cs`

**File to modify:**
- `FusionTestBase.cs` — add `public DateTime? StayStart { get; set; }` and `public DateTime? StayEnd { get; set; }`

**Note on ComponentsMap:** DateRangePicker registers TWO entries in ComponentsMap — one for startDate (readExpr: "startDate") and one for endDate (readExpr: "endDate"). The HtmlExtensions factory handles this dual registration.

- [ ] **Step 1:** Create phantom type — `ReadExpr => "startDate"` (primary property)
- [ ] **Step 2:** Create event args — `DateTime? StartDate`, `DateTime? EndDate`, `int DaySpan`, `bool IsInteracted`
- [ ] **Step 3:** Create events singleton — `Changed` → `"change"`
- [ ] **Step 4:** Create extensions:
  - `StartDate()` → `TypedComponentSource<DateTime>` with readExpr `"startDate"`
  - `EndDate()` → `TypedComponentSource<DateTime>` with readExpr `"endDate"`
  - `Value()` → same as `StartDate()` (primary read)
  - No `SetValue()` — DateRangePicker is set by user interaction, not programmatically
- [ ] **Step 5:** Create HTML extensions — `setup.Helper.EJS().DateRangePickerFor(setup.Expression)`. Register TWO ComponentsMap entries: one with readExpr `"startDate"`, one with readExpr `"endDate"`, using qualified binding paths (e.g. `StayStart`, `StayEnd`).
- [ ] **Step 6:** Create reactive extensions
- [ ] **Step 7:** Add properties to test base, build

### Task 8: FusionDateRangePicker tests + sandbox

- [ ] **Step 1:** Unit tests — StartDate/EndDate typed sources, schema validation
- [ ] **Step 2:** Sandbox: `DateRangePickerModel.cs` (`DateTime? StayStart`, `DateTime? StayEnd`, `string? ResidentName`), controller, view with conditions on startDate/endDate
- [ ] **Step 3:** Nav link in layout
- [ ] **Step 4:** Playwright tests — verify date conditions fire correctly
- [ ] **Step 5:** Commit: `git commit -m "feat: FusionDateRangePicker vertical slice with sandbox + tests"`

---

## Chunk 4: FusionInputMask

### Task 9: FusionInputMask vertical slice (7 files)

**Pattern:** Scalar string. Clone FusionAutoComplete pattern. SF builder = `MaskedTextBoxFor`. ReadExpr = `"value"`.

**Files to create:**
- `Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMask.cs`
- `Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionInputMask/Events/FusionInputMaskOnChanged.cs`
- `Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskReactiveExtensions.cs`

**File to modify:**
- `FusionTestBase.cs` — add `public string? PhoneNumber { get; set; }`

- [ ] **Step 1:** Phantom type — `ReadExpr => "value"` (MaskedTextBox reads `.value` from ej2 instance)
- [ ] **Step 2:** Event args — `string? Value`, `bool IsInteracted`
- [ ] **Step 3:** Events singleton — `Changed` → `"change"`
- [ ] **Step 4:** Extensions — `SetValue(string)`, `Value()` → `TypedComponentSource<string>`
- [ ] **Step 5:** HTML extensions — `setup.Helper.EJS().MaskedTextBoxFor(setup.Expression)` with `Mask()` builder method
- [ ] **Step 6:** Reactive extensions
- [ ] **Step 7:** Test base property, build

### Task 10: FusionInputMask tests + sandbox

- [ ] **Step 1:** Unit tests (snapshot + schema)
- [ ] **Step 2:** Sandbox: `InputMaskModel.cs` (PhoneNumber, SSN, InsuranceId), controller (Index only), view with events + conditions (no HTTP POST)
- [ ] **Step 3:** Nav link, Playwright tests
- [ ] **Step 4:** Commit: `git commit -m "feat: FusionInputMask vertical slice with sandbox + tests"`

---

## Chunk 5: FusionRichTextEditor

### Task 11: FusionRichTextEditor vertical slice (7 files)

**Pattern:** Scalar string (HTML content). SF builder = `RichTextEditorFor`. ReadExpr = `"value"` (RTE `.value` returns HTML string).

**Files to create:**
- `Alis.Reactive.Fusion/Components/FusionRichTextEditor/FusionRichTextEditor.cs`
- `Alis.Reactive.Fusion/Components/FusionRichTextEditor/FusionRichTextEditorEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionRichTextEditor/Events/FusionRichTextEditorOnChanged.cs`
- `Alis.Reactive.Fusion/Components/FusionRichTextEditor/FusionRichTextEditorExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionRichTextEditor/FusionRichTextEditorHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionRichTextEditor/FusionRichTextEditorReactiveExtensions.cs`

**File to modify:**
- `FusionTestBase.cs` — add `public string? CarePlan { get; set; }`

- [ ] **Step 1:** Phantom type — `ReadExpr => "value"`
- [ ] **Step 2:** Event args — `string? Value`, `bool IsInteracted`
- [ ] **Step 3:** Events singleton — `Changed` → `"change"`
- [ ] **Step 4:** Extensions — `SetValue(string)`, `Value()` → `TypedComponentSource<string>`
- [ ] **Step 5:** HTML extensions — `setup.Helper.EJS().RichTextEditorFor(setup.Expression)`, register ComponentsMap
- [ ] **Step 6:** Reactive extensions
- [ ] **Step 7:** Test base, build

### Task 12: FusionRichTextEditor tests + sandbox

- [ ] **Step 1:** Unit tests
- [ ] **Step 2:** Sandbox: `RichTextEditorModel.cs` (CarePlan, DischargeSummary), controller (Index only), view with events + conditions (no HTTP POST)
- [ ] **Step 3:** Nav link, Playwright tests
- [ ] **Step 4:** Commit: `git commit -m "feat: FusionRichTextEditor vertical slice with sandbox + tests"`

---

## Chunk 6: FusionSwitch

### Task 13: FusionSwitch vertical slice (7 files)

**Pattern:** Boolean. Clone NativeCheckBox conceptually but Fusion vendor. SF builder = `SwitchFor`. ReadExpr = `"checked"`. Coercion = `"boolean"`.

**Files to create:**
- `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitch.cs`
- `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchEvents.cs`
- `Alis.Reactive.Fusion/Components/FusionSwitch/Events/FusionSwitchOnChanged.cs`
- `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchHtmlExtensions.cs`
- `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchReactiveExtensions.cs`

**File to modify:**
- `FusionTestBase.cs` — add `public bool ReceiveNotifications { get; set; }`

- [ ] **Step 1:** Phantom type — `ReadExpr => "checked"` (Switch reads `.checked` from ej2 instance)
- [ ] **Step 2:** Event args — `bool Checked`, `bool IsInteracted`
- [ ] **Step 3:** Events singleton — `Changed` → `"change"`
- [ ] **Step 4:** Extensions — `SetChecked(bool)` → `SetPropMutation("checked", coerce: "boolean")`, `Value()` → `TypedComponentSource<bool>`
- [ ] **Step 5:** HTML extensions — `setup.Helper.EJS().SwitchFor(setup.Expression)`, register ComponentsMap with `readExpr: "checked"`
- [ ] **Step 6:** Reactive extensions
- [ ] **Step 7:** Test base, build

### Task 14: FusionSwitch tests + sandbox

- [ ] **Step 1:** Unit tests
- [ ] **Step 2:** Sandbox: `SwitchModel.cs` (ReceiveNotifications, EmailAlerts, SmsAlerts), controller (Index only), view with events + conditions (no HTTP POST)
- [ ] **Step 3:** Nav link, Playwright tests — verify boolean toggle via conditions
- [ ] **Step 4:** Commit: `git commit -m "feat: FusionSwitch vertical slice with sandbox + tests"`

---

## Chunk 7: Shared ComponentGather page + final verification

### Task 15: ComponentGather sandbox page — all components in one form

**Purpose:** One page with ALL 6 new components + a NativeTextBox (existing) in a single `<form>`. A POST button gathers every field via `IncludeAll()` and POSTs to an echo endpoint. This proves all component types work together in a real HTTP gather flow — scalar strings, booleans, dates, and the DateRangePicker's structured start/end dates.

**Files to create:**
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/ComponentGatherModel.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/ComponentGatherController.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/ComponentGather/Index.cshtml`

- [ ] **Step 1:** Create `ComponentGatherModel.cs` — one property per component type:

```csharp
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ComponentGatherModel
    {
        // Existing component types (prove they still work)
        public string? ResidentName { get; set; }          // NativeTextBox
        // New batch 2 components
        public string? CareNotes { get; set; }             // NativeTextArea
        public DateTime? MedicationTime { get; set; }       // FusionDateTimePicker
        public DateTime? StayStart { get; set; }            // FusionDateRangePicker (startDate)
        public DateTime? StayEnd { get; set; }              // FusionDateRangePicker (endDate)
        public string? PhoneNumber { get; set; }            // FusionInputMask
        public string? CarePlan { get; set; }               // FusionRichTextEditor
        public bool ReceiveNotifications { get; set; }      // FusionSwitch
    }
}
```

- [ ] **Step 2:** Create `ComponentGatherController.cs` — Index + Echo POST endpoint

```csharp
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ComponentGatherController : Controller
    {
        public IActionResult Index()
        {
            return View(new ComponentGatherModel
            {
                ResidentName = "Margaret Thompson",
                CareNotes = "Initial assessment completed.",
                ReceiveNotifications = true
            });
        }

        [HttpPost]
        public IActionResult Echo([FromBody] ComponentGatherModel? model)
        {
            if (model == null)
                return BadRequest(new { error = "Empty body" });
            return Ok(model);
        }
    }
}
```

- [ ] **Step 3:** Create `Views/ComponentGather/Index.cshtml` — single form with all 8 components (1 existing + 6 new + DateRangePicker's dual binding). Uses design system tag helpers. Submit button gathers via `IncludeAll()`, POSTs to Echo, shows gathered JSON in result area.

- [ ] **Step 4:** Add nav link in `_Layout.cshtml`
- [ ] **Step 5:** Build: `dotnet build Alis.Reactive.SandboxApp`

### Task 16: ComponentGather Playwright tests

**File to create:**
- `tests/Alis.Reactive.PlaywrightTests/Requests/WhenGatheringAllComponentTypes.cs`

- [ ] **Step 1:** Create Playwright test that:
  - Navigates to `/Sandbox/ComponentGather`, waits for boot
  - Fills/selects values in every component
  - Clicks Submit, intercepts the POST request via `RunAndWaitForRequestAsync`
  - Asserts the JSON body contains ALL fields with correct types:
    - `ResidentName` = string
    - `CareNotes` = string (textarea)
    - `MedicationTime` = date value (non-null)
    - `StayStart` = date value (from DateRangePicker)
    - `StayEnd` = date value (from DateRangePicker)
    - `PhoneNumber` = string (masked)
    - `CarePlan` = string (HTML content)
    - `ReceiveNotifications` = boolean

- [ ] **Step 2:** Run: `dotnet test tests/Alis.Reactive.PlaywrightTests --filter "WhenGatheringAllComponentTypes"`
- [ ] **Step 3:** Commit: `git commit -m "feat: ComponentGather page proving all component types work together in HTTP POST"`

### Task 17: TS gather tests + full suite verification

- [ ] **Step 1:** Add new components to gather test: `when-gathering-form-values.test.ts` (additive)
  - NativeTextArea (native, value, string) — POST + GET
  - FusionDateTimePicker (fusion, value, Date) — POST + GET
  - FusionDateRangePicker (fusion, startDate, Date) — POST + GET (scalar, not array)
  - FusionDateRangePicker (fusion, endDate, Date) — POST + GET (second scalar)
  - FusionInputMask (fusion, value, string) — POST + GET
  - FusionRichTextEditor (fusion, value, string) — POST + GET
  - FusionSwitch (fusion, checked, boolean) — POST + GET

- [ ] **Step 2:** Run full test suite:

```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

- [ ] **Step 3:** Commit: `git commit -m "test: gather coverage for all 6 new components + shared ComponentGather page"`

- [ ] **Step 4:** Verify no existing tests broke — modified files outside vertical slices are ONLY: `NativeTestBase.cs`, `FusionTestBase.cs`, `_Layout.cshtml`, `when-gathering-form-values.test.ts` (all additive).

---

## Summary

| Task | Component | Deliverables |
|------|-----------|-------------|
| 1-4 | NativeTextArea | 7 slice files + sandbox (model + controller + view) + unit tests + Playwright |
| 5-6 | FusionDateTimePicker | 7 slice files + sandbox + unit tests + Playwright |
| 7-8 | FusionDateRangePicker | 7 slice files + sandbox + unit tests + Playwright (startDate/endDate, NOT array) |
| 9-10 | FusionInputMask | 7 slice files + sandbox + unit tests + Playwright |
| 11-12 | FusionRichTextEditor | 7 slice files + sandbox + unit tests + Playwright |
| 13-14 | FusionSwitch | 7 slice files + sandbox + unit tests + Playwright |
| 15-16 | ComponentGather | Shared page with ALL components in one form + POST echo + Playwright (HTTP body assertion) |
| 17 | TS gather tests | 14 new gather tests (7 component reads × POST + GET) + full suite verification |

**Sandbox pages:** Each component gets its own page for component-specific demos (events, conditions, property read/write). HTTP gather is tested ONLY on the shared ComponentGather page where all types coexist in one form.

**Total new files:** ~70 (7 slice + 3 sandbox) × 6 components + ComponentGather page + tests
**Modified files:** 3 (NativeTestBase, FusionTestBase, _Layout.cshtml) + 1 gather test (all additive)
**Existing code changes:** Zero (additive properties to test models, additive nav links, additive gather tests)
