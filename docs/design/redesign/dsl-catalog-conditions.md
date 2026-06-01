# conditions

Conditional routing in the reactive pipeline. A guard evaluates a deterministic
`ConditionGraph` over a typed value source, then routes execution down the
first matching branch. `When/Then/ElseIf/Else` is the first-match decision form;
`Confirm` is the user-decision async gate.

The surface is a tall fluent chain that reads like a sentence:

```
p.When(SOURCE).OPERATOR [.And/.Or/.Not]
    .Then(t => PIPELINE)
   [.ElseIf(SOURCE).OPERATOR.Then(t => PIPELINE)]   -- repeatable
   [.Else(e => PIPELINE)]                            -- terminal (void)
```

Every operator returns a `GuardBuilder<TModel>`; `.Then(...)` returns a
`BranchBuilder<TModel>` for `ElseIf`/`Else`. `Confirm` is its own guard that
goes straight to `.Then(...)`.

This catalog is exhaustive for: `When/Then/ElseIf/Else`; every compare operator
(`Eq/NotEq/Gt/Gte/Lt/Lte/In/NotIn/Between/Contains/StartsWith/EndsWith/Matches/MinLength/MaxLength/ArrayContains`);
the 6 presence operators (`Truthy/Falsy/IsNull/NotNull/IsEmpty/NotEmpty`);
guard composition (`And`/`Or`/`Not`, flat and nested); source variances
(event args, component read, source-vs-source, URL, plugin); and `Confirm`.

All samples are TALL — one fluent call per line, read top-to-bottom. Senior-living
domain throughout (residents, care levels, facilities, billing, assessments).

> **Source note.** `MaxLength` is the planned condition compare token (`max-length`,
> VO `MaximumTextLength` mirroring `MinimumTextLength`) called for in the redesign
> naming sheet §1.5 to sit beside the existing `MinLength`. It is documented here as
> part of the target surface; every other operator below is present in current source.

---

## Sources — where the value comes from

Three `When` overloads cover every value source. The operator's operand type is
inferred from the source's `TProp`, so comparisons are compile-time typed.

### When(args, x => x.Prop) — event payload

Inside a `.Reactive()` or `Event<T>` handler the event args object is the source.
`TProp` flows from the selected property.

```csharp
Html.On(plan, t => t.Event(m => m.CareLevel, "change"))
    .When(args, x => x.Value)
    .Eq("MemoryCare")
    .Then(p => p
        .Set(m => m.SecuredUnitRequired, true)
        .Dispatch("care-level-elevated"));
```

### When(component.Read()) — component read

Grab a typed component reference, then read one of its members as the source.

```csharp
var budget = p.Component<FusionNumericTextBox>(m => m.MonthlyBudget);

Html.On(plan, t => t.Event(m => m.MonthlyBudget, "change"))
    .When(budget.Value())
    .Gte(8000m)
    .Then(p => p
        .Set(m => m.RecommendedTier, "Premium"));
```

### When(p.FromUrl<T>(...)) — URL query parameter

Read a typed value straight from the browser address bar.

```csharp
Html.On(plan, t => t.PageLoad())
    .When(p.FromUrl<int>("waitlistPosition"))
    .Lte(5)
    .Then(p => p
        .Set(m => m.PriorityIntakeBanner, "visible"));
```

### When(p.Plugin<T>(...)) — plugin member

Read a method return value or property from a registered plugin.

```csharp
Html.On(plan, t => t.PageLoad())
    .When(p.Plugin<string>("geo", "currentRegion"))
    .Eq("Midwest")
    .Then(p => p
        .Set(m => m.RegionalPricingTable, "MidwestRates"));
```

### When(component.Read()).Eq(otherComponent.Read()) — source vs source

The right operand can itself be a typed source, so two live runtime values compare.

```csharp
var assessed = p.Component<FusionNumericTextBox>(m => m.AssessedCareScore);
var baseline = p.Component<FusionNumericTextBox>(m => m.BaselineCareScore);

Html.On(plan, t => t.Event(m => m.AssessedCareScore, "change"))
    .When(assessed.Value())
    .Gt(baseline.Value())
    .Then(p => p
        .Set(m => m.CareLevelIncreasing, true));
```

---

## When / Then / ElseIf / Else

### When ... Then

A single guarded branch. The `Then` pipeline runs when the condition holds.

```csharp
Html.On(plan, t => t.Event(m => m.IsVeteran, "change"))
    .When(args, x => x.Value)
    .Truthy()
    .Then(p => p
        .Set(m => m.VaBenefitsPanel, "visible"));
```

