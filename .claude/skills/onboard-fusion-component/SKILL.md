---
name: onboard-fusion-component
description: Guides onboarding of Syncfusion EJ2 components — adding new vertical slices, events, methods, or props to existing components, and experimenting with SF APIs to verify behavior before integration.
disable-model-invocation: true
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
├── FusionXxxEvents.cs              ← EXISTS? ADD event definitions, don't recreate
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
| `ej2.prop = value` | YES | component property action |
| `ej2.method()` | YES | component method action |
| `ej2.method("arg")` | YES | component method action with literal args |
| `ej2.method(data)` from response | YES | component method action with response-backed value path |
| `ej2.prop` (read for conditions) | YES | `new ComponentValueExpression<T>(id, vendor, valueMemberPath)` |
| `e.prop = true` on event args | YES | event-object property action |
| `e.method(data)` on event args | YES | event-object method action |
| `evt.text` send to server | YES | `g.FromEvent(args, x => x.Text, "param")` |
| `ej2.method()` → use return value | NO (v2) | Return value capture not supported |
| `ej2.method()[0].prop` chained | NO (v2) | Variable concept not in plan |

**If request maps to NO → explain why and stop. Do not invent workarounds.**

### Adding a Method/Prop to Existing Component

One-line addition to `FusionXxxExtensions.cs`. Reference: `FusionAutoCompleteExtensions.cs`

```csharp
// Void method → component method action
public static ComponentRef<FusionXxx, TModel> ShowPopup<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => self.Call("showPopup");

// Property set → component property action
public static ComponentRef<FusionXxx, TModel> Disable<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => self.Set("enabled", false, coerceAs: "boolean");

// Property set from response → component property action + response path
public static ComponentRef<FusionXxx, TModel> SetDataSource<TModel, TResponse>(
    this ComponentRef<FusionXxx, TModel> self,
    ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
    where TModel : class where TResponse : class
{
    var sourcePath = ExpressionPathHelper.ToResponsePath(path);
    return self.SetFromPath("dataSource", sourcePath);
}

// Property set from generic event payload → component property action + event path
public static ComponentRef<FusionXxx, TModel> SetDataSource<TModel, TSource>(
    this ComponentRef<FusionXxx, TModel> self,
    TSource source, Expression<Func<TSource, object?>> path)
    where TModel : class
{
    var sourcePath = ExpressionPathHelper.ToEventPath(path);
    return self.SetFromPath("dataSource", sourcePath);
}

// Read value → ComponentValueExpression
public static ComponentValueExpression<string> Value<TModel>(
    this ComponentRef<FusionXxx, TModel> self) where TModel : class
    => new ComponentValueExpression<string>(self.TargetId, Component.Vendor, Component.ValueMemberPath);
```

### Adding an Event (Simple — No Methods on Args)

Reference: `Events/FusionAutoCompleteOnChanged.cs` + `FusionAutoCompleteEvents.cs`

**File 1:** Create `Events/FusionXxxOnYyy.cs`:
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
public ReactiveEvent<FusionXxxYyyArgs> Yyy =>
    new ReactiveEvent<FusionXxxYyyArgs>("yyy", new FusionXxxYyyArgs());
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
    // property action on event args
    public static void PreventDefault(
        this FusionXxxFilteringArgs args,
        ICommandEmitter pipeline)
    {
        pipeline.SetEventProperty("preventDefaultAction", true, coerceAs: "boolean");
    }

    // method action on event args with response data
    public static void UpdateData<TResponse>(
        this FusionXxxFilteringArgs args,
        ICommandEmitter pipeline,
        ResponseBody<TResponse> source,
        Expression<Func<TResponse, object?>> path)
        where TResponse : class
    {
        var sourcePath = ExpressionPathHelper.ToResponsePath(path);
        pipeline.CallEventMemberFromPath("updateData", sourcePath);
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

Read ALL files in `FusionAutoComplete/` as reference. The pattern is 5 base files + 1 event file per event:

1. `FusionXxx.cs` — extends `FusionComponent` (abstract base that provides `Vendor => "fusion"`), implements `IInputComponent`, declares `ValueMemberPath`
2. `FusionXxxExtensions.cs` — component action helpers (SetValue, Enable, ShowPopup, Value, etc.)
3. `FusionXxxHtmlExtensions.cs` — `Html.InputField().Xxx()` factory + `Fields<TItem>()` + `ComponentRegistration`
4. `FusionXxxEvents.cs` — singleton, one `ReactiveEvent<TArgs>` property per event
5. `FusionXxxReactiveExtensions.cs` — `.Reactive()` on builder, generic `TArgs`

Plus under `Events/`: one file per event (e.g., `FusionXxxOnChanged.cs`, `FusionXxxOnFiltering.cs`). The number of event files varies by component.

Note: Gather extensions (`Include<FusionXxx, TModel>`) live in the core project (`GatherExtensions.cs`), not in the component directory.

Copy each file, rename class/type names. Do not invent structure.

### ComponentRegistration (Critical — Inside HtmlExtensions)

The `FusionXxxHtmlExtensions.cs` factory method MUST register the component with the plan. Without this, authoring cannot create bindings or gathers for the component. Template from `FusionAutoCompleteHtmlExtensions.cs`:

```csharp
public static void FusionXxx<TModel, TProp>(
    this InputBoundField<TModel, TProp> setup,
    Action<XxxBuilder> build)
    where TModel : class
{
    setup.Plan.RegisterComponent(setup.BindingPath, new ComponentRegistration(
        setup.ElementId, Component.Vendor, setup.BindingPath, Component.ValueMemberPath, "xxx",
        CoercionTypes.InferFromType(typeof(TProp))));

    var builder = setup.Helper.EJS().XxxFor(setup.Expression)
        .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
    build(builder);
    setup.Render(builder.Render());
}
```

Parameters: `elementId`, `vendor` (from `Component.Vendor`), `bindingPath`, `valueMemberPath` (from `Component.ValueMemberPath`), `componentType` string (SF component name, lowercase), `CoercionTypes.InferFromType(typeof(TProp))`.

### Event Naming Convention

Three distinct names per event follow different conventions:
- **Property name** on the events class: past tense (e.g. `Changed`, `Filtering`)
- **SF event string** in `ReactiveEvent<TArgs>`: present tense (e.g. `"change"`, `"filtering"`)
- **Args class name**: uses the SF event string style, not the property name (e.g. `FusionAutoCompleteChangeArgs`, NOT `FusionAutoCompleteChangedArgs`)

## What Does NOT Change

When onboarding any component, event, method, or prop — **NONE of these change:**
- TS runtime execution and resolution modules
- JSON schema (reactive-plan.schema.json)
- TS types (types/*.ts)
- Core plan authoring model (Alis.Reactive project)

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

See `references/playwright-patterns.md` for DOM structure details and test templates.

### C# Unit Tests (If Adding New V2 Authoring Patterns)

Only needed when adding a pattern that does not already exist in the V2 authoring model. If you are adding methods or events using existing member targeting, action building, or request input patterns, the existing tests already cover serialization.

### Sandbox Demo (Always Required — Full Vertical Slice)

See `references/sandbox-templates.md` for complete model, controller, and view templates.

### Run All Tests Before Done

```bash
npm test                                                    # TS unit tests
dotnet test tests/Alis.Reactive.UnitTests                   # Core C# tests
dotnet test tests/Alis.Reactive.Fusion.UnitTests             # Fusion tests
dotnet test tests/Alis.Reactive.PlaywrightTests              # Browser tests
```
