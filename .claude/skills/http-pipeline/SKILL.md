---
name: http-pipeline
description: Use when building HTTP requests in the reactive plan — Get/Post/Put/Delete, Gather (Include/FromEvent/Static), Response (OnSuccess/OnError), Chained requests, Parallel requests, WhileLoading, and Validate. The HTTP pipeline grammar for Alis.Reactive.
---

# HTTP Pipeline Grammar

## Entry Points

```
p.VERB(url).CHAIN

VERB :=
  | Get(url)
  | Post(url)
  | Post(url, gather => { ... })     -- inline gather
  | Put(url, gather => { ... })      -- inline gather
  | Delete(url)
```

## Request Chain

```
p.VERB(url)
  [.Gather(g => { ... })]           -- optional: request params/body
  [.AsFormData()]                    -- optional: multipart (default: JSON)
  [.WhileLoading(l => { ... })]     -- optional: commands during flight
  [.Validate<TValidator>("formId")] -- optional: client-side validation
  .Response(r => { ... })           -- response handlers
```

All chain methods return `HttpRequestBuilder<TModel>` — order is flexible but Response should be last.

## Gather Grammar

```
.Gather(g => {
    g.GATHER_ITEM;
    g.GATHER_ITEM;  -- multiple items compose
})

GATHER_ITEM :=
  | g.Static("param", value)
  | g.FromEvent(args, x => x.Property, "param")
  | g.Include<TComponent, TModel>(m => m.Property)
  | g.IncludeAll()
```

| Method | Resolves | Example JSON |
|--------|----------|--------------|
| `Static("page", 1)` | Literal at plan time | `{ kind: "static", param: "page", value: 1 }` |
| `FromEvent(args, x => x.Text, "q")` | Event payload at runtime | `{ kind: "event", param: "q", path: "evt.text" }` |
| `Include<FusionAutoComplete, M>(m => m.Name)` | Component value at runtime | `{ kind: "component", componentId: "...", vendor: "fusion", readExpr: "value" }` |
| `IncludeAll()` | All registered components | Expands at render time to ComponentGather items |

**FromEvent vs Include:**
- `FromEvent` — value from the event that triggered this pipeline (e.g., filtering text)
- `Include` — value from a component's current state (e.g., form field for POST body)

**Parameter naming:**
- `Static("page", 1)` — sends param name `page` (explicit)
- `FromEvent(args, x => x.Text, "q")` — sends param name `q` (explicit)
- `Include<T, M>(m => m.SearchTerm)` — sends param name `SearchTerm` (from **binding path**)
- `IncludeAll()` — each component sends its binding path as param name
- Controller `[FromQuery]` / `[FromBody]` parameter names MUST match these names

**GET requests:** gather items become URL query params (`?SearchTerm=text&page=1`)
**POST/PUT requests:** gather items become JSON body fields (`{ "SearchTerm": "text", "page": 1 }`)

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
    s.Component<T>(m => m.Y).SetDataSource(json, x => x.Items);
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
    args.UpdateData(s, json, j => j.Items);                 // → "responseBody.items"
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
```csharp
.Reactive(plan, evt => evt.Filtering, (args, p) =>
{
    args.PreventDefault(p);
    p.Get("/api/search")
     .Gather(g => g.FromEvent(args, x => x.Text, "q"))
     .Response(r => r.OnSuccess<SearchResponse>((json, s) =>
     {
         args.UpdateData(s, json, j => j.Items);
         s.Element("status").SetText("loaded");
     }));
})
```

**Pattern 2: Form POST with validation**
```csharp
p.Post("/api/save")
 .Gather(g => g.IncludeAll())
 .Validate<SaveValidator>("form")
 .WhileLoading(l => l.Element("btn").Hide())
 .Response(r => r
    .OnSuccess(s =>
    {
        s.Element("btn").Show();
        s.Element("msg").SetText("Saved!");
    })
    .OnError(400, e =>
    {
        e.Element("btn").Show();
        e.ValidationErrors("form");
    }))
```

**Pattern 3: Cascade (parent change populates child)**
```csharp
.Reactive(plan, evt => evt.Changed, (args, p) =>
{
    p.Get("/api/children")
     .Gather(g => g.Include<FusionDropDownList, TModel>(m => m.Parent))
     .Response(r => r.OnSuccess<ChildResponse>((json, s) =>
     {
         s.Component<FusionDropDownList>(m => m.Child).SetDataSource(json, x => x.Items);
         s.Component<FusionDropDownList>(m => m.Child).DataBind();
     }));
})
```

## Key Rules

- **FromEvent for filtering** — value from event args (typed text during keystroke)
- **Include for form fields** — value from component's current state
- **updateData for filtering** — the ONLY correct SF API for async filtered data
- **DataBind after SetDataSource** — needed for cascade patterns (not needed after updateData)
- **PreventDefault before async HTTP** — suppresses SF "No records found" flash
- **WhileLoading is sequential only** — no conditions or nested HTTP
- **OnAllSettled is sequential only** — no conditions or nested HTTP
- **Chained fires on success only** — no error chaining
