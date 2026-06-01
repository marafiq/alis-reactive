# validation

The validation DSL declares **browser** validation rules on a model by subclassing
`ReactiveValidator<T>` (which is itself a FluentValidation `AbstractValidator<T>`).
Every `ClientRule(...)` chain records two things at once: a server-side
FluentValidation rule (`RuleFor`) **and** a deterministic client-side rule that
lowers into the plan. FluentValidation stays the server authority; the reactive
validator mirrors a deterministic subset of it into the browser.

Rules are simple, deterministic, array- and object-capable, and bound by
controlled element IDs. Async / `MustAsync` / DB-touching rules stay server-only
via the FluentValidation `When`/`Unless` overrides — declaring a `ClientRule`
inside one of those throws.

Registration (once, at startup):

```csharp
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());
```

`AddReactiveFluentValidation` also accepts `.Add<TValidator>()` and
`.AddFromAssembly(assembly)`. Each `ReactiveValidator<T>` it discovers is wired
both as the FluentValidation validator and as the client-metadata source the
plan reads.

This catalog is exhaustive for: `ReactiveValidator<T>`, `ClientRule` (scalar,
`ClientRuleEach`, nested `ClientRule(field, validator)`, `ClientRulesFrom`),
every rule verb, the `WhenField*` conditional family, `WhenFields` composition
(And / Or / Not), server-only `When`/`Unless`, the HTTP `Validate` gate, and
`ValidationErrors` (the `ShowValidationErrors` reaction).

Source: `Alis.Reactive.FluentValidator/` (authoring surface),
`Alis.Reactive/Validation/` (rule model), `Alis.Reactive/PlanModel/Validation/`
(plan lowering).

> Naming note. The task brief used informal names (`WhenFieldGreaterThan`,
> `WhenFieldNull`, `NotEqualTo`, `ShowValidationErrors`). The finalized C# names
> are: `WhenFieldGt` / `WhenFieldGte` / `WhenFieldLt` / `WhenFieldLte`,
> `WhenFieldNull` / `WhenFieldNotNull`, `WhenFieldEmpty` / `WhenFieldNotEmpty`,
> `WhenFieldIn` / `WhenFieldNotIn`, `WhenFieldBetween`; comparison verbs
> `GreaterThan` / `LessThan` / `EqualTo` / `NotEqual` / `NotEqualTo` /
> `GreaterThanOrEqualTo` / `LessThanOrEqualTo` / `Min` / `Max`; and the pipeline
> reaction `ValidationErrors(formId)` (lowers to a `ShowValidationErrors` plan
> node). This catalog uses the finalized names.

## ReactiveValidator<T>

### ReactiveValidator<T>
Subclass this to declare a model's validation. The constructor records rules; `RuleFor(...)` is server-only, `ClientRule(...)` records server + client together.

```csharp
public class ResidentIntakeValidator
    : ReactiveValidator<ResidentIntakeModel>
{
    public ResidentIntakeValidator()
    {
        ClientRule(m => m.ResidentName)
            .Required("'Resident Name' is required.");

        ClientRule(m => m.PrimaryDiagnosis)
            .Required("'Primary Diagnosis' is required.");

        ClientRule(m => m.Age)
            .GreaterThan(0m, "'Age' must be greater than 0.");
    }
}
```

### RuleFor (server-only, alongside ClientRule)
A plain FluentValidation `RuleFor` runs on the server only — it carries no browser rule. Pair it with `ClientRule` to mirror it client-side.

```csharp
public class FacilityValidator : ReactiveValidator<FacilityModel>
{
    public FacilityValidator()
    {
        RuleFor(m => m.LicenseNumber)
            .NotEmpty()
            .Must(BeOnFileWithState);

        ClientRule(m => m.FacilityName)
            .Required("'Facility Name' is required.");
    }
}
```

## ClientRule

### ClientRule (scalar)
Begins a rule chain for one scalar model property. The property expression drives the deterministic element ID; verbs chain on the returned builder.

