---
name: reactive-dsl
description: Use when writing C# reactive plan code in .cshtml views — Html.On, triggers (DomReady, CustomEvent, ServerPush, SignalR, component .Reactive), pipeline commands (Element, Dispatch, Component), and plan rendering. The core DSL grammar for Alis.Reactive.
---

# Reactive DSL Grammar

## View

```
VIEW :=
  @model TModel
  @{ var plan = Html.ReactivePlan<TModel>(); PLAN_WIRING }
  HTML_BODY
  @Html.RenderPlan(plan)

PLAN_WIRING :=
  | Html.On(plan, t => t.TRIGGER);
  | Html.On(plan, t => { t.TRIGGER; t.TRIGGER; });
```

## Required @using

```
@using Alis.Reactive.Native.Extensions       -- Html.On, ReactivePlan, RenderPlan, InputField
@using Alis.Reactive.Native.Components       -- NativeTextBox, NativeButton, NativeCheckBox, ...
@using Alis.Reactive.Fusion.Components       -- FusionAutoComplete, FusionDropDownList, ...
@using Alis.Reactive.Builders.Requests       -- Gather Include<TComponent, TModel>(...) (vendor-agnostic)
@using Alis.Reactive.Native.AppLevel         -- NativeDrawer, NativeLoader, DrawerSize
@using Alis.Reactive.Fusion.AppLevel         -- FusionToast, FusionToastTone
@using Alis.Reactive                         -- Field.Of(...) for row and item members
@using Syncfusion.EJ2                        -- Html.EJS()
@using Syncfusion.EJ2.DropDowns              -- VisualMode (MultiSelect)
```

## Razor Output — `@{ }` vs `@()`

```
RAZOR_VOID    := @{ EXPR; }            -- InputField (writes through the view's writer), Html.On
RAZOR_CONTENT := @(EXPR)               -- IHtmlContent: NativeButton, NativeActionLink, Html.FusionGrid, ...
RAZOR_HELPER  := @Html.RenderPlan(plan) -- IHtmlContent helper

-- InputField renders inline via the writer → @{ }; .Reactive hangs off the render
@{ Html.InputField(plan, m => m.Prop, o => o.OPTIONS).COMPONENT(b => b.CHAIN).Reactive(evt => evt.EVENT, (args, p) => { PIPELINE }); }

-- A non-input component renders against the plan with its explicit ID → @()
@(Html.FusionGrid<TModel, TRow>(plan, "roster", b => b.CHAIN).Reactive(evt => evt.RowSelected<TRow>(), (args, p) => { PIPELINE }))

-- NativeButton returns IHtmlContent → @()
@(Html.NativeButton("id", "text").CssClass("...").Reactive(plan, evt => evt.Click, (args, p) => { PIPELINE }))

-- A hidden field is a hidden InputField; it writes itself and returns an empty handle
@Html.HiddenFieldFor(plan, m => m.Prop)
```

**Wrong context = silent render failure.** `@{ Html.NativeButton(...); }` compiles but renders nothing.

## Trigger

```
TRIGGER :=
  | DomReady(p => { PIPELINE })
  | CustomEvent("name", p => { PIPELINE })
  | CustomEvent<TPayload>("name", (payload, p) => { PIPELINE })
  | ServerPush("url", p => { PIPELINE })                              -- SSE, every message
  | ServerPush("url", "eventType", p => { PIPELINE })                 -- SSE, filtered
  | ServerPush<TPayload>("url", "eventType", (payload, p) => { PIPELINE })
  | SignalR("hubUrl", "methodName", p => { PIPELINE })
  | SignalR<TPayload>("hubUrl", "methodName", (payload, p) => { PIPELINE })
```

`TPayload` must be `class, new()`. Payload is a phantom — properties become `evt.` dot-paths.
ServerPush and SignalR are remote triggers — the async lane.

Component events use `.Reactive()` on the rendered component, not `Html.On`. `evt` is the
component's declaration; an event is a member it declares by the JavaScript event name:

```
.Reactive(evt => evt.EVENT, (args, p) => { PIPELINE })                 -- InputField and plan-rendered components
.Reactive(plan, evt => evt.Click, (args, p) => { PIPELINE })           -- NativeButton

EVENT := Change | Filtering | Select | Focus | Blur | Created | ...    -- the component's own names
       | RowSelected<TRow>() | CardClick<TCard>() | ...                 -- events typed by the app's class
```

## Pipeline

