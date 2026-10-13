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
| a query class, `Medications([FromQuery] MedicationSearchQuery query)` | `.Gather<MedicationSearchQuery>(g => ..)` | `Components/Fusion/AutoComplete/Index.cshtml:138` |
| a route value, `Rate(string suiteType)` | `.Gather<SuiteQuery>(g => g.Set(x => x.SuiteType, ..))` | `Components/Brought/TomSelect/Index.cshtml:44` |
| a body class, `AlertHypertension([FromBody] AlertHypertensionRequest? request)` | `.Gather<AlertHypertensionRequest>(g => ..)` | `Conditions/AdmissionAssessment/_CardiacAssessment.cshtml:31` |
| route values and a JSON body (ASP.NET Core), `HoldVisit(string slotId, [FromBody] HoldRequest? hold)` | `.Gather<HoldVisitRequest>(g => ..)` where `HoldVisitRequest : HoldRequest { SlotId }` | `Components/Brought/VisitPlanner/FrontDesk.cshtml:83` |

In ASP.NET Core an action that binds a JSON body takes its route values as their own parameters.
The page's request class is then the body class plus the route members:

```csharp
// Models/Components/Brought/VisitPlanner/FrontDeskHoldsModel.cs
public class HoldRequest
{
    public int VisitsThisWeek { get; set; }
    public string? Family { get; set; }
    public string? Source { get; set; }
}

public sealed class HoldVisitRequest : HoldRequest
{
    public string SlotId { get; set; } = "";
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

Route value from a brought component (`Components/Brought/TomSelect/Index.cshtml:41-44`):

```csharp
var suite = p.Component<TomSelect>(m => m.SuiteType);
quote.Get(rateUrl)   // rateUrl = Url.SandboxRouteTemplate("Rate", "TomSelect", "suiteType")
    .Gather<SuiteQuery>(g => g.Set(x => x.SuiteType, suite.Value))
```

Route value, body members and a header in one request (`Components/Brought/VisitPlanner/FrontDesk.cshtml:82-88`;
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

Every input of the form, plus one member the form does not carry (`Validation/MoveInReview/Index.cshtml:42`):

```csharp
p.Post(Url.SandboxRoute("Save", "MoveInReview")).Gather(g => g.Form().Set(x => x.Pharmacy.Network, "Riverside Health"))
```

Named inputs into a request class other than the view model (`Conditions/AdmissionAssessment/_CardiacAssessment.cshtml:30-31`):

```csharp
then.Post(alertHypertensionUrl)
 .Gather<AlertHypertensionRequest>(g => g.Form(m => m.SystolicBP, m => m.ResidentName))
```

Literal members and one input (`Components/Fusion/Grid/Index.cshtml:124-128`):

```csharp
p.Post(Url.SandboxRoute("Data", "Grid"))
 .Gather<GridDataRequest>(g => g
     .Set(x => x.Skip, 0)
     .Set(x => x.Take, 10)
     .Form(m => m.MinAge))
```

Headers only, request class = the view model (`HttpPipeline/Http/Index.cshtml:667-671`):

```csharp
p.Get(Url.SandboxRoute("EchoHeadersPost", "Http"))
 .Gather(g => g
     .Header("X-Api-Version", "2024-01-15")
     .Header("X-Request-Id", "test-header-123")
     .Header("X-Tenant-Id", "facility-42"))
```

Form data, for files (`Components/Fusion/FileUpload/Index.cshtml:45-47`):

```csharp
p.Post(Url.SandboxRoute("Echo", "FileUpload"))
 .Gather(g => g.Form())
 .AsFormData()
```

## The Page's URL Query: `p.Url<TQuery>()`

`TQuery` is the class the page's action binds with `[FromQuery]`. A member reads the query key of its
binding name, matched ignoring case. A scalar member reads the key's first value; an array member reads
every value of the key. The result is a source like any other: conditions, `Text.Set`, `Set`, `Header`.

```csharp
// Models/HttpPipeline/Http/HttpShowcaseModel.cs
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
// HttpPipeline/Http/Index.cshtml:31-38, 43-46 (two DomReady handlers in the view)
var link = p.Url<HttpShowcaseQuery>();
p.When(link.Get(q => q.Tab)).Eq("medications")
 .Then(then => then.Element("url-cond-meds").Show())
 .Else(els => els.Element("url-cond-meds").Hide());

