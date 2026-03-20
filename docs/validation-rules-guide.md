# How to Add Validation to a Form

> Step-by-step guide. Follow in order. Every rule you write validates on the server AND in the browser automatically.

---

## Step 1: Create Your Model

Your model is a plain C# class with properties. Every property you want to validate must be here.

```csharp
namespace YourApp.Models
{
    public class ResidentModel
    {
        // Text fields — use string? (nullable) so NotEmpty can detect missing values
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ConfirmEmail { get; set; }
        public string? Phone { get; set; }

        // Numeric fields — the type determines how comparisons work in the browser
        public int Age { get; set; }           // coerceAs: "number" (automatic)
        public decimal MonthlyRate { get; set; } // coerceAs: "number" (automatic)

        // Date fields — compared as timestamps in the browser
        public DateTime AdmissionDate { get; set; }   // coerceAs: "date" (automatic)
        public DateTime DischargeDate { get; set; }    // coerceAs: "date" (automatic)

        // Boolean fields — used as condition sources (WhenField)
        public bool IsEmployed { get; set; }

        // Conditional fields
        public string? JobTitle { get; set; }
        public decimal Salary { get; set; }
    }
}
```

**Key:** The property TYPE matters. `int` → numeric comparison. `DateTime` → date comparison. `string` → string comparison. This is determined at extraction time, not runtime.

---

## Step 2: Create Your Validator

### Which Base Class?

| Base class | When to use |
|-----------|-------------|
| `AbstractValidator<T>` | All rules are unconditional |
| `ReactiveValidator<T>` | You need `WhenField` / `WhenFieldNot` conditional rules |

### Required Using Statements

```csharp
using FluentValidation;                              // Always needed
using Alis.Reactive.FluentValidator.Validators;       // Only if using IsEmpty or IsExclusiveBetween
```

### Unconditional Validator

