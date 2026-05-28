---
title: Validation
description: Client-side validation with explicit ReactiveValidator metadata, peer comparisons, conditional operators, and fail-closed orchestration.
sidebar:
  order: 8
---

Validation lives inside the HTTP pipeline. You write a `ReactiveValidator<T>` in C#, attach it to a request with `.Validate<T>()`, and declare browser rules explicitly with `ClientRule(...)`. FluentValidation still runs normally on the server. The runtime evaluates the declared browser rules before the request fires.

From the [Grammar Tree](../../mental-model/#the-grammar-tree) — the validation subset:

```
pipeline.Post(url)
├── .Validate<TValidator>("formId")        § bind declared browser rules
├── .Gather(g => { })                      § collect request data
├── .Response(r => { })                    § handle response
│   └── r.OnError(400, e => e.ValidationErrors("formId"))  § server errors
```

## How do I write a validator?

Extend `ReactiveValidator<T>`. Use standard FluentValidation rules for server validation, and add matching `ClientRule(...)` metadata for rules that must run in the browser:

```csharp
public class ResidentIntakeValidator : ReactiveValidator<ResidentIntakeModel>
{
    public ResidentIntakeValidator()
    {
        RuleFor(x => x.ResidentName).NotEmpty().MaximumLength(100);
        ClientRule(x => x.ResidentName)
            .Required("Resident name is required.")
            .MaxLength(100, "Resident name must be at most 100 characters.");

        RuleFor(x => x.AdmissionDate).NotEmpty();
        ClientRule(x => x.AdmissionDate)
            .Required("Admission date is required.");

        RuleFor(x => x.CareLevel).NotEmpty();
        ClientRule(x => x.CareLevel)
            .Required("Care level is required.");

        RuleFor(x => x.Age).InclusiveBetween(18, 120);
        ClientRule(x => x.Age)
            .Range(18, 120, "Age must be between 18 and 120.");

        RuleFor(x => x.Email).EmailAddress();
        ClientRule(x => x.Email)
            .Email("Email must be valid.");
    }
}
```

`ReactiveValidator<T>` extends `AbstractValidator<T>` — all standard FluentValidation methods work. Browser validation is explicit metadata declared with `ClientRule(...)`. Use `WhenField(...)` when the same rule needs a deterministic browser condition; regular `When`, `Unless`, and async rules stay server-only.

## How do I attach validation to a form?

Inside a pipeline, call `.Validate<TValidator>("formId")` on the HTTP request:

```csharp
@(Html.NativeButton("save-btn", "Save")
    .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white")
    .Reactive(plan, evt => evt.Click, (args, pipeline) =>
    {
        pipeline.Post("/api/residents", g => g.IncludeAll())
            .Validate<ResidentIntakeValidator>("intake-form")
            .WhileLoading(l => l.Element("spinner").Show())
            .Response(r => r
                .OnSuccess(p =>
                {
                    p.Element("spinner").Hide();
                    p.Element("status").SetText("Saved");
                })
                .OnError(400, e =>
                {
                    e.Element("spinner").Hide();
                    e.ValidationErrors("intake-form");
                }));
    }))
```

At render time, the framework reads browser metadata from `ResidentIntakeValidator` and embeds it in the plan JSON. At runtime, the browser evaluates those rules before sending the request. If validation fails, the request is aborted and errors appear at each field.

The `"intake-form"` string is the form ID — it must match the `id` attribute on the form container element. Errors display in each field's validation slot (rendered by `Html.InputField`).

`.OnError(400, e => e.ValidationErrors("intake-form"))` handles server-side validation errors — when the server returns 400 with validation messages, they are displayed at the matching fields.

## What rule types are available?

Browser rules are declared with `ClientRule(...)`. Peer-field comparisons use typed expressions so the browser rule names the peer field explicitly.

### Presence rules

| ClientRule method | Plan rule | Description |
|------------------|-----------|-------------|
| `Required(message)` | `required` | Field must not be empty, null, or false |
| `Empty(message)` | `empty` | Field must be empty |

```csharp
RuleFor(x => x.ResidentName).NotEmpty();
ClientRule(x => x.ResidentName).Required("Resident name is required.");
```

### Length rules

| ClientRule method | Plan rule | Constraint |
|------------------|-----------|-----------|
| `MinLength(n, message)` | `minLength` | Minimum character count |
| `MaxLength(n, message)` | `maxLength` | Maximum character count |

```csharp
RuleFor(x => x.ResidentName).MinimumLength(2).MaximumLength(100);
ClientRule(x => x.ResidentName)
    .MinLength(2, "Resident name must be at least 2 characters.")
    .MaxLength(100, "Resident name must be at most 100 characters.");
```

### Pattern rules

| ClientRule method | Plan rule | Description |
|------------------|-----------|-------------|
| `Email(message)` | `email` | Must match email format |
| `Regex(regex, message)` | `regex` | Must match the regular expression |
| `CreditCard(message)` | `creditCard` | Must pass Luhn check |

```csharp
RuleFor(x => x.Email).EmailAddress();
RuleFor(x => x.PhoneNumber).Matches(@"^\(\d{3}\) \d{3}-\d{4}$");
ClientRule(x => x.Email).Email("Email must be valid.");
ClientRule(x => x.PhoneNumber).Regex(@"^\(\d{3}\) \d{3}-\d{4}$", "Phone number is invalid.");
```

### Comparison rules

| ClientRule method | Plan rule | Description |
|------------------|-----------|-------------|
| `GreaterThanOrEqualTo(n, message)` | `min` | Value >= n (inclusive) |
| `LessThanOrEqualTo(n, message)` | `max` | Value <= n (inclusive) |
| `GreaterThan(n, message)` | `gt` | Value > n (exclusive) |
| `LessThan(n, message)` | `lt` | Value < n (exclusive) |

```csharp
RuleFor(x => x.Age).GreaterThanOrEqualTo(18).LessThanOrEqualTo(120);
RuleFor(x => x.Temperature).GreaterThan(95.0m).LessThan(107.0m);
ClientRule(x => x.Age)
    .GreaterThanOrEqualTo(18, "Age must be at least 18.")
    .LessThanOrEqualTo(120, "Age must be at most 120.");
```

### Range rules

| ClientRule method | Plan rule | Constraint |
|------------------|-----------|-----------|
| `Range(a, b, message)` | `range` | [min, max] inclusive |
| `ExclusiveRange(a, b, message)` | `exclusiveRange` | (min, max) exclusive |

```csharp
RuleFor(x => x.Age).InclusiveBetween(18, 120);
RuleFor(x => x.Score).ExclusiveBetween(0, 100);
ClientRule(x => x.Age).Range(18, 120, "Age must be between 18 and 120.");
ClientRule(x => x.Score).ExclusiveRange(0, 100, "Score must be between 0 and 100.");
```

### Equality rules

| ClientRule method | Plan rule | Description |
|------------------|-----------|-------------|
| `EqualTo(value, message)` | `equalTo` | Must equal a literal value |
| `EqualTo(x => x.Other, message)` | `equalTo` | Must equal another field's value |
| `NotEqual(value, message)` | `notEqual` | Must not equal a literal value |
| `NotEqualTo(x => x.Other, message)` | `notEqualTo` | Must not equal another field's value |

```csharp
RuleFor(x => x.PasswordConfirm)
    .Equal(x => x.Password);
ClientRule(x => x.PasswordConfirm)
    .EqualTo(x => x.Password, "Passwords must match.");
RuleFor(x => x.Status).NotEqual("Discharged");
ClientRule(x => x.Status)
    .NotEqual("Discharged", "Status is not allowed.");
```

### Array rules

| Rule | Plan rule | Description |
|------|-----------|-------------|
| `AtLeastOne(message)` | `atLeastOne` | Array must have at least one element |

Use `AtLeastOne(...)` for multi-select fields like `NativeCheckList` or `FusionMultiSelect`.

## How do I add conditional rules?

Use `WhenField()` and `WhenFieldNot()` inside `ReactiveValidator<T>` to make rules depend on another field's value:

### Truthy — rule applies when field has a value

```csharp
WhenField(x => x.IsEmployed, () =>
{
    RuleFor(x => x.EmployerId).NotEmpty();
    ClientRule(x => x.EmployerId).Required("Employer is required.");
});
```

### Equality — rule applies when field equals a value

```csharp
WhenField(x => x.CareLevel, "Memory Care", () =>
{
    RuleFor(x => x.CognitiveScore).NotEmpty().InclusiveBetween(0, 30);
    ClientRule(x => x.CognitiveScore)
        .Required("Cognitive score is required.")
        .Range(0, 30, "Cognitive score must be between 0 and 30.");
});
```

### Falsy — rule applies when field is empty

```csharp
WhenFieldNot(x => x.HasInsurance, () =>
{
    RuleFor(x => x.SelfPayAgreement).NotEmpty();
    ClientRule(x => x.SelfPayAgreement).Required("Self-pay agreement is required.");
});
```

### Inequality — rule applies when field does not equal a value

```csharp
WhenFieldNot(x => x.Status, "Discharged", () =>
{
    RuleFor(x => x.RoomNumber).NotEmpty();
});
```

At runtime, the condition is evaluated against the form's current values before applying the rule. If the condition field changes, the dependent rules re-evaluate on the next validation pass.

> **Server vs client conditions:** `WhenField()` extracts to both client and server. FluentValidation's standard `.When()` method only runs on the server — it is skipped during client extraction because it may depend on data not available in the browser (database lookups, service calls).

## What operators are available for conditional rules?

Beyond the truthy, equality, falsy, and inequality checks shown above, `WhenField*` methods cover 17 additional operators for a total of 21 — the same full set used by [runtime conditions](../conditions/). Each method takes a field expression, a threshold value (where applicable), and an `Action` that defines the conditional rules.

### Comparison operators

| Method | Condition |
|--------|-----------|
| `WhenFieldGt<TProp>(field, value, rules)` | Field > value |
| `WhenFieldGte<TProp>(field, value, rules)` | Field >= value |
| `WhenFieldLt<TProp>(field, value, rules)` | Field < value |
| `WhenFieldLte<TProp>(field, value, rules)` | Field <= value |

```csharp
WhenFieldGte(x => x.Age, 65, () =>
{
    RuleFor(x => x.MedicareId).NotEmpty();
});
```

### Presence operators

| Method | Condition |
|--------|-----------|
| `WhenFieldNull<TProp>(field, rules)` | Field is null |
| `WhenFieldNotNull<TProp>(field, rules)` | Field is not null |
| `WhenFieldEmpty(field, rules)` | Field is null or empty string |
| `WhenFieldNotEmpty(field, rules)` | Field is not null and not empty string |

`WhenFieldEmpty` and `WhenFieldNotEmpty` accept `Expression<Func<T, string>>` — they apply only to string properties.

```csharp
WhenFieldNotNull(x => x.DischargeDate, () =>
{
    RuleFor(x => x.DischargeReason).NotEmpty();
});
```

### Membership operators

| Method | Condition |
|--------|-----------|
| `WhenFieldIn<TProp>(field, values, rules)` | Field is one of the values |
| `WhenFieldNotIn<TProp>(field, values, rules)` | Field is none of the values |
| `WhenFieldBetween<TProp>(field, low, high, rules)` | Field is in [low, high] |

```csharp
WhenFieldIn(x => x.CareLevel, new[] { "Memory Care", "Skilled Nursing" }, () =>
{
    RuleFor(x => x.NursingLicense).NotEmpty();
});
```

### Text operators

All text operators accept `Expression<Func<T, string>>` — they apply only to string properties.

| Method | Condition |
|--------|-----------|
| `WhenFieldContains(field, substring, rules)` | Field contains substring |
| `WhenFieldStartsWith(field, prefix, rules)` | Field starts with prefix |
| `WhenFieldEndsWith(field, suffix, rules)` | Field ends with suffix |
| `WhenFieldMatches(field, pattern, rules)` | Field matches regex |
| `WhenFieldMinLength(field, length, rules)` | Field has at least N characters |

```csharp
WhenFieldStartsWith(x => x.RoomNumber, "MC-", () =>
{
    RuleFor(x => x.MemoryCareConsent).NotEmpty();
});
```

### Array operator

| Method | Condition |
|--------|-----------|
| `WhenFieldArrayContains<TProp>(field, value, rules)` | Array field contains the element |

```csharp
WhenFieldArrayContains(x => x.SelectedServices, "PhysicalTherapy", () =>
{
    RuleFor(x => x.TherapistName).NotEmpty();
});
```

## How do I compose multiple conditions?

Use `WhenFields()` to combine conditions with And, Or, and Not. The lambda receives a `FieldConditionBuilder<T>` — start with `.Field(x => x.Prop)`, pick an operator, then chain `.And()`, `.Or()`, or `.Not()`:

```csharp
WhenFields(c => c.Field(x => x.CareLevel).Eq("Memory Care")
                  .And(c.Field(x => x.Age).Gte(65)), () =>
{
    RuleFor(x => x.CognitiveScore).NotEmpty().InclusiveBetween(0, 30);
});
```

Both conditions must be true for the rules to apply. The composed condition extracts to the JSON plan as a nested `all`/`any`/`not` tree and evaluates in the browser the same way individual `WhenField` conditions do.

### Or — either condition

```csharp
WhenFields(c => c.Field(x => x.CareLevel).Eq("Memory Care")
                  .Or(c.Field(x => x.CareLevel).Eq("Skilled Nursing")), () =>
{
    RuleFor(x => x.NursingLicense).NotEmpty();
});
```

### Not — invert a condition

```csharp
WhenFields(c => c.Field(x => x.Status).Eq("Discharged").Not(), () =>
{
    RuleFor(x => x.RoomNumber).NotEmpty();
});
```

> **Relationship to runtime conditions:** `WhenField` and `WhenFields` conditions use the same operator set as [runtime conditions](../conditions/). The difference is scope: runtime conditions branch pipeline actions, while validation conditions guard which rules apply to a form submission.

## How do nested validators handle conditions?

When a validator uses `SetValidator()` for a nested property, conditions compose automatically:

```csharp
public class ResidentIntakeValidator : ReactiveValidator<ResidentIntakeModel>
{
    public ResidentIntakeValidator()
    {
        WhenField(x => x.HasInsurance, () =>
        {
            RuleFor(x => x.Insurance).SetValidator(new InsuranceValidator());
        });
    }
}

public class InsuranceValidator : ReactiveValidator<InsuranceInfo>
{
    public InsuranceValidator()
    {
        RuleFor(x => x.Provider).NotEmpty();

        WhenField(x => x.IsMedicare, () =>
        {
            RuleFor(x => x.MedicareId).NotEmpty();
            ClientRule(x => x.MedicareId).Required("Medicare ID is required.");
        });
    }
}
```

The declared browser rules behave as follows:

- **Condition composition:** The `Insurance.Provider` rule gets the parent's condition (`HasInsurance` is truthy). The `Insurance.MedicareId` rule gets both conditions composed with AND: `HasInsurance` is truthy AND `Insurance.IsMedicare` is truthy.
- **Prefix carrying:** Condition fields and explicit peer fields inside the nested validator are prefixed. `IsMedicare` in the nested validator becomes `Insurance.IsMedicare` in the plan, so the browser reads the correct field.
- **Include rules:** When using `Include()` to pull in a shared validator, the parent's condition passes through to the included rules the same way.

## How do cross-property rules work?

When a comparison rule references another property instead of a literal value, declare the browser peer comparison with `ClientRule(...)`. The FluentValidation rule still runs on the server:

```csharp
// Password confirmation must match
RuleFor(x => x.PasswordConfirm)
    .Equal(x => x.Password);
ClientRule(x => x.PasswordConfirm)
    .EqualTo(x => x.Password, "Passwords must match.");

// Discharge date must be after admission
RuleFor(x => x.DischargeDate)
    .GreaterThan(x => x.AdmissionDate);
ClientRule(x => x.DischargeDate)
    .GreaterThan(x => x.AdmissionDate, "Discharge must be after admission.");
```

The plan carries a `field` property pointing to the peer field name. At runtime, the peer field's current value is read from the form and compared against the source field.

Cross-property comparisons support all comparison operators: `Equal`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqualTo`, `LessThan`, `LessThanOrEqualTo`.

## How does type inference work?

Comparison rules automatically infer a `shape` from the C# property type via `Shape.FromClrType()`. This ensures numeric and date comparisons work correctly in the browser:

| C# property type | shape.kind | Runtime behavior |
|------------------|------------|-----------------|
| `string` | `"string"` | Direct string comparison |
| `int`, `decimal`, `double`, `long`, `float`, `byte`, `short`, `uint`, `ushort`, `ulong` | `"number"` | Both values parsed as numbers before comparison |
| `bool` | `"boolean"` | Boolean comparison |
| `DateTime`, `DateTimeOffset`, `DateOnly` | `"date"` | Both values compared as dates |
| `string[]`, `List<string>` | `"array"` | Array with element shape |

In the JSON plan, shape is a nested object (e.g., `{ "kind": "number" }`) rather than a flat string. You never specify shape manually — it is derived from the C# type at extraction time.

## What happens when validation fails?

The validation orchestrator uses a **fail-closed** design — when in doubt, block the request.

### Enriched, visible fields

Errors display inline at the field's validation slot (the `<span>` rendered by `Html.InputField`). The field gets an `alis-has-error` CSS class for styling.

### Enriched, hidden fields

If a field is hidden (e.g., inside a collapsed section), errors go to the **validation summary** instead of inline. The summary is a `<div>` rendered by `Html.RenderPlan()`.

### Unenriched fields with rules

If a field has rules but was not registered in the plan's `ComponentsMap` (not rendered via `Html.InputField`), the first rule's error message appears in the summary. This is fail-closed — the validation blocks rather than silently skipping.

### Missing form container

If the form element with the matching ID does not exist in the DOM, validation returns `false` and blocks the request entirely.

## How does live clearing work?

Validation errors clear as the user interacts with fields:

- **On input** (native components): The error clears immediately, giving responsive feedback
- **On blur/change** (all components): The field re-validates with current rules, showing a new error if the value is still invalid

For Syncfusion components, only blur/change is wired (Syncfusion does not expose a native `input` event).

Live clearing is wired automatically for all enriched fields — no additional configuration needed.

**Previous:** [Plugin System](../plugins/) — register JS plugins, read method results, call void methods, and compose plugin chains.
