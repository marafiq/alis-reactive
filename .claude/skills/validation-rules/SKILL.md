---
name: validation-rules-alis-reactive
description: Guides validators for Alis.Reactive — rules written once in FluentValidation's grammar on ReactiveValidator<T> (the framework implements the grammar; no FluentValidation package), run on the server and in the browser; When/Unless lambdas as browser conditions; children, collections, includes, peers, dates. Use this skill when adding or modifying validators, validation views, or validation tests.
---

# Validation Rules for Alis.Reactive

## When to Use This Skill

- Creating or changing a validator for a model
- Adding validation to a form view
- Writing Playwright tests for validation behavior
- Debugging why a rule does or does not run in the browser

## Written once

A validator derives from `ReactiveValidator<T>` (`using Alis.Reactive.FluentValidator;`) and is
registered through DI with `services.AddReactiveFluentValidation(v => v.AddFromAssemblyContaining<Program>())`.
Its rules are written in FluentValidation's grammar, which the framework implements itself: there is
no FluentValidation package. Each check defines what the server decides and, when the browser can
decide it the same way, the rule the page's plan carries (`Checks.cs`). The server runs every rule;
the browser runs every rule it can evaluate. There is no second vocabulary.

```csharp
public sealed class AdmissionValidator : ReactiveValidator<Admission>
{
    public AdmissionValidator()
    {
        RuleFor(x => x.ResidentName)
            .NotEmpty().WithMessage("'Resident Name' is required.")
            .MaximumLength(100).WithMessage("'Resident Name' must be at most 100 characters.");
        RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate);
        When(x => x.IsVeteran, () => RuleFor(x => x.ServiceNumber).NotEmpty());
        RuleFor(x => x.Address).SetValidator(new AddressValidator());
        RuleForEach(x => x.Contacts).SetValidator(new ContactValidator());
        Include(new SharedRules());
    }
}
```

## What the browser evaluates

The browser reads each input as MVC binds it (blank text is null; an empty non-nullable number,
boolean or date is its default: 0, false, 0001-01-01) and gives the server's own verdict on
that value. "Nothing bound" below is null: blank text, or an empty nullable input.

| Check | Browser rule | Nothing bound |
|---|---|---|
| `NotEmpty()` | `required` (`atLeastOne` on a collection); also fails on the type's default | fails |
| `NotNull()`, not on a collection | `required` | fails |
| `Empty()` | `empty` | passes |
| `MinimumLength(n)`, `MaximumLength(n)`, `Length(a, b)` | `minLength`, `maxLength` | passes |
| `EmailAddress()`: one `@`, something on each side | `email` | passes |
| `Matches(pattern)`, `Matches(pattern, options)` (`IgnoreCase`, `Multiline`, `Singleline` become flags; the browser runs it as a JavaScript `RegExp`, where `\w` is ASCII) | `regex` | passes |
| `CreditCard()`: dashes and spaces dropped, digits only, Luhn | `creditCard` | passes |
| `Url()`: absolute `http` or `https`, the scheme in any case | `url` | passes |
| `InclusiveBetween(a, b)`, `ExclusiveBetween(a, b)` | `range`, `exclusiveRange` | passes |
| `GreaterThan`, `GreaterThanOrEqualTo`, `LessThan`, `LessThanOrEqualTo` (a value or another member; another member with nothing bound fails) | `gt`, `min`, `lt`, `max` | passes |
| `Equal`, `NotEqual` (a value or another member) | `equalTo`, `notEqual` / `notEqualTo` | compared, not skipped |
| `SetValidator(child)`, `RuleForEach(...).SetValidator(child)`, `Include(v)`, `ChildRules(...)` | the child's rules, prefixed | |

An empty non-nullable number is 0, not nothing: `ExclusiveBetween(0, 100)` and `GreaterThan(0)` on
a `decimal` fail when it is left empty, as on the server.

