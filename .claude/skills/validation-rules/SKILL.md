---
name: validation-rules-alis-reactive
description: Guides writing FluentValidation rules on TModel that extract to client-side validation in Alis.Reactive — 16 extractable rule types, coerceAs, cross-property, dates, WhenField conditions. Use this skill when adding or modifying validators, validation views, or validation tests.
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
| WhenField/WhenFieldNot conditions | `ReactiveValidator<T>` | `using Alis.Reactive.FluentValidator;` + `using FluentValidation;` |
| `.IsEmpty()` or `.IsExclusiveBetween()` | Add import | `using Alis.Reactive.FluentValidator.Validators;` |

## Extractable Rules — Quick Reference

> 16 rule types are extractable via FluentValidation. Two additional types (`url` and `atLeastOne`) exist in the schema and TS runtime but have no FluentValidation extraction path yet.

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

// Common pattern: required + range-bounded
RuleFor(x => x.Age).NotEmpty()                              // required — fails when empty
                    .InclusiveBetween(18, 120);              // range — only reached if not empty
```

### Date (coerceAs: "date" automatic from DateTime/DateOnly/DateTimeOffset)

```csharp
RuleFor(x => x.Admission).GreaterThanOrEqualTo(new DateTime(2020, 1, 1)); // min date
RuleFor(x => x.Discharge).GreaterThan(x => x.Admission);                  // gt cross-property
```

> **WARNING: `DateTime.Today` / `DateTime.Now` freezes at construction time.** The constraint value is captured when the validator constructor runs. If the validator is registered as a singleton in DI, the date is frozen at app startup. Use fixed dates (e.g., `new DateTime(2020, 1, 1)`) for client-side rules. For dynamic date constraints (e.g., "must be after today"), use server-only `.When()` guards instead.

### Cross-Property (field set automatically, peer auto-included in descriptor)

```csharp
RuleFor(x => x.ConfirmEmail).Equal(x => x.Email);            // equalTo — skips empty
RuleFor(x => x.AltEmail).NotEqual(x => x.Email);             // notEqualTo — skips empty
RuleFor(x => x.End).GreaterThanOrEqualTo(x => x.Start);      // min cross-property
RuleFor(x => x.End).GreaterThan(x => x.Start);               // gt cross-property
RuleFor(x => x.Start).LessThan(x => x.End);                  // lt cross-property
RuleFor(x => x.Start).LessThanOrEqualTo(x => x.End);        // max cross-property
```

### Server-Only (Not Extractable)

These rules are silently dropped by the adapter and only enforced server-side:

| Rule | Why not extractable |
|------|-------------------|
| `Null()` | No client-side equivalent |
| `PrecisionScale()` | No client-side equivalent |
| `IsInEnum()` | No client-side equivalent |
| `IsEnumName()` | No client-side equivalent |

## Conditional Rules

Uses `FieldCondition` — a tree type supporting all CompareOp operators and boolean composition (All/Any/Not). Requires `ReactiveValidator<T>` base class.

### Equality / Presence (original 4)

```csharp
WhenField(x => x.IsEmployed, () => { ... });                // truthy
WhenFieldNot(x => x.IsEmployed, () => { ... });             // falsy
WhenField(x => x.CareLevel, "Memory Care", () => { ... });  // eq
WhenFieldNot(x => x.CareLevel, "Independent", () => { ... }); // neq
```

### Ordering

```csharp
WhenFieldGt(x => x.Age, 18, () => { ... });          // gt
WhenFieldGte(x => x.Salary, 50000m, () => { ... });  // gte
WhenFieldLt(x => x.Age, 18, () => { ... });          // lt
WhenFieldLte(x => x.Salary, 0m, () => { ... });      // lte
```

### Presence (null/empty)

```csharp
WhenFieldNull(x => x.MiddleName, () => { ... });     // is-null
WhenFieldNotNull(x => x.MiddleName, () => { ... });  // not-null
WhenFieldEmpty(x => x.Email, () => { ... });          // is-empty
WhenFieldNotEmpty(x => x.Notes, () => { ... });       // not-empty
```

### Membership

```csharp
WhenFieldIn(x => x.CareLevel, new[] { "memory-care", "skilled-nursing" }, () => { ... });  // in
WhenFieldNotIn(x => x.CareLevel, new[] { "independent", "assisted" }, () => { ... });      // not-in
WhenFieldBetween(x => x.Age, 18, 65, () => { ... });  // between (inclusive)
```

### Text

```csharp
WhenFieldContains(x => x.Notes, "urgent", () => { ... });       // contains
WhenFieldStartsWith(x => x.Name, "Dr.", () => { ... });         // starts-with
WhenFieldEndsWith(x => x.Email, "@hospital.org", () => { ... });// ends-with
WhenFieldMatches(x => x.Phone, @"^\d{3}-", () => { ... });      // matches (regex)
WhenFieldMinLength(x => x.Notes, 10, () => { ... });            // min-length
```

### Array

```csharp
WhenFieldArrayContains(x => x.Tags, "urgent", () => { ... });   // array-contains
```

### Composition (And / Or / Not)

```csharp
WhenFields(c => c.Field(x => x.IsEmployed).Truthy()
                  .And(c.Field(x => x.Age).Gte(18)),
    () => { RuleFor(x => x.JobTitle).NotEmpty(); });