```csharp
public class ResidentValidator : AbstractValidator<ResidentModel>
{
    public ResidentValidator()
    {
        // ── Text rules ────────────────────────────────────────

        // NotEmpty: value must not be null, empty string, or false
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Resident name is required.");

        // MinimumLength: value must have at least N characters (skips empty — NotEmpty handles that)
        RuleFor(x => x.Name).MinimumLength(2)
            .WithMessage("Name must be at least 2 characters.");

        // MaximumLength: value must have at most N characters (skips empty)
        RuleFor(x => x.Name).MaximumLength(100)
            .WithMessage("Name must be at most 100 characters.");

        // EmailAddress: must match basic email pattern (skips empty)
        RuleFor(x => x.Email).EmailAddress()
            .WithMessage("Enter a valid email address.");

        // Matches: must match regex pattern (skips empty)
        RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$")
            .WithMessage("Phone format: 123-456-7890.");

        // CreditCard: Luhn algorithm check (skips empty)
        RuleFor(x => x.CardNumber).CreditCard()
            .WithMessage("Enter a valid card number.");


        // ── Cross-property text rules ─────────────────────────

        // Equal(x => x.OtherProp): value must match another field (skips empty)
        // The OTHER field ("Email") is automatically included in the validation descriptor
        RuleFor(x => x.ConfirmEmail).Equal(x => x.Email)
            .WithMessage("Emails must match.");

        // NotEqual(x => x.OtherProp): value must differ from another field (skips empty)
        RuleFor(x => x.AlternateEmail).NotEqual(x => x.Email)
            .WithMessage("Alternate email must differ from primary.");

        // NotEqual("value"): value must not be a specific string (skips empty)
        RuleFor(x => x.Status).NotEqual("deleted")
            .WithMessage("Status cannot be 'deleted'.");


        // ── Numeric rules ─────────────────────────────────────
        // coerceAs: "number" is set AUTOMATICALLY because Age is int, Salary is decimal, etc.

        // InclusiveBetween: value must be >= lo AND <= hi (boundaries included, skips empty)
        RuleFor(x => x.Age).InclusiveBetween(0, 120)
            .WithMessage("Age must be between 0 and 120.");

        // IsExclusiveBetween: value must be > lo AND < hi (boundaries EXCLUDED, skips empty)
        // NOTE: Use our extension, NOT FluentValidation's .ExclusiveBetween()
        RuleFor(x => x.Score).IsExclusiveBetween(0m, 100m)
            .WithMessage("Score must be between 0 and 100 (exclusive).");

        // GreaterThanOrEqualTo: value must be >= N (skips empty)
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m)
            .WithMessage("Salary cannot be negative.");

        // LessThanOrEqualTo: value must be <= N (skips empty)
        RuleFor(x => x.Salary).LessThanOrEqualTo(500_000m)
            .WithMessage("Salary must be at most 500,000.");

        // GreaterThan: value must be > N (DOES NOT SKIP EMPTY — implies required)
        RuleFor(x => x.MonthlyRate).GreaterThan(0m)
            .WithMessage("Monthly rate must be greater than zero.");

        // LessThan: value must be < N (skips empty)
        RuleFor(x => x.MaxDeposit).LessThan(1_000_000m)
            .WithMessage("Deposit must be less than 1,000,000.");


        // ── Date rules ────────────────────────────────────────
        // coerceAs: "date" is set AUTOMATICALLY because AdmissionDate is DateTime.
        // DateTime constraints serialize as ISO strings: DateTime(2020,1,1) → "2020-01-01"

        // GreaterThanOrEqualTo(date): admission must be on or after Jan 1, 2020
        RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020, 1, 1))
            .WithMessage("Admission must be on or after January 1, 2020.");

        // GreaterThan(x => x.OtherDate): discharge must be AFTER admission (cross-property)
        RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate)
            .WithMessage("Discharge must be after admission date.");


        // ── Empty rule ────────────────────────────────────────
        // NOTE: Use our extension .IsEmpty(), NOT FluentValidation's .Empty()

        // IsEmpty: value MUST be empty (for conditional fields — see WhenField below)
        RuleFor(x => x.Nickname).IsEmpty()
            .WithMessage("Nickname must be empty.");
    }
}
```

### Conditional Validator (WhenField / WhenFieldNot)

```csharp
using Alis.Reactive.FluentValidator.Validators;

public class ResidentConditionalValidator : ReactiveValidator<ResidentModel>
{
    public ResidentConditionalValidator()
    {
        // ── Unconditional (always applies) ────────────────────
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Name is always required.");

        // ── WhenField(bool): rules apply when IsEmployed is true ──
        WhenField(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.JobTitle).NotEmpty()
                .WithMessage("Job title required when employed.");
            RuleFor(x => x.Salary).GreaterThan(0m)
                .WithMessage("Salary must be positive when employed.");
        });

        // ── WhenFieldNot(bool): rules apply when IsEmployed is false ──
        WhenFieldNot(x => x.IsEmployed, () =>
        {
            RuleFor(x => x.Salary).IsEmpty()
                .WithMessage("Salary must be empty when not employed.");
        });

        // ── WhenField(string, value): rules apply when CareLevel equals "Memory Care" ──
        WhenField(x => x.CareLevel, "Memory Care", () =>
        {
            RuleFor(x => x.EmergencyPhone).NotEmpty()
                .WithMessage("Emergency phone required for Memory Care.");
        });

        // ── WhenFieldNot(string, value): rules apply when CareLevel is NOT "Independent" ──
        WhenFieldNot(x => x.CareLevel, "Independent", () =>
        {
            RuleFor(x => x.PhysicianName).NotEmpty()
                .WithMessage("Physician required unless independent.");
        });
    }
}
```

---

## Step 3: Wire in the View

