---
title: Triggers
description: DomReady, CustomEvent, typed payloads, and component events with .Reactive().
sidebar:
  order: 2
---

A trigger defines *when* a pipeline executes. Every reactive behavior starts with one — page load, a named event, or a component interaction.

From the [Grammar Tree](/csharp-modules/mental-model/#the-grammar-tree) — the trigger-related API:

```
Html.On(plan, t => ...)                              § Triggers (what fires the pipeline)
├── t.DomReady(pipeline => { ... })                  on page load — fires once
├── t.CustomEvent("name", pipeline => { ... })       on named event — fires each time dispatched
└── t.CustomEvent<T>("name", (payload, pipeline) => { ... })
                                                     on named event with typed payload

.Reactive(plan, evt => evt.{EventName}, (args, pipeline) => { ... })
                                                     on component event — wired on the builder
```

## DomReady

Fires once when the page finishes loading. Use it to set initial UI state or dispatch events that kick off other behavior.

```csharp
Html.On(plan, t => t.DomReady(pipeline =>
{
    pipeline.Element("status").SetText("System online");
    pipeline.Dispatch("page-ready");
}));
```

## CustomEvent

Fires when something dispatches the matching event name. Events are dispatched with `pipeline.Dispatch("name")` from any pipeline — this is how different parts of the page communicate.

```csharp
Html.On(plan, t => t.CustomEvent("page-ready", pipeline =>
{
    pipeline.Element("form-section").Show();
}));
```

## CustomEvent with typed payload

When the event carries data, use `CustomEvent<TPayload>`. The `payload` parameter gives you compile-time access to the event's properties — `x => x.Name` becomes `"evt.name"` in the plan, resolved from the actual event data at runtime.

```csharp
Html.On(plan, t => t.CustomEvent<ResidentCreatedPayload>("resident-created", (payload, pipeline) =>
{
    pipeline.Element("name-display").SetText(payload, x => x.Name);
}));
```

Dispatching with data:

```csharp
pipeline.Dispatch("resident-created", new ResidentCreatedPayload
{
    Name = "Jane Doe",
    Facility = "Sunrise Manor"
});
```

## .Reactive() — component events

Components fire their own events — `Changed`, `Click`, `Filtering`. Wire them with `.Reactive()` directly on the component builder. `evt` selects the event, `args` gives you the typed payload.

```csharp
Html.InputField(plan, m => m.Country, o => o.Required().Label("Country"))
    .FusionDropDownList(b => b
        .Fields<CountryItem>(t => t.Text, v => v.Value)
        .Reactive(plan, evt => evt.Changed, (args, pipeline) =>
        {
            pipeline.Element("selected").SetText(args, a => a.Value);
        }));
```

`.Reactive()` is for a specific component's event — the trigger lives with the component. `CustomEvent` is for named events that any part of the page can dispatch or listen to.

## Event chaining

DomReady dispatches → CustomEvent catches → dispatches again. The runtime guarantees all CustomEvent listeners are wired before any DomReady reaction fires — order in the view doesn't matter.

```csharp
Html.On(plan, t => t.DomReady(pipeline =>
{
    pipeline.Element("step-1").SetText("Loaded");
    pipeline.Dispatch("loaded");
}));

Html.On(plan, t => t.CustomEvent("loaded", pipeline =>
{
    pipeline.Element("step-2").SetText("Ready");
}));
```

Next: [Mutations](/csharp-modules/element-mutations/) — what you can do inside a pipeline.
