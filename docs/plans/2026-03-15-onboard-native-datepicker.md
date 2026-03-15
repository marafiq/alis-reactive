# Onboard NativeDatePicker — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Onboard NativeDatePicker as a complete vertical slice — component, builder, extensions, events, reactive, C# unit tests, sandbox view+controller, and BDD Playwright test. Proves the onboarding pattern for all subsequent components.

**Architecture:** NativeDatePicker wraps `<input type="date">`. It follows the exact same vertical slice shape as NativeTextBox (ReadExpr => "value", Changed event, SetValue mutation, Value() source). Duplication from NativeTextBox is intentional — each slice is self-contained. The sandbox page uses the senior living domain (resident admission date, birth date).

**Tech Stack:** C#/.NET 10, NUnit, Verify.NUnit, JsonSchema.Net, Playwright.NUnit

**Spec:** `docs/specs/2026-03-15-bdd-playwright-redesign.md`

---

## VERTICAL SLICE SHAPE — INVIOLABLE

Every task in this plan produces files matching this exact shape. If any file is missing, the slice is incomplete.

```
Component Slice (Alis.Reactive.Native/Components/NativeDatePicker/):
  NativeDatePicker.cs                    — sealed : NativeComponent, IInputComponent
  NativeDatePickerBuilder.cs             — internal ctor, IHtmlContent
  NativeDatePickerHtmlExtensions.cs      — Html.NativeDatePickerFor(plan, expr)
  NativeDatePickerExtensions.cs          — SetValue(), FocusIn(), Value()
  NativeDatePickerEvents.cs              — Singleton, private ctor, Changed event
  NativeDatePickerReactiveExtensions.cs  — Single .Reactive<TModel, TProp, TArgs>()
  Events/NativeDatePickerOnChanged.cs    — NativeDatePickerChangeArgs { Value }

C# Unit Tests (tests/Alis.Reactive.Native.UnitTests/Components/NativeDatePicker/):
  WhenMutatingANativeDatePicker.cs       — SetValue + FocusIn snapshot + schema
  WhenReactingToNativeDatePickerEvents.cs — Changed event + condition snapshot + schema
  WhenDescribingNativeDatePickerEvents.cs — Singleton, JsEvent, Args type

Sandbox (Alis.Reactive.SandboxApp/Areas/Sandbox/):
  Models/NativeDatePickerModel.cs        — AdmissionDate, BirthDate (senior living)
  Controllers/NativeDatePickerController.cs — Index + Echo (own actions, no sharing)
  Views/NativeDatePicker/Index.cshtml    — Html.Field() + .Reactive() + When() conditions

Playwright (tests/Alis.Reactive.PlaywrightTests/Components/Native/):
  WhenUsingNativeDatePicker.cs           — BDD: property write, read, event, conditions, reactive
```

---

## Chunk 1: Component Vertical Slice

### Task 1: Phantom Type + Builder

**Files:**
- Create: `Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePicker.cs`
- Create: `Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerBuilder.cs`

- [ ] **Step 1: Create phantom type**

```csharp
// NativeDatePicker.cs
namespace Alis.Reactive.Native.Components
{
    public sealed class NativeDatePicker : NativeComponent, IInputComponent
    {
        public string ReadExpr => "value";
    }
}
```

- [ ] **Step 2: Create builder**

```csharp
// NativeDatePickerBuilder.cs
using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    public class NativeDatePickerBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;
        private string? _cssClass;

        internal NativeDatePickerBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        internal string ElementId => _elementId;
        internal string BindingPath => _bindingPath;

        public NativeDatePickerBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["type"] = "date"
            };
            if (_cssClass != null) attrs["class"] = _cssClass;

            var result = _html.TextBoxFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build Alis.Reactive.Native`
Expected: Build succeeded

---

### Task 2: Factory + Extensions + Events + Reactive

**Files:**
- Create: `Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerHtmlExtensions.cs`
- Create: `Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerExtensions.cs`
- Create: `Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerEvents.cs`
- Create: `Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerReactiveExtensions.cs`
- Create: `Alis.Reactive.Native/Components/NativeDatePicker/Events/NativeDatePickerOnChanged.cs`

- [ ] **Step 1: Create factory extension**

