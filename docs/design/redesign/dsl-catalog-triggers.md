# Triggers

Exhaustive catalog of the **trigger** surface of the Alis.Reactive DSL — every way to
say *"start this reaction when…"*. A trigger is the browser event that opens a
reactive workflow: the page finishing load, a dispatched custom event, a component
firing an event, the server pushing over SSE, or a SignalR hub message.

Every workflow is attached with the same entry verb — `Html.On(plan, t => …)` — and
every trigger is a method on the fluent `TriggerBuilder<TModel>`, except component
events, which are wired through the component's own `.Reactive(…)` extension. Trigger
calls **chain**: each adds one independent workflow to the plan.

```csharp
Html.On(plan, t => t
    .PageLoad(p => p.Element("status").SetText("Ready"))
    .Event("resident-saved", p => p.Element("banner").SetText("Saved"))
    .ServerPush("/sse/census", p => p.Element("census").SetText("Updated")));
```

> Names are the **finalized green-field names** from `09-dsl-naming-sheet.md`:
> `DomReady → PageLoad`, `CustomEvent → Event`. The plan/wire vocabulary
> (`PageLoadTrigger`/`page-load`, `EventTrigger`/`event`) is shown where it helps a
> dev understand the lane. `StartsWhen` is the underlying trigger node these builder
> calls construct.

---

## The attach verb

### On

Attaches one or more triggers to a plan. The single entry point for all trigger
authoring; the lambda receives a `TriggerBuilder<TModel>` whose methods each add an
independent workflow. Reused by `Behavior.On(trigger, reaction)` internally — one
concept, one name.

```csharp
@{
    var plan = Html.ReactivePlan<ResidentIntakeModel>();

    Html.On(plan, t => t
        .PageLoad(p => p
            .Element("intake-status")
            .SetText("Loading resident profile…")));
}
@Html.RenderPlan(plan)
```

Multiple triggers in one `On` — each call chains and registers its own workflow:

```csharp
Html.On(plan, t => t
    .PageLoad(p => p
        .Element("billing-summary")
        .SetText("Calculating monthly charges…"))
    .Event("care-level-changed", p => p
        .Element("billing-summary")
        .SetText("Recalculating…"))
    .SignalR("/hubs/facility", "OccupancyChanged", p => p
        .Element("occupancy-badge")
        .SetText("Bed count updated")));
```

---

## PageLoad

### PageLoad

Fires once when the page finishes loading. The starting point for any workflow that
must run as soon as the resident's view is on screen — no user action required.
Plan term `PageLoadTrigger`, wire `kind: "page-load"`.

```csharp
Html.On(plan, t => t
    .PageLoad(p => p
        .Element("welcome-banner")
        .SetText("Welcome to Maple Grove Senior Living")));
```

A PageLoad workflow can drive a full HTTP fetch to populate the view on arrival:

```csharp
Html.On(plan, t => t
    .PageLoad(p => p
        .Get("/api/residents/{id}/care-plan", g => g
            .RouteParam("id", FromUrl<int>("residentId")))
        .Response(r => r
            .OnSuccess(s => s
                .Set(m => m.CareLevel, s.ResponseBody<CarePlan>().Read(b => b.Level))
                .Element("care-plan-panel").Show()))));
```

---

## Event

The custom-event listener. The exact mirror of the `p.Dispatch(name)` reaction: one
workflow `Dispatch`es an event by name, another `Event`s on that same name. Plan term
`EventTrigger`, wire `kind: "event"`.

### Event

Listens for a named custom event with no typed payload. Reacts whenever any workflow
(or external code) dispatches that event name.

```csharp
Html.On(plan, t => t
    .Event("assessment-completed", p => p
        .Element("assessment-status")
        .SetText("Assessment recorded")
        .Element("next-step-panel").Show()));
```

The emit side that pairs with it — same string, listen with `Event`, emit with
`Dispatch`:

```csharp
Html.On(plan, t => t
    .PageLoad(p => p
        .Element("assessment-status")
        .SetText("Ready")
        .Dispatch("assessment-completed")));
```

### Event&lt;TPayload&gt;

Listens for a named custom event carrying a **typed payload**, giving compile-time
access to the event's data inside the pipeline. The payload object is the first
lambda parameter; its properties feed conditions, sets, and gathers through the value
spine. Pairs with `Dispatch<TPayload>(name, payload)` / `DispatchFrom<TPayload>`.

