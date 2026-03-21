---
name: onboard-fusion-component
description: Use when onboarding a new Syncfusion EJ2 component, adding events, methods, or props to an existing component, or when unsure if a SF API is supported. Also use when needing to experiment with SF APIs to verify behavior before onboarding.
---

# Onboard Fusion Component

## The Process

```
1. Read SF docs → understand the API
2. Experiment in browser → verify it actually works
3. Check existing files → never duplicate, never overwrite
4. Add only what's new → follow exact patterns from reference files
5. Test → Playwright must pass
```

## Step 1: Read SF EJ2 Docs

Always check the SF API docs first:
- **API reference:** `https://ej2.syncfusion.com/javascript/documentation/api/{component}/`
- **Events:** `https://ej2.syncfusion.com/javascript/documentation/api/{component}/#events`
- **Methods:** `https://ej2.syncfusion.com/javascript/documentation/api/{component}/#methods`
- **How-to guides:** `https://ej2.syncfusion.com/javascript/documentation/{component}/how-to/`
- **Demos:** `https://ej2.syncfusion.com/demos/{component}/`

Identify: property names (camelCase), method signatures, event names, event args properties.

## Step 2: Experiment in Browser

**NEVER onboard an API without verifying it works.** SF docs can be misleading — methods may exist but have no visible effect.

Run a temporary experiment on the sandbox page using JS in the browser console:

```javascript
// Find the ej2 instance
const el = document.getElementById('{componentId}');
const ej2 = el.ej2_instances[0];

// Test property set
ej2.enabled = false;  // Does it disable?

// Test method call
ej2.showPopup();      // Does popup appear?
ej2.showSpinner();    // Does spinner show? (may not on all components)

// Test if method exists
typeof ej2.someMethod; // "function" or "undefined"

// Test event args methods (inside event handler)
// Create a temp AutoComplete and wire the event to test:
const ac = new ej.dropdowns.AutoComplete({
    placeholder: 'test',
    fields: { value: 'value', text: 'text' },
    filtering: function(e) {
        e.preventDefaultAction = true;
        setTimeout(() => {
            e.updateData([{value:'a', text:'A'}]); // Does popup show?
        }, 200);
    }
});
ac.appendTo('#temp-input');
```

**Document results** as comments in the vertical slice extensions file:
```csharp
// NOTE: showSpinner/hideSpinner have no visible effect on SF AutoComplete.
// refresh() causes focus loss mid-typing — not usable during filtering.
// Both verified manually. Omitted intentionally.
```

**Remove the experiment** before onboarding. No temp code in the codebase.

## Step 3: Check Existing Files

**Read before writing. Never duplicate.**

```
Alis.Reactive.Fusion/Components/FusionXxx/
├── FusionXxx.cs                    ← EXISTS? Don't touch
├── FusionXxxExtensions.cs          ← EXISTS? ADD methods, don't recreate
├── FusionXxxHtmlExtensions.cs      ← EXISTS? Don't touch
├── FusionXxxEvents.cs              ← EXISTS? ADD event descriptor, don't recreate
├── FusionXxxReactiveExtensions.cs  ← EXISTS? Don't touch (generic TArgs handles any event)
└── Events/
    └── FusionXxxOnChanged.cs       ← EXISTS? Don't touch
    └── FusionXxxOnFiltering.cs     ← NEW? Create following pattern
```

**If the Events singleton exists**, just add a property. **If the extensions file exists**, just add methods. **If the reactive extensions exist**, they already handle any event type via generic `TArgs` — don't touch.

## Step 4: Add Only What's New

### Capability Check — What the Framework Supports

| JS API Pattern | Supported? | Mechanism |
|---|---|---|
| `ej2.prop = value` | YES | `self.Emit(new SetPropMutation("prop"), value: val)` |
| `ej2.method()` | YES | `self.Emit(new CallMutation("method"))` |
| `ej2.method("arg")` | YES | `self.Emit(new CallMutation("method", args: new[] { new LiteralArg("arg") }))` |
| `ej2.method(data)` from response | YES | `CallMutation` + `SourceArg(new EventSource(path))` |
| `ej2.prop` (read for conditions) | YES | `new TypedComponentSource<T>(id, vendor, readExpr)` |
| `e.prop = true` on event args | YES | `new MutateEventCommand(new SetPropMutation("prop"), value: true)` |
| `e.method(data)` on event args | YES | `new MutateEventCommand(new CallMutation("method", args: ...))` |
| `evt.text` send to server | YES | `g.FromEvent(args, x => x.Text, "param")` |
| `ej2.method()` → use return value | NO (v2) | Return value capture not supported |
| `ej2.method()[0].prop` chained | NO (v2) | Variable concept not in plan |