```csharp
// NativeDatePickerHtmlExtensions.cs
using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    public static class NativeDatePickerHtmlExtensions
    {
        private static readonly NativeDatePicker _component = new NativeDatePicker();

        public static NativeDatePickerBuilder<TModel, TProp> NativeDatePickerFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For<TModel, TProp>(expression);
            var name = html.NameFor(expression);

            plan.AddToComponentsMap(name, new ComponentRegistration(
                uniqueId,
                _component.Vendor,
                name,
                _component.ReadExpr));

            return new NativeDatePickerBuilder<TModel, TProp>(html, expression);
        }
    }
}
```

- [ ] **Step 2: Create mutation extensions**

```csharp
// NativeDatePickerExtensions.cs
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Native.Components
{
    public static class NativeDatePickerExtensions
    {
        private static readonly NativeDatePicker _component = new NativeDatePicker();

        public static ComponentRef<NativeDatePicker, TModel> SetValue<TModel>(
            this ComponentRef<NativeDatePicker, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        public static ComponentRef<NativeDatePicker, TModel> FocusIn<TModel>(
            this ComponentRef<NativeDatePicker, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeDatePicker, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
```

- [ ] **Step 3: Create event args**

```csharp
// Events/NativeDatePickerOnChanged.cs
namespace Alis.Reactive.Native.Components
{
    public class NativeDatePickerChangeArgs
    {
        public string? Value { get; set; }
        public NativeDatePickerChangeArgs() { }
    }
}
```

- [ ] **Step 4: Create events singleton**

```csharp
// NativeDatePickerEvents.cs
namespace Alis.Reactive.Native.Components
{
    public sealed class NativeDatePickerEvents
    {
        public static readonly NativeDatePickerEvents Instance = new NativeDatePickerEvents();
        private NativeDatePickerEvents() { }

        public TypedEventDescriptor<NativeDatePickerChangeArgs> Changed =>
            new TypedEventDescriptor<NativeDatePickerChangeArgs>(
                "change", new NativeDatePickerChangeArgs());
    }
}
```

- [ ] **Step 5: Create reactive extensions**

```csharp
// NativeDatePickerReactiveExtensions.cs
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    public static class NativeDatePickerReactiveExtensions
    {
        private static readonly NativeDatePicker _component = new NativeDatePicker();

        public static NativeDatePickerBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeDatePickerBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeDatePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeDatePickerEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(
                builder.ElementId, descriptor.JsEvent, _component.Vendor,
                builder.BindingPath, _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }
    }
}
```

- [ ] **Step 6: Verify build**

Run: `dotnet build Alis.Reactive.Native`
Expected: Build succeeded

- [ ] **Step 7: Commit**

```bash
git add Alis.Reactive.Native/Components/NativeDatePicker/
git commit -m "feat: onboard NativeDatePicker vertical slice — component, builder, extensions, events, reactive"
```

---

## Chunk 2: C# Unit Tests

### Task 3: Mutation Snapshot Tests

**Files:**
- Create: `tests/Alis.Reactive.Native.UnitTests/Components/NativeDatePicker/WhenMutatingANativeDatePicker.cs`

- [ ] **Step 1: Write mutation tests**

```csharp
namespace Alis.Reactive.Native.UnitTests.Components.NativeDatePicker;

public class WhenMutatingANativeDatePicker : NativeTestBase
{
    [Test]
    public Task SetValue_produces_dom_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<Native.Components.NativeDatePicker>(m => m.AdmissionDate)
                .SetValue("2026-03-15"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task FocusIn_produces_dom_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<Native.Components.NativeDatePicker>(m => m.AdmissionDate)
                .FocusIn());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
```

Note: `NativeTestModel` needs an `AdmissionDate` property (string or DateTime). Add it to the test model.

- [ ] **Step 2: Add AdmissionDate to NativeTestModel**

Add `public string? AdmissionDate { get; set; }` to the existing `NativeTestModel` in `NativeTestBase.cs`.

- [ ] **Step 3: Run tests — expect fail (no snapshots yet)**

Run: `dotnet test tests/Alis.Reactive.Native.UnitTests --filter NativeDatePicker`
Expected: Tests run, snapshots created for first-time verification. Review and accept snapshots.

- [ ] **Step 4: Accept verified snapshots**

Review the generated `.received.txt` files. If correct, rename to `.verified.txt`.

- [ ] **Step 5: Run tests — expect pass**

Run: `dotnet test tests/Alis.Reactive.Native.UnitTests --filter NativeDatePicker`
Expected: PASS

---

### Task 4: Event Descriptor + Reactive Wiring Tests

**Files:**
- Create: `tests/Alis.Reactive.Native.UnitTests/Components/NativeDatePicker/WhenDescribingNativeDatePickerEvents.cs`
- Create: `tests/Alis.Reactive.Native.UnitTests/Components/NativeDatePicker/WhenReactingToNativeDatePickerEvents.cs`