### When ... Then ... Else

Two-way routing. `Else` is terminal and returns void.

```csharp
Html.On(plan, t => t.Event(m => m.IsVeteran, "change"))
    .When(args, x => x.Value)
    .Truthy()
    .Then(p => p
        .Set(m => m.VaBenefitsPanel, "visible"))
    .Else(p => p
        .Set(m => m.VaBenefitsPanel, "hidden"));
```

### When ... ElseIf ... Then

`ElseIf` opens a new guarded branch from a fresh source, then `.Then(...)` again.
Branches evaluate in authored order; first match wins.

```csharp
Html.On(plan, t => t.Event(m => m.MonthlyBudget, "change"))
    .When(args, x => x.Value)
    .Gte(8000m)
    .Then(p => p
        .Set(m => m.RecommendedTier, "Premium"))
    .ElseIf(args, x => x.Value)
    .Gte(5000m)
    .Then(p => p
        .Set(m => m.RecommendedTier, "Standard"));
```

### When ... ElseIf ... Else — full cascade

The complete first-match decision tree with a default tail.

```csharp
Html.On(plan, t => t.Event(m => m.FallRiskScore, "change"))
    .When(args, x => x.Value)
    .Gte(8)
    .Then(p => p
        .Set(m => m.FallPrecautionTier, "High"))
    .ElseIf(args, x => x.Value)
    .Gte(4)
    .Then(p => p
        .Set(m => m.FallPrecautionTier, "Moderate"))
    .Else(p => p
        .Set(m => m.FallPrecautionTier, "Standard"));
```

### ElseIf from a different source

`ElseIf` takes the same overloads as `When` — event args, typed source, or response body.

```csharp
var fundsLeft = p.Component<FusionNumericTextBox>(m => m.RemainingBalance);

Html.On(plan, t => t.Event(m => m.PaymentSource, "change"))
    .When(args, x => x.Value)
    .Eq("Medicaid")
    .Then(p => p
        .Set(m => m.SubsidyWorkflow, "Medicaid"))
    .ElseIf(fundsLeft.Value())
    .Lte(0m)
    .Then(p => p
        .Set(m => m.SubsidyWorkflow, "BalanceReview"))
    .Else(p => p
        .Set(m => m.SubsidyWorkflow, "Private"));
```

---

## Compare operators

### Eq

Equality. Source equals the operand.

```csharp
Html.On(plan, t => t.Event(m => m.Facility, "change"))
    .When(args, x => x.Value)
    .Eq("WillowCreek")
    .Then(p => p
        .Set(m => m.RegionalDirector, "Patel"));
```

### NotEq

Inequality.

```csharp
Html.On(plan, t => t.Event(m => m.PaymentSource, "change"))
    .When(args, x => x.Value)
    .NotEq("Private")
    .Then(p => p
        .Set(m => m.SubsidyFormVisible, true));
```

### Gt

Greater than.

```csharp
Html.On(plan, t => t.Event(m => m.FallRiskScore, "change"))
    .When(args, x => x.Value)
    .Gt(7)
    .Then(p => p
        .Set(m => m.BedAlarmRequired, true));
```

### Gte

Greater than or equal.

```csharp
Html.On(plan, t => t.Event(m => m.AgeYears, "change"))
    .When(args, x => x.Value)
    .Gte(65)
    .Then(p => p
        .Set(m => m.MedicareEligible, true));
```

### Lt

Less than.

```csharp
Html.On(plan, t => t.Event(m => m.BmiValue, "change"))
    .When(args, x => x.Value)
    .Lt(18.5m)
    .Then(p => p
        .Set(m => m.NutritionConsultFlag, true));
```

### Lte

Less than or equal.

```csharp
Html.On(plan, t => t.Event(m => m.RemainingBalance, "change"))
    .When(args, x => x.Value)
    .Lte(0m)
    .Then(p => p
        .Set(m => m.AccountStatus, "PaidInFull"));
```

### In

Membership in a `params` set of literals; operands match `TProp`.

```csharp
Html.On(plan, t => t.Event(m => m.CareLevel, "change"))
    .When(args, x => x.Value)
    .In("AssistedLiving", "MemoryCare", "SkilledNursing")
    .Then(p => p
        .Set(m => m.MedManagementRequired, true));
```

### NotIn

Absence from a `params` set.

```csharp
Html.On(plan, t => t.Event(m => m.RoomType, "change"))
    .When(args, x => x.Value)
    .NotIn("Studio", "Companion")
    .Then(p => p
        .Set(m => m.PrivateRoomFee, 1200m));
```