WhenFields(c => c.Field(x => x.CareLevel).Eq("memory-care")
                  .Or(c.Field(x => x.CareLevel).Eq("skilled-nursing")),
    () => { RuleFor(x => x.Notes).NotEmpty(); });

WhenFields(c => c.Field(x => x.IsEmployed).Truthy().Not(),
    () => { RuleFor(x => x.Notes).NotEmpty(); });

// Complex: (employed AND salary > 50k) OR age >= 65
WhenFields(c =>
    c.Field(x => x.IsEmployed).Truthy()
     .And(c.Field(x => x.Salary).Gt(50000m))
     .Or(c.Field(x => x.Age).Gte(65)),
    () => { RuleFor(x => x.Email).NotEmpty(); });
```

> **Dual purpose:** Every WhenField* method registers both a server-side FV `.When()` predicate and a client-side `FieldCondition` for browser evaluation. FV's `.When()` still works for server-only conditions (DB lookups, service calls).

> **Case sensitivity:** WhenField value comparison is case-sensitive. The condition value must exactly match what the component gathers.

> **Date serialization:** WhenField date condition values serialize as Unix milliseconds (via `ToUnixTimeMilliseconds()`), while rule constraint values use ISO `"yyyy-MM-dd"` format. Specify `DateTimeKind.Utc` explicitly to avoid timezone drift.

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

> **Nullable fields with `gt` become effectively required.** Because `gt` fails on empty, applying `.GreaterThan(x => x.Start)` to an optional `DateTime?` field makes it required on the client even if the model allows null. There is NO extractable mechanism for "validate only when the field has a value" — `.When(x => x.Field.HasValue, ...)` is server-only and not extracted. Workaround: use a different validation strategy for optional cross-property date fields (e.g., validate server-side only, or redesign the form so the field is always required when visible via `WhenField`).

## Fail-Closed

Nothing silently passes. Unknown rules block. Missing coerceAs throws. Unresolvable peers block. Unenriched fields go to summary.

## Verification

After adding or modifying validation rules:

1. **C# unit test**: `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests` — confirm rule extracts to expected JSON shape via `VerifyJson()` + `AssertSchemaValid()`
2. **Build**: `npm run build:all && dotnet build` — confirm no compilation errors
3. **Browser**: Open the form, submit invalid data, confirm rules fire client-side
4. **Playwright**: Run `dotnet test tests/Alis.Reactive.PlaywrightTests` — confirm BDD tests pass for validation behavior

If a rule does not fire in the browser but passes C# tests, check: (a) coerceAs is set for numeric/date fields, (b) the form element has the correct `data-reactive-validation-summary` attribute, (c) the input was created with `Html.InputField()` not raw HTML.

## Full Guide

See `docs/validation-rules-guide.md` for complete walkthrough with models, views, controllers, plan JSON examples, and date handling details.