```csharp
@model YourApp.Models.ResidentModel
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.Native.Components
@{
    var plan = Html.ReactivePlan<ResidentModel>();
}

<form id="resident-form">
    @* Every input field MUST use Html.InputField() — this registers the component *@
    @* Without it, the runtime can't read the field's value for validation *@

    @{ Html.InputField(plan, m => m.Name, o => o.Required().Label("Resident Name"))
       .NativeTextBox(b => b.Placeholder("Full name")); }

    @{ Html.InputField(plan, m => m.Email, o => o.Required().Label("Email"))
       .NativeTextBox(b => b.Placeholder("nurse@facility.com")); }

    @{ Html.InputField(plan, m => m.ConfirmEmail, o => o.Label("Confirm Email"))
       .NativeTextBox(b => b.Placeholder("Confirm email")); }

    @{ Html.InputField(plan, m => m.Age, o => o.Label("Age"))
       .NativeTextBox(b => b.Type("number").Placeholder("0-120")); }

    @{ Html.InputField(plan, m => m.AdmissionDate, o => o.Required().Label("Admission Date"))
       .DatePicker(b => b.Placeholder("Select date")); }

    @{ Html.InputField(plan, m => m.DischargeDate, o => o.Required().Label("Discharge Date"))
       .DatePicker(b => b.Placeholder("Select date")); }
</form>

@* The button wires validation + HTTP post *@
@(Html.NativeButton("save-btn", "Save Resident")
    .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Post("/Residents/Save")
         .Validate<ResidentValidator>("resident-form")    @* ← extracts rules into plan *@
         .Response(r => r.OnSuccess(s =>
         {
             s.Element("result").SetText("Saved!");
         }));
    }))

<p id="result"></p>

@* Validation summary for unenriched/hidden field errors *@
<div data-reactive-validation-summary hidden></div>

@* MUST be last — renders the plan JSON *@
@Html.RenderPlan(plan)
```

**What happens when the user clicks "Save":**
1. Runtime reads ALL validation rules from the plan
2. For each field: reads the component value via `resolveRoot` + `walk(readExpr)`
3. Evaluates each rule (skips conditional rules whose condition is false)
4. If any rule fails: shows error inline next to the field (or in summary if hidden)
5. If all pass: sends the HTTP POST
6. After first submit: live re-validation on blur/change fires automatically

---

## Step 4: Handle Server Response

The controller endpoint handles server-side validation (for `.Must()`, `.When()`, and other server-only rules):

```csharp
[HttpPost]
public IActionResult Save([FromBody] ResidentModel? model)
{
    if (model == null)
        return BadRequest(new { errors = new { Name = new[] { "Request body required." } } });

    var validator = new ResidentValidator();
    var result = validator.Validate(model);

    if (!result.IsValid)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var failure in result.Errors)
            errors[failure.PropertyName] = new[] { failure.ErrorMessage };
        return BadRequest(new { errors });
    }

    return Ok(new { message = "Saved!" });
}
```

Server errors in the `{ errors: { "PropertyName": ["message"] } }` format are automatically routed to the correct inline error spans by the runtime.

---

## Common Mistakes

### DO NOT use FV's `.Empty()` or `.ExclusiveBetween()`

```csharp
// WRONG — FV has no public interface, adapter can't extract
RuleFor(x => x.Nickname).Empty();
RuleFor(x => x.Score).ExclusiveBetween(0m, 100m);

// CORRECT — our extensions with clean interfaces
using Alis.Reactive.FluentValidator.Validators;
RuleFor(x => x.Nickname).IsEmpty();
RuleFor(x => x.Score).IsExclusiveBetween(0m, 100m);
```

### DO NOT use `.When()` for client-side conditions

```csharp
// WRONG — .When() is server-only, adapter skips it entirely
RuleFor(x => x.JobTitle).NotEmpty().When(x => x.IsEmployed);

// CORRECT — use ReactiveValidator + WhenField
WhenField(x => x.IsEmployed, () =>
{
    RuleFor(x => x.JobTitle).NotEmpty();
});
```

### DO NOT forget `Html.InputField()` for peer fields

