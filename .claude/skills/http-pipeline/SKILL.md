---
name: http-pipeline
description: Use when building HTTP requests in the reactive plan — Get/Post/Put/Delete, Gather (the typed request the endpoint binds - Set, Form, Header, route members), the URL query (p.Url<TQuery>()) and event payload (args.Member) sources, Response (OnSuccess/OnError), Chained requests, Parallel requests, WhileLoading, Finally, and Validate. The HTTP pipeline grammar for Alis.Reactive.
---

# HTTP Pipeline Grammar

Source of truth: `Alis.Reactive/PlanAuthoring/Requests/` (`HttpRequestBuilder.cs`, `GatherBuilder.cs`,
`GatherInputDraft.cs`, `RequestRouteTemplate.cs`, `RequestMembers.cs`, `ResponseBuilder.cs`),
`Alis.Reactive/PlanAuthoring/Pipelines/` (`PipelineBuilder.Http.cs`, `UrlQuery.cs`) and
`Alis.Reactive/PlanAuthoring/Events/Data.cs`.
Every example below is copied from a sandbox view under `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/`.

## Entry Points

```
p.VERB(url).CHAIN

VERB :=
  | Get(url)
  | Post(url)
  | Put(url)
  | Delete(url)
```

`url` may hold `{placeholder}` segments; the gathered request fills them (see "Where a member goes").

## Request Chain

```
p.VERB(url)
  [.Gather(g => { ... })]              -- optional: the request is the view model
  [.Gather<TRequest>(g => { ... })]    -- optional: the request is TRequest, the class the endpoint binds
  [.AsFormData()]                      -- optional: multipart form data (default: JSON)
  [.Validate<TValidator>("formId")]    -- optional: client validation before the request
  [.WhileLoading(l => { ... })]        -- optional: commands during flight
  .Response(r => { ... })              -- response handlers
  [.Finally(f => { ... })]             -- optional: after success, error or network failure
```

All chain methods return `HttpRequestBuilder<TModel>`. A request without `Gather` sends no values.

## Gather: the Typed Request

Gather builds the request the endpoint binds, member by member. `TRequest` is that class, so every
parameter name comes from a typed member; there are no string parameter names.

### Choose `TRequest`

| The action binds | Write | Sandbox source |
|------------------|-------|----------------|
| the view model, `Reserve([FromBody] SuiteAssignmentModel assignment)` | `.Gather(g => g.Form())` | `Components/Brought/TomSelect/Index.cshtml:85` |
| a query class, `Search([FromQuery] DirectorySearch search)` | `.Gather<DirectorySearch>(g => ..)` | `Components/Fusion/AutoComplete/Index.cshtml:67` |
| a route value, `Rate(string suiteType)` | `.Gather<SuiteQuery>(g => g.Set(x => x.SuiteType, ..))` | `Components/Brought/TomSelect/Index.cshtml:44` |
| a body class, `AlertHypertension([FromBody] AlertHypertensionRequest? request)` | `.Gather<AlertHypertensionRequest>(g => ..)` | `Conditions/AdmissionAssessment/_CardiacAssessment.cshtml:29` |
| route values and a JSON body (ASP.NET Core), `HoldVisit(string slotId, [FromBody] HoldRequest? hold)` | `.Gather<HoldVisitRequest>(g => ..)` where `HoldVisitRequest : HoldRequest { SlotId }` | `Components/Brought/VisitPlanner/FrontDesk.cshtml:91` |

In ASP.NET Core an action that binds a JSON body takes its route values as their own parameters.
The page's request class is then the body class plus the route members:

```csharp
// Models/Components/Brought/VisitPlanner/FrontDeskHoldsModel.cs:70-84
/// <summary>The body of a hold or a badge, which the desk's actions bind with [FromBody].</summary>
public class HoldRequest
{
    public int VisitsThisWeek { get; set; }

    public string? Family { get; set; }

    public string? Source { get; set; }
}

/// <summary>A hold or a badge as the page sends it: the time in the path, the rest in the body.</summary>
public sealed class HoldVisitRequest : HoldRequest
{
    public string? SlotId { get; set; }
}
```

### Grammar

