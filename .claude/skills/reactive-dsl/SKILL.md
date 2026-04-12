---
name: reactive-dsl
description: Use when writing C# reactive plan code in .cshtml views — Html.On, triggers (DomReady, CustomEvent, component .Reactive), pipeline commands (Element, Dispatch, Component), and plan rendering. The core DSL grammar for Alis.Reactive.
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
@using Alis.Reactive.Fusion.AppLevel         -- FusionToast, FusionConfirm
@using Syncfusion.EJ2                        -- Html.EJS()
@using Syncfusion.EJ2.DropDowns              -- VisualMode (MultiSelect)
```

## Razor Output — `@{ }` vs `@()`

```
RAZOR_VOID    := @{ EXPR; }            -- void methods: InputField, HiddenFieldFor, Html.On
RAZOR_CONTENT := @(EXPR)               -- IHtmlContent:  NativeButton, NativeActionLink
RAZOR_HELPER  := @Html.RenderPlan(plan) -- IHtmlContent helper

-- InputField renders inline via writer → @{ }
@{ Html.InputField(plan, m => m.Prop, o => o.OPTIONS).COMPONENT(b => b.CHAIN); }

-- NativeButton returns IHtmlContent → @()
@(Html.NativeButton("id", "text").CssClass("...").Reactive(plan, evt => evt.Click, (args, p) => { PIPELINE }))

-- HiddenField is void → @{ }
@{ Html.HiddenFieldFor(plan, m => m.Prop); }
```

**Wrong context = silent render failure.** `@{ Html.NativeButton(...); }` compiles but renders nothing.

## Trigger

```
TRIGGER :=
  | DomReady(p => { PIPELINE })
  | CustomEvent("name", p => { PIPELINE })
  | CustomEvent<TPayload>("name", (payload, p) => { PIPELINE })
```

`TPayload` must be `class, new()`. Payload is a phantom — properties become `evt.` dot-paths.

Component events use `.Reactive()` on the builder, not `Html.On`:

```
.Reactive(plan, evt => evt.EVENT, (args, p) => { PIPELINE })

EVENT := Changed | Filtering | Click | Focus | Blur | Selected
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

  -- Text (→ ElementBuilder — supports per-command When)
  | .SetText(bindSource)
  | .SetText(typedSource)                              -- comp.Value()

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
`→ ElementBuilder` supports `.When()` per-command guard (see conditions-dsl).

## Component Actions

```
COMP_ACTION :=
  -- Write
  | .SetValue(string)                                  -- AC, DDL, TextBox, InputMask
  | .SetValue(decimal)                                 -- NumericTextBox (coerce:number)
  | .SetValue(DateTime)                                -- DatePicker, TimePicker
  | .SetChecked(bool)                                  -- Switch, CheckBox (coerce:boolean)
  | .SetDataSource(responseBody, x => x.Items)         -- DDL, AC, MS, MCCB
  | .SetMin(decimal)                                   -- NumericTextBox

  -- Method
  | .DataBind()                                        -- after SetDataSource (cascade)
  | .FocusIn()
  | .FocusOut()
  | .ShowPopup()                                       -- AC, DDL, MS
  | .HidePopup()
  | .Increment()                                       -- NumericTextBox
  | .Decrement()                                       -- NumericTextBox
  | .Enable()                                          -- AC, DDL
  | .Disable()                                         -- AC, DDL

  -- Read (→ TypedComponentSource<T> for conditions/SetText)
  | .Value()        → TypedComponentSource<string>     -- AC, DDL, TextBox, InputMask, RTE
  | .Value()        → TypedComponentSource<string[]>   -- MS, CheckList
  | .Value()        → TypedComponentSource<decimal>    -- NumericTextBox
  | .Value()        → TypedComponentSource<bool>       -- Switch, CheckBox
  | .Value()        → TypedComponentSource<DateTime>   -- DatePicker, TimePicker, DTP
  | .StartDate()    → TypedComponentSource<DateTime>   -- DateRangePicker
  | .EndDate()      → TypedComponentSource<DateTime>   -- DateRangePicker

  -- App-level (NativeDrawer)
  | .SetSize(DrawerSize.Sm|Md|Lg)
  | .Open()
  | .Close()

  -- App-level (NativeLoader)
  | .SetTarget("elementId")
  | .SetTimeout(ms)
  | .Show()
  | .Hide()

  -- App-level (FusionToast) — fluent chain
  | .SetTitle("t").SetContent("c").TOAST_TYPE().SetTimeout(ms)[.ShowCloseButton()][.ShowProgressBar()].Show()
  TOAST_TYPE := .Success() | .Warning() | .Danger() | .Info()
```

## InputField