```csharp
ClientRule(m => m.RoomNumber)
    .Required("'Room Number' is required.")
    .MaxLength(6, "'Room Number' cannot exceed 6 characters.");
```

### ClientRule (nested object)
`ClientRule(field, childValidator)` runs a child `ReactiveValidator<TChild>` against a nested object. It calls `SetValidator` on the server and prefixes the child's client rules with the property path.

```csharp
ClientRule(m => m.EmergencyContact, new EmergencyContactValidator());
```

```csharp
public class EmergencyContactValidator
    : ReactiveValidator<EmergencyContactModel>
{
    public EmergencyContactValidator()
    {
        ClientRule(c => c.FullName)
            .Required("Emergency contact name is required.");

        ClientRule(c => c.Phone)
            .Regex(@"^\d{3}-\d{3}-\d{4}$", "Use format 555-123-4567.");
    }
}
```

### ClientRuleEach (collection)
Validates each element of a collection with an item validator, then `.SetValidator(...)` attaches it. `.AtLeastOne(...)` requires a non-empty collection.

```csharp
ClientRuleEach(m => m.Medications)
    .SetValidator(new MedicationLineValidator());
```

```csharp
ClientRuleEach(m => m.CarePlanTasks)
    .AtLeastOne("Add at least one care-plan task.")
    .SetValidator(new CarePlanTaskValidator());
```

### ClientRulesFrom (share rules into this model)
Pulls another validator's client rules into the current model at the root path. The `ReactiveValidator<T>` overload also includes the server rules (`Include`); the `ReactiveValidator<TSource>` overload copies client rules only.

```csharp
public class CombinedSectionValidator
    : ReactiveValidator<CombinedSection>
{
    public CombinedSectionValidator()
    {
        ClientRulesFrom(new BasicSectionValidator());

        ClientRule(m => m.Phone)
            .Regex(@"^\d{3}-\d{3}-\d{4}$", "Phone must match 123-456-7890.");
    }
}
```

## Rule verbs

Each verb returns the same `ReactiveClientRuleBuilder<TModel, TValue>`, so verbs chain. Every verb takes a trailing user-facing `message`.

### Required
Value must be present (server `NotEmpty`).

```csharp
ClientRule(m => m.ResidentName)
    .Required("'Resident Name' is required.");
```

### Empty
Value must be absent (server `Empty`).

```csharp
ClientRule(m => m.MiddleName)
    .Empty("Leave 'Middle Name' blank for this facility.");
```

### Email
String must be a valid email (`string?` only).

```csharp
ClientRule(m => m.ContactEmail)
    .Email("Enter a valid email address.");
```

### Url
String must be empty or an http(s) URL (`string?` only).

```csharp
ClientRule(m => m.FacilityWebsite)
    .Url("Enter a valid website URL.");
```

### CreditCard
String must be a valid card number (`string?` only).

```csharp
ClientRule(m => m.BillingCardNumber)
    .CreditCard("Enter a valid card number.");
```

### AtLeastOne
Collection / multi-select must have at least one item (server `NotEmpty`).

```csharp
ClientRule(m => m.SelectedServices)
    .AtLeastOne("Select at least one service.");
```

### MinLength
Minimum string length (`string?` only).

```csharp
ClientRule(m => m.CarePlanNotes)
    .MinLength(20, "Care-plan notes need at least 20 characters.");
```

### MaxLength
Maximum string length (`string?` only).

```csharp
ClientRule(m => m.RoomNumber)
    .MaxLength(6, "'Room Number' cannot exceed 6 characters.");
```

### Regex
String must match a pattern (`string?` only).

```csharp
ClientRule(m => m.MedicaidId)
    .Regex(@"^[A-Z]{2}\d{7}$", "Medicaid ID must be 2 letters then 7 digits.");
```

### Range (inclusive)
Value inclusive-between bounds. Works on `TValue` and `TValue?` (`IComparable`).