**If request maps to NO → explain why and stop. Do not invent workarounds.**

### Adding a Method/Prop to Existing Component

One-line addition to `FusionXxxExtensions.cs`. Reference: `FusionAutoCompleteExtensions.cs`

```csharp
// Void method → CallMutation
public static ComponentRef<FusionXxx, TModel> ShowPopup<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => self.Emit(new CallMutation("showPopup"));

// Property set → SetPropMutation
public static ComponentRef<FusionXxx, TModel> Disable<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => self.Emit(new SetPropMutation("enabled"), value: false);

// Property set from response → SetPropMutation + EventSource
public static ComponentRef<FusionXxx, TModel> SetDataSource<TModel, TResponse>(
    this ComponentRef<FusionXxx, TModel> self,
    ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
    where TModel : class where TResponse : class
{
    var sourcePath = ExpressionPathHelper.ToResponsePath(path);
    return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
}

// Read value → TypedComponentSource
public static TypedComponentSource<string> Value<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
```

### Adding an Event (Simple — No Methods on Args)

Reference: `Events/FusionAutoCompleteOnChanged.cs` + `FusionAutoCompleteEvents.cs`

**File 1:** Create `Events/FusionXxxOnYyyEvent.cs`:
```csharp
public class FusionXxxYyyArgs
{
    public string? Value { get; set; }
    public bool IsInteracted { get; set; }
    public FusionXxxYyyArgs() { }
}
```

**File 2:** Add to `FusionXxxEvents.cs`:
```csharp
public TypedEventDescriptor<FusionXxxYyyArgs> Yyy =>
    new TypedEventDescriptor<FusionXxxYyyArgs>("yyy", new FusionXxxYyyArgs());
```

The string `"yyy"` is the SF JS event name from docs. The reactive extensions already handle any `TArgs` — no changes needed there.

### Adding an Event (With Methods on Args)

Reference: `Events/FusionAutoCompleteOnFiltering.cs`

Args class AND typed extensions go in ONE file. Extensions go on the args class itself — NOT on a separate builder:

```csharp
public class FusionXxxFilteringArgs
{
    public string Text { get; set; } = "";
    public FusionXxxFilteringArgs() { }
}

public static class FusionXxxFilteringArgsExtensions
{
    // set-prop on event args
    public static void PreventDefault<TModel>(
        this FusionXxxFilteringArgs args,
        PipelineBuilder<TModel> pipeline) where TModel : class
    {
        pipeline.AddCommand(new MutateEventCommand(
            new SetPropMutation("preventDefaultAction"), value: true));
    }

    // call on event args with response data
    public static void UpdateData<TModel, TResponse>(
        this FusionXxxFilteringArgs args,
        PipelineBuilder<TModel> pipeline,
        ResponseBody<TResponse> source,
        Expression<Func<TResponse, object?>> path)
        where TModel : class where TResponse : class
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

**Why `pipeline` parameter?** `args` is a phantom shared across the entire reactive lambda. Unlike `ComponentRef` (created per-context via `p.Component`/`s.Component`), `args` has no pipeline binding. The builder must be passed explicitly.

### View Usage — FromEvent + UpdateData Pattern

```csharp
.Reactive(plan, evt => evt.Filtering, (args, p) =>
{
    args.PreventDefault(p);
    p.Get("/url")
     .Gather(g => g.FromEvent(args, x => x.Text, "QueryParam"))
     .Response(r => r.OnSuccess<TResponse>((json, s) =>
     {
         args.UpdateData(s, json, j => j.Items);
         s.Element("status").SetText("loaded");
     }));
})
```

**`FromEvent` vs `Include`:**
- `FromEvent(args, x => x.Text, "param")` — value from event args (typed text during filtering)
- `Include<Component, Model>(m => m.Prop)` — value from component's current state (cascade)

### New Component (Full Vertical Slice)

Read ALL files in `FusionAutoComplete/` as reference. The 7 files:

1. `FusionXxx.cs` — phantom type, `IInputComponent`, declares `ReadExpr`
2. `FusionXxxExtensions.cs` — mutations (SetValue, Enable, ShowPopup, Value, etc.)
3. `FusionXxxHtmlExtensions.cs` — `Html.InputField().Xxx()` factory + `Fields<TItem>()`
4. `FusionXxxEvents.cs` — singleton, one `TypedEventDescriptor` per event
5. `FusionXxxReactiveExtensions.cs` — `.Reactive()` on builder, generic `TArgs`
6. `Events/FusionXxxOnChanged.cs` — args class (simple or with extensions)
7. Gather extension — `Include<FusionXxx, TModel>(m => m.Prop)`

Copy each file, rename class/type names. Do not invent structure.

## What Does NOT Change

When onboarding any component, event, method, or prop — **NONE of these change:**
- TS runtime (trigger.ts, commands.ts, element.ts, gather.ts)
- JSON schema (reactive-plan.schema.json)
- TS types (types/*.ts)
- Core descriptors (Alis.Reactive project)

**If you find yourself modifying any of these, STOP — you're doing it wrong.**

## Mistakes to Avoid

| Mistake | Why It's Wrong | Correct |
|---|---|---|
| Using `Static("p", args.Text)` for event args | Resolves at C# compile time → always `""` | `FromEvent(args, x => x.Text, "p")` |
| Using `SetDataSource` for filtering events | SF's filtering lifecycle closes before async HTTP completes | `args.UpdateData(s, json, path)` |
| Calling `DataBind` after `updateData` | `updateData` handles everything internally | Only use `DataBind` after `SetDataSource` in cascade/Changed patterns |
| Forgetting `PreventDefault` on filtering | SF flashes "No records found" during async HTTP | Call `args.PreventDefault(p)` first |
| Putting args extensions on a builder class | Creates indirection, loses compile-time type safety | Extensions go directly on the args class |
| Creating new `EventArgsRef` type | Dead pattern — was tried and removed | Args type IS the API surface |
| Modifying TS runtime for new component | Breaks architecture — plan carries all behavior | Zero runtime changes, always |
| Onboarding without browser experiment | SF docs can be misleading — APIs may not work as described | Always verify with JS in browser first |
| Recreating existing files | Duplicates code, causes conflicts | Check existing files first, add to them |
| Using `ej2.showSpinner()`/`hideSpinner()` on dropdown components | SF spinner is a standalone utility from ej2-popups, not built into dropdown inputs | Use DOM elements for loading indicators |
| Using `ej2.refresh()` during typing | Causes focus loss | Omit — document why in comments |
| Forgetting `AllowFiltering(true)` on MultiSelect/DropDownList | SF AutoComplete has filtering built-in, but MultiSelect/DDL do not — event never fires | Add `.AllowFiltering(true)` in the view builder chain |
| Typing into `#{ComponentId}` for MultiSelect filtering tests | SF MultiSelect filter input is a sibling `input.e-dropdownbase`, not the component input | Use `Locator("xpath=preceding-sibling::input[contains(@class,'e-dropdownbase')]")` |
| Missing sandbox model/controller/view for new event | Playwright tests need a working demo page with HTTP endpoints and status elements | Create the full sandbox vertical slice: model property + item/response classes, controller endpoint, view section |

## Step 5: Tests

Every onboarded component/event needs tests at the layers it touches.

### Playwright Tests (Always Required)

Reference: `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/WhenUsingFusionAutoComplete.cs`

Test file goes in `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/WhenUsingFusionXxx.cs`.

```csharp
[TestFixture]
public class WhenUsingFusionXxx : PlaywrightTestBase
{
    private const string Path = "/Sandbox/XxxPage";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_XxxModel";
    private const string ComponentId = Scope + "__PropertyName";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }
}
```

**For filtering events**, use `PressSequentiallyAsync` (not `FillAsync`) to trigger SF events.

**AutoComplete filtering** — type directly into the component input:
```csharp
private async Task TypeInComponent(string text)
{
    var input = Page.Locator($"#{ComponentId}");
    await Expect(input).ToBeVisibleAsync();
    await input.ClickAsync();
    await input.PressSequentiallyAsync(text, new() { Delay = 50 });
}
```

**MultiSelect filtering** — SF MultiSelect creates a SEPARATE filter input (sibling `input.e-dropdownbase`).
You CANNOT type into the component input itself — you must target the filter input:

```
SF MultiSelect DOM with AllowFiltering:
  .e-multi-select-wrapper (grandparent)
    └── span.e-searcher (parent)
        ├── input.e-dropdownbase (filter input — TYPE HERE)
        └── input#ComponentId (component input — NOT here)
```