p.When(link.Get(q => q.Page)).Gt(1)
 .Then(then => then.Element("url-cond-prev").Show())
 .Else(els => els.Element("url-cond-prev").Hide());

p.Element("url-display-wings").Text.Set(link.Get(q => q.Wing));     // every ?wing= value
```

URL members fill a route value, a header and a query member of another request
(`HttpPipeline/Http/Index.cshtml:480-485`; the URL is `.../ComposeEcho/{id}`):

```csharp
var link = p.Url<HttpShowcaseQuery>();
p.Get(Url.SandboxRouteTemplate("ComposeEcho", "Http", "id"))
 .Gather<ComposeEchoQuery>(g => g
     .Set(x => x.Id, link.Get(q => q.ResidentId))
     .Header("X-Tab", link.Get(q => q.Tab))
     .Set(x => x.Facility, link.Get(q => q.FacilityId)))
```

## Event Payload Members: `args.Member`

`args` is the object the event handed its callback, declared as a `JsObject`. Each of its members is a
source; a member whose value is data reads further with `Get` (`Components/Fusion/Grid/Index.cshtml:167-181`):

```csharp
t.Post(Url.SandboxRoute("Data", "Grid"))
 .Gather<GridDataRequest>(g => g
     .Set(x => x.EventName, args.Name)
     .Set(x => x.Skip, args.Skip)
     .Set(x => x.Take, args.Take)
     .Set(x => x.ActionCurrentPage, args.Action.Get(x => x.CurrentPage))
     .Set(x => x.Sorted, args.Sorted)
     .Form(m => m.MinAge))
