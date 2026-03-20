# Validation Rules Guide — Writing Rules on TModel

> **For developers writing FluentValidation validators that extract to client-side validation.**
> Every rule you write here runs BOTH on the server (FluentValidation) AND in the browser (TS rule engine).

## How It Works

```
C# Validator (FluentValidation)
    ↓ FluentValidationAdapter extracts rules
JSON Plan (validation descriptor)
    ↓ Runtime reads plan
TS Rule Engine (browser evaluation)
```

The adapter reads your FluentValidation rules at render time and serializes them into the plan.
The TS runtime evaluates them in the browser on submit and on blur/change (live re-validation).
You never write JS. The plan carries everything.

---

## Quick Reference — All 18 Rule Types

| # | C# DSL | Plan rule | Plan fields | Browser behavior |
|---|--------|-----------|-------------|------------------|
| 1 | `.NotEmpty()` | `required` | — | fails when empty/null/false |
| 2 | `.NotNull()` | `required` | — | fails when empty/null/false |
| 3 | `.IsEmpty()` | `empty` | — | fails when NOT empty |
| 4 | `.MinimumLength(N)` | `minLength` | `constraint: N` | skips empty |
| 5 | `.MaximumLength(N)` | `maxLength` | `constraint: N` | skips empty |
| 6 | `.EmailAddress()` | `email` | — | skips empty |
| 7 | `.Matches(pattern)` | `regex` | `constraint: "pattern"` | skips empty |
| 8 | `.CreditCard()` | `creditCard` | — | skips empty, Luhn check |
| 9 | `.InclusiveBetween(lo, hi)` | `range` | `constraint: [lo, hi], coerceAs` | skips empty, boundaries included |
| 10 | `.IsExclusiveBetween(lo, hi)` | `exclusiveRange` | `constraint: [lo, hi], coerceAs` | skips empty, boundaries excluded |
| 11 | `.GreaterThanOrEqualTo(val)` | `min` | `constraint: val, coerceAs` | skips empty |
| 12 | `.GreaterThanOrEqualTo(x => x.Prop)` | `min` | `field: "Prop", coerceAs` | skips empty |
| 13 | `.LessThanOrEqualTo(val)` | `max` | `constraint: val, coerceAs` | skips empty |
| 14 | `.LessThanOrEqualTo(x => x.Prop)` | `max` | `field: "Prop", coerceAs` | skips empty |
| 15 | `.GreaterThan(val)` | `gt` | `constraint: val, coerceAs` | **fails when empty (implies required)** |
| 16 | `.GreaterThan(x => x.Prop)` | `gt` | `field: "Prop", coerceAs` | **fails when empty (implies required)** |
| 17 | `.LessThan(val)` | `lt` | `constraint: val, coerceAs` | skips empty |
| 18 | `.LessThan(x => x.Prop)` | `lt` | `field: "Prop", coerceAs` | skips empty |
| 19 | `.Equal(x => x.Prop)` | `equalTo` | `field: "Prop"` | skips empty |
| 20 | `.Equal(val)` | `equalTo` | `constraint: val, coerceAs` | skips empty |
| 21 | `.NotEqual(x => x.Prop)` | `notEqualTo` | `field: "Prop"` | skips empty |
| 22 | `.NotEqual(val)` | `notEqual` | `constraint: val` | skips empty |

> **`.IsEmpty()` and `.IsExclusiveBetween()` are our custom extensions** (in `Alis.Reactive.FluentValidator.Validators`).
> FluentValidation's `.Empty()` and `.ExclusiveBetween()` are NOT extractable — use ours instead.

---

## coerceAs — Automatic Type-Aware Comparison

The adapter determines `coerceAs` from your **property type** at extraction time. You never set it manually.

| C# Property Type | coerceAs | Comparison behavior |
|-----------------|----------|---------------------|
| `int`, `long`, `decimal`, `double`, `float`, `byte`, `short`, `uint`, `ushort`, `ulong` | `"number"` | Numeric comparison |
| `DateTime`, `DateTime?`, `DateTimeOffset`, `DateTimeOffset?`, `DateOnly`, `DateOnly?` | `"date"` | Date comparison (ISO strings, Date objects) |
| `string`, all others | omitted | String comparison |

**Every comparison rule MUST have `coerceAs`.** The adapter sets it automatically. If you build a manual `ValidationDescriptor`, you must set it yourself — the runtime throws without it.

