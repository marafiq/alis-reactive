---
title: Validation
description: Client-side validation with FluentValidation extraction — 16 rule types, conditional rules, cross-property comparisons, and fail-closed orchestration.
sidebar:
  order: 6
---

Validation lives inside the HTTP pipeline. You write a FluentValidation validator in C#, attach it to a request with `.Validate<T>()`, and the framework extracts rules to the JSON plan. The runtime evaluates those rules in the browser before the request fires.

From the [Grammar Tree](../../mental-model/#the-grammar-tree) — the validation subset:

```
pipeline.Post(url)
├── .Validate<TValidator>("formId")        § extract rules from FluentValidation
├── .Gather(g => { })                      § collect request data
├── .Response(r => { })                    § handle response
│   └── r.OnError(400, e => e.ValidationErrors("formId"))  § server errors
```

## How do I write a validator?

Extend `ReactiveValidator<T>` and use standard FluentValidation rules:

```csharp
public class ResidentIntakeValidator : ReactiveValidator<ResidentIntakeModel>
{
    public ResidentIntakeValidator()
    {
        RuleFor(x => x.ResidentName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdmissionDate).NotEmpty();
        RuleFor(x => x.CareLevel).NotEmpty();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

`ReactiveValidator<T>` extends `AbstractValidator<T>` — all standard FluentValidation methods work. The difference is that `ReactiveValidator<T>` also implements `IClientConditionSource`, which enables conditional rule extraction via `WhenField()`.

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

At render time, the framework extracts rules from `ResidentIntakeValidator` and embeds them in the plan JSON. At runtime, the browser evaluates those rules before sending the request. If validation fails, the request is aborted and errors appear at each field.

The `"intake-form"` string is the form ID — it must match the `id` attribute on the form container element. Errors display in each field's validation slot (rendered by `Html.InputField`).

`.OnError(400, e => e.ValidationErrors("intake-form"))` handles server-side validation errors — when the server returns 400 with validation messages, they are displayed at the matching fields.

## What rule types are available?

16 rule types are extracted from FluentValidation. One additional type (`atLeastOne`) is supported by the runtime but must be added manually via `ValidationDescriptor`.

### Presence rules

| FluentValidation | Plan rule | Description |
|------------------|-----------|-------------|
| `NotEmpty()` / `NotNull()` | `required` | Field must not be empty, null, or false |
| `Empty()` | `empty` | Field must be empty |

```csharp
RuleFor(x => x.ResidentName).NotEmpty();
```

### Length rules

| FluentValidation | Plan rule | Constraint |
|------------------|-----------|-----------|
| `MinimumLength(n)` | `minLength` | Minimum character count |
| `MaximumLength(n)` | `maxLength` | Maximum character count |

```csharp
RuleFor(x => x.ResidentName).MinimumLength(2).MaximumLength(100);
```

### Pattern rules

| FluentValidation | Plan rule | Description |
|------------------|-----------|-------------|
| `EmailAddress()` | `email` | Must match email format |
| `Matches(regex)` | `regex` | Must match the regular expression |
| `CreditCard()` | `creditCard` | Must pass Luhn check |

```csharp
RuleFor(x => x.Email).EmailAddress();
RuleFor(x => x.PhoneNumber).Matches(@"^\(\d{3}\) \d{3}-\d{4}$");
```

### Comparison rules

| FluentValidation | Plan rule | Description |
|------------------|-----------|-------------|
| `GreaterThanOrEqualTo(n)` | `min` | Value >= n (inclusive) |
| `LessThanOrEqualTo(n)` | `max` | Value <= n (inclusive) |
| `GreaterThan(n)` | `gt` | Value > n (exclusive) |
| `LessThan(n)` | `lt` | Value < n (exclusive) |

```csharp
RuleFor(x => x.Age).GreaterThanOrEqualTo(18).LessThanOrEqualTo(120);
RuleFor(x => x.Temperature).GreaterThan(95.0m).LessThan(107.0m);
```

### Range rules

| FluentValidation | Plan rule | Constraint |
|------------------|-----------|-----------|
| `InclusiveBetween(a, b)` | `range` | [min, max] inclusive |
| `ExclusiveBetween(a, b)` | `exclusiveRange` | (min, max) exclusive |

```csharp
RuleFor(x => x.Age).InclusiveBetween(18, 120);
RuleFor(x => x.Score).ExclusiveBetween(0, 100);
```

### Equality rules

| FluentValidation | Plan rule | Description |
|------------------|-----------|-------------|
| `Equal(x => x.Other)` | `equalTo` | Must equal another field's value |
| `NotEqual(value)` | `notEqual` | Must not equal a literal value |
| `NotEqual(x => x.Other)` | `notEqualTo` | Must not equal another field's value |

```csharp
RuleFor(x => x.PasswordConfirm).Equal(x => x.Password);
RuleFor(x => x.Status).NotEqual("Discharged");
```

### Array rules

| Rule | Plan rule | Description |
|------|-----------|-------------|
| (manual) | `atLeastOne` | Array must have at least one element |

The `atLeastOne` rule is not extracted from FluentValidation — add it manually when constructing a `ValidationDescriptor`. Used for multi-select fields like `NativeCheckList` or `FusionMultiSelect`.

## How do I add conditional rules?

Use `WhenField()` and `WhenFieldNot()` inside `ReactiveValidator<T>` to make rules depend on another field's value:

### Truthy — rule applies when field has a value

```csharp
WhenField(x => x.IsEmployed, () =>
{
    RuleFor(x => x.EmployerId).NotEmpty();
});
```

### Equality — rule applies when field equals a value

```csharp
WhenField(x => x.CareLevel, "Memory Care", () =>
{
    RuleFor(x => x.CognitiveScore).NotEmpty().InclusiveBetween(0, 30);
});
```

### Falsy — rule applies when field is empty

```csharp
WhenFieldNot(x => x.HasInsurance, () =>
{
    RuleFor(x => x.SelfPayAgreement).NotEmpty();
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

## How do cross-property rules work?

When a comparison rule references another property instead of a literal value, it becomes a cross-property rule:

```csharp
// Password confirmation must match
RuleFor(x => x.PasswordConfirm).Equal(x => x.Password);

// Discharge date must be after admission
RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate);
```

The plan carries a `field` property pointing to the peer field name. At runtime, the peer field's current value is read from the form and compared against the source field.

Cross-property comparisons support all comparison operators: `Equal`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqualTo`, `LessThan`, `LessThanOrEqualTo`.

## How does coercion work?

Comparison rules automatically infer a `coerceAs` type from the C# property type. This ensures numeric and date comparisons work correctly in the browser:

| C# property type | coerceAs | Runtime behavior |
|------------------|----------|-----------------|
| `int`, `decimal`, `double`, `long`, `float`, `byte`, `short`, `uint`, `ushort`, `ulong` | `"number"` | Both values parsed as numbers before comparison |
| `DateTime`, `DateTimeOffset`, `DateOnly` | `"date"` | Both values compared as ISO 8601 strings |
| `string` | omitted | Direct string comparison |

You never specify coercion manually — it is derived from the C# type at extraction time.

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

**Previous:** [HTTP Pipeline](../http-pipeline/) — GET/POST/PUT/DELETE, gather, loading states, typed responses, and chained/parallel requests.