```csharp
// If ConfirmEmail has .Equal(x => x.Email), then BOTH fields must be
// registered via Html.InputField(). If Email is not in the form,
// the equalTo rule fails closed (blocks the form).

// The adapter auto-includes peer fields in the descriptor,
// but the component must be registered via InputField in the view.
```

### DO NOT use `Html.Element()` for input components

```csharp
// WRONG — Element() is for display elements, not input components
p.Element("name-field").SetText("value");

// CORRECT — use Html.InputField() which registers the component
Html.InputField(plan, m => m.Name, o => o.Label("Name"))
    .NativeTextBox(b => b.Placeholder("Name"));
```

### DO NOT build manual ValidationDescriptors without coerceAs

```csharp
// WRONG — comparison rules without coerceAs throw at runtime
new ValidationRule("min", "Too low", constraint: 0)

// CORRECT — always specify coerceAs for comparison rules
new ValidationRule("min", "Too low", constraint: 0, coerceAs: "number")
```

---

## Rule Type Reference — Complete

### Text Rules (no coerceAs needed)

| Rule | C# DSL | When empty |
|------|--------|-----------|
| `required` | `.NotEmpty()` or `.NotNull()` | **Fails** |
| `empty` | `.IsEmpty()` | **Passes** |
| `minLength` | `.MinimumLength(N)` | Skips |
| `maxLength` | `.MaximumLength(N)` | Skips |
| `email` | `.EmailAddress()` | Skips |
| `regex` | `.Matches(pattern)` | Skips |
| `creditCard` | `.CreditCard()` | Skips |
| `notEqual` | `.NotEqual("value")` | Skips |

### Comparison Rules (coerceAs set automatically from property type)

| Rule | C# DSL (fixed value) | C# DSL (cross-property) | When empty |
|------|---------------------|------------------------|-----------|
| `min` | `.GreaterThanOrEqualTo(val)` | `.GreaterThanOrEqualTo(x => x.Prop)` | Skips |
| `max` | `.LessThanOrEqualTo(val)` | `.LessThanOrEqualTo(x => x.Prop)` | Skips |
| `gt` | `.GreaterThan(val)` | `.GreaterThan(x => x.Prop)` | **Fails** |
| `lt` | `.LessThan(val)` | `.LessThan(x => x.Prop)` | Skips |
| `equalTo` | `.Equal(val)` | `.Equal(x => x.Prop)` | Skips |
| `notEqualTo` | — | `.NotEqual(x => x.Prop)` | Skips |

### Range Rules (coerceAs set automatically)

| Rule | C# DSL | When empty |
|------|--------|-----------|
| `range` | `.InclusiveBetween(lo, hi)` | Skips |
| `exclusiveRange` | `.IsExclusiveBetween(lo, hi)` | Skips |

---

## What the Plan Looks Like

For `RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate)`:

```json
{
  "rule": "gt",
  "message": "Discharge must be after admission date.",
  "field": "AdmissionDate",
  "coerceAs": "date"
}
```

- `field` = model property name of the peer field (NOT the DOM element ID)
- `coerceAs` = derived from `AdmissionDate`'s `DateTime` type → `"date"`
- `constraint` is absent — `field` and `constraint` are mutually exclusive
- The adapter auto-includes `"AdmissionDate"` in the descriptor

For `RuleFor(x => x.Age).InclusiveBetween(0, 120)`:

```json
{
  "rule": "range",
  "message": "Age must be between 0 and 120.",
  "constraint": [0, 120],
  "coerceAs": "number"
}
```

---

## Fail-Closed — Nothing Silently Passes

| What goes wrong | What happens |
|----------------|-------------|
| Unknown rule type in plan | Form blocked |
| Comparison rule without `coerceAs` | Runtime throws |
| Peer field not registered in form | Form blocked |
| Condition source field not in form | Rule applies (assumes condition is true) |
| Invalid regex pattern | Form blocked |
| Unenriched field with rules | Error → validation summary |

If validation is misconfigured, the form blocks. It never silently lets bad data through.