```
.Gather(g => g.GATHER_ITEM.GATHER_ITEM ...)            -- TRequest = the view model
.Gather<TRequest>(g => g.GATHER_ITEM.GATHER_ITEM ...)

GATHER_ITEM :=
  | Set(x => x.Member, source)         -- a value read when the request is sent
  | Set(x => x.Member, literal)        -- a value fixed when the view renders
  | Form()                             -- every input of the form, each by its binding name
  | Form(m => m.A, m => m.B)           -- the named inputs of the form
  | Header("X-Name", literal)
  | Header("X-Name", source)

source :=
  | p.Component<T>(m => m.X).Member                 -- a component property, or a method that returns: .Member(..)
  | args.Member                                     -- the triggering event's payload
  | p.Url<TQuery>().Get(q => q.Member)              -- the page's URL query string
  | json.Get(x => x.Member)                         -- a success body, in a request nested in OnSuccess
  | plugin.Member(..)                               -- a registered plugin's member
  | an array value                                  -- e.g. p.From(planner.Slots).Count(..)
```

`x` walks `TRequest`; `m` walks the view model. Nested members work on both sides:
`Set(x => x.Pharmacy.Network, ..)`, `Form(m => m.Db.Name)`.

`Form()` sends the inputs this view and the partials composed with it rendered through
`Html.InputField`, while they are on the page. `Form(m => ..)` sends only the named ones; inside
`Gather<TRequest>` each named input must also be a member of `TRequest`.

### Where a member goes

MVC's route-value rule, applied when the view renders:

1. A top-level member whose name matches a `{placeholder}` in the URL, ignoring case, fills the path.
   The value must be a scalar.
2. Every other member goes to the query string for GET, and to the body for POST, PUT and DELETE:
   JSON by default, form data after `.AsFormData()`.
3. `Header(..)` goes to the HTTP headers.

The key on the wire is the member's binding name (`SlotId`, `Pharmacy.Network`); MVC binding ignores
case. `Form()` sends each input under its binding name too, and a member set with `Set` is sent beside
the inputs; an input bound to the same name wins.

### Checks

Thrown when the view renders:

- A `{placeholder}` that no member fills: `URL template '..' has placeholder '{residentId}', but the
  gathered request sets no member named 'residentId'.`
- A member set twice (names compared ignoring case): `The request member 'X' is gathered twice; set
  each member once.`
- `Form(m => m.X)` inside `Gather<TRequest>` when `TRequest` has no member `X`, or its `X` has a type the
  input's value does not bind to.
- A route member whose value is not a scalar.

Refused by the compiler: a source of `int?` cannot fill an `int` member, because a missing value
cannot fill a member that may not be missing. A `T[]` source fills a `T[]`, `List<T>` or
`IEnumerable<T>` member. A nullable member is left empty with a typed null,
`Set(x => x.FacilityId, (int?)null)` (from the `GatherBuilder.Set` XML doc; no sandbox page uses it yet).

### Examples

Route value from a brought component (`Components/Brought/TomSelect/Index.cshtml:15, 41-44`):

```csharp
var suite = p.Component<TomSelect>(m => m.SuiteType);
quote.Get(rateUrl)   // rateUrl = Url.SandboxRouteTemplate("Rate", "TomSelect", "suiteType")
    .Gather<SuiteQuery>(g => g.Set(x => x.SuiteType, suite.Value))
```

Route value, body members and a header in one request (`Components/Brought/VisitPlanner/FrontDesk.cshtml:90-96`;
`holdUrl` is `.../FrontDesk/Hold/{slotId}`):

```csharp
added.Post(holdUrl)
    .Gather<HoldVisitRequest>(g => g
        .Set(x => x.SlotId, args.Added)
        .Set(x => x.VisitsThisWeek, added.Component<VisitPlanner>(m => m.VisitSlots).Count)
        .Set(x => x.Family, p.Url<FrontDeskQuery>().Get(q => q.Family))
        .Set(x => x.Source, "front-desk")
        .Header("X-Wing", added.Component<VisitPlanner>(m => m.VisitSlots).Wing))
```

Every input of the form, plus one member the form does not carry (`Validation/MoveInReview/Index.cshtml:63`):

```csharp
p.Post(Url.SandboxRoute("Save", "MoveInReview")).Gather(g => g.Form().Set(x => x.Pharmacy.Network, "Riverside Health"))
```

