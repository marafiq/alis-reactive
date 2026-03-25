---
title: Creating a Sandbox Demo Page
description: Step-by-step guide for creating a sandbox vertical slice — model, controller, view, and Playwright tests for a component demo.
sidebar:
  order: 11
---

Every onboarded component needs a sandbox demo page. The demo exercises every capability (property writes, reads, events, conditions, gather) with visible echo elements that Playwright can assert.

**You must NOT modify any core framework files.** The sandbox is a consumer of the framework — it uses the C# DSL exactly as a real application would. If something doesn't work from the sandbox, the framework has a bug.

## The 4-file sandbox vertical slice

```
Areas/Sandbox/Models/Components/{Vendor}/{ComponentName}/
  └── {ComponentName}Model.cs          ← 1. Model + item classes + response DTOs

Areas/Sandbox/Controllers/Components/{Vendor}/
  └── {ComponentName}Controller.cs     ← 2. Index GET + HTTP endpoints

Areas/Sandbox/Views/Components/{Vendor}/{ComponentName}/
  └── Index.cshtml                     ← 3. Numbered sections with echo spans

tests/Alis.Reactive.PlaywrightTests/Components/{Vendor}/
  └── When{ComponentName}{Behavior}.cs ← 4. BDD Playwright tests
```

---

## File 1: Model

One file containing the main model class, item classes (for list components), and response DTOs (for HTTP endpoints).

```csharp
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class XxxModel
    {
        // One property per component capability being demoed
        public string? Category { get; set; }
        public string? SearchTerm { get; set; }
    }

    // Item class — for list components with DataSource
    public class CategoryItem
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    // Response DTO — for HTTP endpoints
    public class CategorySearchResponse
    {
        public List<CategoryItem> Categories { get; set; } = new();
        public int Count { get; set; }
    }
}
```

**Rules:**
- All classes in ONE file (model + items + responses)
- Use senior living domain names (residents, facilities, care levels) when possible
- Property types must match the component's semantic type (`string` for text, `bool` for checkbox, `decimal` for numeric)

---