```

## Chaining From a Response

A request nested in `OnSuccess` sets members from that response (`HttpPipeline/Http/Index.cshtml:500-508`;
`FacilityResident/{facilityId}/{residentId}`):

```csharp
p.Get(Url.SandboxRouteTemplate("ResidentById", "Http", "id"))
 .Gather<ResidentLookup>(g => g.Set(x => x.Id, p.Url<HttpShowcaseQuery>().Get(q => q.ResidentId)))
 .Response(r => r
    .OnSuccess<ResidentByIdResponse>((json, s) =>
    {
        s.Get(Url.SandboxRouteTemplate("FacilityResident", "Http", "facilityId", "residentId"))
         .Gather<FacilityResidentLookup>(g => g
            .Set(x => x.FacilityId, s.Url<HttpShowcaseQuery>().Get(q => q.FacilityId))
            .Set(x => x.ResidentId, json.Get(x => x.ResidentId)))
         ...
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
    s.Component<FusionDropDownList>(m => m.City).DataSource.Set(json.Get(j => j.Cities));
})

CHAINED_REQUEST := (c => c.VERB(url).CHAIN)     -- full request chain
```

**Response pipeline `s`** is a full `PipelineBuilder` — supports Element, Component, Dispatch, When, nested HTTP, etc.

## Typed Response (`Data<T>`)

`json` is `Data<TResponse>`: `json.Get(x => x.Member)` reads a member, and `json` itself is the whole
body (`HttpPipeline/Http/Index.cshtml`, `Components/Fusion/Grid/Index.cshtml`):

```csharp
.OnSuccess<ResidentsResponse>((json, s) =>
{
    s.Element("load-first").Text.Set(json.Get(x => x.First));
    s.Element("load-count").Text.Set(json.Get(x => x.Count));
})

.OnSuccess<ResidentGridResponse>((json, s) =>
{
    s.Component<FusionGrid>("residents-grid").DataSource.Set(json);   // the whole body
})
```

A member reads by its camel-case JSON name. `TResponse` must be a class.

## Error Handling

```csharp
.Response(r => r
    .OnSuccess<T>((json, s) => { ... })
    .OnError(400, e => e.ValidationErrors("formId"))    // display field errors
    .OnError(500, e => e.Element("error").Text.Set("Server error")))
```

Multiple `OnError` handlers allowed (one per status code). `ValidationErrors("formId")` maps server 400 response to field-level error display.
A typed error body reads like a success body (`Components/Brought/VisitPlanner/FrontDesk.cshtml`):

```csharp
.OnError<DeskConflict>(409, (conflict, e) =>
{
    e.Component<VisitPlanner>(m => m.VisitSlots).Value.Set(conflict.Get(x => x.Held));
    e.Element("desk-status").Text.Set(conflict.Get(x => x.Message));
})
```

## WhileLoading

```csharp
p.Get("/api/data")
 .WhileLoading(l =>
 {
     l.Element("spinner").Show();
     l.Element("btn").ClassList.Add("disabled");
 })
 .Response(r => r.OnSuccess(s =>
 {
     s.Element("spinner").Hide();
     s.Element("btn").ClassList.Remove("disabled");
 }))
```

Commands execute before the HTTP request fires. **Sequential commands only** — no conditions, HTTP, or parallel inside WhileLoading.

## Chained Requests

From `HttpPipeline/Http/Index.cshtml`:

```csharp
p.Get(Url.SandboxRoute("Residents", "Http"))
 .Response(r => r
    .OnSuccess<ResidentsResponse>((json, s) =>
    {
        s.Element("chain-resident-first").Text.Set(json.Get(x => x.First));
    })
    .Chained(c => c
        .Get(Url.SandboxRoute("Facilities", "Http"))
        .Response(r2 => r2.OnSuccess<FacilitiesResponse>((json2, s2) =>
        {
            s2.Element("chain-facility-first").Text.Set(json2.Get(x => x.First));
        }))))
```

Chained fires **only on 2xx success** of the parent request. Can nest further.

To send what the first answer said, take its body as the callback's first parameter (from
`Components/Brought/VisitPlanner/FrontDesk.cshtml`):

```csharp
.Chained<ResidentWeekResponse>((week, holds) => holds.Get(holdsUrl)
    .Gather<FrontDeskResident>(g => g.Header("X-Wing", week.Get(x => x.Wing)))
    .Response(answer => answer.OnSuccess<FamilyHoldsResponse>((held, s) => ...)))
```

## Parallel Requests

From `HttpPipeline/Http/Index.cshtml`:

```csharp
p.Element("parallel-spinner").Show();
p.Parallel(
    a => a.Get(Url.SandboxRoute("Residents", "Http"))
          .Response(ra => ra.OnSuccess<ResidentsResponse>((json, sa) =>
              sa.Element("parallel-resident-first").Text.Set(json.Get(x => x.First)))),
    b => b.Get(Url.SandboxRoute("Facilities", "Http"))
          .Response(rb => rb.OnSuccess<FacilitiesResponse>((json, sb) =>
              sb.Element("parallel-facility-first").Text.Set(json.Get(x => x.First))))
).OnAllSettled(s =>
{
    s.Element("parallel-spinner").Hide();
    s.Element("parallel-all").Text.Set("All parallel requests completed!");
});
```

All branches fire concurrently. `OnAllSettled` fires after ALL complete (success or failure).
**Sequential commands only** in OnAllSettled — no conditions, HTTP, or parallel.

## Complete Patterns

**Pattern 1: Server-filtered dropdown (Filtering event)**
Source: `Components/Fusion/AutoComplete/Index.cshtml:134-145`; the action binds `[FromQuery] MedicationSearchQuery`.
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
Source: `Patterns/Cascading/Index.cshtml:43-59`; the action binds `[FromQuery] string? Country`, and
`CityLookupQuery { string? Country }` names that query for the page.
```csharp
.Reactive(evt => evt.Change, (args, p) =>
{
    p.Get(Url.SandboxRoute("Cities", "Cascading"))
     .Gather<CityLookupQuery>(g => g.Form(m => m.Country))
     .Response(r => r
        .OnSuccess<CityLookupResponse>((json, s) =>
        {
            var city = s.Component<FusionDropDownList>(m => m.City);
            city.DataSource.Set(json.Get(j => j.Cities));
            city.DataBind();
        }));
})
```

## Key Rules

- **Gather names the class the endpoint binds** — `Gather<TRequest>`, or `Gather(..)` when the action binds the view model; no string parameter names
- **A `{placeholder}` is filled by the member of its name** — every placeholder must be filled; the rest goes to the query (GET) or the body
- **Form() for the form's inputs** — `Form()` sends every input, `Form(m => ..)` the named ones, each by its binding name
- **The payload for filtering** — `Set(x => x.M, args.Text)` sends the text typed during the keystroke
- **updateData for filtering** — the ONLY correct SF API for async filtered data
- **Call DataBind after setting DataSource** — needed for cascade patterns (not needed after a payload's UpdateData)
- **PreventDefault before async HTTP** — suppresses SF "No records found" flash
- **WhileLoading is sequential only** — no conditions or nested HTTP
- **OnAllSettled is sequential only** — no conditions or nested HTTP
- **Chained fires on success only** — no error chaining
