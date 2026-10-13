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
        p.When(args.Checked).Truthy()
            .Then(t => t.Element("due-date-section").Show())
            .Else(e => e.Element("due-date-section").Hide());
    }); }
```

`evt => evt.Change` selects the component's `change` event. `args` is the
object that event hands you, typed: `args.Checked` is its `checked` value, read
in the browser when the event fires.

## Page and Document Events

Use `Html.On(...)` for triggers that are not attached to one rendered component.

```csharp
@{
    Html.On(plan, t => t.DomReady(p =>
    {
        p.Element("status").Text.Set("Ready");
    }));
}
```

Custom events can carry a typed payload. Declare the event once, with its name and payload, so
the page that raises it and the page that hears it cannot disagree on either. From the visit
board page:

```csharp
public sealed class BoardChange
{
    public string Summary { get; set; } = "";

    public int Booked { get; set; }
}

public static class VisitBoardEvents
{
    public static readonly AppEvent<BoardChange> BoardChanged = AppEvent.Named<BoardChange>("board-changed");
}
```

```csharp
Html.On(plan, t => t.CustomEvent<BoardChange>(VisitBoardEvents.BoardChanged, (change, p) =>
{
    p.Element("board-log").Text.Set(change.Get(x => x.Summary));
    p.Element("board-log-booked").Text.Set(change.Get(x => x.Booked));
}));
```

`change.Get(x => x.Summary)` reads one member of the payload. Pipelines raise
the same event, filling each member from a value. The board raises it when
another desk takes a time; `taken` is that server message and `planner` the
page's visit planner:

```csharp
p.Dispatch<BoardChange>(VisitBoardEvents.BoardChanged, change => change
    .Set(x => x.Summary, taken.Get(x => x.Message))
    .Set(x => x.Booked, planner.Count));
```

An event with no payload is `AppEvent.Named("name")`, heard with `t.CustomEvent(evt, p => ...)` and
raised with `p.Dispatch(evt)`.

Server-sent events and SignalR hubs are declared the same way: a class lists the messages the
server sends, by the names it sends them under, and the server writes each under that name.

```csharp
public sealed class ResidentStatusFeed : SignalRHub
{
    public SignalRMessage<ResidentStatusPayload> StatusChanged => Message<ResidentStatusPayload>("StatusChanged");
}

Html.On(plan, t => t.SignalR(SignalRHub.At<ResidentStatusFeed>("/hubs/resident-status"),
    h => h.StatusChanged, (payload, p) =>
    {
        p.Element("resident-status").Text.Set(payload.Get(x => x.Status));
    }));

// On the server:
await residentHub.Clients.All.SendAsync(new ResidentStatusFeed().StatusChanged.Name, payload);
```

A stream is `ServerPushStream` with `Message<T>("event-name")` members, used with
`t.ServerPush(ServerPushStream.At<TStream>(url), s => s.Member, ...)`.

## Element Members

`p.Element("id")` is a normal DOM element, found by its ID.

```csharp
p.Element("summary").Text.Set("Saved");
p.Element("details").Show();
p.Element("banner").ClassList.Add("is-active");
```

The element API is intentionally small: `Text`, `Html`, `ClassList` (`Add`,
`Remove`, `Toggle`), `Show()` and `Hide()`. Text and HTML take a literal or
any value, shown as it arrives.

## Components

A component is a JavaScript object. Its C# class declares the properties,
methods and events its own API has, and you use them as members of the object.
`p.Component<T>(..)` returns the component itself, so hold it in a variable
named for its role. From the cascading page, in a request's success route
(`s`, with the response body `json`):

```csharp
var city = s.Component<FusionDropDownList>(m => m.City);
city.DataSource.Set(json.Get(j => j.Cities));   // write a property
city.DataBind();                                // call a method
```

Each write and each call is its own statement, and it runs in the callback
you write it in. A write takes a literal or any value: another component, an
event payload, a response body, a URL parameter.

```csharp
p.Component<FusionDropDownList>(m => m.Category).Value.Set("Books");
p.Component<FusionDropDownList>(m => m.Category).ShowPopup();
```

Each component offers exactly the members its class declares.

## Component Reads

A property is itself a value, and so is a method that returns one.

```csharp
var careLevel = p.Component<FusionDropDownList>(m => m.CareLevel);