---

## Writing a Validator

### Basic Example — Resident Admission

```csharp
using FluentValidation;
using Alis.Reactive.FluentValidator.Validators; // for IsEmpty, IsExclusiveBetween

public class ResidentModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
    public int Age { get; set; }
    public decimal MonthlyRate { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime DischargeDate { get; set; }
}

public class ResidentValidator : AbstractValidator<ResidentModel>
{
    public ResidentValidator()
    {
        // Required
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Name).MinimumLength(2).WithMessage("Name must be at least 2 characters.");
        RuleFor(x => x.Name).MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        // Email + confirmation
        RuleFor(x => x.Email).EmailAddress().WithMessage("Valid email required.");
        RuleFor(x => x.ConfirmEmail).Equal(x => x.Email)
            .WithMessage("Emails must match.");

        // Numeric range
        RuleFor(x => x.Age).InclusiveBetween(0, 120)
            .WithMessage("Age must be between 0 and 120.");

        // gt implies required — rate must be positive
        RuleFor(x => x.MonthlyRate).GreaterThan(0m)
            .WithMessage("Monthly rate must be greater than zero.");

        // Date constraints
        RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020, 1, 1))
            .WithMessage("Admission date must be on or after January 1, 2020.");

        // Cross-property date comparison
        RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate)
            .WithMessage("Discharge date must be after admission date.");
    }
}
```

### What the Adapter Produces

For `RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate)`:

```json
{
  "rule": "gt",
  "message": "Discharge date must be after admission date.",
  "field": "AdmissionDate",
  "coerceAs": "date"
}
```

- `field: "AdmissionDate"` — the adapter reads `MemberToCompare.Name`
- `coerceAs: "date"` — the adapter reads `MemberToCompare.PropertyType` → `DateTime` → `"date"`
- No `constraint` — `field` and `constraint` are mutually exclusive
- The adapter automatically ensures `"AdmissionDate"` appears in the descriptor (even with zero rules)

For `RuleFor(x => x.MonthlyRate).GreaterThan(0m)`:

```json
{
  "rule": "gt",
  "message": "Monthly rate must be greater than zero.",
  "constraint": 0,
  "coerceAs": "number"
}
```

- `constraint: 0` — fixed comparison value
- `coerceAs: "number"` — from `typeof(decimal)` → `"number"`

---

## Conditional Rules — WhenField

Use `ReactiveValidator<T>` instead of `AbstractValidator<T>` to get client-extractable conditions.

```csharp
public class ConditionalResidentValidator : ReactiveValidator<ResidentModel>
{
    public ConditionalResidentValidator()
    {
        // Always required
        RuleFor(x => x.Name).NotEmpty();

        // Only when employed
        WhenField(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.JobTitle).NotEmpty()
                .WithMessage("Job title required when employed.");
            RuleFor(x => x.Salary).GreaterThan(0m)
                .WithMessage("Salary must be positive when employed.");
        });

        // Only when NOT employed
        WhenFieldNot(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.Salary).IsEmpty()
                .WithMessage("Salary must be empty when not employed.");
        });

        // When specific value
        WhenField(x => x.CareLevel, "Memory Care", () =>
        {
            RuleFor(x => x.EmergencyPhone).NotEmpty()
                .WithMessage("Emergency phone required for Memory Care.");
        });
    }
}
```

**Condition operators:** `truthy` (WhenField bool), `falsy` (WhenFieldNot bool), `eq` (WhenField string value), `neq` (WhenFieldNot string value).

**Standard `.When()` / `.Unless()` are server-only** — the adapter skips them. Only `WhenField` / `WhenFieldNot` extract to the client.

---

## Wiring in a View

```csharp
@{
    var plan = Html.ReactivePlan<ResidentModel>();
}

<form id="resident-form">
    @{ Html.InputField(plan, m => m.Name, o => o.Required().Label("Name"))
       .NativeTextBox(b => b.Placeholder("Full name")); }

    @{ Html.InputField(plan, m => m.AdmissionDate, o => o.Required().Label("Admission Date"))
       .DatePicker(b => b.Placeholder("Select date")); }
</form>

@(Html.NativeButton("save-btn", "Save")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Post("/Residents/Save")
         .Validate<ResidentValidator>("resident-form")
         .Response(r => r.OnSuccess(s => { /* handle success */ }));
    }))

@Html.RenderPlan(plan)
```

