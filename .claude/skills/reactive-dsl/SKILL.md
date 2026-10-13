---
name: reactive-dsl
description: Use when writing C# reactive plan code in .cshtml views — Html.On, triggers (DomReady, CustomEvent, ServerPush, SignalR, component .Reactive), pipeline commands (Element, Dispatch, Component), and plan rendering. The core DSL grammar for Alis.Reactive.
---

# Reactive DSL Grammar

Source of truth: `Alis.Reactive/Components/Contracts/JsObject.cs`, `JsMembers.cs`,
`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs`, `HtmlElement.cs`,
`Alis.Reactive/PlanAuthoring/Events/Data.cs`. Every example below is copied from a sandbox view under
`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/`.

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
@using Alis.Reactive                         -- Html.On, ReactivePlan, RenderPlan, InputField, Field.Of(...)
@using Alis.Reactive.Native.Components       -- NativeTextBox, NativeButton, NativeCheckBox, ...
@using Alis.Reactive.Fusion.Components       -- FusionAutoComplete, FusionDropDownList, ...
@using Alis.Reactive.Builders.Requests       -- HttpRequestBuilder, GatherBuilder<TRequest, TModel>, ResponseBuilder
@using Alis.Reactive.Native.AppLevel         -- NativeDrawer, NativeLoader, DrawerSize
@using Alis.Reactive.Fusion.AppLevel         -- FusionToast, FusionToastTone
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
  | CustomEvent(EVT, p => { PIPELINE })                               -- EVT: AppEvent
  | CustomEvent(EVT, (payload, p) => { PIPELINE })                    -- EVT: AppEvent<TPayload>
  | ServerPush(ServerPushStream.At<TStream>(url), s => s.MESSAGE, (payload, p) => { PIPELINE })
  | SignalR(SignalRHub.At<THub>(url), h => h.MESSAGE, (payload, p) => { PIPELINE })
```

Event names are declared once, never typed at the call:

```csharp
public static class CareEvents                       // document events
{
    public static readonly AppEvent Refreshed = AppEvent.Named("roster-refreshed");
    public static readonly AppEvent<ResidentFlagged> Flagged = AppEvent.Named<ResidentFlagged>("resident-flagged");
}
public sealed class FacilityAlertStream : ServerPushStream   // SSE: event names the server writes
{
    public ServerPushMessage<FacilityAlert> Alert => Message<FacilityAlert>("facility-alert");
}
public sealed class ResidentStatusFeed : SignalRHub          // SignalR: client methods the hub calls
{
    public SignalRMessage<ResidentStatusPayload> StatusChanged => Message<ResidentStatusPayload>("StatusChanged");
}
```

The server writes the same declaration's `.Name` (`SendAsync(new ResidentStatusFeed().StatusChanged.Name, ..)`).
`TPayload` must be `class, new()`. `payload` is `Data<TPayload>`: `payload.Get(x => x.Member)` reads a
member by its camel-case JSON name, and `payload` itself is a value too.
ServerPush and SignalR are remote triggers — the async lane.

Component events use `.Reactive()` on the rendered component, not `Html.On`. `evt` is the
component's declaration; an event is a member it declares by the JavaScript event name:

```
.Reactive(evt => evt.EVENT, (args, p) => { PIPELINE })                 -- InputField and plan-rendered components
.Reactive(plan, evt => evt.Click, (args, p) => { PIPELINE })           -- NativeButton

EVENT := Change | Filtering | Select | Focus | Blur | Created | ...    -- the component's own names
       | RowSelected<TRow>() | CardClick<TCard>() | ...                 -- events typed by the app's class