- [ ] **Step 1: Write event descriptor tests**

```csharp
namespace Alis.Reactive.Native.UnitTests.Components.NativeDatePicker;

public class WhenDescribingNativeDatePickerEvents
{
    [Test]
    public void Singleton_returns_same_instance()
    {
        var a = NativeDatePickerEvents.Instance;
        var b = NativeDatePickerEvents.Instance;
        Assert.That(a, Is.SameAs(b));
    }

    [Test]
    public void Changed_descriptor_has_correct_js_event()
    {
        var descriptor = NativeDatePickerEvents.Instance.Changed;
        Assert.That(descriptor.JsEvent, Is.EqualTo("change"));
    }

    [Test]
    public void Changed_descriptor_provides_args_instance()
    {
        var descriptor = NativeDatePickerEvents.Instance.Changed;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<NativeDatePickerChangeArgs>());
    }
}
```

- [ ] **Step 2: Write reactive wiring + condition tests**

```csharp
namespace Alis.Reactive.Native.UnitTests.Components.NativeDatePicker;

public class WhenReactingToNativeDatePickerEvents : NativeTestBase
{
    [Test]
    public Task Changed_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeDatePickerChangeArgs>("date-changed",
            (args, p) =>
                p.Component<Native.Components.NativeDatePicker>(m => m.AdmissionDate)
                    .SetValue("2026-01-01"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Changed_event_with_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NativeDatePickerChangeArgs>("date-guarded",
            (args, p) =>
                p.When(args, x => x.Value).NotNull()
                    .Then(then => then.Element("status").SetText("date selected"))
                    .Else(else_ => else_.Element("status").SetText("no date")));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
```

- [ ] **Step 3: Run tests, accept snapshots, verify pass**

Run: `dotnet test tests/Alis.Reactive.Native.UnitTests --filter NativeDatePicker`
Expected: All NativeDatePicker tests PASS

- [ ] **Step 4: Run full Native unit test suite**

Run: `dotnet test tests/Alis.Reactive.Native.UnitTests`
Expected: All tests PASS (existing + new)

- [ ] **Step 5: Commit**

```bash
git add tests/Alis.Reactive.Native.UnitTests/Components/NativeDatePicker/
git add tests/Alis.Reactive.Native.UnitTests/NativeTestBase.cs
git commit -m "test: NativeDatePicker C# unit tests — mutations, events, reactive wiring with conditions"
```

---

## Chunk 3: Sandbox View + Controller

### Task 5: Model + Controller + View

**Files:**
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/NativeDatePickerModel.cs`
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/NativeDatePickerController.cs`
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/NativeDatePicker/Index.cshtml`

- [ ] **Step 1: Create model (senior living domain)**

```csharp
// NativeDatePickerModel.cs
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NativeDatePickerModel
    {
        public string? AdmissionDate { get; set; }
        public string? BirthDate { get; set; }
    }
}
```

- [ ] **Step 2: Create controller (self-contained, no shared actions)**

```csharp
// NativeDatePickerController.cs
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class NativeDatePickerController : Controller
    {
        public IActionResult Index()
        {
            return View(new NativeDatePickerModel());
        }
    }
}
```

- [ ] **Step 3: Create view exercising full DSL chain**

The view MUST exercise: property write, property read, event, event-args condition (typed), component-read condition (typed), reactive wiring. Every input MUST use `Html.Field()`.

```razor
@model NativeDatePickerModel
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.Native.Components
@{
    ViewData["Title"] = "NativeDatePicker";
    var plan = Html.ReactivePlan<NativeDatePickerModel>();

    // Property write: DomReady sets initial admission date
    Html.On(plan, t => t.DomReady(p =>
    {
        p.Component<NativeDatePicker>(m => m.AdmissionDate).SetValue("2026-03-15");
    }));

    // Property read: DomReady reads admission date value into echo
    Html.On(plan, t => t.DomReady(p =>
    {
        var comp = p.Component<NativeDatePicker>(m => m.AdmissionDate);
        p.Element("value-echo").SetText(comp.Value());
    }));
}