Named inputs into a request class other than the view model (`Conditions/AdmissionAssessment/_CardiacAssessment.cshtml:28-29`):

```csharp
then.Post(alertHypertensionUrl)
 .Gather<AlertHypertensionRequest>(g => g.Form(m => m.SystolicBP, m => m.ResidentName))
```

Literal members, fixed when the view renders (`Components/Fusion/Grid/Census.cshtml:25-29`; `PageSize` is
the view's `const int`):

```csharp
p.Post(Url.SandboxRoute("CensusResidents", "Grid"))
    .Gather<AgeCensusQuery>(g => g
        .Set(x => x.Skip, 0)
        .Set(x => x.Take, PageSize)
        .Set(x => x.RequiresCounts, true))
```

Headers beside a route member, one header a literal and one read from the URL
(`HttpPipeline/ShiftEmailLink/Index.cshtml:131-135`; `partnersUrl` is `.../Residents/{residentId}/Partners`):

```csharp
pharmacy => pharmacy.Get(partnersUrl)
    .Gather<ShiftEmailPartnerRequest>(g => g
        .Set(x => x.ResidentId, link.Get(q => q.Resident))
        .Header("X-Partner", "riverside-pharmacy")
        .Header("X-Community", link.Get(q => q.Community)))
```

Form data, for files (`Components/Fusion/FileUpload/Index.cshtml:68-70`):

```csharp
p.Post(Url.SandboxRoute("Attach", "FileUpload"))
    .Gather(g => g.Form())
    .AsFormData()
```

## The Page's URL Query: `p.Url<TQuery>()`

`TQuery` is the class the page's action binds with `[FromQuery]`. A member reads the query key of its
binding name, matched ignoring case. A scalar member reads the key's first value; an array member reads
every value of the key. The result is a source like any other: conditions, `Text.Set`, `Set`, `Header`.

```csharp
// Models/HttpPipeline/ShiftEmailLink/ShiftEmailLinkModel.cs:17-30
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
// HttpPipeline/ShiftEmailLink/Index.cshtml:18-26
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

`link.Get(q => q.Wing)` reads every `?wing=` value of the link.

URL members fill a route value, a header and a query member of another request
(`HttpPipeline/ShiftEmailLink/Index.cshtml:33-38`; `chartUrl` is `.../Residents/{residentId}/Chart`):

```csharp
var link = p.Url<ShiftEmailLinkQuery>();
p.Get(chartUrl)
 .Gather<ShiftEmailChartRequest>(g => g
     .Set(x => x.ResidentId, link.Get(q => q.Resident))
     .Header("X-Community", link.Get(q => q.Community))
     .Set(x => x.Shift, link.Get(q => q.Shift)))
```

## Event Payload Members: `args.Member`

`args` is the object the event handed its callback, declared as a `JsObject`. Each of its members is a
source; a member whose value is data reads further with `Get` (`Components/Fusion/Grid/Census.cshtml:70-83`):

```csharp
.Reactive(evt => evt.DataStateChange, (args, p) =>
    p.Post(Url.SandboxRoute("CensusResidents", "Grid"))
        .Gather<AgeCensusQuery>(g => g
            .Set(x => x.Skip, args.Skip)
            .Set(x => x.Take, args.Take)
            .Set(x => x.RequiresCounts, args.RequiresCounts)
            .Set(x => x.Sorted, args.Sorted)
            .Set(x => x.Change, args.Action.Get(a => a.RequestType))
            .Set(x => x.SortedColumn, args.Action.Get(a => a.ColumnName))
            .Set(x => x.SortedDirection, args.Action.Get(a => a.Direction))
            .Set(x => x.ToPage, args.Action.Get(a => a.CurrentPage))
            .Set(x => x.FromPage, args.Action.Get(a => a.PreviousPage))
            .Set(x => x.PageSize, args.Action.Get(a => a.PageSize))
            .Form(m => m.MinAge))
```

## Chaining From a Response

A request nested in `OnSuccess` sets members from that response (`HttpPipeline/ShiftEmailLink/Index.cshtml:40-54`;
`lastDoseUrl` is `.../Residents/{residentId}/Doses/{medication}`, one route value from the page's URL and
one from the chart just answered):

```csharp
.OnSuccess<ShiftEmailChart>((chart, s) =>
{
    s.Element("file-resident").Text.Set(chart.Get(x => x.Name));
    s.Element("file-room").Text.Set(chart.Get(x => x.Room));
    s.Element("file-community").Text.Set(chart.Get(x => x.Community));
    s.Element("due-dose").Text.Set(chart.Get(x => x.DueDose));
    var sameLink = s.Url<ShiftEmailLinkQuery>();
    s.Get(lastDoseUrl)
     .Gather<ShiftEmailDoseRequest>(g => g
         .Set(x => x.ResidentId, sameLink.Get(q => q.Resident))
         .Set(x => x.Medication, chart.Get(x => x.DueMedication))
         .Header("X-Community", sameLink.Get(q => q.Community)))
     .Response(dose => dose.OnSuccess<ShiftEmailLastDose>((given, d) =>
         d.Element("last-dose").Text.Set(given.Get(x => x.LastGiven))));
})
```

## Response Grammar

```
.Response(r => {
    r.OnSuccess(HANDLER);                    -- untyped (any 2xx)
    r.OnSuccess<TResponse>(TYPED_HANDLER);   -- typed JSON response
    r.OnError(STATUS, HANDLER);              -- status-specific error
    r.OnError<TError>(STATUS, TYPED_HANDLER); -- status-specific error with a typed body
    r.Chained(CHAINED_REQUEST);              -- sequential follow-up
})

HANDLER := (s => { PIPELINE_COMMANDS })

TYPED_HANDLER := ((json, s) => {
    s.Element("x").Text.Set(json.Get(x => x.Property));
    s.Component<FusionDropDownList>(m => m.Room).DataSource.Set(json.Get(j => j.Rooms));
})

CHAINED_REQUEST := (c => c.VERB(url).CHAIN)     -- full request chain
```

**Response pipeline `s`** is a full `PipelineBuilder` — supports Element, Component, Dispatch, When, nested HTTP, etc.

## Typed Response (`Data<T>`)

`json` is `Data<TResponse>`: `json.Get(x => x.Member)` reads a member, and `json` itself is the whole
body (`HttpPipeline/ResidentRegister/Index.cshtml:22-28`; `Components/Fusion/Grid/Census.cshtml:16-22`, where
the grid's data source is the whole `{result, count}` body):

```csharp
.Response(r => r.OnSuccess<RegisterSummary>((summary, s) =>
{
    s.Element("register-loading").Hide();
    s.Element("register-record-count").Text.Set(summary.Get(x => x.Records));
    s.Element("register-resident-count").Text.Set(summary.Get(x => x.Residents));
    s.Element("register-newest").Text.Set(summary.Get(x => x.NewestResident));
}))

// The residents the server answers with, how many the census counts, and what the last change did.
void ShowCensus(Data<AgeCensusPage> answer, PipelineBuilder<AgeCensusViewModel> s)
{
    s.Component<FusionGrid>(GridId).DataSource.Set(answer);
    s.Element("census-summary").Text.Set(answer.Get(x => x.Summary));
    s.Element("census-change").Text.Set(answer.Get(x => x.Change));
}
```

A member reads by its camel-case JSON name. `TResponse` must be a class (a record class works).
A typed handler can be a local function of the view, passed as `.OnSuccess<AgeCensusPage>(ShowCensus)`.

## Error Handling

One `OnError` handler per status code, typed or not (`HttpPipeline/ResidentRegister/Index.cshtml:86-102`):

```csharp
.Response(r => r
   .OnSuccess<RegisterRoomMoved>((moved, s) =>
   {
       s.Element("move-status").Text.Set(moved.Get(x => x.Message));
       s.Element("move-status").ClassList.Remove("text-danger");
       s.Dispatch(ResidentRegisterEvents.RegisterChanged);
   })
   .OnError(422, e =>
   {
       e.Element("move-status").Text.Set("Choose a resident and the room they move to.");
       e.Element("move-status").ClassList.Add("text-danger");
   })
   .OnError<RegisterRoomTaken>(409, (taken, e) =>
   {
       e.Element("move-status").Text.Set(taken.Get(x => x.Reason));
       e.Element("move-status").ClassList.Add("text-danger");
   }));
```

`ValidationErrors("formId")` maps a server 400 response to field-level error display
(`Components/Fusion/AutoComplete/Index.cshtml:134`: `.OnError(400, e => e.ValidationErrors(FormId))`).
A typed error body reads like a success body (`Components/Brought/VisitPlanner/FrontDesk.cshtml:115-124`):

```csharp
.OnError<DeskConflict>(409, (conflict, e) =>
{
    e.Component<VisitPlanner>(m => m.VisitSlots).Value.Set(conflict.Get(x => x.Held));
    e.Element("desk-status").Text.Set(conflict.Get(x => x.Message));
    var toast = e.Component<FusionToast>();
    toast.Title.Set("Time not held");
    toast.Content.Set(conflict.Get(x => x.Message));
    toast.Tone.Set(FusionToastTone.Danger);
    toast.Show();
})
```

## WhileLoading

From `Components/AppLevel/Loader/Index.cshtml:26-37`:

```csharp
p.Get(Url.SandboxRoute("Census", "Loader"))
 .WhileLoading(l =>
 {
     var loader = l.Component<NativeLoader>();
     loader.Timeout.Set(3000);
     loader.Show();
 })
 .Response(r => r.OnSuccess<CensusSummary>((json, s) =>
 {
     s.Component<NativeLoader>().Hide();
     s.Element("census-status").Text.Set(json.Get(x => x.Summary));
 }));
```

Commands execute before the HTTP request fires. **Sequential commands only** — no conditions, HTTP, or parallel inside WhileLoading.

## Chained Requests

From `HttpPipeline/DischargeNotice/Index.cshtml:31-49` (a discharge the office refuses notifies no one):

```csharp
p.Post(Url.SandboxRoute("Discharge", "DischargeNotice", new { resident = resident.Key }))
 .Response(r => r
    .OnSuccess<DischargeResult>((json, s) =>
    {
        s.Element($"{resident.Key}-discharge-status").Text.Set(json.Get(x => x.Message));
    })
    .OnError<DischargeResult>(409, (refused, e) =>
    {
        e.Element($"{resident.Key}-discharge-status").Text.Set(refused.Get(x => x.Message));
    })
    .Chained(c => c
        .Post(Url.SandboxRoute("NotifyFamily", "DischargeNotice"))
        .Gather<FamilyNoticeRequest>(g => g.Set(x => x.Channel, resident.Channel))
        .Response(notice => notice
            .OnError<FamilyNoticeResult>(503, (json, e) =>
            {
                e.Element($"{resident.Key}-family-reason").Text.Set(json.Get(x => x.Message));
                e.Element($"{resident.Key}-family-status").Text.Set("The family was not notified.");
            }))));
```

Chained fires **only on 2xx success** of the parent request. Can nest further.

To send what the first answer said, take its body as the callback's first parameter
(`Components/Brought/VisitPlanner/FrontDesk.cshtml:52-63`):

```csharp
.Chained<ResidentWeekResponse>((week, holds) => holds.Get(holdsUrl)
    .Gather<FrontDeskResident>(g => g
        .Set(x => x.ResidentId, link.Get(q => q.Resident))
        .Header("X-Family", link.Get(q => q.Family))
        .Header("X-Wing", week.Get(x => x.Wing)))
    .Response(answer => answer.OnSuccess<FamilyHoldsResponse>((held, s) =>
    {
        planner.Value.Set(held.Get(x => x.Held));
        s.Element("desk-family").Text.Set(held.Get(x => x.Family));
        s.Element("desk-name").Text.Set(held.Get(x => x.Desk));
        s.Element("desk-held").Text.Set(planner.Count);
    })))
```

## Parallel Requests

From `HttpPipeline/ShiftEmailLink/Index.cshtml:128-145` (each branch is routed by its own `X-Partner` header):

```csharp
var link = p.Url<ShiftEmailLinkQuery>();
p.Element("partners-status").Text.Set("Checking with the pharmacy and the lab...");
p.Parallel(
        pharmacy => pharmacy.Get(partnersUrl)
            .Gather<ShiftEmailPartnerRequest>(g => g
                .Set(x => x.ResidentId, link.Get(q => q.Resident))
                .Header("X-Partner", "riverside-pharmacy")
                .Header("X-Community", link.Get(q => q.Community)))
            .Response(r => r.OnSuccess<ShiftEmailPartnerAnswer>((answer, s) =>
                s.Element("pharmacy-answer").Text.Set(answer.Get(x => x.Answer)))),
        lab => lab.Get(partnersUrl)
            .Gather<ShiftEmailPartnerRequest>(g => g
                .Set(x => x.ResidentId, link.Get(q => q.Resident))
                .Header("X-Partner", "northgate-lab")
                .Header("X-Community", link.Get(q => q.Community)))
            .Response(r => r.OnSuccess<ShiftEmailPartnerAnswer>((answer, s) =>
                s.Element("lab-answer").Text.Set(answer.Get(x => x.Answer)))))
    .OnAllSettled(done => done.Element("partners-status").Text.Set("Pharmacy and lab checked."));
```

All branches fire concurrently. `OnAllSettled` fires after ALL complete (success or failure).
**Sequential commands only** in OnAllSettled — no conditions, HTTP, or parallel.

## Complete Patterns

**Pattern 1: Server-filtered list (Filtering event)**
Source: `Components/Fusion/AutoComplete/Index.cshtml:62-77`; the action binds `[FromQuery] DirectorySearch`.
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

**Pattern 2: Form POST with validation**
Source: `Patterns/Todo/Index.cshtml:44-55`; the action binds `[FromBody] TodoModel`, the view model.
```csharp
p.Post(Url.SandboxRoute("Save", "Todo")).Gather(g => g.Form())
 .Validate<TodoValidator>("todo-form")
 .Response(r => r
    .OnSuccess(s =>
    {
        var toast = s.Component<FusionToast>();
        toast.Title.Set("Todo");
        toast.Content.Set("Todo saved successfully");
        toast.Tone.Set(FusionToastTone.Success);
        toast.Show();
    })
    .OnError(400, e => e.ValidationErrors("todo-form")));
```

**Pattern 3: Cascade (parent change populates child)**
Source: `Patterns/BedAssignment/Index.cshtml:24-37`; the action binds `[FromQuery] WingRoomsQuery`
(`WingRoomsQuery { string Wing }`).
```csharp
@{ Html.InputField(plan, m => m.Wing, o => o.Label("Wing"))
    .FusionDropDownList(b => b
        .DataSource(Model.Wings)
        .Fields<BedWing>(w => w.Name, w => w.Code)
        .Placeholder("Choose a wing"))
    .Reactive(evt => evt.Change, (args, p) =>
        p.Get(roomsUrl)
            .Gather<WingRoomsQuery>(g => g.Form(m => m.Wing))
            .Response(r => r.OnSuccess<WingRooms>((offered, s) =>
            {
                // The new wing's rooms replace the old; a room chosen before is cleared, and its beds with it.
                s.Component<FusionDropDownList>(m => m.Room).DataSource.Set(offered.Get(x => x.Rooms));
                s.Element("room-summary").Text.Set(offered.Get(x => x.Summary));
            }))); }
```

## Key Rules

- **Gather names the class the endpoint binds** — `Gather<TRequest>`, or `Gather(..)` when the action binds the view model; no string parameter names
- **A `{placeholder}` is filled by the member of its name** — every placeholder must be filled; the rest goes to the query (GET) or the body
- **Form() for the form's inputs** — `Form()` sends every input, `Form(m => ..)` the named ones, each by its binding name
- **The payload for filtering** — `Set(x => x.M, args.Text)` sends the text typed during the keystroke
- **updateData for filtering** — the ONLY correct SF API for async filtered data
- **Call DataBind after setting DataSource when a later step in the same pipeline depends on the new items** — Admission Placement writes the wing list, then `Value`, calls `DataBind()`, then reads `Text` (`Components/Fusion/DropDownList/Index.cshtml:62-69`); Bed Assignment's cascade writes `DataSource` alone (not needed after a payload's UpdateData)
- **PreventDefault before async HTTP** — suppresses SF "No records found" flash
- **WhileLoading is sequential only** — no conditions or nested HTTP
- **OnAllSettled is sequential only** — no conditions or nested HTTP
- **Chained fires on success only** — no error chaining