```

`args` is the object the event hands its callback, used as the object: `args.Value` is a source,
`args.Cancel.Set(true)` writes back, `args.UpdateData(..)` calls one of its methods.

## Pipeline

```
PIPELINE :=
  | p.Element("id").ELEMENT_ACTION
  | p.Dispatch(EVT)                                  -- AppEvent
  | p.Dispatch(EVT, payload)                         -- AppEvent<T>, payload fixed at render
  | p.Dispatch<T>(EVT, d => d.Set(x => x.F, SOURCE)) -- AppEvent<T>, payload read when it runs
  | var c = p.Component<TComp>(m => m.Prop); c.MEMBER_USE   -- model-bound
  | var c = p.Component<TComp>("refId"); c.MEMBER_USE        -- explicit id
  | var c = p.Component<TComp>(); c.MEMBER_USE              -- app-level singleton
  | p.When(SOURCE).OPERATOR.BRANCH                   -- → conditions-dsl
  | p.Confirm("msg").Then(t => { PIPELINE })         -- async halt
  | p.HTTP_REQUEST                                    -- → http-pipeline
  | p.Parallel(BRANCHES).OnAllSettled(...)            -- → http-pipeline
  | p.ValidationErrors("formId")
  | p.Into("elementId")
```

Statements, not chains: each write or call is its own statement. `p.Component<T>(..)` returns `T`
itself, so hold it in a variable named for its role (`rate`, `grid`, `toast`).

**Where a step lands.** A step lands in the callback it is written in, whichever pipeline
variable or object writes it. Declare an object once at the top of a trigger and use it in any
route (`Components/Brought/VisitPlanner/FrontDesk.cshtml`):

```csharp
var link = p.Url<FrontDeskQuery>();
var planner = p.Component<VisitPlanner>(m => m.VisitSlots);
var resident = p.Element("desk-resident");
var loading = p.Element("desk-loading");
p.Get(weekUrl)
    .Gather<FrontDeskResident>(g => g.Set(x => x.ResidentId, link.Get(q => q.Resident)))
    .WhileLoading(l => loading.Show())
    .Response(r => r
        .OnSuccess<ResidentWeekResponse>((week, s) =>
        {
            planner.Wing.Set(week.Get(x => x.Wing));
            planner.Capacity.Set(week.Get(x => x.WeeklyVisits));
            resident.Text.Set(week.Get(x => x.ResidentName));
        }))
    .Finally(f => loading.Hide());
```

A step written outside any callback throws when the view renders.

## Element Actions

`p.Element("id")` is the element the view rendered with that id (`HtmlElement`).

```
p.Element("id").ACTION

ACTION :=
  | .Text.Set("static")                 -- textContent
  | .Text.Set(SOURCE)                   -- any value read when the step runs: number, date or text
  | .Html.Set("static")                 -- innerHTML
  | .Html.Set(SOURCE)
  | .ClassList.Add("class")             -- each takes a literal or a string source
  | .ClassList.Remove("class")
  | .ClassList.Toggle("class")
  | .Show()                             -- removes the hidden attribute
  | .Hide()                             -- sets the hidden attribute

READ :=                                 -- arrays, usable wherever a source goes
  | p.Element("id").ClassList.Items     -- the classes, ReactiveArray<string>
  | p.Element("id").Children            -- the child elements, ReactiveArray<ChildElement>
```

From `Components/Native/DomOps/Index.cshtml` and `Components/Brought/VisitPlanner/VisitBoard.cshtml`:

```csharp
p.Element("dom-risk-count").Text.Set(
    p.Element("dom-card").ClassList.Items.Where(x => x.StartsWith("risk-")).Count());
p.Element("dom-high-risk-count").Text.Set(
    p.Element("risk-list").Children.Count(x => x.GetAttribute("data-risk") == "high"));
p.Element("board-legend").ClassList.Toggle("hidden");
p.Element("board-legend").Html.Set("<strong>Taken</strong> times belong to another desk's family; <em>dashed</em> times are quiet hours.");
```

## Component Members

A component is a JavaScript object declared once as a C# class (`Alis.Reactive.Fusion.Components`,
`Alis.Reactive.Native.Components`, or the app's own assembly). Each of its members is a real C#
member, used directly:

```
MEMBER_USE :=
  | c.PROPERTY.Set(literal)               -- Prop<T>: write a value fixed at render
  | c.PROPERTY.Set(SOURCE)                -- Prop<T>: write a value read when the step runs
  | c.PROPERTY                            -- Prop<T> or ReadOnly<T>: a source
  | c.PROPERTY.Get(x => x.Member)         -- read into a property whose value is data (a row, a slot)
  | c.METHOD()                            -- call
  | c.METHOD(arg1[, arg2, ...])           -- each argument a literal or a SOURCE, in the declared order
  | c.FUNCTION([args])                    -- a method that returns a value: a source

