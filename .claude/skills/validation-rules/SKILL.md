---
name: validation-rules-alis-reactive
description: Write FluentValidation rules on TModel that extract to client-side validation in Alis.Reactive — 18 rule types, coerceAs, cross-property, dates, WhenField conditions. Use when adding/modifying validators, validation views, or validation tests.
---

# Validation Rules for Alis.Reactive

## When to Use This Skill

Use when:
- Creating a new FluentValidation validator for a model
- Adding validation to a form view
- Writing Playwright tests for validation behavior
- Debugging why a validation rule doesn't fire in the browser

## Base Classes

| Need | Use | Import |
|------|-----|--------|
| Unconditional rules only | `AbstractValidator<T>` | `using FluentValidation;` |
| WhenField/WhenFieldNot conditions | `ReactiveValidator<T>` | `using FluentValidation;` |
| `.IsEmpty()` or `.IsExclusiveBetween()` | Add import | `using Alis.Reactive.FluentValidator.Validators;` |

## Extractable Rules — Quick Reference

### Text (no coerceAs)

```csharp
RuleFor(x => x.Name).NotEmpty();                          // required — fails when empty
RuleFor(x => x.Name).MinimumLength(2);                    // minLength — skips empty
RuleFor(x => x.Name).MaximumLength(100);                  // maxLength — skips empty
RuleFor(x => x.Email).EmailAddress();                     // email — skips empty
RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{4}$");         // regex — skips empty
RuleFor(x => x.Card).CreditCard();                        // creditCard — skips empty
RuleFor(x => x.Nickname).IsEmpty();                       // empty — passes when empty
RuleFor(x => x.Status).NotEqual("deleted");               // notEqual — skips empty
```

### Numeric (coerceAs: "number" automatic from int/decimal/etc.)

```csharp
RuleFor(x => x.Age).InclusiveBetween(0, 120);             // range — boundaries included
RuleFor(x => x.Score).IsExclusiveBetween(0m, 100m);       // exclusiveRange — boundaries excluded
RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m);          // min — skips empty
RuleFor(x => x.Salary).LessThanOrEqualTo(500_000m);       // max — skips empty
RuleFor(x => x.Rate).GreaterThan(0m);                     // gt — FAILS when empty (implies required)
RuleFor(x => x.Deposit).LessThan(1_000_000m);             // lt — skips empty
```

### Date (coerceAs: "date" automatic from DateTime/DateOnly)

```csharp
RuleFor(x => x.Admission).GreaterThanOrEqualTo(new DateTime(2020, 1, 1)); // min date
RuleFor(x => x.Discharge).GreaterThan(x => x.Admission);                  // gt cross-property
```

### Cross-Property (field set automatically, peer auto-included in descriptor)

```csharp
RuleFor(x => x.ConfirmEmail).Equal(x => x.Email);            // equalTo — skips empty
RuleFor(x => x.AltEmail).NotEqual(x => x.Email);             // notEqualTo — skips empty
RuleFor(x => x.End).GreaterThanOrEqualTo(x => x.Start);      // min cross-property
RuleFor(x => x.End).GreaterThan(x => x.Start);               // gt cross-property
```

## Conditional Rules

```csharp
public class MyValidator : ReactiveValidator<MyModel>  // NOTE: ReactiveValidator, not AbstractValidator
{
    public MyValidator()
    {
        WhenField(x => x.IsEmployed, () => {                // truthy
            RuleFor(x => x.JobTitle).NotEmpty();
        });
        WhenFieldNot(x => x.IsEmployed, () => {             // falsy
            RuleFor(x => x.Salary).IsEmpty();
        });
        WhenField(x => x.CareLevel, "Memory Care", () => {  // eq
            RuleFor(x => x.EmergencyPhone).NotEmpty();
        });
        WhenFieldNot(x => x.CareLevel, "Independent", () => { // neq
            RuleFor(x => x.Physician).NotEmpty();
        });
    }
}
```

## Wiring in View

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
         .Response(r => r.OnSuccess(s => { /* success */ }));
    }))

<div data-reactive-validation-summary hidden></div>
@Html.RenderPlan(plan)
```

## DO NOT

| Wrong | Right | Why |
|-------|-------|-----|
| `RuleFor(x).Empty()` | `RuleFor(x).IsEmpty()` | FV's Empty has no interface |
| `RuleFor(x).ExclusiveBetween(a,b)` | `RuleFor(x).IsExclusiveBetween(a,b)` | FV can't distinguish from inclusive |
| `.When(x => x.Bool)` | `WhenField(x => x.Bool, () => {})` | `.When()` is server-only |
| Manual `min` rule without `coerceAs` | Let adapter set it | Runtime throws without coerceAs |
| `p.Element("input-id")` for inputs | `Html.InputField(plan, m => m.Prop)` | Element() is for display, not input |

## Empty Behavior

| Rule | When empty | Why |
|------|-----------|-----|
| `required` | **Fails** | That's the point |
| `empty` | **Passes** | Empty is the valid state |
| `gt` | **Fails** | gt implies required |
| All others | **Skips** | Use `required` separately for emptiness |

## Fail-Closed

Nothing silently passes. Unknown rules block. Missing coerceAs throws. Unresolvable peers block. Unenriched fields go to summary.

## Full Guide

See `docs/validation-rules-guide.md` for complete walkthrough with models, views, controllers, plan JSON examples, and date handling details.