```csharp
ClientRule(m => m.Age)
    .Range(0, 120, "Age must be between 0 and 120.");
```

```csharp
ClientRule(m => m.MonthlyBilling)
    .Range(0m, 25000m, "Billing must be between 0 and 25,000.");
```

### ExclusiveRange
Value strictly between bounds. Works on `TValue` and `TValue?`.

```csharp
ClientRule(m => m.AssessmentScore)
    .ExclusiveRange(0, 100, "Score must be strictly between 0 and 100.");
```

### Min
Value at or above a minimum (alias of `GreaterThanOrEqualTo`). `TValue` / `TValue?`.

```csharp
ClientRule(m => m.MobilityScore)
    .Min(1, "Mobility score must be at least 1.");
```

### Max
Value at or below a maximum (alias of `LessThanOrEqualTo`). `TValue` / `TValue?`.

```csharp
ClientRule(m => m.FallRiskScore)
    .Max(10, "Fall-risk score cannot exceed 10.");
```

### GreaterThanOrEqualTo (literal)
Value `>=` a constant. `TValue` / `TValue?`.

```csharp
ClientRule(m => m.Salary)
    .GreaterThanOrEqualTo(0m, "Salary must be at least 0.");
```

### LessThanOrEqualTo (literal)
Value `<=` a constant. `TValue` / `TValue?`.

```csharp
ClientRule(m => m.Salary)
    .LessThanOrEqualTo(500000m, "Salary must be at most 500,000.");
```

### GreaterThan (literal)
Value `>` a constant. `TValue` / `TValue?`.

```csharp
ClientRule(m => m.PainLevel)
    .GreaterThan(0m, "'Pain Level' must be greater than 0.");
```

### LessThan (literal)
Value `<` a constant. `TValue` / `TValue?`.

```csharp
ClientRule(m => m.CareHoursPerDay)
    .LessThan(24m, "Care hours per day must be under 24.");
```

### EqualTo (literal)
Value must equal a constant.

```csharp
ClientRule(m => m.AcknowledgedPolicy)
    .EqualTo(true, "You must acknowledge the facility policy.");
```

### NotEqual (literal)
Value must not equal a constant.

```csharp
ClientRule(m => m.CareLevel)
    .NotEqual("Unassigned", "Assign a real care level before saving.");
```

### Chaining multiple verbs on one field
Verbs stack on the same property; each adds a server rule and a client rule.

```csharp
ClientRule(m => m.FullName)
    .Required("Name is required.")
    .MaxLength(100, "Name must be at most 100 characters.");
```

```csharp
ClientRule(m => m.Salary)
    .GreaterThanOrEqualTo(0m, "Salary must be at least 0.")
    .LessThanOrEqualTo(500000m, "Salary must be at most 500,000.");
```

## Cross-property (peer-field) verbs

These verbs accept a peer-field expression instead of a literal, comparing one property against another on the same model.

### EqualTo (peer field)
Field must equal another field — confirm-style checks.

```csharp
ClientRule(m => m.ConfirmEmail)
    .EqualTo(m => m.Email, "Emails must match.");
```

### NotEqualTo (peer field)
Field must differ from another field.

```csharp
ClientRule(m => m.SecondaryContact)
    .NotEqualTo(m => m.PrimaryContact, "Contacts must be different people.");
```

### GreaterThan (peer field)
Field must be greater than another field. `TValue` / `TValue?`.

```csharp
ClientRule(m => m.DischargeDate)
    .GreaterThan(m => m.AdmissionDate, "Discharge must be after admission.");
```

### GreaterThanOrEqualTo (peer field)
Field must be `>=` another field. `TValue` / `TValue?`.

```csharp
ClientRule(m => m.ReviewDate)
    .GreaterThanOrEqualTo(m => m.AdmissionDate, "Review cannot precede admission.");
```

### LessThan (peer field)
Field must be less than another field. `TValue` / `TValue?`.