Messages come from `.WithMessage(...)` or the default template (FluentValidation's English wording).
The server fills every placeholder (`{PropertyName}`, `{PropertyValue}`, `{ComparisonValue}`,
`{MaxLength}`, `{From}`, ..., with `{Name:format}`); the browser fills those known at render and drops
a sentence that names the entered value. A message with nothing else to say keeps its check on the
server.

An app's own check is an extension method that composes the checks above, so both sides run it
(MoveInReviewValidator.cs, `WristbandCode`):

```csharp
public static IRuleBuilderOptions<T, string?> WristbandCode<T>(this IRuleBuilder<T, string?> rule) =>
    rule.Matches(@"^WB-\d{6}$").WithMessage("Enter the wristband code as WB- and six digits, like WB-104233.");

RuleFor(x => x.WristbandCode).WristbandCode();
```

A comparison with another member (`GreaterThan(x => x.AdmissionDate)`) reads that member by its path
from the model, so it may be on another object (`x => x.Stay.AdmitDate`).

## Stays on the server

Nothing is listed: a rule stays on the server by what it is.

- `Must` and `MustAsync`: C# (a lookup, a service) the browser cannot run (analyzer ALIS005 notes it).
  A validator with a `MustAsync` rule validates with `ValidateAsync`; `Validate` refuses it.
- rules under a condition the browser cannot test: one that calls a method (`When(x => IsReturning(x), ..)`)
  or reads anything but members and fixed values
- a check whose message is written from the model (`WithMessage(x => $"... {x.Name}")`): the server says it
- patterns with `IgnorePatternWhitespace` or `RightToLeft`, and patterns with syntax JavaScript reads differently (`\A`, `\z`, `\G`, inline `(?i)`, `(?>...)`, `(?#...)`, `\p{..}`, class subtraction)
- a rule on a computed value (`RuleFor(x => x.Name.Trim())`): it has no input
- the checks before `.ServerOnly()`: use it for a member the page does not render

A browser rule on a member with no input on the page checks a missing value, as the server does
when the request does not carry the member: a required rule blocks the form and shows its message
in the validation summary (the rule may belong to a partial that has not loaded); a length or a
pattern passes. Mark a required rule `.ServerOnly()` when the server fills the member in. A peer or a `When` member with no input reads as empty, as the
server binds a member the request does not carry; render it as a hidden field (`o.Hidden()`) when
its value matters.

## Conditions

Write the condition as a lambda over the model; the browser applies the same one. A condition after a
rule's checks applies to the checks written before it.

```csharp
When(x => x.IsVeteran && new[] { "Memory Care", "Skilled Nursing" }.Contains(x.CareLevel), () => { ... });
When(x => x.Score > 7, () => { ... }).Otherwise(() => { ... });
Unless(x => string.IsNullOrEmpty(x.Portal), () => RuleFor(x => x.Portal).Url());
When(x => x.Notes != null && x.Notes.Contains("urgent"), () => { ... });
When(x => x.Tags.Contains("fall-risk"), () => { ... });
RuleFor(x => x.ReassessmentPlan).NotEmpty().When(x => x.FallRiskScore >= 5 && x.FallRiskScore <= 7);
```

Understood: `==`, `!=`, `<`, `<=`, `>`, `>=` between a member and a fixed value (constants and
captured variables); a bool member alone; `== null`, `!= null`; `string.IsNullOrEmpty`;
`Contains`, `StartsWith`, `EndsWith`, `Regex.IsMatch`; a fixed set's `Contains(member)`; a
collection member's `Contains(value)`; a nullable member's `HasValue` and `Value`; `&&`, `||`, `!`.
Enums compare by name. Anything else, including members of .NET's own values (`x.Name.Length`,
`x.Tags.Count`), keeps the enclosed rules on the server.

> **Rules are read when the page renders,** from the validator the request resolves: a scoped
> dependency, a captured setting or `DateTime.Today` is the one that request's server sees. A page
> left open past midnight keeps the date it rendered with.

## On the server

```csharp
var result = new MyValidator().Validate(model);       // ValidateAsync when a rule is MustAsync
if (!result.IsValid)
    return BadRequest(new { errors = result.ErrorsByField });
```

`ErrorsByField` groups the messages by the field's path (`Address.City`, `Lines[0].Qty`), the shape
`ValidationErrors(formId)` shows beside each field. To validate one model against two validators,
compose them: `var rules = new InlineValidator<T>(); rules.Include(a); rules.Include(b);`.

## Wiring in a view

```csharp
@{ var plan = Html.ReactivePlan<MyModel>(); }

<form id="my-form">
    @{ Html.InputField(plan, m => m.Name, o => o.Required().Label("Name"))
       .NativeTextBox(b => b.Placeholder("Name")); }
</form>

@(Html.NativeButton("save-btn", "Save")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Post("/Save")
         .Validate<MyValidator>("my-form")
         .Response(r => r
            .OnSuccess(s => { /* success */ })
            .OnError(400, e => e.ValidationErrors("my-form")));
    }))

@Html.RenderPlan(plan)
```

For a root plan, `RenderPlan` also writes the page's validation summary. It shows the errors that
have no message slot beside an input: a server error for a hidden field or a field not on the page,
and a rule for a partial's field that has not loaded yet.

## Verification

Playwright is the proof: open the form, submit invalid data, see the message; then pin it with a
BDD test through `scripts/playwright.sh`. The worked example is `/Sandbox/Validation/MoveInReview`:
one validator with a default message, `When`/`Otherwise`, `Unless`, a rule's own `.When`, a
condition that calls a method, a composed check and `.ServerOnly()`, pinned by
`WhenCoordinatorReviewsMoveIn`. If a rule does not run in the browser, check: it is one
the table lists; its condition is a lambda the browser understands; the form id passed to
`.Validate<TValidator>(...)` matches the form element; the input was created with
`Html.InputField()`.
