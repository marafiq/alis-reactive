---
title: Events, Payloads, Members
description: How reactive events, typed payloads, component properties, and component methods work.
---

This is the core mental model for daily use.

A component event starts a pipeline. The event gives you a typed payload. The
pipeline reads values and writes or calls members.

## Component Events

Use `.Reactive(...)` on a rendered component.

```csharp
@{ Html.InputField(plan, m => m.IsUrgent, o => o.Label("Urgent"))
    .NativeCheckBox(b => b.CssClass("h-4 w-4 rounded border-border text-accent"))
    .Reactive(evt => evt.Change, (args, p) =>
    {
        p.When(args, x => x.Checked).Truthy()
         .Then(t => t.Element("due-date").Show())
         .Else(e => e.Element("due-date").Hide());
    }); }
```

`evt => evt.Change` selects the component's `change` event. `args` is the typed
payload for that event.

The runtime reads the browser payload. The C# placeholder only records the path.

## Page and Document Events

Use `Html.On(...)` for triggers that are not attached to one rendered component.

```csharp
@{
    Html.On(plan, t => t.DomReady(p =>
    {
        p.Element("status").SetText("Ready");
    }));
}
```

Custom events can carry a typed payload.

```csharp
public sealed class UnitChangedPayload
{
    public string? UnitId { get; set; }
}
```

```csharp
Html.On(plan, t => t.CustomEvent<UnitChangedPayload>(
    "unitChanged",
    (payload, p) =>
    {
        p.Element("selected-unit").SetText(payload, x => x.UnitId);
    }));
```

Pipelines can dispatch the same event.

```csharp
p.DispatchWith<UnitChangedPayload>("unitChanged", d => d
    .Set(x => x.UnitId,
        p.Component<FusionDropDownList>(m => m.UnitId).Get(c => c.Value)));
```

## Element Members

`p.Element("id")` targets a normal DOM element by ID.

```csharp
p.Element("summary").SetText("Saved");
p.Element("details").Show();
p.Element("banner").AddClass("is-active");
```

The element API is intentionally small: text, HTML, class changes, and hidden
state.

## Components

A component is a JavaScript object. Its C# class declares the properties,
methods and events its own API has, by their JavaScript names. Three verbs
touch any declared member, and `c` is the declaration:

```csharp
var unit = p.Component<FusionDropDownList>(m => m.UnitId);

unit.Set(c => c.Value, "north")      // write a property
    .Call(c => c.DataBind);          // call a method

unit.Call(c => c.ShowPopup);
unit.Call(c => c.FocusIn);
```

A write takes a literal or any value source: another component, an event
payload, a response body, a URL parameter.

```csharp
s.Component<FusionDropDownList>(m => m.City)
    .Set(c => c.DataSource, json.Read(j => j.Cities))
    .Call(c => c.DataBind);
```

Each component offers exactly the members its class declares.

## Component Reads

`Get` reads a property, or what a method returns, as a typed value source.

```csharp
var unitId = p.Component<FusionDropDownList>(m => m.UnitId).Get(c => c.Value);

p.When(unitId).NotEmpty()
 .Then(t => t.Element("unit-warning").Hide());
```

The runtime reads the browser object when the pipeline runs.

## Event Payloads

Payload fields are plain C# properties. When the payload also has methods, its
class declares them, and `p.Payload(args)` writes, calls and reads them:

```csharp
.Reactive(evt => evt.Filtering, (args, p) =>
{
    p.Payload(args).Set(a => a.PreventDefaultAction, true);
    p.Get(Url.SandboxRoute("Medications", "AutoComplete"))
     .Gather(g => g.FromEvent(args, x => x.Text, "MedicationType"))
     .Response(r => r.OnSuccess<MedicationSearchResponse>((json, s) =>
         s.Payload(args).Call(a => a.UpdateData, json.Read(j => j.Medications))));
})
```

## Value Sources

The same source shape is reused everywhere.

```csharp
p.When(args, x => x.Value).Eq("north");
p.When(p.FromUrl<string>("mode")).Eq("edit");
p.Element("message").SetText(body, x => x.Message);
```

Use sources for conditions, element text, gather fields, route parameters,
headers, dispatch payloads, and plugin arguments.

## Plugins

Plugins are the boundary for browser APIs the DSL does not model.

```csharp
plan.RegisterPlugin("clipboard", plugin =>
{
    plugin.Command("writeText", args => args.Arg<string>());
});

p.Plugin("clipboard", "writeText")
 .Arg(p.Component<FusionDropDownList>(m => m.UnitId).Get(c => c.Value))
 .Fire();
```

Keep component APIs typed. Keep string names at the plugin boundary.