p.Element("s1-current-level").Text.Set(careLevel.Value);
p.When(careLevel.Value).Eq("Memory Care")
    .Then(then => then.Component<FusionDropDownList>(m => m.Protocol).Value.Set("Enhanced Monitoring"));
```

The value is read in the browser when the step runs.

## Event Payloads

The object an event hands you is used as the object. Its members are values,
a member the component reads back can be written, and its methods can be
called:

```csharp
.Reactive(evt => evt.Filtering, (args, p) =>
{
    args.PreventDefaultAction.Set(true);
    p.Get(Url.SandboxRoute("Medications", "AutoComplete"))
     .Gather<MedicationSearchQuery>(g => g.Set(x => x.MedicationType, args.Text))
     .Response(r => r
        .OnSuccess<MedicationSearchResponse>((json, s) =>
        {
            args.UpdateData(json.Get(j => j.Medications));
            s.Element("filter-status").Text.Set("results loaded");
        }));
})
```

`args.Text` is the text typed so far, sent as the request's `MedicationType`.
`args.UpdateData(..)` hands the results to the popup.

## Value Sources

The same values are reused everywhere.

```csharp
p.Element("change-value").Text.Set(args.Value);
p.When(p.Url<HttpShowcaseQuery>().Get(q => q.Tab)).Eq("medications")
 .Then(then => then.Element("url-cond-meds").Show());
s.Element("load-first").Text.Set(json.Get(x => x.First));
```

Use them for conditions, element text, request members (route values
included), headers, dispatch payloads, and plugin arguments.

## How do I combine conditions?

Chain `And` for conditions that must all hold, or `Or` for conditions of which one must hold. One
chain uses one of them; to mix them, put the inner part in a group, so the order of evaluation is
written where you read it. From the escalation routing page:

```csharp
p.When(assessment.Value).Gte(85m)
    .And(cs => cs.When(supervisorOverride.Checked).Truthy()
        .Or(careTrack.Value).Eq("memory"))
    .Then(then => then.Dispatch(EscalationRoutingEvents.ComponentRouteIsolation))
    .ElseIf(assessment.Value).Gte(60m)
    .And(cs => cs.When(supervisorOverride.Checked).Truthy().Not())
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

p.Element("url-display-wings").Text.Set(link.Get(q => q.Wing));
```

Each member reads the key of its name, ignoring case. A single-value member reads
the key's first value; an array member such as `Wing` reads every value of the
key.

## Plugins

Plugins are the boundary for browser APIs the DSL does not model. A plugin is declared once as a
class, like a component: its name, and each member it offers with its types. From the visit board
page (`VisitBoardModel.cs` and `VisitBoard.cshtml`):

```csharp
public sealed class VisitBoardSlugPlugin : Plugin
{
    public VisitBoardSlugPlugin()
        : base("slugify")
    {
    }

    /// <summary>The plugin is itself the function: it takes text and returns its slug.</summary>
    public TypedSource<string> Slug(Arg<string> text) => Invoke<string>(text, name: "");
}
```

```csharp
var slugify = plan.RegisterPlugin<VisitBoardSlugPlugin>();

TypedSource<string> boardCode = slugify.Slug(planner.Wing);
s.Element("board-code").Text.Set(boardCode);
```

A plugin declares its members the way a component does: `Invoke<T>(..)` for a member that returns
a value (`name: ""` when the plugin is itself the function), `ReadOnly<T>()` for one that is read,
and `Call(..)` for one that is called for its effect. The plugin's name is its only string; every
use is typed.
