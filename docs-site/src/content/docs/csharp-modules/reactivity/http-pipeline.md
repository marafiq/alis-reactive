---
title: HTTP Pipeline
description: GET/POST/PUT/DELETE requests, gather strategies, loading states, typed responses, chained and parallel requests, and validation integration.
sidebar:
  order: 5
---

HTTP requests live inside a `.Reactive()` or `Html.On()` pipeline — typically triggered by a button click or a component event. You gather input values, validate, post, and handle the response in one fluent chain.

From the [Grammar Tree](/csharp-modules/mental-model/#the-grammar-tree) — the HTTP subset:

```
pipeline.Get(url) / .Post(url) / .Put(url) / .Delete(url)  § start a request
├── .Gather(g => { })                                       § collect request data
│   ├── g.Static("key", value)                              § fixed value
│   ├── g.Include<TComp, TModel>(m => m.Prop)               § component value
│   ├── g.IncludeAll()                                      § all registered components
│   └── g.FromEvent(args, x => x.Prop, "paramName")         § event payload value
├── .AsFormData()                                           § multipart content type
├── .Validate<TValidator>("formId")                         § client-side validation
├── .WhileLoading(l => { })                                 § commands during flight
├── .Response(r => { })                                     § response handlers
│   ├── r.OnSuccess(pipeline => { })                        § handle 2xx
│   ├── r.OnSuccess<T>((json, pipeline) => { })             § typed JSON response
│   ├── r.OnError(code, e => { })                           § status-specific error
│   └── r.Chained(c => { })                                 § sequential follow-up
├── pipeline.Parallel(a => ..., b => ...)                   § concurrent requests
│   └── .OnAllSettled(pipeline => { })                      § after all complete
```

## How do I start a request?

Inside a `.Reactive()` handler, call a verb method on `pipeline`:

```csharp
Html.NativeButton("save-btn", "Save")
    .Reactive(plan, evt => evt.Click, (args, pipeline) =>
    {
        pipeline.Post("/api/residents", g => g.IncludeAll())
         .Response(/* removed for brevity */);
    });
```

The available verbs:

```csharp
pipeline.Get("/api/residents")
pipeline.Post("/api/residents", g => { /* gather */ })
pipeline.Put("/api/residents", g => { /* gather */ })
pipeline.Delete("/api/residents/42")
```

Each returns the request builder, which you chain with `.Gather()`, `.WhileLoading()`, `.Validate()`, and `.Response()`.

`Post` and `Put` also accept an inline gather configuration -- a shorthand for the most common pattern:

```csharp
pipeline.Post("/api/residents", g => { /* gather */ })   // POST with inline gather
pipeline.Put("/api/residents", g => { /* gather */ })     // PUT with inline gather
```

## How do I collect values for the request?

Gather defines what data goes into the request body (POST/PUT) or query string (GET/DELETE). There are four strategies.

### Static -- fixed values known at build time

```csharp
pipeline.Post("/api/residents")
    .Gather(g =>
    {
        g.Static("action", "create");
        g.Static("facilityId", 42);
    });
```

At runtime, the request body includes `{ "action": "create", "facilityId": 42 }`.

### Include -- read a component's current value

Read the current value of a form component at request time:

```csharp
pipeline.Post("/api/residents")
    .Gather(g =>
    {
        g.Include<FusionDropDownList, ResidentModel>(m => m.FacilityId);
        g.Include<FusionNumericTextBox, ResidentModel>(m => m.RoomNumber);
    });
```

Each `Include` reads the component's live value at request time (e.g., the text of an input, the checked state of a checkbox). The parameter name in the request body comes from the model expression: `m => m.FacilityId` sends the field `"FacilityId"`.

`Include` is provided by each vendor project, so Fusion and Native components each have their own overload:

```csharp
// Fusion components
g.Include<FusionDropDownList, TModel>(m => m.Country);

// Native components
g.Include<NativeTextBox, TModel>(m => m.FirstName);
```

### IncludeAll -- every registered component

For form submissions where you want all fields:

```csharp
pipeline.Post("/api/residents")
    .Gather(g => g.IncludeAll());
```

At render time, `IncludeAll()` expands to an `Include` for every component registered in the plan. The controller receives a JSON body with every field.

### FromEvent -- read from the triggering event's payload

Read a value from the event that triggered this pipeline:

```csharp
Html.InputField(m => m.MedicationSearch, /* removed for brevity */)
    .Reactive(plan, evt => evt.Filtering, (args, pipeline) =>
    {
        pipeline.Get("/api/medications/search")
            .Gather(g => g.FromEvent(args, x => x.Text, "q"))
            .Response(r => r.OnSuccess<SearchResponse>((json, pipeline) =>
            {
                pipeline.Element("result").SetText(json, x => x.MatchCount);
            }));
    })
```

`FromEvent(args, x => x.Text, "q")` resolves the expression to `"evt.text"` and sends it as query parameter `q`. The third argument is the parameter name -- it does not have to match the property name.

### Can I mix strategies?

Yes. Combine freely in a single gather block:

```csharp
pipeline.Post("/api/residents/search")
    .Gather(g =>
    {
        g.Include<FusionDropDownList, ResidentModel>(m => m.FacilityId);
        g.Static("page", 1);
        g.Static("pageSize", 25);
        g.FromEvent(args, x => x.Text, "searchTerm");
    });
```

### How does gather behave for GET vs POST?

- **GET / DELETE**: Gather items become URL query parameters (`/api/search?q=smith&page=1`)
- **POST / PUT**: Gather items become JSON body fields (`{ "q": "smith", "page": 1 }`)

### How do I send multipart form data?

The default content type is JSON. For file uploads:

```csharp
pipeline.Post("/api/upload")
    .Gather(g => g.IncludeAll())
    .AsFormData()
    .Response(r => r.OnSuccess(pipeline => pipeline.Element("status").SetText("Uploaded")));
```

## How do I show a loading state during the request?

`WhileLoading` accepts commands to execute while the request is in-flight:

```csharp
pipeline.Post("/api/residents", g => g.IncludeAll())
    .WhileLoading(l =>
    {
        l.Element("spinner").Show();
        l.Element("save-btn").AddClass("opacity-50");
    })
    .Response(r => r.OnSuccess(pipeline =>
    {
        pipeline.Element("spinner").Hide();
        pipeline.Element("save-btn").RemoveClass("opacity-50");
        pipeline.Element("status").SetText("Saved");
    }));
```

WhileLoading accepts sequential commands only -- no conditions, HTTP requests, or parallel branches inside it. It is designed for simple visual state changes: show/hide spinners, disable buttons, dim sections.

## How do I handle the response?

The `.Response()` method configures what happens after the request completes.

### OnSuccess -- when you do not need the response body

```csharp
pipeline.Delete("/api/residents/42")
    .Response(r => r.OnSuccess(pipeline =>
    {
        pipeline.Element("status").SetText("Resident record deleted");
        pipeline.Element("resident-row-42").Hide();
    }));
```

### OnSuccess&lt;T&gt; -- typed JSON response

When the server returns structured JSON, declare a response type and get compile-time access to its properties:

```csharp
public class ResidentResponse
{
    public string FullName { get; set; } = "";
    public string FacilityName { get; set; } = "";
    public int TotalCount { get; set; }
}

pipeline.Get("/api/residents/42")
    .Response(r => r.OnSuccess<ResidentResponse>((json, pipeline) =>
    {
        pipeline.Element("name").SetText(json, x => x.FullName);
        pipeline.Element("facility").SetText(json, x => x.FacilityName);
        pipeline.Element("count").SetText(json, x => x.TotalCount);
    }));
```

The `json` parameter gives you typed access to the response properties -- it is used for type inference, just like event payloads. The expression `x => x.FullName` compiles to `"responseBody.fullName"`. At runtime, the actual JSON response is walked at that path.

The response type must be a class with a parameterless constructor. Properties are camelCased in the path, matching standard JSON conventions.

### OnError -- status-specific error handling

Handle specific HTTP error codes:

```csharp
pipeline.Post("/api/residents", g => g.IncludeAll())
    .Response(r => r
        .OnSuccess(pipeline => pipeline.Element("status").SetText("Saved"))
        .OnError(400, e => e.ValidationErrors("resident-form"))
        .OnError(409, e => e.Element("error").SetText("Duplicate resident record"))
        .OnError(500, e => e.Element("error").SetText("Server error -- please try again")));
```

Multiple `OnError` handlers are allowed -- one per status code. The first matching handler executes.

### What is ValidationErrors?

When the server returns a 400 response with validation errors, `ValidationErrors` maps them to the form's input fields:

```csharp
// ... inside a .Response() handler:
.OnError(400, e => e.ValidationErrors("resident-form"))
```

This command reads the 400 response body, finds the matching form by ID, and displays errors at each field's validation slot.

### What is Into?

`Into` injects the HTTP response body as `innerHTML` of a target element. It is used for loading partial views:

```csharp
pipeline.Get("/api/residents/grid")
    .Response(r => r.OnSuccess(pipeline => pipeline.Into("grid-container")));
```

The server returns an HTML fragment, and `Into` replaces the contents of `#grid-container` with it.

## How do I validate before sending the request?

Validate the form before the request leaves the browser:

```csharp
pipeline.Post("/api/residents", g => g.IncludeAll())
    .Validate<ResidentValidator>("resident-form")
    .WhileLoading(l => l.Element("spinner").Show())
    .Response(r => r
        .OnSuccess(pipeline => pipeline.Element("status").SetText("Saved"))
        .OnError(400, e => e.ValidationErrors("resident-form")));
```

`.Validate<TValidator>("formId")` extracts validation rules from the FluentValidation validator `TValidator` at render time. The runtime evaluates those rules against the form's component values before issuing the HTTP request. If validation fails, the request is aborted and errors are displayed at the form fields.

## How do I chain sequential requests?

When one request depends on the result of another, chain them inside `.Response()`:

```csharp
pipeline.Get("/api/facilities")
    .Response(r => r
        .OnSuccess<FacilitiesResponse>((json, pipeline) =>
        {
            pipeline.Component<FusionDropDownList>(m => m.FacilityId)
                .SetDataSource(json, x => x.Facilities)
                .DataBind();
        })
        .Chained(c => c
            .Get("/api/care-levels")
            .Response(r2 => r2.OnSuccess<CareLevelsResponse>((json2, pipeline2) =>
            {
                pipeline2.Component<FusionDropDownList>(m => m.CareLevel)
                    .SetDataSource(json2, x => x.CareLevels)
                    .DataBind();
            }))));
```

The chained request fires **only on 2xx success** of the parent request. Inside `.Chained()`, you have the full request API -- `Get`/`Post`/`Put`/`Delete`, `.Gather()`, and `.Response()`.

### What does the cascading dropdown pattern look like?

A dropdown changes, the handler reads its value via `Include`, fetches dependent data, and populates a child dropdown:

```csharp
Html.InputField(m => m.Country, /* removed for brevity */)
    .Reactive(plan, evt => evt.Changed, (args, pipeline) =>
    {
        pipeline.Get("/api/cities")
            .Gather(g => g.Include<FusionDropDownList, LocationModel>(m => m.Country))
            .Response(r => r.OnSuccess<CityLookupResponse>((json, pipeline) =>
            {
                pipeline.Component<FusionDropDownList>(m => m.City)
                    .SetDataSource(json, x => x.Cities)
                    .DataBind();
            }));
    })
```

`DataBind()` is required after `SetDataSource` in cascade patterns to refresh the dropdown rendering.

## How do I fire multiple requests at once?

`Parallel` fires multiple requests concurrently and reacts when all complete:

```csharp
Html.On(plan, t => t.DomReady(pipeline =>
{
    pipeline.Element("loading").Show();
    pipeline.Parallel(
        a => a.Get("/api/facilities")
              .Response(r => r.OnSuccess<FacilitiesResponse>((json, pipeline) =>
              {
                  pipeline.Component<FusionDropDownList>(m => m.FacilityId)
                      .SetDataSource(json, x => x.Facilities)
                      .DataBind();
              })),
        b => b.Get("/api/care-levels")
              .Response(r => r.OnSuccess<CareLevelsResponse>((json, pipeline) =>
              {
                  pipeline.Component<FusionDropDownList>(m => m.CareLevel)
                      .SetDataSource(json, x => x.CareLevels)
                      .DataBind();
              }))
    )
    .OnAllSettled(pipeline =>
    {
        pipeline.Element("loading").Hide();
        pipeline.Element("form-section").Show();
    });
}));
```

### How does Parallel work?

Each branch is an independent request with its own response handlers. All branches fire at the same time. Pass as many branches as you need:

```csharp
pipeline.Parallel(
    a => a.Get("/api/facilities").Response(/* removed for brevity */),
    b => b.Get("/api/care-levels").Response(/* removed for brevity */),
    c => c.Get("/api/staff").Response(/* removed for brevity */)
);
```

### What is OnAllSettled?

`OnAllSettled` fires after **all** branches complete, regardless of whether individual branches succeeded or failed:

```csharp
// ... continued from a Parallel() call:
.OnAllSettled(pipeline =>
{
    pipeline.Element("loading").Hide();
    pipeline.Element("form-section").Show();
});
```

It accepts sequential commands only -- no conditions, HTTP requests, or parallel branches inside it.

### Can I run commands before the parallel requests?

Yes. Commands before `Parallel()` execute first:

```csharp
pipeline.Element("loading").Show();           // runs first
pipeline.Parallel(
    a => a.Get("/api/facilities").Response(...),
    b => b.Get("/api/staff").Response(...)
)
.OnAllSettled(pipeline => pipeline.Element("loading").Hide());
```

## How does Confirm work with HTTP requests?

Wrap the HTTP pipeline in a Confirm guard to require user approval before sending:

```csharp
@(Html.NativeButton("delete-btn", "Delete Resident")
    .CssClass("rounded-md bg-red-600 px-4 py-2 text-sm text-white")
    .Reactive(plan, evt => evt.Click, (args, pipeline) =>
    {
        pipeline.Confirm("Are you sure you want to delete resident #42?")
            .Then(then =>
            {
                then.Delete("/api/residents/42")
                    .WhileLoading(l => l.Element("spinner").Show())
                    .Response(r => r.OnSuccess(pipeline =>
                    {
                        pipeline.Element("spinner").Hide();
                        pipeline.Element("status").SetText("Deleted");
                    }));
            });
    }))
```

If the user cancels, the entire HTTP pipeline is skipped.

## Complete example: resident intake form

Here is a full form workflow -- load reference data, validate, submit, handle success/error:

```csharp
@{
    var plan = Html.ReactivePlan<ResidentIntakeModel>();

    // Load reference data on page load
    Html.On(plan, t => t.DomReady(pipeline =>
    {
        pipeline.Parallel(
            a => a.Get("/api/facilities")
                  .Response(r => r.OnSuccess<FacilitiesResponse>((json, pipeline) =>
                  {
                      pipeline.Component<FusionDropDownList>(m => m.FacilityId)
                          .SetDataSource(json, x => x.Facilities)
                          .DataBind();
                  })),
            b => b.Get("/api/care-levels")
                  .Response(r => r.OnSuccess<CareLevelsResponse>((json, pipeline) =>
                  {
                      pipeline.Component<FusionDropDownList>(m => m.CareLevel)
                          .SetDataSource(json, x => x.CareLevels)
                          .DataBind();
                  }))
        )
        .OnAllSettled(pipeline =>
        {
            pipeline.Element("loading").Hide();
            pipeline.Element("intake-form").Show();
        });
    }));
}
```

And the save button's pipeline:

```csharp
@(Html.NativeButton("submit-btn", "Submit Intake")
    .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white")
    .Reactive(plan, evt => evt.Click, (args, pipeline) =>
    {
        pipeline.Confirm("Submit this intake form?")
            .Then(then =>
            {
                then.Post("/api/residents", g => g.IncludeAll())
                    .Validate<ResidentIntakeValidator>("intake-form")
                    .WhileLoading(l => l.Element("spinner").Show())
                    .Response(r => r
                        .OnSuccess(pipeline =>
                        {
                            pipeline.Element("spinner").Hide();
                            pipeline.Element("status").SetText("Intake submitted");
                            pipeline.Element("status").AddClass("text-green-600");
                        })
                        .OnError(400, e =>
                        {
                            e.Element("spinner").Hide();
                            e.ValidationErrors("intake-form");
                        }));
            });
    }))
```

This single pipeline handles confirmation, validation, loading state, success notification, and error display. Every step is described in the plan. The runtime executes it exactly as written.

**Previous:** [Conditions](/csharp-modules/reactivity/conditions/) -- runtime branching with When/Then/ElseIf/Else and guard composition.