<div class="space-y-8">
    <div>
        <h1 class="text-2xl font-bold tracking-tight">NativeDatePicker — Full API</h1>
        <p class="mt-2 text-text-secondary">
            Exercises NativeDatePicker API: property writes (SetValue), property reads (Value),
            events (Changed), typed conditions (When), and reactive wiring.
        </p>
    </div>

    <!-- Section 1: Property Write -->
    <section class="rounded-lg border border-border bg-white p-6 shadow-sm">
        <h2 class="text-base font-semibold mb-4">1. Property Write (SetValue)</h2>
        <p class="text-sm text-text-secondary mb-4">
            On DomReady, sets admission date to 2026-03-15.
        </p>
        @{ Html.Field("Admission Date", false, m => m.AdmissionDate, expr =>
            Html.NativeDatePickerFor(plan, expr)
                .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
        ); }
    </section>

    <!-- Section 2: Property Read -->
    <section class="rounded-lg border border-border bg-white p-6 shadow-sm">
        <h2 class="text-base font-semibold mb-4">2. Property Read (Value as Source)</h2>
        <p class="text-sm text-text-secondary mb-4">
            On DomReady, reads the admission date and echoes it.
        </p>
        <div class="font-mono text-sm">
            <p>Value echo: <span id="value-echo" class="text-text-muted">&mdash;</span></p>
        </div>
    </section>

    <!-- Section 3: Event + Event-Args Condition (typed access) -->
    <section class="rounded-lg border border-border bg-white p-6 shadow-sm">
        <h2 class="text-base font-semibold mb-4">3. Changed Event with Typed Condition</h2>
        <p class="text-sm text-text-secondary mb-4">
            The birth date wires a Changed event. When a date is selected
            (<code>When(args, x => x.Value).NotNull()</code>), a status message shows.
        </p>
        @{ Html.Field("Birth Date", false, m => m.BirthDate, expr =>
            Html.NativeDatePickerFor(plan, expr)
                .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
                .Reactive(plan, evt => evt.Changed, (args, p) =>
                {
                    p.When(args, x => x.Value).NotNull()
                        .Then(t =>
                        {
                            t.Element("date-status").SetText("date selected");
                            t.Element("date-status").AddClass("text-green-600");
                            t.Element("date-status").RemoveClass("text-text-muted");
                        })
                        .Else(e =>
                        {
                            t.Element("date-status").SetText("no date");
                            t.Element("date-status").RemoveClass("text-green-600");
                        });
                })
        ); }
        <div class="font-mono text-sm mt-2">
            <p>Status: <span id="date-status" class="text-text-muted">&mdash;</span></p>
        </div>
    </section>

    <!-- Section 4: Component-Read Condition (typed access) -->
    <section class="rounded-lg border border-border bg-white p-6 shadow-sm">
        <h2 class="text-base font-semibold mb-4">4. Component Value Condition</h2>
        <p class="text-sm text-text-secondary mb-4">
            Button reads the admission date via <code>When(comp.Value()).IsEmpty()</code>
            and shows/hides a warning.
        </p>
        @(Html.NativeButton("check-date-btn", "Check Admission Date")
            .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white hover:bg-accent/90 transition-colors")
            .Reactive(plan, evt => evt.Click, (args, p) =>
            {
                var comp = p.Component<NativeDatePicker>(m => m.AdmissionDate);
                p.When(comp.Value()).IsEmpty()
                    .Then(t => t.Element("admission-warning").SetText("admission date is required"))
                    .Else(e => e.Element("admission-warning").SetText("admission date set"));
            }))
        <div class="font-mono text-sm mt-2">
            <p>Warning: <span id="admission-warning" class="text-text-muted">&mdash;</span></p>
        </div>
    </section>

    <!-- Plan JSON -->
    <section class="rounded-lg border border-border bg-white p-6 shadow-sm">
        <h2 class="text-base font-semibold mb-4">Plan JSON</h2>
        <pre class="rounded-md bg-slate-900 text-emerald-400 p-4 font-mono text-xs overflow-x-auto max-h-96"><code id="plan-json">@Html.Raw(plan.RenderFormatted())</code></pre>
    </section>
</div>

@Html.RenderPlan(plan)
```

Note: The view has a bug in Section 3 — the Else block references `t` instead of `e`. Fix during implementation.

- [ ] **Step 4: Verify build**

Run: `dotnet build`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/Areas/Sandbox/Models/NativeDatePickerModel.cs
git add Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/NativeDatePickerController.cs
git add Alis.Reactive.SandboxApp/Areas/Sandbox/Views/NativeDatePicker/Index.cshtml
git commit -m "feat: NativeDatePicker sandbox page — senior living domain, full DSL chain"
```

---

## Chunk 4: BDD Playwright Test

### Task 6: BDD Playwright Test

