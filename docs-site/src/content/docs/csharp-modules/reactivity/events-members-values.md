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

Custom events can carry a typed payload. Declare the event once, with its name and payload, so
the page that raises it and the page that hears it cannot disagree on either.

```csharp
public sealed class UnitChangedPayload
{
    public string? UnitId { get; set; }
}

public static class UnitEvents
{
    public static readonly AppEvent<UnitChangedPayload> UnitChanged =
        AppEvent.Named<UnitChangedPayload>("unitChanged");
}
```

```csharp
Html.On(plan, t => t.CustomEvent(UnitEvents.UnitChanged, (payload, p) =>
{
    p.Element("selected-unit").SetText(payload, x => x.UnitId);
}));
```

Pipelines raise the same event.

```csharp
p.DispatchWith(UnitEvents.UnitChanged, d => d
    .Set(x => x.UnitId,
        p.Component<FusionDropDownList>(m => m.UnitId).Get(c => c.Value)));
```

An event with no payload is `AppEvent.Named("name")`, heard with `t.CustomEvent(evt, p => ...)` and
raised with `p.Dispatch(evt)`.

Server-sent events and SignalR hubs are declared the same way: a class lists the messages the
server sends, by the names it sends them under, and the server writes each under that name.

```csharp
public sealed class ResidentStatusFeed : SignalRHub
{
    public SignalRMessage<ResidentStatusPayload> StatusChanged =>
        Message<ResidentStatusPayload>("StatusChanged");
}

Html.On(plan, t => t.SignalR(SignalRHub.At<ResidentStatusFeed>("/hubs/resident-status"),
    h => h.StatusChanged, (payload, p) =>
    {
        p.Element("resident-status").SetText(payload, x => x.Status);
    }));

// On the server:
await residentHub.Clients.All.SendAsync(new ResidentStatusFeed().StatusChanged.Name, payload);
```

A stream is `ServerPushStream` with `Message<T>("event-name")` members, used with
`t.ServerPush(ServerPushStream.At<TStream>(url), s => s.Member, ...)`.

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
     .Gather<MedicationSearchQuery>(g => g.Set(x => x.MedicationType, p.Payload(args).Get(a => a.Text)))
     .Response(r => r.OnSuccess<MedicationSearchResponse>((json, s) =>
         s.Payload(args).Call(a => a.UpdateData, json.Read(j => j.Medications))));
})
```

`p.Payload(args).Get(a => a.Text)` reads a payload member as a value, here the
text typed so far, sent as the request's `MedicationType`.

## Value Sources

The same source shape is reused everywhere.

```csharp
p.When(args, x => x.Value).Eq("north");
p.When(p.Url<HttpShowcaseQuery>().Get(q => q.Tab)).Eq("medications");
p.Element("message").SetText(body, x => x.Message);
```

Use sources for conditions, element text, request members (route values
included), headers, dispatch payloads, and plugin arguments.

## How do I combine conditions?

Chain `And` for conditions that must all hold, or `Or` for conditions of which one must hold. One
chain uses one of them; to mix them, put the inner part in a group, so the order of evaluation is
written where you read it. From the escalation routing page:

```csharp
p.When(assessment.Get(c => c.Value)).Gte(85m)
    .And(cs => cs.When(supervisorOverride.Get(c => c.Checked)).Truthy()
        .Or(careTrack.Get(c => c.Value)).Eq("memory"))
    .Then(then => then.Dispatch(EscalationRoutingEvents.ComponentRouteIsolation))
    .ElseIf(assessment.Get(c => c.Value)).Gte(60m)
    .And(cs => cs.When(supervisorOverride.Get(c => c.Checked)).Truthy().Not())
    .Then(then => then.Dispatch(EscalationRoutingEvents.ComponentRouteMonitor));
```

The first branch reads "85 or more, and either an override or the memory track". `Not()` negates
the one comparison before it. A group has no `Then` of its own, and the compiler offers each
operator only where it means something: `Gt` and `Between` on numbers and dates, `Contains` and
`StartsWith` on text, `ArrayContains` on lists.

## How do I read the page's URL?

Describe the query string with the class the page's action binds with
`[FromQuery]`, and read it through `p.Url<TQuery>()`:

```csharp
public sealed class HttpShowcaseQuery
{
    public string? Tab { get; set; }
    public int FacilityId { get; set; }
    public int Page { get; set; }
    public int ResidentId { get; set; }
    public string[] Wing { get; set; } = Array.Empty<string>();
}
```

```csharp
var link = p.Url<HttpShowcaseQuery>();
p.When(link.Get(q => q.Page)).Gt(1)
 .Then(then => then.Element("url-cond-prev").Show())
 .Else(els => els.Element("url-cond-prev").Hide());

p.Element("url-display-wings").SetText(link.Get(q => q.Wing));
```

Each member reads the key of its name, ignoring case. A single-value member reads
the key's first value; an array member such as `Wing` reads every value of the
key.

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