### Between

Inclusive range with a low and a high operand.

```csharp
Html.On(plan, t => t.Event(m => m.SystolicBp, "change"))
    .When(args, x => x.Value)
    .Between(120, 139)
    .Then(p => p
        .Set(m => m.BpCategory, "Elevated"));
```

### Contains

Substring match for a string source.

```csharp
Html.On(plan, t => t.Event(m => m.Allergies, "change"))
    .When(args, x => x.Value)
    .Contains("Penicillin")
    .Then(p => p
        .Set(m => m.AntibioticWarning, "visible"));
```

### StartsWith

String prefix match.

```csharp
Html.On(plan, t => t.Event(m => m.RoomNumber, "change"))
    .When(args, x => x.Value)
    .StartsWith("3")
    .Then(p => p
        .Set(m => m.Wing, "MemoryCareWing"));
```

### EndsWith

String suffix match.

```csharp
Html.On(plan, t => t.Event(m => m.ContactEmail, "change"))
    .When(args, x => x.Value)
    .EndsWith("@willowcreek.org")
    .Then(p => p
        .Set(m => m.InternalStaffFlag, true));
```

### Matches

Regular-expression test against a string source.

```csharp
Html.On(plan, t => t.Event(m => m.MedicareId, "change"))
    .When(args, x => x.Value)
    .Matches(@"^\d{3}-\d{2}-\d{4}[A-Z]$")
    .Then(p => p
        .Set(m => m.MedicareIdValid, true));
```

### MinLength

String/collection minimum length.

```csharp
Html.On(plan, t => t.Event(m => m.CarePlanNotes, "input"))
    .When(args, x => x.Value)
    .MinLength(20)
    .Then(p => p
        .Set(m => m.NotesSufficient, true));
```

### MaxLength

String/collection maximum length. Planned token mirroring `MinLength` (naming sheet §1.5).

```csharp
Html.On(plan, t => t.Event(m => m.RoomLabel, "input"))
    .When(args, x => x.Value)
    .MaxLength(12)
    .Then(p => p
        .Set(m => m.LabelFitsDoorTag, true));
```

### ArrayContains

Membership of an item inside an array source.

```csharp
Html.On(plan, t => t.Event(m => m.SelectedServices, "change"))
    .When(args, x => x.Items)
    .ArrayContains("PhysicalTherapy")
    .Then(p => p
        .Set(m => m.TherapyScheduleVisible, true));
```

---

## Presence operators

These take no operand — they test the source value's presence/shape.

### Truthy

Present and non-falsy (non-null, non-zero, non-empty, non-false).

```csharp
Html.On(plan, t => t.Event(m => m.PowerOfAttorneyOnFile, "change"))
    .When(args, x => x.Value)
    .Truthy()
    .Then(p => p
        .Set(m => m.LegalReviewComplete, true));
```

### Falsy

Absent or falsy (null, zero, empty, false).

```csharp
Html.On(plan, t => t.Event(m => m.EmergencyContact, "change"))
    .When(args, x => x.Value)
    .Falsy()
    .Then(p => p
        .Set(m => m.IntakeBlockedReason, "MissingEmergencyContact"));
```

### IsNull

Value is null or undefined.

```csharp
Html.On(plan, t => t.Event(m => m.DischargeDate, "change"))
    .When(args, x => x.Value)
    .IsNull()
    .Then(p => p
        .Set(m => m.ResidentStatus, "Active"));
```

### NotNull

Value is not null and not undefined.

```csharp
Html.On(plan, t => t.Event(m => m.DischargeDate, "change"))
    .When(args, x => x.Value)
    .NotNull()
    .Then(p => p
        .Set(m => m.ResidentStatus, "Discharged"));
```

### IsEmpty

Empty string or empty collection.

```csharp
Html.On(plan, t => t.Event(m => m.MedicationList, "change"))
    .When(args, x => x.Items)
    .IsEmpty()
    .Then(p => p
        .Set(m => m.MedReconciliationDue, false));
```

### NotEmpty

Non-empty string or collection.

```csharp
Html.On(plan, t => t.Event(m => m.MedicationList, "change"))
    .When(args, x => x.Items)
    .NotEmpty()
    .Then(p => p
        .Set(m => m.MedReconciliationDue, true));
```

---

## Guard composition

### And (flat)

Chain `.And(source).Operator` directly — produces a flat all-of guard.

```csharp
Html.On(plan, t => t.Event(m => m.CareLevel, "change"))
    .When(args, x => x.Value)
    .Eq("MemoryCare")
    .And(args, x => x.AmbulationStatus)
    .Eq("Wheelchair")
    .Then(p => p
        .Set(m => m.AccessibleRoomRequired, true));
```

