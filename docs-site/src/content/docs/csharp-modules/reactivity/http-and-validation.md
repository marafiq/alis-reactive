---
title: HTTP and Validation
description: How requests, gather, responses, loading state, and ReactiveValidator fit the plan.
---

HTTP is an async reaction in the pipeline.

`Gather` builds the request your action binds. The response routes success and
error outcomes back into normal pipelines.

## How do I send the form?

When the action binds the view model, gather the form:

```csharp
p.Post("/Todo/Save").Gather(g => g.Form())
 .Response(r => r.OnSuccess(s =>
 {
     s.Element("status").SetText("Saved");
 }));
```

`Form()` sends every input the view rendered through `Html.InputField` (and the
partials composed with it), while the input is on the page. Each input goes under
its binding name, the name MVC binds, so `Save([FromBody] TodoModel model)`
receives it.

`Form(m => m.Country, m => m.City)` sends only the inputs you name.

## How do I add a value the form does not carry?

`Set` the member beside the form:

```csharp
p.Post(Url.SandboxRoute("Save", "MoveInReview")).Gather(g => g.Form().Set(x => x.Pharmacy.Network, "Riverside Health"))
```

An input fills only its own member, so `Pharmacy.Network` still arrives next to
the form's `Pharmacy.Phone` input. When an input is bound to the same name as a
member you set, the input wins.

## What if the action binds another class?

Name that class with `Gather<TRequest>`. Each member is a typed lambda over
`TRequest`, so there is no parameter name to misspell:

```csharp
p.Get(Url.SandboxRoute("Medications", "AutoComplete"))
 .Gather<MedicationSearchQuery>(g => g.Set(x => x.MedicationType, p.Payload(args).Get(a => a.Text)))
```

`Form(m => ..)` works here too. Each input you name must also be a member of
`TRequest`, of a type the input's value binds to; otherwise the view fails when
it renders:

```csharp
then.Post(alertHypertensionUrl)
 .Gather<AlertHypertensionRequest>(g => g.Form(m => m.SystolicBP, m => m.ResidentName))
```

## How do I fill a route value?

Set the member named like the URL's `{placeholder}`; case does not matter:

```csharp
var rateUrl = Url.SandboxRouteTemplate("Rate", "TomSelect", "suiteType");

quote.Get(rateUrl)
    .Gather<SuiteQuery>(g => g.Set(x => x.SuiteType, suite.Get(x => x.Value)))
```

Every placeholder must be filled. A placeholder no member fills fails when the
view renders, and the message names it.

In ASP.NET Core, an action that binds a JSON body takes its route values as
their own parameters:

```csharp
[HttpPost("FrontDesk/Hold/{slotId}")]
public IActionResult HoldVisit(string slotId, [FromBody] HoldRequest? hold)
```

The page's request class is the body class plus the route members:

```csharp
public sealed class HoldVisitRequest : HoldRequest
{
    public string SlotId { get; set; } = "";
}
```

```csharp
added.Post(holdUrl)
    .Gather<HoldVisitRequest>(g => g
        .Set(x => x.SlotId, p.Payload(args).Get(a => a.Added))
        .Set(x => x.VisitsThisWeek, added.Component<VisitPlanner>(m => m.VisitSlots).Get(c => c.Count))
        .Set(x => x.Family, p.Url<FrontDeskQuery>().Get(q => q.Family))
        .Set(x => x.Source, "front-desk")
        .Header("X-Wing", added.Component<VisitPlanner>(m => m.VisitSlots).Get(c => c.Wing)))
```

`SlotId` fills `{slotId}` in the path, the other members are the JSON body, and
`X-Wing` is a header.

## What can a member be set from?

A literal, or any value: a component member, the event's payload
(`p.Payload(args).Get(..)`), the page's URL query (`p.Url<TQuery>().Get(..)`),
a success body (in a request nested in `OnSuccess`), a plugin, or an array
value. `Header(name, value)` takes a literal or a value too.

The compiler keeps the types honest: an `int?` value cannot fill an `int`
member, and an array fills an array, `List<T>` or `IEnumerable<T>` member.

## Where does each value go?

- A member named like a `{placeholder}` fills the path.
- For `GET`, the other members become the query string.
- For `POST`, `PUT`, and `DELETE`, they become the request body. JSON is the
  default; `AsFormData()` sends form data, which a request with files needs.
- Each key is the member's binding name (`SlotId`, `Pharmacy.Network`). MVC
  binds it ignoring case.

Setting the same member twice fails when the view renders.

## Loading and Cleanup

Use `WhileLoading(...)` for the in-flight state and `Finally(...)` for cleanup.

```csharp
p.Post("/Todo/Save").Gather(g => g.Form())
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
p.Post("/Todo/Save").Gather(g => g.Form())
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
p.Post("/Todo/Save").Gather(g => g.Form())
 .Response(r => r
    .OnError(400, e => e.ValidationErrors("todo-form"))
    .OnError(e => e.Element("status").SetText("Save failed.")));
```

Exact status routes run before general error routes.

## Validate Before the Request

`Validate<TValidator>(formId)` runs client validation before the HTTP request.

```csharp
p.Post("/Todo/Save").Gather(g => g.Form())
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

The browser gives each rule the answer the server gives. It reads a field the way MVC binds it, so
blank text is null and an empty `decimal` is 0: `ExclusiveBetween(0, 100)` on an empty `decimal`
fails in both places, and `GreaterThan` on an empty `DateTime?` passes in both. `EmailAddress` and
`CreditCard` are FluentValidation's own checks, not stricter browser versions.

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
