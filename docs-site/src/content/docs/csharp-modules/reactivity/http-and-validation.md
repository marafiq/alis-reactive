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
mounted. A value you gather yourself beside it keeps its place: an input fills
only its own member, so `.Static("Pharmacy.Network", "Riverside Health")` still
arrives next to an input bound to `Pharmacy.Phone`.

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
 .WhileLoading(l => l.Component<NativeLoader>().Call(c => c.Show))
 .Finally(f => f.Component<NativeLoader>().Call(c => c.Hide));
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

The validator is a FluentValidation validator that derives from `ReactiveValidator<T>`.
Rules are written once, in FluentValidation's grammar; the server runs them all, and the
browser runs every rule it can evaluate.

```csharp
public sealed class TodoValidator : ReactiveValidator<TodoModel>
{
    public TodoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("'Title' is required.");
    }
}
```

The browser evaluates `NotEmpty`, `NotNull` (not on a collection), `Empty`, `MinimumLength`, `MaximumLength`, `Length`,
`EmailAddress`, `Matches`, `CreditCard`, `Url()`, `InclusiveBetween`, `ExclusiveBetween`, and
comparisons with a value or a sibling member. `SetValidator`, `RuleForEach(...).SetValidator`,
`Include` and `ChildRules` bring a child validator's rules along. `Must`, `Custom` and async rules stay on
the server, and so do a rule whose message is written from the model (`WithMessage(x => ...)`),
a `Matches` pattern that JavaScript reads differently from .NET (`^\d{5}\z`, `(?i)...`),
and any rule followed by `.ServerOnly()`.

## Conditional Rules

Write the condition as a lambda over the model. The browser applies the same condition.

```csharp
When(x => x.IsUrgent, () =>
{
    RuleFor(x => x.DueDate).NotEmpty().WithMessage("Urgent todos need a due date.");
});
```

The browser understands comparisons of model members with values, `string.IsNullOrEmpty`,
`Contains`, `StartsWith`, `EndsWith`, `Regex.IsMatch` and membership in a fixed set, joined by
`&&`, `||` and `!`. `Otherwise` and `Unless` work the same way. A condition written as a method
group, over the validation context, or async keeps its rules on the server.

## Parallel Requests

Parallel branches start together.

```csharp
p.Parallel(
    a => a.Get("/lookups/units"),
    b => b.Get("/lookups/care-levels"))
 .OnAllSettled(done => done.Element("status").SetText("Lookups loaded"));
```

Use this when the branches are independent.
