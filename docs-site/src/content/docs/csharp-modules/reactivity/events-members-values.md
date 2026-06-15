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
    .NativeCheckBox(b => b.Reactive(plan, evt => evt.Changed, (args, p) =>
    {
        p.When(args, x => x.Checked).Truthy()
         .Then(t => t.Element("due-date").Show())
         .Else(e => e.Element("due-date").Hide());
    })); }
```

`evt => evt.Changed` selects the browser event. `args` is the typed payload for
that event.

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
        p.Component<FusionDropDownList>(m => m.UnitId).Value()));
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

## Component Properties

Property writes use `Set...` methods.

```csharp
p.Component<FusionDropDownList>(m => m.UnitId)
 .SetValue("north")
 .DataBind();
```

The method name is public C#. The plan records the browser property member.

## Component Methods

Method calls use verbs.

```csharp
p.Component<FusionDropDownList>(m => m.UnitId).ShowPopup();
p.Component<FusionDropDownList>(m => m.UnitId).FocusIn();
```

Each component exposes only the calls that its C# slice declares.

## Component Reads

Readable members return typed value sources.

```csharp
var unitId = p.Component<FusionDropDownList>(m => m.UnitId).Value();

p.When(unitId).NotEmpty()
 .Then(t => t.Element("unit-warning").Hide());
```

The runtime reads the browser object when the pipeline runs.

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
 .Arg(p.Component<FusionDropDownList>(m => m.UnitId).Value())
 .Fire();
```

Keep component APIs typed. Keep string names at the plugin boundary.