```
INPUT_FIELD :=
  @{ Html.InputField(plan, m => m.Prop, o => o.OPTIONS).COMPONENT_TYPE(b => b.BUILDER_CHAIN); }

OPTIONS := .Label("text") [.Required()]

COMPONENT_TYPE :=
  -- Fusion (b is SF builder — chain SF methods then .Reactive())
  | .AutoComplete(b => b [.DataSource(items)] [.Fields<T>(t,v [,g])] [.Placeholder("...")] [.REACTIVE])
  | .DropDownList(b => b [.DataSource(items)] .Fields<T>(t,v [,g]) [.Placeholder("...")] [.REACTIVE])
  | .MultiSelect(b => b [.DataSource(items)] .Fields<T>(t,v [,g]) [.Placeholder("...")] [.AllowFiltering(true)] [.Mode(VisualMode.Box)] [.REACTIVE])
  | .MultiColumnComboBox(b => b [.DataSource(items)] .Fields<T>(t,v [,g]) [.REACTIVE])
  | .DatePicker(b => b [.REACTIVE])
  | .DateTimePicker(b => b [.REACTIVE])
  | .DateRangePicker(b => b [.REACTIVE])
  | .TimePicker(b => b [.REACTIVE])
  | .NumericTextBox(b => b [.Min(n)] [.Max(n)] [.Step(n)] [.REACTIVE])
  | .Switch(b => b [.REACTIVE])                                      -- model prop MUST be bool
  | .InputMask(b => b [.REACTIVE])
  | .RichTextEditor(b => b [.REACTIVE])
  | .FileUpload(b => b [.REACTIVE])

  -- Native (b is framework builder — chain config then .Reactive())
  | .NativeTextBox(b => b [.Placeholder("...")] [.Type("email")] [.CssClass("...")] [.REACTIVE])
  | .NativeCheckBox(b => b [.CssClass("...")] [.REACTIVE])           -- model prop MUST be bool
  | .NativeDropDown(b => b .Items(selectListItems) [.Placeholder("...")] [.CssClass("...")] [.REACTIVE])
  | .NativeTextArea(b => b [.Rows(n)] [.Placeholder("...")] [.CssClass("...")] [.REACTIVE])
  | .NativeCheckList(b => b .Option("val","text")... [.CssClass("...")] [.REACTIVE])
  | .NativeRadioGroup(b => b .Option("val","text")... [.CssClass("...")] [.REACTIVE])

Fields<T> :=
  | .Fields<TItem>(t => t.TextProp, v => v.ValueProp)
  | .Fields<TItem>(t => t.TextProp, v => v.ValueProp, g => g.GroupByProp)

REACTIVE :=
  | .Reactive(plan, evt => evt.EVENT, (args, p) => { PIPELINE })
  | .Reactive(plan, evt => evt.EVENT1, ...).Reactive(plan, evt => evt.EVENT2, ...)
```

## NativeButton

```
BUTTON := @(Html.NativeButton("id", "text") [.CssClass("...")] [.Type("submit")] .Reactive(plan, evt => evt.Click, (args, p) => { PIPELINE }))
```

**Must use `@()`.** Returns `IHtmlContent`. Using `@{ }` silently renders nothing.

## NativeHiddenField

```
HIDDEN := @{ Html.HiddenFieldFor(plan, m => m.Prop); }
```

No InputField wrapper. No Reactive. Set value via `p.Component<NativeHiddenField>(m => m.Prop).SetValue("v")`.

## Dispatch

```
DISPATCH :=
  | p.Dispatch("eventName")
  | p.Dispatch<TPayload>("eventName", new TPayload { ... })
```

## Event Args Extensions (Filtering)

```
.Reactive(plan, evt => evt.Filtering, (args, p) =>
{
    args.PreventDefault(p);                              -- suppress SF client filter
    p.Get("/url")
     .Gather(g => g.FromEvent(args, x => x.Text, "q"))  -- event payload → query param
     .Response(r => r.OnSuccess<TResp>((json, s) =>
     {
         args.UpdateData(s, json, j => j.Items);          -- feed data to popup
     }));
})
```

`PreventDefault(p)` and `UpdateData(s, json, path)` need the pipeline builder because
args is a phantom with no pipeline binding.

## Expression Path Resolution

```
| payload, x => x.Text          → "evt.text"
| payload, x => x.Address.City  → "evt.address.city"
| responseBody, x => x.Data     → "responseBody.data"
| m => m.Address.City (ID)      → "Namespace_Model__Address_City"
| m => m.Address.City (binding) → "Address.City"
```

## Key Rules

- `@()` for NativeButton/NativeActionLink, `@{ }` for InputField/HiddenField/Html.On
- NumericTextBox needs SF methods (.Min/.Max/.Step) BEFORE .Reactive()
- AllowFiltering(true) required on MultiSelect/DropDownList for Filtering event
- Switch/NativeCheckBox model property MUST be `bool`
- DomReady fires AFTER all custom-event listeners wired (two-phase boot)
- `.Reactive()` stays close to its component — it is the last call inside the build callback
- `Html.On` is for triggers not tied to a specific component (DomReady, CustomEvent)
- Every view ends with `@Html.RenderPlan(plan)` — no exceptions
- When adding a new sandbox view, update the section's `Index.cshtml` to link to it

## Canonical Reference — Verified Patterns from Sandbox Views

These patterns are extracted from 127 real sandbox views. Use exactly these forms.

**Most common: .Reactive() on component (252+ uses)**
Source: `Areas/Sandbox/Views/Components/Fusion/DropDownList/Index.cshtml`
```csharp
@{ Html.InputField(plan, m => m.Category, o => o.Label("Category (SF DropDownList)"))
   .FusionDropDownList(b => b
       .DataSource(categories)
       .Reactive(plan, evt => evt.Changed, (args, p) =>
       {
           p.Element("change-value").SetText(args, x => x.Value);
       })); }
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

p.Element("s1-current-level").SetText(careLevel.Value());

p.When(careLevel.Value()).Eq("Memory Care")
 .Then(then =>
 {
     then.Component<FusionDropDownList>(m => m.Protocol).SetValue("Enhanced Monitoring");
 })
 .ElseIf(careLevel.Value()).Eq("Skilled Nursing")
 .Then(then =>
 {
     then.Component<FusionDropDownList>(m => m.Protocol).SetValue("Standard Protocol");
 })
 .Else(else_ =>
 {
     else_.Component<FusionDropDownList>(m => m.Protocol).SetValue("");
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