```csharp
ClientRule(m => m.DepositAmount)
    .LessThan(m => m.MonthlyBilling, "Deposit must be less than monthly billing.");
```

### LessThanOrEqualTo (peer field)
Field must be `<=` another field. `TValue` / `TValue?`.

```csharp
ClientRule(m => m.CopayAmount)
    .LessThanOrEqualTo(m => m.MonthlyBilling, "Copay cannot exceed monthly billing.");
```

## WhenField (conditional rules)

`WhenField*` wraps a block of rules; the rules apply on the client only when the
named field's value satisfies the condition (server-side it lowers to
FluentValidation `When`). Inside the block, declare `RuleFor` + `ClientRule` as usual.

### WhenField (truthy bool)
Apply rules when a boolean field is true (or any value is truthy).

```csharp
WhenField(m => m.IsVeteran, () =>
{
    RuleFor(m => m.VaId).NotEmpty();
    ClientRule(m => m.VaId)
        .Required("'VA ID' is required.");
});
```

### WhenField (equals value)
Apply rules when a field equals a specific value.

```csharp
WhenField(m => m.CareLevel, "Hospice", () =>
{
    ClientRule(m => m.HospiceProvider)
        .Required("Hospice provider is required.");
});
```

### WhenFieldNot (falsy bool)
Apply rules when a boolean field is false (or value is falsy).

```csharp
WhenFieldNot(m => m.HasEmergencyContact, () =>
{
    ClientRule(m => m.AlternateContact)
        .Required("Provide an alternate contact.");
});
```

### WhenFieldNot (not-equals value)
Apply rules when a field does not equal a value.

```csharp
WhenFieldNot(m => m.CareLevel, "Independent", () =>
{
    ClientRule(m => m.CarePlanNotes)
        .Required("Care-plan notes are required for assisted residents.");
});
```

### WhenFieldGt
Apply rules when a numeric field is greater than a value.

```csharp
WhenFieldGt(m => m.CareHoursPerDay, 8m, () =>
{
    ClientRule(m => m.SecondaryCaregiver)
        .Required("A secondary caregiver is required.");
});
```

### WhenFieldGte
Apply rules when a numeric field is `>=` a value.

```csharp
WhenFieldGte(m => m.Age, 18, () =>
{
    ClientRule(m => m.JobTitle)
        .Required("Adults must provide a job title.");
});
```

### WhenFieldLt
Apply rules when a numeric field is less than a value.

```csharp
WhenFieldLt(m => m.Age, 18, () =>
{
    ClientRule(m => m.GuardianName)
        .Required("Guardian name required for minors.");
});
```

### WhenFieldLte
Apply rules when a numeric field is `<=` a value.

```csharp
WhenFieldLte(m => m.MobilityScore, 3, () =>
{
    ClientRule(m => m.MobilityNotes)
        .Required("Add mobility notes for low-mobility residents.");
});
```

### WhenFieldNull
Apply rules when a field is null.

```csharp
WhenFieldNull(m => m.DischargeDate, () =>
{
    ClientRule(m => m.ActiveCarePlan)
        .Required("An active care plan is required while the resident is admitted.");
});
```

### WhenFieldNotNull
Apply rules when a field is not null.

```csharp
WhenFieldNotNull(m => m.DischargeDate, () =>
{
    ClientRule(m => m.DischargeSummary)
        .Required("A discharge summary is required once a discharge date is set.");
});
```

### WhenFieldEmpty
Apply rules when a string field is empty (`string?`).

```csharp
WhenFieldEmpty(m => m.Email, () =>
{
    ClientRule(m => m.Phone)
        .Required("Provide a phone number when no email is on file.");
});
```

### WhenFieldNotEmpty
Apply rules when a string field is non-empty (`string?`).

```csharp
WhenFieldNotEmpty(m => m.Email, () =>
{
    ClientRule(m => m.FullName)
        .Required("Name is required when an email is provided.");
});
```

### WhenFieldIn
Apply rules when a field's value is in a set.