```csharp
Html.On(plan, t => t
    .Event<CareLevelChangedPayload>("care-level-changed", (args, p) => p
        .Set(m => m.CareLevel, args, a => a.NewLevel)
        .Element("care-level-display")
        .SetText(args, a => a.NewLevel)));
```

Reading a payload property into a condition before reacting:

```csharp
Html.On(plan, t => t
    .Event<BillingRecalculatedPayload>("billing-recalculated", (args, p) => p
        .When(args, a => a.MonthlyTotal).Gt(5000m)
            .Then(then => then
                .Element("high-cost-warning").Show())
        .Else(els => els
            .Element("high-cost-warning").Hide())));
```

---

## ComponentEvent (the `.Reactive()` wiring)

A component firing one of its own browser events is the `ComponentEvent` trigger
(plan term `ComponentEventTrigger`, wire `kind: "component-event"`). It is **never**
authored on `TriggerBuilder` directly — it is wired through the component's typed
`.Reactive(…)` extension, the one shared verb across every Native and Fusion slice:
*"wire this browser event into a reactive pipeline."* The event is chosen with a typed
selector (`evt => evt.Changed`) so the args type flows in compile-checked.

There are **two shapes** of `.Reactive`, split by whether the component already holds
the plan:

### .Reactive — model-bound input components (plan passed in)

Input components are rendered through `Html.InputField(plan, m => m.Prop)`, so their
`.Reactive` takes the `plan` explicitly. The selector picks the event; `(args, p)`
gives the typed event args plus the pipeline. Always the **last** call in the build
callback.

Native input:

```csharp
@(Html.InputField(plan, m => m.ResidentName)
    .NativeTextBox(b => b
        .Placeholder("Enter resident name")
        .Reactive(plan, evt => evt.Changed, (args, p) => p
            .Element("name-preview")
            .SetText(args, a => a.Value))))
```

Fusion input — same shape, vendor-prefixed builder:

```csharp
@(Html.InputField(plan, m => m.Country)
    .FusionDropDownList(b =>
    {
        b.Fields<CountryItem>(t => t.Text, v => v.Value);
        b.Reactive(plan, evt => evt.Changed, (args, p) => p
            .When(args, a => a.Value).Eq("US")
                .Then(then => then
                    .Element("state-field").Show())
            .Else(els => els
                .Element("state-field").Hide()));
    }))
```

Driving an HTTP pipeline straight off a component event:

```csharp
@(Html.InputField(plan, m => m.FacilityId)
    .FusionDropDownList(b =>
    {
        b.Fields<FacilityItem>(t => t.Name, v => v.Id);
        b.Reactive(plan, evt => evt.Changed, (args, p) => p
            .Get("/api/facilities/{id}/available-beds", g => g
                .RouteParam("id", args, a => a.Value))
            .Response(r => r
                .OnSuccess(s => s
                    .Element("bed-count")
                    .SetText(s.ResponseBody<BedAvailability>().Read(x => x.OpenBeds)))));
    }))
```

### .Reactive — display & app-level components (plan held by the builder)

Display/container components (Accordion, ListView, Menu, Sidebar, …) are rendered
through `Html.FusionAccordion(plan, "id", …)`, so the builder already carries the
plan — their `.Reactive` omits the `plan` parameter.

```csharp
@(Html.FusionAccordion(plan, "care-services", b =>
    {
        b.Item("Dining", "Three chef-prepared meals daily");
        b.Item("Wellness", "On-site nursing and therapy");
    })
    .Reactive(evt => evt.Expanded, (args, p) => p
        .Element("services-detail")
        .SetText("Service details expanded")))
```

### .Reactive — buttons (click → dispatch / HTTP)

A button's `.Reactive(plan, evt => evt.Click, …)` is the idiomatic way to start a
workflow from an explicit click.

```csharp
@(Html.NativeButton("save-intake", "Save Intake")
    .CssClass("btn btn-primary")
    .Reactive(plan, evt => evt.Click, (args, p) => p
        .Post("/api/residents/intake", g => g
            .IncludeAll())
        .Response(r => r
            .OnSuccess(s => s
                .Element("save-result").SetText("Intake saved"))
            .OnError(e => e
                .Element("save-result").SetText("Save failed")))))
```

### Reading a component-event payload as an array source

A `.Reactive` event whose payload carries an array (`T[]`) feeds the typed array
pipeline through `p.From(args, e => e.Items)` — the same value spine, array shape.

