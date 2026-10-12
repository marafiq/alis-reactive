---
name: http-pipeline
description: Use when building HTTP requests in the reactive plan — Get/Post/Put/Delete, Gather (the typed request the endpoint binds - Set, Form, Header, route members), the URL query (p.Url<TQuery>()) and event payload (p.Payload(args).Get) sources, Response (OnSuccess/OnError), Chained requests, Parallel requests, WhileLoading, Finally, and Validate. The HTTP pipeline grammar for Alis.Reactive.
---

# HTTP Pipeline Grammar

Source of truth: `Alis.Reactive/PlanAuthoring/Requests/` (`HttpRequestBuilder.cs`, `GatherBuilder.cs`,
`GatherInputDraft.cs`, `RequestRouteTemplate.cs`, `RequestMembers.cs`) and
`Alis.Reactive/PlanAuthoring/Pipelines/` (`PipelineBuilder.Http.cs`, `UrlQuery.cs`, `PayloadRef.cs`).
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
| the view model, `Reserve([FromBody] SuiteAssignmentModel assignment)` | `.Gather(g => g.Form())` | `Components/Brought/TomSelect/Index.cshtml:87` |
| a query class, `Medications([FromQuery] MedicationSearchQuery query)` | `.Gather<MedicationSearchQuery>(g => ..)` | `Components/Fusion/AutoComplete/Index.cshtml:138` |
| a route value, `Rate(string suiteType)` | `.Gather<SuiteQuery>(g => g.Set(x => x.SuiteType, ..))` | `Components/Brought/TomSelect/Index.cshtml:44` |
| a body class, `AlertHypertension([FromBody] AlertHypertensionRequest? request)` | `.Gather<AlertHypertensionRequest>(g => ..)` | `Conditions/AdmissionAssessment/_CardiacAssessment.cshtml:31` |
| route values and a JSON body (ASP.NET Core), `HoldVisit(string slotId, [FromBody] HoldRequest? hold)` | `.Gather<HoldVisitRequest>(g => ..)` where `HoldVisitRequest : HoldRequest { SlotId }` | `Components/Brought/VisitPlanner/FrontDesk.cshtml:81` |

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
  | p.Component<T>(m => m.X).Get(c => c.Member)     -- component property or returning method
  | p.Payload(args).Get(a => a.Member)              -- the triggering event's payload
  | p.Url<TQuery>().Get(q => q.Member)              -- the page's URL query string
  | json.Read(x => x.Member)                        -- a success body, in a request nested in OnSuccess
  | p.Plugin(..)                                    -- a plugin value
  | an array value                                  -- e.g. a Count over an array plugin
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

Route value from a brought component (`Components/Brought/TomSelect/Index.cshtml:43-44`):

```csharp
quote.Get(rateUrl)   // rateUrl = Url.SandboxRouteTemplate("Rate", "TomSelect", "suiteType")
    .Gather<SuiteQuery>(g => g.Set(x => x.SuiteType, suite.Get(x => x.Value)))
```

Route value, body members and a header in one request (`Components/Brought/VisitPlanner/FrontDesk.cshtml:80-86`;
`holdUrl` is `.../FrontDesk/Hold/{slotId}`):

```csharp
added.Post(holdUrl)
    .Gather<HoldVisitRequest>(g => g
        .Set(x => x.SlotId, p.Payload(args).Get(a => a.Added))
        .Set(x => x.VisitsThisWeek, added.Component<VisitPlanner>(m => m.VisitSlots).Get(c => c.Count))
        .Set(x => x.Family, p.Url<FrontDeskQuery>().Get(q => q.Family))
        .Set(x => x.Source, "front-desk")
        .Header("X-Wing", added.Component<VisitPlanner>(m => m.VisitSlots).Get(c => c.Wing)))
```

Every input of the form, plus one member the form does not carry (`Validation/MoveInReview/Index.cshtml:40`):

```csharp
p.Post(Url.SandboxRoute("Save", "MoveInReview")).Gather(g => g.Form().Set(x => x.Pharmacy.Network, "Riverside Health"))
```