```csharp
WhenFieldIn(m => m.CareLevel, new[] { "memory-care", "skilled-nursing" }, () =>
{
    ClientRule(m => m.FallRiskPlan)
        .Required("A fall-risk plan is required for high-acuity care.");
});
```

### WhenFieldNotIn
Apply rules when a field's value is not in a set.

```csharp
WhenFieldNotIn(m => m.PaymentSource, new[] { "Medicaid", "Medicare" }, () =>
{
    ClientRule(m => m.SelfPayDetails)
        .Required("Self-pay details are required.");
});
```

### WhenFieldBetween
Apply rules when a numeric field is inclusive-between two bounds.

```csharp
WhenFieldBetween(m => m.Bmi, 30m, 40m, () =>
{
    ClientRule(m => m.DietitianReview)
        .Required("A dietitian review is required for this BMI range.");
});
```

### WhenFieldContains
Apply rules when a string field contains a substring (`string?`).

```csharp
WhenFieldContains(m => m.Notes, "urgent", () =>
{
    ClientRule(m => m.Phone)
        .Required("Phone is required for urgent cases.");
});
```

### WhenFieldStartsWith
Apply rules when a string field starts with a prefix (`string?`).

```csharp
WhenFieldStartsWith(m => m.RoomNumber, "ICU-", () =>
{
    ClientRule(m => m.CriticalCarePlan)
        .Required("A critical-care plan is required for ICU rooms.");
});
```

### WhenFieldEndsWith
Apply rules when a string field ends with a suffix (`string?`).

```csharp
WhenFieldEndsWith(m => m.ContactEmail, "@statehealth.gov", () =>
{
    ClientRule(m => m.CaseworkerId)
        .Required("A caseworker ID is required for state-managed residents.");
});
```

### WhenFieldMatches
Apply rules when a string field matches a regex (`string?`).

```csharp
WhenFieldMatches(m => m.MedicaidId, @"^TX", () =>
{
    ClientRule(m => m.TexasWaiverForm)
        .Required("Texas residents require the waiver form.");
});
```

### WhenFieldMinLength
Apply rules when a string field reaches a minimum length (`string?`).

```csharp
WhenFieldMinLength(m => m.CarePlanNotes, 1, () =>
{
    ClientRule(m => m.ReviewingNurse)
        .Required("A reviewing nurse is required once notes are entered.");
});
```

### WhenFieldArrayContains
Apply rules when a collection field contains a specific item.

```csharp
WhenFieldArrayContains(m => m.Allergies, "penicillin", () =>
{
    ClientRule(m => m.AlternateAntibiotic)
        .Required("Specify an alternate antibiotic for penicillin allergy.");
});
```

## WhenFields (composite guards)

`WhenFields` builds a guard from `fields.Field(...)` comparisons and composes
them with `.And(...)`, `.Or(...)`, `.Not()`. The leaf comparisons mirror the
`WhenField*` operators: `Truthy`/`Falsy`, `Eq`/`Neq`, `Gt`/`Gte`/`Lt`/`Lte`,
`IsNull`/`NotNull`, `IsEmpty`/`NotEmpty`, `In`/`NotIn`, `Between`, `Contains`/
`StartsWith`/`EndsWith`/`Matches`/`MinLength`, `ArrayContains`.

### WhenFields And
Apply rules only when all leaf conditions hold.

```csharp
WhenFields(fields => fields
    .Field(m => m.TakesPainMedication).Truthy()
    .And(fields.Field(m => m.PainLevel).Gt(7m)),
    () =>
    {
        RuleFor(m => m.PainLocation)
            .NotEmpty()
            .WithMessage("'Pain Location' is required for severe pain.");
        ClientRule(m => m.PainLocation)
            .Required("'Pain Location' is required for severe pain.");
    });
```

### WhenFields Or
Apply rules when any leaf condition holds.

