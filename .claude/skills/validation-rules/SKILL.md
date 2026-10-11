---
name: validation-rules-alis-reactive
description: Guides validators for Alis.Reactive — rules written once in FluentValidation's grammar on ReactiveValidator<T>, run on the server and projected to the browser; When/Unless lambdas as browser conditions; children, collections, includes, peers, dates. Use this skill when adding or modifying validators, validation views, or validation tests.
---

# Validation Rules for Alis.Reactive

## When to Use This Skill

- Creating or changing a FluentValidation validator for a model
- Adding validation to a form view
- Writing Playwright tests for validation behavior
- Debugging why a rule does or does not run in the browser

## Written once

A validator derives from `ReactiveValidator<T>` (`using Alis.Reactive.FluentValidator;` and
`using FluentValidation;`) and is registered through DI with
`services.AddReactiveFluentValidation(v => v.AddFromAssemblyContaining<Program>())`. Its rules are
plain FluentValidation. The server runs every rule; the browser runs every rule it can evaluate,
read from FluentValidation's own rule descriptors (`ClientRuleProjection.cs`). There is no second
vocabulary.

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

| FluentValidation | Browser rule | Empty value |
|---|---|---|
| `NotEmpty()` | `required` (`atLeastOne` on a collection) | fails |
| `NotNull()`, not on a collection (a cleared input is sent as null) | `required` | fails |
| `Empty()` | `empty` | passes |
| `MinimumLength(n)`, `MaximumLength(n)`, `Length(a, b)` | `minLength`, `maxLength` | skipped |
| `EmailAddress()` | `email` | skipped |
| `Matches(pattern)` | `regex` | skipped |
| `CreditCard()` | `creditCard` | skipped |
| `Url()` (this framework, `ReactiveRuleBuilderExtensions`) | `url` | skipped |
| `InclusiveBetween(a, b)`, `ExclusiveBetween(a, b)` | `range`, `exclusiveRange` | skipped |
| `GreaterThanOrEqualTo`, `LessThanOrEqualTo`, `LessThan` (value or sibling) | `min`, `max`, `lt` | skipped |
| `GreaterThan` (value or sibling) | `gt` | **fails** (today's behavior) |
| `Equal`, `NotEqual` (value or sibling) | `equalTo`, `notEqual` / `notEqualTo` | skipped |
| `SetValidator(child)`, `RuleForEach(...).SetValidator(child)`, `Include(v)`, `ChildRules(...)` | the child's rules, prefixed | |

Messages come from `.WithMessage(...)` or FluentValidation's default template, formatted at render
(`{PropertyName}`, `{MinLength}`, `{ComparisonValue}`, ...).

A peer comparison (`GreaterThan(x => x.AdmissionDate)`) must name a sibling member; FluentValidation
keeps only the member, so a peer on another object is refused at startup. Write that rule in the
validator of the object that owns both.

## Stays on the server

- `Must`, `Custom`, `MustAsync` and any other predicate or async rule (analyzer ALIS005 notes it)
- a rule's own `.When(...)` / `.Unless(...)` chained on the rule (analyzer ALIS006)
- rules inside a `When` written as a method group, over the validation context, or async
- rule sets other than the default, and `DependentRules`
- a child validator chosen per model by a factory (`SetValidator(x => ...)`) or by type (`SetInheritanceValidator`)
- a component whose message is written from the model (`WithMessage(x => $"... {x.Name}")`): the server says it
- any rule followed by `.ServerOnly()`: use it for a member the page does not render

A browser rule on a member with no input on the page blocks the form and shows its message in the
validation summary (the rule may belong to a partial that has not loaded). Mark it `.ServerOnly()`
when the server owns the member.

## Conditions

Write the condition as a lambda over the model; the browser applies the same one.

```csharp
When(x => x.IsVeteran && new[] { "Memory Care", "Skilled Nursing" }.Contains(x.CareLevel), () => { ... });
When(x => x.Score > 7, () => { ... }).Otherwise(() => { ... });
Unless(x => string.IsNullOrEmpty(x.Portal), () => RuleFor(x => x.Portal).Url());
When(x => x.Notes != null && x.Notes.Contains("urgent"), () => { ... });
When(x => x.Tags.Contains("fall-risk"), () => { ... });
```

Understood: `==`, `!=`, `<`, `<=`, `>`, `>=` between a member and a fixed value (constants and
captured variables); a bool member alone; `== null`, `!= null`; `string.IsNullOrEmpty`;
`Contains`, `StartsWith`, `EndsWith`, `Regex.IsMatch`; a fixed set's `Contains(member)`; a
collection member's `Contains(value)`; `&&`, `||`, `!`. Enums compare by name. Anything else keeps
the enclosed rules on the server.

> **`DateTime.Today` / `DateTime.Now` freeze at startup.** The browser's rules are read once, when
> the app starts. Use fixed dates for browser rules, or keep a moving bound on the server with
> `.ServerOnly()`.

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
one validator with a default message, `When`/`Otherwise`, `Unless`, a method-group condition and
`.ServerOnly()`, pinned by `WhenCoordinatorReviewsMoveIn`. If a rule does not run in the browser, check: it is one
the table lists; its condition is a lambda the browser understands; the form id passed to
`.Validate<TValidator>(...)` matches the form element; the input was created with
`Html.InputField()`.
