---
name: validation-rules-alis-reactive
description: Guides validators for Alis.Reactive — server rules via FluentValidation, client rules recorded in the same call through ReactiveValidator<T>'s ClientRule(...), WhenField conditions, Shape inference, cross-property, dates. Use this skill when adding or modifying validators, validation views, or validation tests.
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
| Server-only rules | `AbstractValidator<T>` | `using FluentValidation;` |
| Client rules (`ClientRule(...)`) and WhenField conditions | `ReactiveValidator<T>` | `using Alis.Reactive.FluentValidator;` + `using FluentValidation;` |

## Client Rules — Quick Reference

> Client validation is recorded, not extracted. `ClientRule(x => x.Prop)`
> registers the server FluentValidation rule AND the client metadata in one
> call (`ReactiveValidator.ClientRule` wraps `RuleFor`; the extensions live in
> `ReactiveClientRuleBuilder.cs`). A plain `RuleFor(...)` is server-only by
> construction — nothing reaches the browser from it. Every extension takes
> the error message explicitly.

### Text (shape: "string", automatic)

```csharp
ClientRule(x => x.Name).Required("Name is required.");             // required — fails when empty
ClientRule(x => x.Name).MinLength(2, "Min 2 characters.");         // minLength — skips empty
ClientRule(x => x.Name).MaxLength(100, "Max 100 characters.");     // maxLength — skips empty
ClientRule(x => x.Email).Email("Enter a valid email.");            // email — skips empty
ClientRule(x => x.Phone).Regex(@"^\d{3}-\d{4}$", "Bad format.");   // regex — skips empty
ClientRule(x => x.Card).CreditCard("Enter a valid card.");         // creditCard — skips empty
ClientRule(x => x.Site).Url("Enter a valid URL.");                 // url — skips empty
ClientRule(x => x.Nickname).Empty("Must be empty.");               // empty — passes when empty
ClientRule(x => x.Status).NotEqual("deleted", "Already deleted."); // notEqual — skips empty
```

### Numeric (shape.kind: "number", automatic from int/decimal/etc.)

```csharp
ClientRule(x => x.Age).Range(0, 120, "0–120.");                    // range — boundaries included
ClientRule(x => x.Score).ExclusiveRange(0m, 100m, "0–100 excl."); // exclusiveRange — boundaries excluded
ClientRule(x => x.Salary).Min(0m, "No negatives.");                // min — skips empty
ClientRule(x => x.Salary).Max(500_000m, "Too high.");              // max — skips empty
ClientRule(x => x.Rate).GreaterThan(0m, "Must be positive.");      // gt — FAILS when empty (implies required)
ClientRule(x => x.Deposit).LessThan(1_000_000m, "Too high.");      // lt — skips empty

// Common pattern: required + range-bounded
ClientRule(x => x.Age).Required("Age is required.")
                      .Range(18, 120, "18–120.");
```

### Date (shape.kind: "date", automatic from DateTime/DateOnly/DateTimeOffset)

```csharp
ClientRule(x => x.Admission).Min(new DateTime(2020, 1, 1), "Too early.");   // min date
ClientRule(x => x.Discharge).GreaterThan(x => x.Admission, "After admission."); // gt cross-property
```

> **WARNING: `DateTime.Today` / `DateTime.Now` freezes at construction time.** The constraint value is captured when the validator constructor runs. If the validator is registered as a singleton in DI, the date is frozen at app startup. Use fixed dates (e.g., `new DateTime(2020, 1, 1)`) for client-side rules. For dynamic date constraints (e.g., "must be after today"), use server-only `.When()` guards instead.

### Cross-Property (peer auto-included in the plan)

```csharp
ClientRule(x => x.ConfirmEmail).EqualTo(x => x.Email, "Must match email.");      // equalTo — skips empty
ClientRule(x => x.AltEmail).NotEqualTo(x => x.Email, "Must differ.");            // notEqualTo — skips empty
ClientRule(x => x.End).GreaterThanOrEqualTo(x => x.Start, "After start.");       // min cross-property
ClientRule(x => x.End).GreaterThan(x => x.Start, "Strictly after start.");       // gt cross-property
ClientRule(x => x.Start).LessThan(x => x.End, "Before end.");                    // lt cross-property
ClientRule(x => x.Start).LessThanOrEqualTo(x => x.End, "Not after end.");        // max cross-property
```