```csharp
private async Task TypeInComponent(string text)
{
    // Target the filter input sibling, not the component input
    var filterInput = Page.Locator($"#{ComponentId}")
        .Locator("xpath=preceding-sibling::input[contains(@class,'e-dropdownbase')]");
    await Expect(filterInput).ToBeVisibleAsync(new() { Timeout = 5000 });
    await filterInput.ClickAsync();
    await filterInput.PressSequentiallyAsync(text, new() { Delay = 50 });
}
```

**Test the user-visible behavior**, not framework internals:
- Filtering: type → HTTP fires → popup shows results → status element updates
- Changed: select item → value displayed in echo element
- Cascade: parent change → child populated from server
- Conditions: value matches → then branch, doesn't → else branch

**For `updateData` popup verification**, check popup items (not ej2.dataSource):
```csharp
var popupItems = Page.Locator(".e-ddl.e-popup .e-list-item");
await Expect(popupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
```

### C# Unit Tests (If Adding New Descriptor Patterns)

Only needed when adding a pattern that doesn't exist yet. If you're just adding methods/events using existing mechanisms (SetPropMutation, CallMutation, MutateEventCommand, EventGather), the existing tests already cover the serialization.

### Sandbox Demo (Always Required — Full Vertical Slice)

A sandbox demo is a vertical slice across 3 files. All 3 must be updated.

**File 1: Model** (`Areas/Sandbox/Models/XxxModel.cs`)

Add a model property for the new capability. For server-filtered events, also add the item class
and response class:

```csharp
// Model property
public string[]? Supplies { get; set; }

// Item class for DataSource
public class SupplyItem
{
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
    public string Category { get; set; } = "";
}

// Response class for HTTP endpoint
public class SupplySearchResponse
{
    public List<SupplyItem> Supplies { get; set; } = new();
    public int Count { get; set; }
}
```

**File 2: Controller** (`Areas/Sandbox/Controllers/XxxController.cs`)

For server-filtered events, add an HTTP GET endpoint with server-side text filtering:

```csharp
[HttpGet]
public IActionResult Supplies([FromQuery] string? Supplies)
{
    var all = new List<SupplyItem> { /* ... */ };

    // Gather sends "null" string when nothing selected — treat same as empty
    var search = Supplies == "null" ? null : Supplies;
    var filtered = string.IsNullOrEmpty(search)
        ? all
        : all.Where(s => s.Text.Contains(search, StringComparison.OrdinalIgnoreCase)
                      || s.Value.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

    return Ok(new SupplySearchResponse { Supplies = filtered, Count = filtered.Count });
}
```

**File 3: View** (`Areas/Sandbox/Views/Xxx/Index.cshtml`)

Add a numbered section. Include `<span id="xxx">` elements for Playwright assertions:

```html
<section class="rounded-lg border border-border bg-white p-6 shadow-sm">
    <h2 class="text-base font-semibold mb-4">N. Filtering Event (Server-Filtered HTTP)</h2>
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        @{ Html.InputField(plan, m => m.Supplies, o => o.Label("Supplies (Server-Filtered)"))
            .MultiSelect(b => b
                .Fields<SupplyItem>(t => t.Text, v => v.Value)
                .AllowFiltering(true)       <!-- REQUIRED for MultiSelect filtering -->
                .Reactive(plan, evt => evt.Filtering, (args, p) =>
                {
                    args.PreventDefault(p);
                    p.Get("/Sandbox/Xxx/Supplies")
                     .Gather(g => g.FromEvent(args, x => x.Text, "Supplies"))
                     .Response(r => r.OnSuccess<SupplySearchResponse>((json, s) =>
                     {
                         args.UpdateData(s, json, j => j.Supplies);
                         s.Element("filter-status").SetText("results loaded");
                     }));
                })); }
    </div>
    <div class="font-mono text-sm mt-2">
        <p>Filter status: <span id="filter-status" class="text-text-muted">&mdash;</span></p>
    </div>
</section>
```

**AllowFiltering note:** SF AutoComplete has filtering built-in. SF MultiSelect and DropDownList
require `.AllowFiltering(true)` explicitly — without it, the filtering event never fires.

### Run All Tests Before Done

```bash
npm test                                                    # TS unit tests
dotnet test tests/Alis.Reactive.UnitTests                   # Core C# tests
dotnet test tests/Alis.Reactive.Fusion.UnitTests             # Fusion tests
dotnet test tests/Alis.Reactive.PlaywrightTests              # Browser tests
```
