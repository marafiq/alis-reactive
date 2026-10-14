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
/// <summary>What the board's change log is told after the planner answers a push.</summary>
public sealed class BoardChange
{
    public string Summary { get; set; } = "";

    public int Booked { get; set; }

    /// <summary>The code of the desk that saw the change.</summary>
    public string Desk { get; set; } = "";

    /// <summary>Whether the change took a time this family had booked.</summary>
    public bool ReleasedHere { get; set; }

    /// <summary>How many of this family's booked times the change took.</summary>
    public int Released { get; set; }
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
    p.Element("board-log-desk").Text.Set(change.Get(x => x.Desk));
    p.Element("board-log-released").Text.Set(change.Get(x => x.Released));
    p.When(change.Get(x => x.ReleasedHere)).Truthy()
        .Then(lost => lost.Element("board-log-lost").Show())
        .Else(kept => kept.Element("board-log-lost").Hide());
}));
```

`change.Get(x => x.Summary)` reads one member of the payload. Pipelines raise
the same event, filling each member from a value. The board raises it when
another desk takes a time, from whichever branch handled it (`branch`);
`taken` is that server message, `planner` the page's visit planner and
`slugify` the page's plugin:

```csharp
void TellTheLog(PipelineBuilder<VisitBoardModel> branch, bool releasedHere, int released) =>
    branch.Dispatch<BoardChange>(VisitBoardEvents.BoardChanged, change => change
        .Set(x => x.Summary, taken.Get(x => x.Message))
        .Set(x => x.Booked, planner.Count)
        .Set(x => x.Desk, slugify.Slug(planner.Wing))
        .Set(x => x.ReleasedHere, releasedHere)
        .Set(x => x.Released, released));
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
named for its role. From the notify physician page, in a request's success
route (`s`, with the response body `onCall`), the on-call physicians are
written to the list's `DataSource`, and its `DataBind()` and `ShowPopup()`
methods are called:

```csharp
var physician = s.Component<FusionAutoComplete>(m => m.Physician);
physician.DataSource.Set(onCall.Get(x => x.Physicians));
physician.DataBind();
physician.ShowPopup();
```

Each write and each call is its own statement, and it runs in the callback
you write it in. A write takes a literal or any value: another component, an
event payload, a response body, a URL parameter. From the admission placement
page, a literal written to the care level's `Text`, and a response body's
member written to the wing's `Value`:

```csharp
p.Component<FusionDropDownList>(m => m.CareLevel).Text.Set("Memory Care");
```

```csharp
wing.Value.Set(offered.Get(x => x.OpenRoomWing));
wing.DataBind();
```

To empty a drop-down list or an autocomplete, call its `Clear()`: writing
empty text to `Value` leaves the old choice in place.

Each component offers exactly the members its class declares.

## Component Reads

A property is itself a value, and so is a method that returns one.

From the care level review page:

```csharp
var careLevel = p.Component<FusionDropDownList>(m => m.CareLevel);
var protocol = p.Component<FusionDropDownList>(m => m.Protocol);

p.When(careLevel.Value).Eq("Memory Care")
    .Then(_ => protocol.Value.Set("Enhanced Monitoring"))
    .ElseIf(careLevel.Value).Eq("Skilled Nursing")
    .Then(_ => protocol.Value.Set("Full Clinical"))
    .Else(_ => protocol.Value.Set("Basic Support"));
```

The value is read in the browser when the step runs.

## Event Payloads

The object an event hands you is used as the object. Its members are values,
a member the component reads back can be written, and its methods can be
called. From the notify physician page, where typing part of a name searches
the physician directory:

```csharp
.Reactive(evt => evt.Filtering, (args, p) =>
{
    // The directory answers for what was typed; until it does, her care team is not offered.
    args.PreventDefaultAction.Set(true);
    p.Get(Url.SandboxRoute("Search", "AutoComplete"))
        .Gather<DirectorySearch>(g => g.Set(x => x.Name, args.Text))
        .WhileLoading(l => l.Element("directory-status").Text.Set("Searching the physician directory..."))
        .Response(r => r.OnSuccess<DirectoryMatches>((found, s) =>
        {
            s.Element("directory-status").Text.Set(found.Get(x => x.Summary));
            // A name nobody in the directory has closes the list the last search left open.
            s.When(found.Get(x => x.Count)).Eq(0)
                .Then(none => none.Component<FusionAutoComplete>(m => m.Physician).HidePopup())
                .Else(some => args.UpdateData(found.Get(x => x.Physicians)));
        }));
})
```

`args.PreventDefaultAction.Set(true)` keeps the list from filtering on its own,
`args.Text` is the text typed so far, sent as the request's `Name`, and
`args.UpdateData(..)` hands the matches to the popup.

## Value Sources

The same values are reused everywhere.

An event payload (bed assignment), the page's URL (resident file, where
`link` is `p.Url<ShiftEmailLinkQuery>()`) and a response body (resident
register):

```csharp
chosen.Element("held-bed").Text.Set(args.Value);
```

```csharp
p.When(link.Get(q => q.Tab)).Eq("medications")
 .Then(medications => medications.Element("file-medications").Show())
 .Else(overview => overview.Element("file-overview").Show());
```

```csharp
s.Element("register-newest").Text.Set(summary.Get(x => x.NewestResident));
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
    .Then(then =>
    {
        then.Dispatch(EscalationRoutingEvents.ComponentRouteIsolation);
    })
    .ElseIf(assessment.Value).Gte(60m)
    .And(cs => cs.When(supervisorOverride.Checked).Truthy().Not())
    .Then(then =>
    {
        then.Dispatch(EscalationRoutingEvents.ComponentRouteMonitor);
    })
    .Else(else_ =>
    {
        else_.Dispatch(EscalationRoutingEvents.ComponentRouteRoutine);
    });
```

The first branch reads "85 or more, and either an override or the memory track". `Not()` negates
the one comparison before it. A group has no `Then` of its own, and the compiler offers each
operator only where it means something: `Gt` and `Between` on numbers and dates, `Contains` and
`StartsWith` on text, `ArrayContains` on lists.

## How do I read the page's URL?

Describe the query string with the class the page's action binds with
`[FromQuery]`, and read it through `p.Url<TQuery>()`. From the resident file
page, opened from a link in the shift email such as
`?community=maple-grove&resident=1042&shift=Night&tab=medications&page=2&wing=Maple&wing=Cedar`:

```csharp
public sealed class ShiftEmailLinkQuery
{
    public string Community { get; set; } = "";

    public int Resident { get; set; }

    public string Shift { get; set; } = "";

    public string Tab { get; set; } = "";

    public int Page { get; set; }

    public string[] Wing { get; set; } = Array.Empty<string>();
}
```

```csharp
var link = p.Url<ShiftEmailLinkQuery>();
p.Element("link-shift").Text.Set(link.Get(q => q.Shift));
p.Element("link-wings").Text.Set(link.Get(q => q.Wing));
p.Element("notes-page").Text.Set(link.Get(q => q.Page));
p.When(link.Get(q => q.Tab)).Eq("medications")
 .Then(medications => medications.Element("file-medications").Show())
 .Else(overview => overview.Element("file-overview").Show());
p.When(link.Get(q => q.Page)).Gt(1)
 .Then(older => older.Element("notes-older-hint").Show());
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
