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
     s.Element("status").Text.Set("Saved");
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
`TRequest`, so there is no parameter name to misspell. From the notify physician
page, where the directory search binds `Search([FromQuery] DirectorySearch search)`
and, inside the AutoComplete's `Filtering` event, `args.Text` is the text typed so far:

```csharp
p.Get(Url.SandboxRoute("Search", "AutoComplete"))
    .Gather<DirectorySearch>(g => g.Set(x => x.Name, args.Text))
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
var suite = p.Component<TomSelect>(m => m.SuiteType);

quote.Get(rateUrl)
    .Gather<SuiteQuery>(g => g.Set(x => x.SuiteType, suite.Value))
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
    public string? SlotId { get; set; }
}
```

From the front desk page, when the visit planner's week changes (`args` is that
event's payload, and `args.Added` the time just booked):

```csharp
added.Post(holdUrl)
    .Gather<HoldVisitRequest>(g => g
        .Set(x => x.SlotId, args.Added)
        .Set(x => x.VisitsThisWeek, added.Component<VisitPlanner>(m => m.VisitSlots).Count)
        .Set(x => x.Family, p.Url<FrontDeskQuery>().Get(q => q.Family))
        .Set(x => x.Source, "front-desk")
        .Header("X-Wing", added.Component<VisitPlanner>(m => m.VisitSlots).Wing))
```

`SlotId` fills `{slotId}` in the path, the other members are the JSON body, and
`X-Wing` is a header.

## What can a member be set from?

A literal, or any value: a component member (`suite.Value`), the event's
payload (`args.Added`), the page's URL query (`p.Url<TQuery>().Get(..)`), a
success body (`json.Get(..)`, in a request nested in `OnSuccess`), a plugin, or
an array value. `Header(name, value)` takes a literal or a value too.

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
p.Post("/Todo/Save").Gather(g => g.Form())
 .Response(r => r.OnSuccess<SaveResponse>((body, s) =>
 {
     s.Element("status").Text.Set(body.Get(x => x.Message));
 }));
```

`body.Get(x => x.Message)` reads one member of the body, and `body` itself is
the whole body. Either one is a value wherever the pipeline takes one: a
condition, a component property, a request member.

## Route Errors

Use exact status routes when a status has a known meaning.

```csharp
p.Post("/Todo/Save").Gather(g => g.Form())
 .Response(r => r
    .OnError(400, e => e.ValidationErrors("todo-form"))
    .OnError(e => e.Element("status").Text.Set("Save failed.")));
```

Exact status routes run before general error routes.

## Validate Before the Request

`Validate<TValidator>(formId)` runs client validation before the HTTP request.

```csharp
p.Post("/Todo/Save").Gather(g => g.Form())
 .Validate<TodoValidator>("todo-form")
 .Response(r => r
    .OnSuccess(s => s.Element("status").Text.Set("Saved"))
    .OnError(400, e => e.ValidationErrors("todo-form")));
```

The validator derives from `ReactiveValidator<T>` and is written in FluentValidation's grammar.
Rules are written once; the server runs them all, and the browser runs every rule it can evaluate.
You do not install FluentValidation: the framework implements its grammar, so each check knows
whether the browser can run it.

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
`EmailAddress`, `Matches`, `CreditCard`, `Url`, `InclusiveBetween`, `ExclusiveBetween`, and
comparisons with a value or another member. `SetValidator`, `RuleForEach(...).SetValidator`,
`Include` and `ChildRules` bring a child validator's rules along. `Must` and `MustAsync` stay on
the server, and so do a rule whose message is written from the model (`WithMessage(x => ...)`),
a `Matches` pattern that JavaScript reads differently from .NET (`^\d{5}\z`, `(?i)...`),
and any rule followed by `.ServerOnly()`.

### How do I write my own check?

Write an extension method that composes the checks above. The server and the browser both run it.
From the move-in review page:

```csharp
public static class WristbandRules
{
    /// <summary>The wristband printer reads WB- and six digits.</summary>
    public static IRuleBuilderOptions<T, string?> WristbandCode<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Matches(@"^WB-\d{6}$").WithMessage("Enter the wristband code as WB- and six digits, like WB-104233.");
}

RuleFor(x => x.WristbandCode).WristbandCode();
```

A check that needs C# (a database lookup, a service) is `Must` or `MustAsync`, and the server
alone runs it.

### What does the controller send back?

`Validate` returns the broken rules; `ErrorsByField` is what a 400 response sends for the page to
show beside each field. A validator with a `MustAsync` rule validates with `ValidateAsync`.

```csharp
var result = new TodoValidator().Validate(model);
if (!result.IsValid)
    return BadRequest(new { errors = result.ErrorsByField });
```

The browser gives each rule the answer the server gives. It reads a field the way MVC binds it, so
blank text is null and an empty `decimal` is 0: `ExclusiveBetween(0, 100)` on an empty `decimal`
fails in both places, and `GreaterThan` on an empty `DateTime?` passes in both. `EmailAddress` and
`CreditCard` are the server's own checks, not stricter browser versions.

## Conditional Rules

Write the condition as a lambda over the model. The browser applies the same condition.

```csharp
When(x => x.IsUrgent, () =>
{
    RuleFor(x => x.DueDate).NotEmpty().WithMessage("Urgent todos need a due date.");
});
```

A condition can also follow a rule's checks, and it applies to the checks written before it; a check
written after it is not under it. From the move-in review page, where a moderate score needs a
reassessment plan and every plan is kept short:

```csharp
RuleFor(x => x.ReassessmentPlan).NotEmpty().WithMessage("A moderate fall risk needs a reassessment plan.")
    .When(x => x.FallRiskScore >= 5 && x.FallRiskScore <= 7)
    .MaximumLength(120).WithMessage("Keep the reassessment plan under 120 characters.");
```

The browser understands comparisons of model members with values, `string.IsNullOrEmpty`,
`Contains`, `StartsWith`, `EndsWith`, `Regex.IsMatch` and membership in a fixed set, joined by
`&&`, `||` and `!`. `Otherwise` and `Unless` work the same way. A condition that calls a method,
or reads anything else, keeps its rules on the server.

## Parallel Requests

Parallel branches start together.

```csharp
p.Parallel(
    a => a.Get("/lookups/units"),
    b => b.Get("/lookups/care-levels"))
 .OnAllSettled(done => done.Element("status").Text.Set("Lookups loaded"));
```

Use this when the branches are independent.