**Files:**
- Create: `tests/Alis.Reactive.PlaywrightTests/Components/Native/WhenUsingNativeDatePicker.cs`

- [ ] **Step 1: Write BDD test class**

Each test maps to a behavior the vertical slice exposes. Tests prove the full DSL chain works end-to-end in a real browser.

```csharp
namespace Alis.Reactive.PlaywrightTests.Components.Native;

[TestFixture]
public class WhenUsingNativeDatePicker : PlaywrightTestBase
{
    private const string Path = "/Sandbox/NativeDatePicker";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeDatePickerModel__";

    [Test]
    public async Task Page_loads_without_errors()
    {
        await NavigateTo(Path);
        await Expect(Page).ToHaveTitleAsync("NativeDatePicker");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task DomReady_sets_initial_date_value()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted");

        var input = Page.Locator($"#{S}AdmissionDate");
        await Expect(input).ToHaveValueAsync("2026-03-15");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Value_echoed_from_component_read()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted");

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToContainTextAsync("2026-03-15");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Changed_event_with_condition_shows_status()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted");

        var input = Page.Locator($"#{S}BirthDate");
        await input.FillAsync("1945-06-15");

        var status = Page.Locator("#date-status");
        await Expect(status).ToContainTextAsync("date selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Component_value_condition_shows_warning_when_empty()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted");

        // Clear the admission date that was set by DomReady
        var input = Page.Locator($"#{S}AdmissionDate");
        await input.ClearAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check Admission Date" }).ClickAsync();

        var warning = Page.Locator("#admission-warning");
        await Expect(warning).ToContainTextAsync("admission date is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Component_value_condition_shows_set_when_filled()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted");

        // DomReady already set the date — just click check
        await Page.GetByRole(AriaRole.Button, new() { Name = "Check Admission Date" }).ClickAsync();

        var warning = Page.Locator("#admission-warning");
        await Expect(warning).ToContainTextAsync("admission date set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }
}
```

- [ ] **Step 2: Run Playwright test**

Run: `dotnet test tests/Alis.Reactive.PlaywrightTests --filter NativeDatePicker`
Expected: All 6 tests PASS

- [ ] **Step 3: Run full test suite**

Run all 6 test suites:
```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```
Expected: ALL tests PASS (existing + new)

- [ ] **Step 4: Commit**

```bash
git add tests/Alis.Reactive.PlaywrightTests/Components/Native/WhenUsingNativeDatePicker.cs
git commit -m "test: NativeDatePicker BDD Playwright test — property write, read, event, conditions"
```

---

## Chunk 5: Verification

### Task 7: Verify Vertical Slice Compliance

- [ ] **Step 1: Verify all 7 component files exist**

```bash
ls Alis.Reactive.Native/Components/NativeDatePicker/
```

Expected files:
```
NativeDatePicker.cs
NativeDatePickerBuilder.cs
NativeDatePickerHtmlExtensions.cs
NativeDatePickerExtensions.cs
NativeDatePickerEvents.cs
NativeDatePickerReactiveExtensions.cs
Events/NativeDatePickerOnChanged.cs
```

- [ ] **Step 2: Verify builder constructor is internal**

```bash
grep "internal NativeDatePickerBuilder" Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerBuilder.cs
```
Expected: match found

- [ ] **Step 3: Verify single Reactive overload**

```bash
grep -c "public static NativeDatePickerBuilder" Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerReactiveExtensions.cs
```
Expected: 1

- [ ] **Step 4: Verify events singleton has private constructor**

```bash
grep "private NativeDatePickerEvents()" Alis.Reactive.Native/Components/NativeDatePicker/NativeDatePickerEvents.cs
```
Expected: match found

- [ ] **Step 5: Verify view uses Html.Field() for all inputs**

```bash
grep -c "Html.Field(" Alis.Reactive.SandboxApp/Areas/Sandbox/Views/NativeDatePicker/Index.cshtml
```
Expected: 2 (AdmissionDate + BirthDate)

- [ ] **Step 6: Verify no raw HTML inputs in view**

```bash
grep "<input " Alis.Reactive.SandboxApp/Areas/Sandbox/Views/NativeDatePicker/Index.cshtml
```
Expected: no matches

- [ ] **Step 7: Verify controller has no shared endpoints**

Controller should only have `Index()` and its own actions. No `Echo()`, no `ValidateClient()`.

- [ ] **Step 8: Final commit**

```bash
git commit --allow-empty -m "verify: NativeDatePicker vertical slice compliance — all checks pass"
```