SOURCE := json.Get(x => x.Items) | json      -- a response body (json itself is the whole body)
        | payload.Get(x => x.Field)          -- a custom event's payload or a message
        | args.Field                         -- the event's payload object
        | p.Component<T>(m => m.Other).Value -- another component's property
        | p.Url<TQuery>().Get(q => q.Member) -- the page's URL query string
        | plugin.Member(..)                  -- a registered plugin's member
        | p.From(source).Count(..), p.Element("id").Children, ...  -- arrays
```

Examples, each from a sandbox view:

```csharp
p.Component<FusionDropDownList>(m => m.Category).Value.Set("Books");          // DropDownList/Index.cshtml
var city = s.Component<FusionDropDownList>(m => m.City);                      // Patterns/Cascading/Index.cshtml
city.DataSource.Set(json.Get(j => j.Cities));
city.DataBind();
p.When(careLevel.Value).Eq("Memory Care")                                     // Conditions/CareLevelCascade/Index.cshtml
grid.HideColumn(Field.Of((ResidentDirectoryGridItem x) => x.PrimaryNurse));   // Grid/Operations.cshtml
p.Component<FusionGrid>(GridId).SetCellValue<string>(6002, Field.Of((ResidentDirectoryGridItem x) => x.RiskLevel), "Critical");
.Gather<ResidentSelectionRecordsRequest>(g => g.Set(x => x.SelectedRecords, grid.GetSelectedRecords<ResidentDirectoryGridItem>()))
s.Component<FusionGrid>("residents-grid").DataSource.Set(json);              // Grid/Index.cshtml: the body is the rows
```

- A member typed by the app's class is a generic method of the declaration:
  `grid.GetSelectedRecords<TRow>()`, `grid.AddRecordAt<TRow>(..)`, `evt.RowSelected<TRow>()`.
- `Field.Of((TRow r) => r.Member)` names a row or item member the way the component does
  (`"member"`); `Field<TValue>` makes a cell value match the column's type.
- An enum literal is its member name or its `[JsonStringEnumMemberName]` text.
- An event payload with behavior is used the same way: `args.PreventDefaultAction.Set(true)`,
  `args.UpdateData(json.Get(j => j.Medications))`, `args.Waitlist()`.

App-level objects are referenced without an ID (`Components/AppLevel/Drawer/Index.cshtml`,
`Components/AppLevel/Loader/Index.cshtml`, `Patterns/Todo/Index.cshtml`):

```csharp
var drawer = p.Component<NativeDrawer>();
drawer.Size.Set(DrawerSize.Md);
drawer.Open();

var loader = l.Component<NativeLoader>();
loader.Timeout.Set(3000);
loader.Show();
s.Component<NativeLoader>().Hide();

var toast = s.Component<FusionToast>();
toast.Title.Set("Todo");
toast.Content.Set("Todo saved successfully");
toast.Tone.Set(FusionToastTone.Success);
toast.Show();
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
`p.Component<NativeHiddenField>(m => m.Prop).Value.Set("v")`.

## Dispatch

```
DISPATCH :=
  | p.Dispatch(CareEvents.Refreshed)
  | p.Dispatch(CareEvents.Flagged, new ResidentFlagged { ... })
  | p.Dispatch<ResidentFlagged>(CareEvents.Flagged, d => d.Set(x => x.ResidentId, SOURCE))
```

From `CoreBehaviors/DispatchSource/Index.cshtml`:

```csharp
p.Dispatch<DispatchSourceModel>(DispatchSourceEvents.Transfer, d => d
    .Set(x => x.Name, p.Component<NativeTextBox>(m => m.Name).Value)
    .Set(x => x.Status, "active")
    .Set(x => x.Address.City, p.Component<NativeTextBox>(m => m.Address.City).Value)
);
```

## Event Payloads with Behavior (Filtering)

