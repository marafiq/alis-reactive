---
title: Components
description: How native, Fusion, and app-level components fit the same public DSL.
---

Components are browser objects registered in the Reactive Plan.

They all follow the same shape: event, payload, property, method.

## Render

Model-bound components start with `Html.InputField(...)`.

```csharp
@{ Html.InputField(plan, m => m.CareLevel, o => o.Label("Care Level"))
    .FusionDropDownList(b => b.Placeholder("Select care level")); }
```

The model expression gives the component a deterministic ID. The same expression
targets the component later.

A component that is not a form input renders against the plan with an ID you
choose, and the same ID targets it:

```csharp
@(Html.FusionGrid<ResidentDirectoryModel, ResidentDirectoryGridItem>(plan, "resident-directory-grid", b => b
    .AllowSelection(true)
    .Columns(Columns()))
    .Reactive(evt => evt.RowSelected<ResidentDirectoryGridItem>(), (args, p) => { ... }))
```

## React

Component `.Reactive(...)` attaches a typed event to the rendered component.

```csharp
.Reactive(evt => evt.Change, (args, p) =>
{
    p.When(args, x => x.Value).Eq("memory")
     .Then(t => t.Element("memory-care-panel").Show())
     .Else(e => e.Element("memory-care-panel").Hide());
})
```

The payload type belongs to the selected event. IntelliSense shows the payload
members for that event.

## Target

Use `p.Component<TComponent>(...)` to work with the component later.

```csharp
p.Component<FusionDropDownList>(m => m.CareLevel)
 .Set(c => c.Value, "assisted")
 .Call(c => c.DataBind);
```

`Set`, `Call` and `Get` reach every member the component's class declares, by
the name its JavaScript API uses.

## Read

Readable members return typed sources.

```csharp
var careLevel = p.Component<FusionDropDownList>(m => m.CareLevel).Get(c => c.Value);

p.When(careLevel).Eq("memory")
 .Then(t => t.Element("memory-care-panel").Show());
```

The same typed source can feed conditions, gather, dispatch payloads, and plugin
arguments.

## Families

| Family | Use for |
| --- | --- |
| Native | Normal HTML inputs and buttons. |
| Fusion | Syncfusion EJ2 controls with richer browser behavior. |
| App-level | Layout-owned services such as toast, confirm, loader, and drawer. |

App-level components are targeted without a model expression:

```csharp
p.Component<FusionToast>()
 .Set(c => c.Content, "Saved")
 .Set(c => c.Tone, FusionToastTone.Success)
 .Call(c => c.Show);
```

Do not learn Alis.Reactive by memorizing every component. Learn the object
model. A new component should still read as event, payload, property, method,
and one the framework does not ship is brought the same way: declare its class,
render it, and every verb works with it.