### Server-Only

A plain `RuleFor(...)` rule is server-only by construction — nothing reaches
the browser unless `ClientRule(...)` records it. Rules with no client-side
equivalent stay `RuleFor`-only:

| Rule | Why server-only |
|------|-----------------|
| `Null()` | No client-side equivalent |
| `PrecisionScale()` | No client-side equivalent |
| `IsInEnum()` | No client-side equivalent |
| `IsEnumName()` | No client-side equivalent |
| `MustAsync()` / DB lookups | Async and server state stay on the server |

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
    () => { ClientRule(x => x.JobTitle).Required("Required when employed."); });

WhenFields(c => c.Field(x => x.CareLevel).Eq("memory-care")
                  .Or(c.Field(x => x.CareLevel).Eq("skilled-nursing")),
    () => { ClientRule(x => x.Notes).Required("Notes are required."); });

WhenFields(c => c.Field(x => x.IsEmployed).Truthy().Not(),
    () => { ClientRule(x => x.Notes).Required("Notes are required."); });

// Complex: (employed AND salary > 50k) OR age >= 65
WhenFields(c =>
    c.Field(x => x.IsEmployed).Truthy()
     .And(c.Field(x => x.Salary).Gt(50000m))
     .Or(c.Field(x => x.Age).Gte(65)),
    () => { ClientRule(x => x.Email).Required("Email is required."); });
```

> **Dual purpose:** Every WhenField* method registers a server-side FV `.When()` predicate and scopes the client-side `FieldCondition` onto the `ClientRule(...)` rules declared inside it. Plain `RuleFor` inside a WhenField stays server-only. FV's `.When()` still works for server-only conditions (DB lookups, service calls).

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
| `RuleFor(x).Empty()` alone | `ClientRule(x).Empty(message)` | client metadata is recorded through `ClientRule` (`ReactiveClientRuleBuilder.cs:89`), not extracted from FV |
| `RuleFor(x).ExclusiveBetween(a,b)` alone | `ClientRule(x).ExclusiveRange(a,b,message)` | same — `ReactiveClientRuleBuilder.cs:158` records server rule and client metadata together |
| `.When(x => x.Bool)` | `WhenField(x => x.Bool, () => {})` | `.When()` is server-only |
| Manual `min` rule without shape | Let adapter set it | Runtime throws without shape |
| `p.Element("input-id")` for inputs | `Html.InputField(plan, m => m.Prop)` | Element() is for display, not input |

## Empty Behavior

| Rule | When empty | Why |
|------|-----------|-----|
| `required` | **Fails** | That's the point |
| `empty` | **Passes** | Empty is the valid state |
| `gt` | **Fails** | gt implies required |
| All others | **Skips** | Use `required` separately for emptiness |

> **Nullable fields with `gt` become effectively required.** Because `gt` fails on empty, applying `ClientRule(x => x.End).GreaterThan(x => x.Start, ...)` to an optional `DateTime?` field makes it required on the client even if the model allows null. There is NO extractable mechanism for "validate only when the field has a value" — `.When(x => x.Field.HasValue, ...)` is server-only and not extracted. Workaround: use a different validation strategy for optional cross-property date fields (e.g., validate server-side only, or redesign the form so the field is always required when visible via `WhenField`).

## Fail-Closed

Nothing silently passes. Unknown rules block. Missing shape throws. Unresolvable peers block. Unenriched fields go to summary.

## Verification

After adding or modifying validation rules:

1. **Contract + build**: `npm run typecheck && dotnet build` — the rule's plan model regenerates `runtime/types/plan.ts` and compiles cleanly (no separate C# unit/schema harness exists)
2. **Build assets**: `npm run build:all` — confirm the browser bundles build with no errors
3. **Browser**: Open the form, submit invalid data, confirm rules fire client-side
4. **Playwright**: Run `scripts/playwright.sh` — confirm the BDD tests prove the validation behavior in the browser

If a rule does not fire in the browser but passes C# tests, check: (a) shape is set for numeric/date fields, (b) the form element has the correct `data-reactive-validation-summary` attribute, (c) the input was created with `Html.InputField()` not raw HTML.

## Full Guide

See the docs-site page `csharp-modules/reactivity/validation.md` for complete walkthrough with models, views, controllers, plan JSON examples, and date handling details.