```csharp
@(Html.FusionListView(plan, "resident-tags", b => b
        .Fields<TagItem>(t => t.Text, v => v.Id))
    .Reactive(evt => evt.Selected, (args, p) => p
        .Set(m => m.SelectedTagCount,
            p.From(args, e => e.SelectedItems)
             .Count()
             .AsArraySource())))
```

---

## ServerPush

Server-Sent Events: *"the server pushes events to me."* The browser opens an SSE
connection to a URL and the workflow runs on each received event. Plan term
`ServerPushTrigger`, wire `kind: "server-push"`. The event filter (`ServerPushEventFilter`)
decides which SSE events match: **any** event, or a **named** event type.

### ServerPush — any event

The two-argument overload listens to **every** event on the stream
(`AnyServerPushEvent`, filter `kind: "any"`).

```csharp
Html.On(plan, t => t
    .ServerPush("/sse/facility-alerts", p => p
        .Element("alert-banner").Show()
        .Element("alert-text").SetText("New facility alert received")));
```

### ServerPush — named event type

The three-argument overload filters to a single SSE event type
(`NamedServerPushEvent`, filter `kind: "named"`). Only that event type runs the
workflow.

```csharp
Html.On(plan, t => t
    .ServerPush("/sse/census", "bed-freed", p => p
        .Element("available-beds").SetText("A bed has opened")
        .Dispatch("refresh-availability")));
```

### ServerPush&lt;TPayload&gt; — named event type with typed payload

The generic overload filters to a named SSE event type **and** delivers a typed
payload, giving compile-time access to the pushed data.

```csharp
Html.On(plan, t => t
    .ServerPush<VitalsReadingPayload>("/sse/resident/{id}/vitals", "vitals-update", (args, p) => p
        .Set(m => m.HeartRate, args, a => a.HeartRate)
        .When(args, a => a.HeartRate).Gt(100)
            .Then(then => then
                .Element("vitals-warning").Show()
                .Dispatch("alert-nursing"))));
```

---

## SignalR

A SignalR hub message: the workflow runs when a named hub method is invoked on the
client. Plan term `SignalRTrigger`, wire `kind: "signalr"`. Proper-noun protocol name,
aligned across every layer.

### SignalR — hub method, untyped

The three-argument overload listens for a hub method by name with no typed payload.

```csharp
Html.On(plan, t => t
    .SignalR("/hubs/facility", "OccupancyChanged", p => p
        .Element("occupancy-badge").SetText("Occupancy updated")
        .Dispatch("refresh-dashboard")));
```

### SignalR&lt;TPayload&gt; — hub method, typed payload

The generic overload delivers a typed payload from the hub method, giving
compile-time access to the message data inside the pipeline.

```csharp
Html.On(plan, t => t
    .SignalR<MedicationDuePayload>("/hubs/care", "MedicationDue", (args, p) => p
        .Element("med-alert").Show()
        .Element("med-resident").SetText(args, a => a.ResidentName)
        .Element("med-name").SetText(args, a => a.MedicationName)
        .When(args, a => a.IsCritical).Truthy
            .Then(then => then
                .Element("med-alert").AddClass("critical"))));
```

---

## Chaining every trigger together

All trigger calls return the `TriggerBuilder`, so a single `On` can register the full
range of triggers for one view — page-load setup, custom-event reactions, server
pushes, and SignalR — each an independent workflow.

```csharp
Html.On(plan, t => t
    .PageLoad(p => p
        .Element("dashboard-status").SetText("Live"))
    .Event("resident-admitted", p => p
        .Element("admit-toast").Show())
    .Event<DischargePayload>("resident-discharged", (args, p) => p
        .Element("discharge-name").SetText(args, a => a.ResidentName))
    .ServerPush("/sse/alerts", p => p
        .Element("alert-feed").SetText("New alert"))
    .ServerPush<CensusPayload>("/sse/census", "census-update", (args, p) => p
        .Set(m => m.OccupiedBeds, args, a => a.Occupied))
    .SignalR("/hubs/facility", "ShiftChanged", p => p
        .Element("shift-banner").SetText("Shift handover"))
    .SignalR<EmergencyPayload>("/hubs/care", "EmergencyRaised", (args, p) => p
        .Element("emergency-banner").Show()
        .Element("emergency-detail").SetText(args, a => a.Description)));
```