## File 2: Controller

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/{ComponentName}")]
    public class XxxController : Controller
    {
        // Index — returns the demo view with initial data
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.Categories = new List<CategoryItem>
            {
                new() { Value = "electronics", Text = "Electronics" },
                new() { Value = "clothing", Text = "Clothing" },
            };

            return View(
                "~/Areas/Sandbox/Views/Components/{Vendor}/{ComponentName}/Index.cshtml",
                new XxxModel());
        }

        // Filtering endpoint — for server-filtered components
        [HttpGet("Search")]
        public IActionResult Search([FromQuery] string? SearchTerm)
        {
            var all = new List<CategoryItem> { /* ... */ };

            // "null" string comes from gather when field is empty
            var q = SearchTerm == "null" ? null : SearchTerm;
            var filtered = string.IsNullOrEmpty(q)
                ? all
                : all.Where(i => i.Text.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(new CategorySearchResponse { Categories = filtered, Count = filtered.Count });
        }

        // Echo endpoint — for gather testing
        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
```

**Rules:**
- Route: `[Area("Sandbox")] [Route("Sandbox/Components/{ComponentName}")]`
- View path: always use full `~/Areas/Sandbox/Views/...` path
- Gather sends `"null"` string when field is empty — handle it
- Echo endpoint returns the raw dictionary for gather verification

---

## File 3: View (Index.cshtml)

### Header and plan setup

```cshtml
@model XxxModel
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.Native.Components
@using Alis.Reactive.Fusion.Components
@{
    ViewData["Title"] = "ComponentName";
    var plan = Html.ReactivePlan<XxxModel>();
    var categories = (List<CategoryItem>)ViewBag.Categories;
}
```

### Numbered sections

Each section tests one capability. Include echo `<span>` elements for every value Playwright needs to assert:

```html
<div class="space-y-8">
    <div>
        <h1 class="text-2xl font-bold tracking-tight">FusionXxx — Full API</h1>
    </div>

    <!-- Section 1: Property Write -->
    <section class="rounded-lg border border-border bg-white p-6 shadow-sm">
        <h2 class="text-base font-semibold mb-4">1. Property Write (SetValue)</h2>
        <p class="text-sm text-text-secondary mb-4">
            On DomReady, SetValue sets the component to "Books".
        </p>

        @{ Html.InputField(plan, m => m.Category, o => o.Label("Category"))
            .DropDownList(b => b
                .DataSource(categories)
                .Placeholder("Select a category")); }

        <div class="font-mono text-sm mt-4">
            <p>SetValue result: <span id="set-value-result" class="text-text-muted">&mdash;</span></p>
        </div>
    </section>

    <!-- Section 2: Property Read (Value echo) -->
    <section class="rounded-lg border border-border bg-white p-6 shadow-sm">
        <h2 class="text-base font-semibold mb-4">2. Property Read (Value)</h2>
        <div class="font-mono text-sm mt-4">
            <p>Value echo: <span id="value-echo" class="text-text-muted">&mdash;</span></p>
        </div>
    </section>

    <!-- Section 3: Method Call -->
    <!-- Section 4: Changed Event with Typed Args -->
    <!-- Section 5: Condition (When/Then/Else) -->
    <!-- Section 6: Component Read Condition -->
    <!-- Section 7: Gather (IncludeAll or Include) -->
    <!-- Section N: Filtering Event (if applicable) -->

    <!-- Plan JSON — always last -->
    <section class="rounded-lg border border-border bg-white p-6 shadow-sm">
        <h2 class="text-base font-semibold mb-4">Plan JSON</h2>
        <pre id="plan-json">@Html.Raw(plan.RenderFormatted())</pre>
    </section>
</div>

@Html.RenderPlan(plan)
```

### Section checklist

Include one section per capability the component supports:

| # | Section | What it tests | Required echo spans |
|---|---------|--------------|-------------------|
| 1 | Property Write | `SetValue`/`SetChecked` on DomReady | `set-value-result` |
| 2 | Property Read | `Value()` echoed to `SetText` | `value-echo` |
| 3 | Method Call | `ShowPopup()`, `FocusIn()`, `Toggle()` | `method-result` |
| 4 | Changed Event | `.Reactive(evt => evt.Changed)` with args | `change-value`, `change-interacted` |
| 5 | Condition | `When(args, x => x.Value).Eq(...)` | `condition-result` |
| 6 | Component Read | `When(comp.Value()).Eq(...)` from button | `component-read-result` |
| 7 | Gather | `Post(..., g => g.IncludeAll())` echo | `gather-result` |
| 8+ | Advanced | Filtering, cascade, etc. | `filter-status`, `cascade-status` |

**Every assertion point must have a `<span id="...">` element.** Playwright tests assert text content on these spans.

### Plan wiring pattern

DomReady and custom event handlers go in the `@{ }` block at the top:

```cshtml
@{
    // DomReady — property write + read
    Html.On(plan, t => t.DomReady(p =>
    {
        p.Component<FusionDropDownList>(m => m.Category).SetValue("Books");
        var comp = p.Component<FusionDropDownList>(m => m.Category);
        p.Element("value-echo").SetText(comp.Value());
        p.Element("set-value-result").SetText("SetValue applied");
    }));

    // Gather — triggered by button
    Html.On(plan, t => t.CustomEvent("do-gather", p =>
    {
        p.Post("/Sandbox/Components/Xxx/Echo", g => g.IncludeAll())
         .Response(r => r.OnSuccess<Dictionary<string, object>>((json, s) =>
         {
             s.Element("gather-result").SetText("gathered");
         }));
    }));
}
```

### Footer

Always end with `@Html.RenderPlan(plan)` — this emits the JSON plan element that the runtime discovers on boot.

---

## File 4: Playwright tests

```csharp
using Microsoft.Playwright.NUnit;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionXxx : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Xxx";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task page_loads_with_plan_json()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#plan-json")).Not.ToBeEmptyAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_ready_sets_initial_value()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#set-value-result"))
            .ToHaveTextAsync("SetValue applied", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task value_echo_reads_component_value()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#value-echo"))
            .ToHaveTextAsync("Books", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_fires_on_selection()
    {
        await NavigateAndBoot();
        // interact with component...
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("—", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
```

**Rules:**
- Class name: `When{Verb}FusionXxx` or `WhenUsingFusionXxx`
- Extend `PlaywrightTestBase`
- `NavigateAndBoot()` helper — navigate + wait for trace `"booted"`
- Every test ends with `AssertNoConsoleErrors()`
- Assert on echo span text, not framework internals
- Use `Timeout = 5000` on assertions that wait for async behavior
- For SF component interaction, use `PressSequentiallyAsync` (not `FillAsync`) to trigger real events

---

## Verify in the browser — mandatory

After creating all files, you MUST verify in the browser before considering the work done.

### 1. Build and run

```bash
npm run build:all        # Rebuild JS + CSS
dotnet build             # Rebuild C#
dotnet run --project Alis.Reactive.SandboxApp
```

### 2. Open the sandbox page

Navigate to `http://localhost:5220/Sandbox/Components/{ComponentName}` and check:

- [ ] Page loads without console errors (open DevTools → Console)
- [ ] Boot trace appears: `[alis:boot] booted`
- [ ] Section 1 (Property Write): echo span shows the set value, not "—"
- [ ] Section 2 (Property Read): echo span shows the component's current value
- [ ] Section 3 (Method Call): click the button — does the method have a visible effect?
- [ ] Section 4 (Events): interact with the component — do echo spans update?
  - **Check for `[object Object]`** — this means the event arg property is an object, not a primitive. Go back to the SF docs or browser console to find the correct property name
- [ ] Section 5 (Conditions): do Then/Else branches fire correctly?
- [ ] Plan JSON section: JSON is rendered, contains `"vendor": "fusion"` and correct mutation kinds

### 3. Check event arg shapes in console

If echo spans show `[object Object]`, the event args class has wrong property types. Debug with:

```javascript
const el = document.getElementById('{componentId}');
const ej2 = el.ej2_instances[0];
ej2.change = function(e) { console.log('change args:', JSON.stringify(e, null, 2)); };
```

Then interact with the component and read the console output. Fix the event args class to use properties that are primitives (string, number, bool), not objects.

### 4. Only then write Playwright tests

Playwright tests assert on the same echo spans you just verified manually. If the spans don't work in the browser, the tests will fail for the same reason.

---

## What you must NOT do

| Forbidden | Why | Correct approach |
|-----------|-----|-----------------|
| Modify any file in `Alis.Reactive/` | Core framework is off-limits | File a bug if something doesn't work |
| Modify any file in `Alis.Reactive.Fusion/` | Component vertical slices are separate work | Use the onboarding guide for that |
| Modify `Scripts/*.ts` | Runtime is a dumb executor | Plan carries all behavior |
| Modify `reactive-plan.schema.json` | Schema is the contract | Existing primitives cover all cases |
| Add inline `<script>` to views | Framework boots from plan JSON only | Use `Html.On()` and `.Reactive()` |
| Assert on plan JSON structure | Implementation detail | Assert on visible DOM state |

**Previous:** [Onboarding a Component](../onboarding-component/) — the 7-file vertical slice for adding a new Syncfusion component.