**Key points:**
- Every input MUST use `Html.InputField()` — this registers the component in `plan.components` for enrichment
- `.Validate<TValidator>("formId")` extracts rules and embeds them in the plan
- The runtime enriches validation fields from `plan.components` automatically (maps `fieldName` → `fieldId`, `vendor`, `readExpr`)
- Live re-validation on blur/change happens automatically after the first submit

---

## Server-Only Rules (Not Extracted)

These FluentValidation rules are NOT extractable and run server-side only:

| FV Method | Why server-only |
|-----------|-----------------|
| `.Must()` / `.MustAsync()` | Custom lambda — can't serialize to plan |
| `.Custom()` / `.CustomAsync()` | Custom validation context |
| `.When()` / `.Unless()` | Server-side conditions (use `WhenField` instead) |
| `.ForEach()` | Collection iteration |
| `.SetValidator()` (polymorphic) | Runtime type dispatch |
| `.PrecisionScale()` | Decimal precision — server concern |
| `.IsInEnum()` / `.IsEnumName()` | Enum membership — server concern |
| FV's `.Empty()` | No public interface — use `.IsEmpty()` instead |
| FV's `.ExclusiveBetween()` | No distinguishing interface — use `.IsExclusiveBetween()` instead |

---

## Custom Extensions (Ours)

These are in `Alis.Reactive.FluentValidator.Validators`. Use these instead of FV's equivalents.

```csharp
using Alis.Reactive.FluentValidator.Validators;

// Instead of .Empty()
RuleFor(x => x.Nickname).IsEmpty();

// Instead of .ExclusiveBetween(lo, hi)
RuleFor(x => x.Score).IsExclusiveBetween(0m, 100m);
```

**Why?** FluentValidation's `EmptyValidator` and `ExclusiveBetweenValidator` don't expose public interfaces. Our versions implement `IEmptyValidator` and `IExclusiveBetweenValidator` which the adapter matches directly — no reflection, no string matching.

---

## Date Handling

DateTime constraints are serialized as timezone-safe ISO strings:

| DateTime value | Serialized as | Parsed by browser |
|---------------|---------------|-------------------|
| `new DateTime(2020, 1, 1)` (midnight) | `"2020-01-01"` | Local midnight (no UTC shift) |
| `new DateTime(2020, 1, 1, 14, 30, 0)` (with time) | `"2020-01-01T14:30:00"` | Local datetime |

Syncfusion DatePicker returns `Date` objects. The rule engine's `coerce(value, "date")` handles both:
- `Date` objects → `getTime()` (milliseconds)
- `"YYYY-MM-DD"` strings → local midnight (avoids the off-by-one day UTC bug)
- ISO datetime strings → `new Date(str).getTime()`

---

## Cross-Property Rules

When a rule references another property (`x => x.OtherProp`), the adapter:

1. Sets `field: "OtherProp"` (the model property name)
2. Sets `coerceAs` from `OtherProp`'s property type
3. Ensures `"OtherProp"` appears in the validation descriptor (even with zero rules)

At runtime, the peer reader looks up `"OtherProp"` in the enriched field map → resolves `fieldId` → reads the component value via `resolveRoot` + `walk(readExpr)`.

**Both fields must be registered as components** (via `Html.InputField().NativeTextBox()` / `.DatePicker()` / etc.).
If the peer field isn't in the form, the rule fails closed (blocks the form).

---

## Empty Behavior Summary

| Rule | Empty value behavior |
|------|---------------------|
| `required` | **Fails** (that's the point) |
| `empty` | **Passes** (empty is valid) |
| `gt` | **Fails** (gt implies required) |
| All others | **Skips** (no error — `required` handles emptiness separately) |

This means: if a field is optional, don't add `required`. The comparison rules (`min`, `max`, `range`, etc.) will skip empty values automatically. Only `gt` implies the field must have a value.

---

## Fail-Closed Guarantee

- Unknown rule type → **blocks** (doesn't silently pass)
- Missing `coerceAs` on comparison rules → **throws** (no silent default)
- Peer field not in descriptor → **blocks** (can't read → fail-closed)
- Condition source not enriched → **blocks** (can't evaluate → assume true → rule applies)
- Broken regex pattern → **blocks** (exception caught → fail-closed)

Nothing silently passes. If something is wrong, validation blocks the form.