```
PIPELINE :=
  | p.Element("id").ELEMENT_ACTION
  | p.Dispatch("name")
  | p.Dispatch<T>("name", payload)
  | p.Component<TComp>(m => m.Prop).COMP_ACTION     -- model-bound
  | p.Component<TComp>("refId").COMP_ACTION          -- string ref
  | p.Component<TComp>().COMP_ACTION                 -- app-level singleton
  | p.When(SOURCE).OPERATOR.BRANCH                   -- → conditions-dsl
  | p.Confirm("msg").Then(t => { PIPELINE })         -- async halt
  | p.HTTP_REQUEST                                    -- → http-pipeline
  | p.Parallel(BRANCHES).OnAllSettled(...)            -- → http-pipeline
  | p.ValidationErrors("formId")
  | p.Into("elementId")
```

## Element Actions

```
p.Element("id").ACTION

ACTION :=
  -- Class (→ PipelineBuilder)
  | .AddClass("class")
  | .RemoveClass("class")
  | .ToggleClass("class")

  -- Text (→ PipelineBuilder)
  | .SetText("static")
  | .SetText(payload, x => x.Prop)                     -- event/response phantom
  | .SetText(responseBody, x => x.Prop)                -- typed HTTP response

  -- Text (→ ElementBuilder)
  | .SetText(bindSource)
  | .SetText(typedSource)                              -- comp.Get(c => c.Value)

  -- HTML (→ PipelineBuilder)
  | .SetHtml("static")
  | .SetHtml(payload, x => x.Prop)

  -- HTML (→ ElementBuilder)
  | .SetHtml(bindSource)
  | .SetHtml(typedSource)

  -- Visibility (→ PipelineBuilder)
  | .Show()
  | .Hide()
```

**Return type matters.** `→ PipelineBuilder` chains to next command.

## Component Actions

