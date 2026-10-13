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
    p.When(args.Value).Eq("memory")
     .Then(t => t.Element("memory-care-panel").Show())
     .Else(e => e.Element("memory-care-panel").Hide());
})
```

`args` is the object the selected event hands you, typed for that event.
IntelliSense shows its members.

## Target

Use `p.Component<TComponent>(...)` to work with the component later. It returns
the component itself, so hold it in a variable named for its role:

```csharp
var careLevel = p.Component<FusionDropDownList>(m => m.CareLevel);
careLevel.Value.Set("assisted");
careLevel.DataBind();
```

Every member the component's class declares, by the name its JavaScript API
uses, is a member of that variable: a property you write or read, a method you
call. Each write or call is its own statement.

## Read

A property is itself a typed value, and so is a method that returns one.

```csharp
var careLevel = p.Component<FusionDropDownList>(m => m.CareLevel);

p.When(careLevel.Value).Eq("memory")
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
var toast = p.Component<FusionToast>();
toast.Content.Set("Saved");
toast.Tone.Set(FusionToastTone.Success);
toast.Show();
```

Do not learn Alis.Reactive by memorizing every component. Learn the object
model. A new component should still read as event, payload, property, method,
and one the framework does not ship is brought the same way: declare its class,
render it, and its members work like those of any shipped component.