Named inputs into a request class other than the view model (`Conditions/AdmissionAssessment/_CardiacAssessment.cshtml:30-31`):

```csharp
then.Post(alertHypertensionUrl)
 .Gather<AlertHypertensionRequest>(g => g.Form(m => m.SystolicBP, m => m.ResidentName))
```

Literal members and one input (`Components/Fusion/Grid/Index.cshtml:125-129`):

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
every value of the key. The result is a source like any other: conditions, `SetText`, `Set`, `Header`.

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
// HttpPipeline/Http/Index.cshtml:31-36, 43-46 (two DomReady handlers in the view)
var link = p.Url<HttpShowcaseQuery>();
p.When(link.Get(q => q.Tab)).Eq("medications")
 .Then(then => then.Element("url-cond-meds").Show())
 .Else(els => els.Element("url-cond-meds").Hide());

p.When(link.Get(q => q.Page)).Gt(1)
 .Then(then => then.Element("url-cond-prev").Show())
 .Else(els => els.Element("url-cond-prev").Hide());

p.Element("url-display-wings").SetText(link.Get(q => q.Wing));     // every ?wing= value
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

## Event Payload Members: `p.Payload(args).Get(..)`

`p.Payload(args)` is the handle for the object the event handed its callback; `Get` reads one of its
members as a source (`Components/Fusion/Grid/Index.cshtml:169-184`):

```csharp
var dataState = t.Payload(args);
t.Post(Url.SandboxRoute("Data", "Grid"))
 .Gather<GridDataRequest>(g => g
     .Set(x => x.EventName, dataState.Get(a => a.Name))
     .Set(x => x.Skip, dataState.Get(a => a.Skip))
     .Set(x => x.Take, dataState.Get(a => a.Take))
     .Set(x => x.ActionCurrentPage, dataState.Get(a => a.Action.CurrentPage))
     .Set(x => x.Sorted, dataState.Get(a => a.Sorted))
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
            .Set(x => x.ResidentId, json.Read(x => x.ResidentId)))
         ...
```

## Response Grammar

```
.Response(r => {
    r.OnSuccess(HANDLER);                    -- untyped (any 2xx)
    r.OnSuccess<TResponse>(TYPED_HANDLER);   -- typed JSON response
    r.OnError(STATUS, HANDLER);              -- status-specific error
    r.Chained(CHAINED_REQUEST);              -- sequential follow-up
})

HANDLER := (s => { PIPELINE_COMMANDS })

TYPED_HANDLER := ((json, s) => {
    s.Element("x").SetText(json, x => x.Property);  -- "responseBody.property"
    s.Component<T>(m => m.Y).Set(c => c.DataSource, json.Read(x => x.Items));
})

CHAINED_REQUEST := (c => c.VERB(url).CHAIN)     -- full request chain
```

**Response pipeline `s`** is a full `PipelineBuilder` — supports Element, Component, Dispatch, When, nested HTTP, etc.

## Typed Response (ResponseBody&lt;T&gt;)

```csharp
.OnSuccess<ApiResponse>((json, s) =>
{
    // json is ResponseBody<ApiResponse> — phantom for compile-time path inference
    s.Element("name").SetText(json, x => x.Data.Name);     // → "responseBody.data.name"
    s.Element("count").SetText(json, x => x.TotalCount);   // → "responseBody.totalCount"
    s.Payload(args).Call(a => a.UpdateData, json.Read(j => j.Items));  // → "responseBody.items"
    s.Component<FusionGrid>("roster").Set(c => c.DataSource, json.Read(x => x.Rows)); // x => x is the whole body
})
```

`TResponse` must be `class, new()`. Properties map to camelCase dot-paths. Runtime walks `responseBody.path` in the execution context.

## Error Handling

```csharp
.Response(r => r
    .OnSuccess<T>((json, s) => { ... })
    .OnError(400, e => e.ValidationErrors("formId"))    // display field errors
    .OnError(500, e => e.Element("error").SetText("Server error")))
```

Multiple `OnError` handlers allowed (one per status code). `ValidationErrors("formId")` maps server 400 response to field-level error display.

## WhileLoading