A component is a JavaScript object declared once (`Alis.Reactive.Fusion.Components`,
`Alis.Reactive.Native.Components`, or the app's own assembly). Three verbs touch any declared
member; `c` is the declaration:

```
COMP_ACTION :=
  | .Set(c => c.PROPERTY, literal | typedSource)           -- write a property
  | .Call(c => c.METHOD)                                   -- call a method
  | .Call(c => c.METHOD, arg)                              -- one argument: literal or typedSource
  | .Call(c => c.METHOD, arg1, arg2[, arg3])               -- each argument a literal or a typedSource
  | .Get(c => c.PROPERTY)        → TypedComponentSource<T> -- read, for SetText, conditions, gather
  | .Get(c => c.FUNCTION[, args]) → TypedComponentSource<T> -- read what a method returns

typedSource := json.Read(x => x.Items)                     -- response body (x => x is the whole body)
             | p.Payload(args).Get(a => a.Field)            -- event payload
             | p.Component<T>(m => m.Other).Get(c => c.Value)
             | url, plugin and array sources (the sources the DSL already has)
```

Examples, each from a sandbox view:

```
p.Component<FusionDropDownList>(m => m.Protocol).Set(c => c.Value, "Full Clinical")
s.Component<FusionDropDownList>(m => m.City).Set(c => c.DataSource, json.Read(j => j.Cities)).Call(c => c.DataBind)
p.When(p.Component<FusionDropDownList>(m => m.CareLevel).Get(c => c.Value)).Eq("Memory Care")
grid.Call(c => c.HideColumn, Field.Of((ResidentDirectoryGridItem x) => x.PrimaryNurse))
grid.Call(c => c.SetCellValue<string>(), 6002, Field.Of((ResidentDirectoryGridItem x) => x.RiskLevel), "Critical")
g.Include(grid.Get(c => c.GetSelectedRecords<ResidentDirectoryGridItem>()), "selectedRecords")
```

- A member typed by the app's class is a generic method of the declaration:
  `c.GetSelectedRecords<TRow>()`, `c.AddRecordAt<TRow>()`, `evt.RowSelected<TRow>()`.
- `Field.Of((TRow r) => r.Member)` names a row or item member the way the component does
  (`"member"`); `Field<TValue>` makes a cell value match the column's type.
- An enum literal is its member name or its `[JsonStringEnumMemberName]` text.
- A payload with behavior: `p.Payload(args).Set(a => a.Cancel, true)`, `p.Payload(args).Call(a => a.UpdateData, json.Read(x => x.Items))`.

App-level objects are referenced without an ID and use the same verbs:

```
p.Component<NativeDrawer>().Set(c => c.Size, DrawerSize.Md).Call(c => c.Open)
p.Component<NativeDrawer>().Call(c => c.Close)
l.Component<NativeLoader>().Set(c => c.Target, "incident-report-form").Call(c => c.Show)
s.Component<NativeLoader>().Call(c => c.Hide)
s.Component<FusionToast>()
    .Set(c => c.Title, "Todo")
    .Set(c => c.Content, "Todo saved successfully")
    .Set(c => c.Tone, FusionToastTone.Success)
    .Call(c => c.Show)
```

## InputField

```
INPUT_FIELD :=
  @{ Html.InputField(plan, m => m.Prop, o => o.OPTIONS).COMPONENT_TYPE(b => b.BUILDER_CHAIN); }

OPTIONS := .Label("text") [.Required()] | .Hidden()      -- Hidden: no label or message wrapper

COMPONENT_TYPE :=
  -- Fusion (b is the Syncfusion builder; .Reactive(...) follows the render call)
  | .FusionAutoComplete(b => b [.DataSource(items)] [.Fields<T>(t,v [,g])] [.Placeholder("...")]) [REACTIVE]
  | .FusionDropDownList(b => b [.DataSource(items)] [.Fields<T>(t,v [,g])] [.Placeholder("...")]) [REACTIVE]
  | .FusionMultiSelect(b => b [.DataSource(items)] [.Fields<T>(t,v [,g])] [.Placeholder("...")] [.AllowFiltering(true)] [.Mode(VisualMode.Box)]) [REACTIVE]
  | .FusionMultiColumnComboBox(b => b [.DataSource(items)] [.Fields<T>(t,v [,g])]) [REACTIVE]
  | .FusionDatePicker(b => b) [REACTIVE]
  | .FusionDateTimePicker(b => b) [REACTIVE]
  | .FusionDateRangePicker(b => b) [REACTIVE]
  | .FusionTimePicker(b => b) [REACTIVE]
  | .FusionNumericTextBox(b => b [.Min(n)] [.Max(n)] [.Step(n)]) [REACTIVE]
  | .FusionSwitch(b => b) [REACTIVE]                                -- model prop MUST be bool
  | .FusionInputMask(b => b) [REACTIVE]
  | .FusionRichTextEditor(b => b) [REACTIVE]
  | .FusionFileUpload(b => b) [REACTIVE]

  -- Native (b is the framework builder; .Reactive(...) follows the render call)
  | .NativeTextBox(b => b [.Placeholder("...")] [.Type("email")] [.CssClass("...")]) [REACTIVE]
  | .NativeCheckBox(b => b [.CssClass("...")]) [REACTIVE]           -- model prop MUST be bool
  | .NativeDropDown(b => b .Items(selectListItems) [.Placeholder("...")] [.CssClass("...")]) [REACTIVE]
  | .NativeTextArea(b => b [.Rows(n)] [.Placeholder("...")] [.CssClass("...")]) [REACTIVE]
  | .NativeCheckList(b => b .Option("val","text")... [.CssClass("...")]) [REACTIVE]
  | .NativeRadioGroup(b => b .Option("val","text")... [.CssClass("...")]) [REACTIVE]

Fields<T> :=
  | .Fields<TItem>(t => t.TextProp, v => v.ValueProp)
  | .Fields<TItem>(t => t.TextProp, v => v.ValueProp, g => g.GroupByProp)

REACTIVE :=                                                   -- chained after COMPONENT_TYPE(...)
  | .Reactive(evt => evt.EVENT, (args, p) => { PIPELINE })
  | .Reactive(evt => evt.EVENT1, ...).Reactive(evt => evt.EVENT2, ...)
```

## NativeButton

```
BUTTON := @(Html.NativeButton("id", "text") [.CssClass("...")] [.Type("submit")] .Reactive(plan, evt => evt.Click, (args, p) => { PIPELINE }))
```

**Must use `@()`.** Returns `IHtmlContent`. Using `@{ }` silently renders nothing.

## NativeHiddenField

```
HIDDEN := @Html.HiddenFieldFor(plan, m => m.Prop)
```

A hidden InputField: no label or message wrapper, still registered for gather. Set its value with
`p.Component<NativeHiddenField>(m => m.Prop).Set(c => c.Value, "v")`.

## Dispatch

```
DISPATCH :=
  | p.Dispatch("eventName")
  | p.Dispatch<TPayload>("eventName", new TPayload { ... })
```

## Event Payloads with Behavior (Filtering)

Source: `Areas/Sandbox/Views/Components/Fusion/AutoComplete/Index.cshtml`
```
.Reactive(evt => evt.Filtering, (args, p) =>
{
    p.Payload(args).Set(a => a.PreventDefaultAction, true);           -- suppress the client filter
    p.Get(Url.SandboxRoute("Medications", "AutoComplete"))
     .Gather(g => g.FromEvent(args, x => x.Text, "MedicationType"))    -- event payload → query param
     .Response(r => r
        .OnSuccess<MedicationSearchResponse>((json, s) =>
        {
            s.Payload(args).Call(a => a.UpdateData, json.Read(j => j.Medications));  -- feed the popup
        }));
})
```

A payload's fields are plain C# properties; its methods are declared on a payload that derives
from `JsObject`. `p.Payload(args)` is the handle that writes, calls and reads them.

## Expression Path Resolution

```
| payload, x => x.Text          → "evt.text"
| payload, x => x.Address.City  → "evt.address.city"
| responseBody, x => x.Data     → "responseBody.data"
| m => m.Address.City (ID)      → "Namespace_Model__Address_City"
| m => m.Address.City (binding) → "Address.City"
```

## Key Rules

- `@()` for NativeButton/NativeActionLink and plan-rendered components, `@{ }` for InputField/Html.On
- NumericTextBox needs SF methods (.Min/.Max/.Step) BEFORE .Reactive()
- AllowFiltering(true) required on MultiSelect/DropDownList for Filtering event
- Switch/NativeCheckBox model property MUST be `bool`
- DomReady fires AFTER all custom-event listeners wired (two-phase boot)
- `.Reactive()` follows the render call of its component, never inside the Syncfusion build callback
- `Html.On` is for triggers not tied to a specific component (DomReady, CustomEvent, ServerPush, SignalR)
- Every view ends with `@Html.RenderPlan(plan)` — no exceptions
- When adding a new sandbox view, update the section's `Index.cshtml` to link to it

## Canonical Reference — Verified Patterns from Sandbox Views

These patterns are extracted from 127 real sandbox views. Use exactly these forms.

**Most common: .Reactive() on the rendered component**
Source: `Areas/Sandbox/Views/Components/Fusion/DropDownList/Index.cshtml`
```csharp
@{ Html.InputField(plan, m => m.Category, o => o.Label("Category (SF DropDownList)"))
    .FusionDropDownList(b => b
        .DataSource(categories)
        .Placeholder("Select a category"))
    .Reactive(evt => evt.Change, (args, p) =>
    {
        p.Element("change-value").SetText(args, x => x.Value);
    }); }
```

**DomReady with HTTP (36 uses)**
Source: `Areas/Sandbox/Views/HttpPipeline/Http/Index.cshtml`
```csharp
Html.On(plan, t => t.DomReady(p =>
    p.Get("/Sandbox/HttpPipeline/Http/Residents")
     .WhileLoading(l => l.Element("load-spinner").Show())
     .Response(r => r.OnSuccess<ResidentsResponse>((json, s) =>
     {
         s.Element("load-spinner").Hide();
         s.Element("load-first").SetText(json, x => x.First);
     }))));
```

**Button + POST + Validate (34 uses)**
Source: `Areas/Sandbox/Views/Validation/Contract/Index.cshtml`
```csharp
@(Html.NativeButton("submit-btn", "Submit")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Post("/Sandbox/Validation/Contract/Submit", g => g.IncludeAll())
         .Validate<ResidentValidator>("resident-form")
         .Response(r => r
            .OnSuccess(s => s.Element("result").SetText("Saved"))
            .OnError(400, e => e.ValidationErrors("resident-form")));
    }))
```

**Conditions with component reads (150+ uses)**
Source: `Areas/Sandbox/Views/Conditions/CareLevelCascade/Index.cshtml:37-51`
```csharp
var careLevel = p.Component<FusionDropDownList>(m => m.CareLevel);

p.Element("s1-current-level").SetText(careLevel.Get(c => c.Value));

p.When(careLevel.Get(c => c.Value)).Eq("Memory Care")
 .Then(then =>
 {
     then.Component<FusionDropDownList>(m => m.Protocol).Set(c => c.Value, "Enhanced Monitoring");
 })
 .ElseIf(careLevel.Get(c => c.Value)).Eq("Skilled Nursing")
 .Then(then =>
 {
     then.Component<FusionDropDownList>(m => m.Protocol).Set(c => c.Value, "Full Clinical");
 })
 .Else(else_ =>
 {
     else_.Component<FusionDropDownList>(m => m.Protocol).Set(c => c.Value, "");
 });
```

**CustomEvent with typed payload (26 uses)**
Source: `Areas/Sandbox/Views/CoreBehaviors/Payload/Index.cshtml`
```csharp
Html.On(plan, t => t.CustomEvent<PayloadShowcaseModel>("payload-test", (payload, p) =>
{
    p.Element("int-value").SetText(payload, x => x.IntValue);
    p.Element("address-street").SetText(payload, x => x.Address!.Street);
}));
```
- One .Reactive() per event — multiple events chain on same builder
