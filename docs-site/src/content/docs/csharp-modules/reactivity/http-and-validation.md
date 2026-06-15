---
title: HTTP and Validation
description: How requests, gather, responses, loading state, and ReactiveValidator fit the plan.
---

HTTP is an async reaction in the pipeline.

The request reads values through `Gather`. The response routes success and error
outcomes back into normal pipelines.

## Send a Request

```csharp
p.Post("/Todo/Save", g => g.IncludeAll())
 .Response(r => r.OnSuccess(s =>
 {
     s.Element("status").SetText("Saved");
 }));
```

`IncludeAll()` gathers every registered input component that is currently
mounted.

## Gather Specific Values

Gather can read literals, URL parameters, headers, event payloads, plugin values,
and component values.

```csharp
p.Post("/Residents/{residentId}/notes", g => g
    .RouteParam("residentId", p.FromUrl<int>("residentId"))
    .Header("X-Requested-With", "Alis.Reactive")
    .IncludeAll());
```

For `GET`, gathered payload values become query string values. For `POST`,
`PUT`, and `DELETE`, gathered payload values become the request body.

`AsJson()` is the default. Use `AsFormData()` when the request includes files.

## Loading and Cleanup

Use `WhileLoading(...)` for the in-flight state and `Finally(...)` for cleanup.

```csharp
p.Post("/Todo/Save", g => g.IncludeAll())
 .WhileLoading(l => l.Component<NativeLoader>().Show())
 .Finally(f => f.Component<NativeLoader>().Hide());
```

`Finally(...)` runs after success, error, or network failure.

## Route Success

Typed success bodies expose readable value sources.

```csharp
public sealed class SaveResponse
{
    public string Message { get; set; } = "";
}
```

```csharp
p.Post("/Todo/Save", g => g.IncludeAll())
 .Response(r => r.OnSuccess<SaveResponse>((body, s) =>
 {
     s.Element("status").SetText(body, x => x.Message);
 }));
```

Use `body.Read(...)` when the response value must feed a condition or another
value-consuming primitive.

## Route Errors

Use exact status routes when a status has a known meaning.

```csharp
p.Post("/Todo/Save", g => g.IncludeAll())
 .Response(r => r
    .OnError(400, e => e.ValidationErrors("todo-form"))
    .OnError(e => e.Element("status").SetText("Save failed.")));
```

Exact status routes run before general error routes.

## Validate Before the Request

`Validate<TValidator>(formId)` runs client validation before the HTTP request.

```csharp
p.Post("/Todo/Save", g => g.IncludeAll())
 .Validate<TodoValidator>("todo-form")
 .Response(r => r
    .OnSuccess(s => s.Element("status").SetText("Saved"))
    .OnError(400, e => e.ValidationErrors("todo-form")));
```

The validator must expose client metadata through `ReactiveValidator<T>`.

```csharp
public sealed class TodoValidator : ReactiveValidator<TodoModel>
{
    public TodoValidator()
    {
        ClientRule(x => x.Title)
            .Required("'Title' is required.");
    }
}
```

`ClientRule(...)` is the browser metadata path. Plain `RuleFor(...)` remains a
server-only FluentValidation rule.

## Conditional Client Rules

Use `WhenField(...)` when a condition must exist in both server and client
validation.

```csharp
WhenField(x => x.IsUrgent, () =>
{
    ClientRule(x => x.DueDate)
        .Required("Urgent todos need a due date.");
});
```

Use normal FluentValidation `When(...)`, `Unless(...)`, and async rules for
server-only validation.

## Parallel Requests

Parallel branches start together.

```csharp
p.Parallel(
    a => a.Get("/lookups/units"),
    b => b.Get("/lookups/care-levels"))
 .OnAllSettled(done => done.Element("status").SetText("Lookups loaded"));
```

Use this when the branches are independent.