### And (flat, repeated)

`.And().And()` flattens into one all-of guard over all terms.

```csharp
Html.On(plan, t => t.Event(m => m.IntakeStep, "change"))
    .When(args, x => x.AssessmentComplete)
    .Truthy()
    .And(args, x => x.PhysicianOrdersSigned)
    .Truthy()
    .And(args, x => x.DepositPaid)
    .Truthy()
    .Then(p => p
        .Set(m => m.ReadyForMoveIn, true));
```

### Or (flat)

Chain `.Or(source).Operator` — produces a flat any-of guard.

```csharp
Html.On(plan, t => t.Event(m => m.PaymentSource, "change"))
    .When(args, x => x.Value)
    .Eq("Medicaid")
    .Or(args, x => x.Value)
    .Eq("VA")
    .Then(p => p
        .Set(m => m.SubsidyWorkflowVisible, true));
```

### And across source kinds

Flat composition mixes source kinds — here event args and a component read.

```csharp
var fundsLeft = p.Component<FusionNumericTextBox>(m => m.RemainingBalance);

Html.On(plan, t => t.Event(m => m.PaymentSource, "change"))
    .When(args, x => x.Value)
    .Eq("Private")
    .And(fundsLeft.Value())
    .Lt(2000m)
    .Then(p => p
        .Set(m => m.LowBalanceReminder, "visible"));
```

### And (nested group)

`.And(cs => cs.When(...).Operator)` opens a grouped sub-condition — needed for
precedence the flat shape cannot express.

```csharp
Html.On(plan, t => t.Event(m => m.CareLevel, "change"))
    .When(args, x => x.IntakeComplete)
    .Truthy()
    .And(cs => cs
        .When(args, x => x.CareLevel)
        .Eq("MemoryCare"))
    .Then(p => p
        .Set(m => m.SecuredUnitRequired, true));
```

### Or (nested group)

A grouped any-of sub-condition.

```csharp
Html.On(plan, t => t.Event(m => m.FallRiskScore, "change"))
    .When(args, x => x.ResidentActive)
    .Truthy()
    .And(cs => cs
        .Or(o => o
            .When(args, x => x.FallRiskScore)
            .Gte(8))))
    .Then(p => p
        .Set(m => m.BedAlarmRequired, true));
```

### Mixed grouping — (a OR b) AND c

The canonical reason nested grouping exists: an OR group AND-ed with another term.

```csharp
Html.On(plan, t => t.Event(m => m.CareLevel, "change"))
    .When(args, x => x.IntakeComplete)
    .Truthy()
    .And(cs => cs
        .When(args, x => x.CareLevel)
        .Eq("MemoryCare")
        .Or(args, x => x.FallRiskScore)
        .Gte(8))
    .Then(p => p
        .Set(m => m.NursePreReviewRequired, true));
```

### Not

Inverts the guard built so far.

```csharp
Html.On(plan, t => t.Event(m => m.BackgroundCheckStatus, "change"))
    .When(args, x => x.Value)
    .Eq("Cleared")
    .Not()
    .Then(p => p
        .Set(m => m.OnboardingBlocked, true));
```

### Not over a nested group

`Not()` inverts the whole composed condition, including grouped sub-conditions.

```csharp
Html.On(plan, t => t.Event(m => m.IntakeStep, "change"))
    .When(args, x => x.DepositPaid)
    .Truthy()
    .And(cs => cs
        .When(args, x => x.PhysicianOrdersSigned)
        .Truthy())
    .Not()
    .Then(p => p
        .Set(m => m.MoveInBlocked, true));
```

---

## Confirm

### Confirm (flat guard)

A user-decision async gate. The pipeline proceeds only when the user accepts.
`Confirm` goes straight to `.Then(...)` — no source, no operator.

```csharp
Html.On(plan, t => t.DispatchFrom("discharge-resident"))
    .Confirm("Discharge this resident? This closes the active care plan.")
    .Then(p => p
        .Set(m => m.ResidentStatus, "Discharged")
        .Dispatch("resident-discharged"));
```

### Confirm before a billing action

Guarding an irreversible billing mutation behind user acceptance.

```csharp
Html.On(plan, t => t.DispatchFrom("post-monthly-charges"))
    .Confirm("Post this month's care charges to all active resident accounts?")
    .Then(p => p
        .Set(m => m.BillingRunStatus, "Posting")
        .Dispatch("billing-run-started"));
```