```csharp
WhenFields(fields => fields
    .Field(m => m.HasGuardian).Truthy()
    .Or(fields.Field(m => m.Age).Lt(18)),
    () =>
    {
        ClientRule(m => m.GuardianSignature)
            .Required("A guardian signature is required.");
    });
```

### WhenFields Not
Apply rules when a leaf condition does not hold.

```csharp
WhenFields(fields => fields
    .Field(m => m.HasInsurance).Truthy().Not(),
    () =>
    {
        ClientRule(m => m.PrivatePayPlan)
            .Required("A private-pay plan is required.");
    });
```

### WhenFields In / Between leaf comparisons
Leaf operators take literals; `In`/`NotIn` are params arrays, `Between` takes two bounds.

```csharp
WhenFields(fields => fields
    .Field(m => m.CareLevel).In("memory-care", "skilled-nursing")
    .And(fields.Field(m => m.Bmi).Between(30m, 40m)),
    () =>
    {
        ClientRule(m => m.NutritionConsult)
            .Required("A nutrition consult is required.");
    });
```

## Server-only conditions

The FluentValidation `When`/`Unless`/`WhenAsync`/`UnlessAsync` are overridden so
their bodies stay server-only — declaring a `ClientRule` inside one throws. Use
these for async, DB-touching, or otherwise non-deterministic rules that must not
reach the browser. Use `WhenField*` instead when you want the condition mirrored
on the client.

### When (server-only)
Rules inside run on the server only, gated by a predicate.

```csharp
When(m => m.IsNewResident, () =>
{
    RuleFor(m => m.Ssn)
        .Must(BeUniqueInDatabase)
        .WithMessage("This SSN is already on file.");
});
```

### Unless (server-only)
Inverse of `When` — rules run unless the predicate holds.

```csharp
Unless(m => m.PaymentSource == "PrivatePay", () =>
{
    RuleFor(m => m.MedicaidId)
        .MustAsync(VerifyWithStateRegistry)
        .WithMessage("Medicaid ID could not be verified.");
});
```

### When + Otherwise (server-only)
The server-only `When` returns a condition builder whose `Otherwise` body also stays server-only.

```csharp
When(m => m.IsRespiteStay, () =>
{
    RuleFor(m => m.RespiteEndDate).NotNull();
})
.Otherwise(() =>
{
    RuleFor(m => m.LongTermCarePlanId).NotEmpty();
});
```

### WhenAsync / UnlessAsync (server-only)
Async predicates keep their rules server-only.

```csharp
WhenAsync(async (m, ct) => await HasOpenAssessmentAsync(m.ResidentId, ct), () =>
{
    RuleFor(m => m.AssessmentLockToken).NotEmpty();
});
```

## Validate gate (HTTP pipeline)

### Validate
HTTP-request gate: run the named validation source against the form before sending. The type argument is the validator/metadata source; `formId` is the form container's DOM ID used for error display.

```csharp
Html.On(plan, t => t.Event("submit-intake"))
    .Then(p => p
        .Post("/sandbox/admission/step1")
        .Validate<Step1Validator>("step1-form")
        .Gather(g => g
            .Include(m => m.ResidentName)
            .Include(m => m.PrimaryDiagnosis))
        .Response(r => r
            .OnSuccess(s => s.Dispatch("step1-saved"))
            .OnError(400, e => e.ValidationErrors("step1-form"))));
```

## ValidationErrors (ShowValidationErrors reaction)

### ValidationErrors
Reaction that renders accumulated validation errors into the form container. Typically wired on an `OnError(400, ...)` route so a server `400` validation response surfaces in the same summary slot as client rules.

```csharp
Html.On(plan, t => t.Event("submit-screening"))
    .Then(p => p
        .Post("/sandbox/admission/step4")
        .Validate<Step4Validator>("screening-form")
        .Gather(g => g
            .Include(m => m.EmergencyContact))
        .Response(r => r
            .OnSuccess(s => s.Dispatch("screening-saved"))
            .OnError(400, e => e
                .ValidationErrors("screening-form"))));
```