```csharp
p.Get("/api/data")
 .WhileLoading(l =>
 {
     l.Element("spinner").Show();
     l.Element("btn").AddClass("disabled");
 })
 .Response(r => r.OnSuccess(s =>
 {
     s.Element("spinner").Hide();
     s.Element("btn").RemoveClass("disabled");
 }))
```

Commands execute before the HTTP request fires. **Sequential commands only** — no conditions, HTTP, or parallel inside WhileLoading.

## Chained Requests

```csharp
p.Get("/api/residents")
 .Response(r => r
    .OnSuccess<ResidentsResponse>((json, s) =>
    {
        s.Element("residents").SetText(json, x => x.First);
    })
    .Chained(c => c
        .Get("/api/facilities")
        .Response(r2 => r2.OnSuccess<FacilitiesResponse>((json2, s2) =>
        {
            s2.Element("facilities").SetText(json2, x => x.First);
        }))))
```

Chained fires **only on 2xx success** of the parent request. Can nest further.

## Parallel Requests

```csharp
p.Element("spinner").Show();
p.Parallel(
    a => a.Get("/api/residents")
          .Response(r => r.OnSuccess<ResResponse>((json, s) =>
              s.Element("r1").SetText(json, x => x.First))),
    b => b.Get("/api/facilities")
          .Response(r => r.OnSuccess<FacResponse>((json, s) =>
              s.Element("r2").SetText(json, x => x.First)))
)
.OnAllSettled(s =>
{
    s.Element("spinner").Hide();
    s.Element("status").SetText("All loaded");
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
    p.Payload(args).Set(a => a.PreventDefaultAction, true);
    p.Get(Url.SandboxRoute("Medications", "AutoComplete"))
     .Gather<MedicationSearchQuery>(g => g.Set(x => x.MedicationType, p.Payload(args).Get(a => a.Text)))
     .Response(r => r
        .OnSuccess<MedicationSearchResponse>((json, s) =>
        {
            s.Payload(args).Call(a => a.UpdateData, json.Read(j => j.Medications));
            s.Element("filter-status").SetText("results loaded");
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
        s.Component<FusionToast>()
            .Set(c => c.Title, "Todo")
            .Set(c => c.Content, "Todo saved successfully")
            .Set(c => c.Tone, FusionToastTone.Success)
            .Call(c => c.Show);
    })
    .OnError(400, e => e.ValidationErrors("todo-form")));
```

**Pattern 3: Cascade (parent change populates child)**
Source: `Patterns/Cascading/Index.cshtml:43-52`; the action binds `[FromQuery] string? Country`, and
`CityLookupQuery { string? Country }` names that query for the page.
```csharp
.Reactive(evt => evt.Change, (args, p) =>
{
    p.Get(Url.SandboxRoute("Cities", "Cascading"))
     .Gather<CityLookupQuery>(g => g.Form(m => m.Country))
     .Response(r => r
        .OnSuccess<CityLookupResponse>((json, s) =>
        {
            s.Component<FusionDropDownList>(m => m.City)
                .Set(c => c.DataSource, json.Read(j => j.Cities))
                .Call(c => c.DataBind);
        }));
})
```

## Key Rules

- **Gather names the class the endpoint binds** — `Gather<TRequest>`, or `Gather(..)` when the action binds the view model; no string parameter names
- **A `{placeholder}` is filled by the member of its name** — every placeholder must be filled; the rest goes to the query (GET) or the body
- **Form() for the form's inputs** — `Form()` sends every input, `Form(m => ..)` the named ones, each by its binding name
- **The payload for filtering** — `Set(x => x.M, p.Payload(args).Get(a => a.Text))` sends the text typed during the keystroke
- **updateData for filtering** — the ONLY correct SF API for async filtered data
- **Call DataBind after setting DataSource** — needed for cascade patterns (not needed after a payload's UpdateData)
- **PreventDefault before async HTTP** — suppresses SF "No records found" flash
- **WhileLoading is sequential only** — no conditions or nested HTTP
- **OnAllSettled is sequential only** — no conditions or nested HTTP
- **Chained fires on success only** — no error chaining