Source: `Areas/Sandbox/Views/Components/Fusion/AutoComplete/Index.cshtml`
```csharp
.Reactive(evt => evt.Filtering, (args, p) =>
{
    args.PreventDefaultAction.Set(true);                                          // suppress the client filter
    p.Get(Url.SandboxRoute("Medications", "AutoComplete"))
     .Gather<MedicationSearchQuery>(g => g.Set(x => x.MedicationType, args.Text))  // event payload → query member
     .Response(r => r
        .OnSuccess<MedicationSearchResponse>((json, s) =>
        {
            args.UpdateData(json.Get(j => j.Medications));                         // feed the popup
            s.Element("filter-status").Text.Set("results loaded");
        }));
})
```

An event payload is declared as a `JsObject`: `ReadOnly<T>` members for what it carries, `Prop<T>`
for what the component reads back (`Cancel`, `PreventDefaultAction`), and methods for what it can do.
`args` is that object, live in the callback.

## Member Path Resolution

```
| payload.Get(x => x.Text)          → the payload's "text"
| payload.Get(x => x.Address.City)  → the payload's "address.city"
| json.Get(x => x.Data)             → the response body's "data"
| m => m.Address.City (ID)          → "Namespace_Model__Address_City"
| m => m.Address.City (binding)     → "Address.City"
```

A data member reads by its camel-case JSON name: `x => x.MRN` reads `mrn`.

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

These patterns are copied from real sandbox views. Use exactly these forms.

**Most common: .Reactive() on the rendered component**
Source: `Areas/Sandbox/Views/Components/Fusion/DropDownList/Index.cshtml`
```csharp
@{ Html.InputField(plan, m => m.Category, o => o.Label("Category (SF DropDownList)"))
    .FusionDropDownList(b => b
        .DataSource(categories)
        .Placeholder("Select a category"))
    .Reactive(evt => evt.Change, (args, p) =>
    {
        p.Element("change-value").Text.Set(args.Value);
    }); }
```

**DomReady with HTTP**
Source: `Areas/Sandbox/Views/HttpPipeline/Http/Index.cshtml`
```csharp
Html.On(plan, t => t.DomReady(p =>
    p.Get(Url.SandboxRoute("Residents", "Http"))
     .WhileLoading(l => l.Element("load-spinner").Show())
     .Response(r => r
        .OnSuccess<ResidentsResponse>((json, s) =>
        {
            s.Element("load-spinner").Hide();
            s.Element("load-first").Text.Set(json.Get(x => x.First));
        }))));
```

**Button + POST + Validate**
Source: `Areas/Sandbox/Views/Validation/Contract/Index.cshtml`
```csharp
@(Html.NativeButton("submit-btn", "Submit Admission")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Post(Url.SandboxRoute("Submit", "ValidationContract")).Gather(g => g.Form())
         .Validate<ResidentValidator>("resident-form")
         .Response(r => r
            .OnSuccess(s => s.Element("result").Text.Set("Admission saved"))
            .OnError(400, e => e.ValidationErrors("resident-form")));
    }))
```

**Conditions with component reads**
Source: `Areas/Sandbox/Views/Conditions/CareLevelCascade/Index.cshtml`
```csharp
var careLevel = p.Component<FusionDropDownList>(m => m.CareLevel);

p.Element("s1-current-level").Text.Set(careLevel.Value);

p.When(careLevel.Value).Eq("Memory Care")
    .Then(then =>
    {
        then.Component<FusionDropDownList>(m => m.Protocol).Value.Set("Enhanced Monitoring");
    })
    .ElseIf(careLevel.Value).Eq("Skilled Nursing")
    .Then(then =>
    {
        then.Component<FusionDropDownList>(m => m.Protocol).Value.Set("Full Clinical");
    })
    .Else(else_ =>
    {
        else_.Component<FusionDropDownList>(m => m.Protocol).Value.Set("");
    });
```

**CustomEvent with typed payload**
Source: `Areas/Sandbox/Views/CoreBehaviors/Payload/Index.cshtml`
```csharp
Html.On(plan, t => t.CustomEvent<PayloadShowcaseModel>(PayloadShowcaseEvents.PayloadTest, (payload, p) =>
{
    p.Element("int-value").Text.Set(payload.Get(x => x.IntValue));
    p.Element("address-street").Text.Set(payload.Get(x => x.Address!.Street));
}));
```
- One .Reactive() per event — multiple events chain on same builder
