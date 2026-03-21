---
title: Guard Operators
description: Complete reference for all condition operators available in guard expressions.
sidebar:
  order: 2
---

Guard operators control pipeline branching at runtime. Every operator is a method on `ConditionSourceBuilder<TModel, TProp>`, reached via `p.When(source).Operator()`. The source provides the left-hand value; the operator defines the comparison.

---

## How do I start a condition?

### From an event payload

Use the event args object and a property expression. The expression compiles to a dot-path (e.g., `x => x.Value` becomes `"evt.value"`):

```csharp
p.When(args, x => x.Value).Eq("active")
    .Then(t => /* ... */)
    .Else(e => /* ... */);
```

### From a component value

Use `.Value()` on a `ComponentRef` to get a typed source. The runtime reads the component's current value via the plan's `readExpr`:

```csharp
var name = p.Component<NativeTextBox>(m => m.ResidentName);
p.When(name.Value()).NotEmpty()
    .Then(t => /* ... */)
    .Else(e => /* ... */);
```

Both approaches produce the same `ConditionSourceBuilder` with the same operators.

---

## Comparison operators

Compare the source value against a typed literal operand. The operand type is enforced at compile time via `TProp`.

| C# Method | Plan `op` | Description |
|-----------|-----------|-------------|
| `.Eq(value)` | `eq` | Strict equality |
| `.NotEq(value)` | `neq` | Strict inequality |
| `.Gt(value)` | `gt` | Greater than |
| `.Gte(value)` | `gte` | Greater than or equal |
| `.Lt(value)` | `lt` | Less than |
| `.Lte(value)` | `lte` | Less than or equal |

```csharp
p.When(args, x => x.Age).Gte(65)
    .Then(t => t.Element("status").SetText("senior"))
    .Else(e => e.Element("status").SetText("standard"));
```

### Source-vs-source comparison

Every comparison operator also accepts a `TypedSource<TProp>` on the right-hand side, enabling comparisons between two live values:

```csharp
var startDate = p.Component<FusionDateRangePicker>(m => m.StayStart);
var endDate = startDate.EndDate();

p.When(startDate.StartDate()).Lt(endDate)
    .Then(t => t.Element("date-status").SetText("valid range"))
    .Else(e => e.Element("date-status").SetText("start must be before end"));
```

---

## Presence operators

Test the nature of the value itself. No operand needed.

| C# Method | Plan `op` | Description |
|-----------|-----------|-------------|
| `.Truthy()` | `truthy` | Non-null, non-empty, non-false, non-zero |
| `.Falsy()` | `falsy` | Null, empty string, false, or zero |
| `.IsNull()` | `is-null` | Strictly null or undefined |
| `.NotNull()` | `not-null` | Not null and not undefined |
| `.IsEmpty()` | `is-empty` | Empty string or empty array |
| `.NotEmpty()` | `not-empty` | Non-empty string or non-empty array |

```csharp
var isVeteran = p.Component<NativeCheckBox>(m => m.IsVeteran);
p.When(isVeteran.Value()).Truthy()
    .Then(t => t.Element("veteran-section").Show())
    .Else(e => e.Element("veteran-section").Hide());
```

---

## Membership operators

| C# Method | Plan `op` | Description |
|-----------|-----------|-------------|
| `.In(values)` | `in` | Source value is one of the listed values. Takes `params TProp[]`. |
| `.NotIn(values)` | `not-in` | Source value is not in the list |
| `.Between(low, high)` | `between` | Source value falls within `[low, high]` inclusive |

```csharp
p.When(args, x => x.Value).In("MC", "MC+")
    .Then(t => t.Element("memory-care-notice").Show())
    .Else(e => e.Element("memory-care-notice").Hide());

p.When(args, x => x.Value).Between(18, 120)
    .Then(t => t.Element("age-status").SetText("valid"))
    .Else(e => e.Element("age-status").SetText("out of range"));
```

---

## Text operators

Operate on string source values.

| C# Method | Plan `op` | Description |
|-----------|-----------|-------------|
| `.Contains(str)` | `contains` | Source string contains the substring |
| `.StartsWith(str)` | `starts-with` | Source string starts with the prefix |
| `.EndsWith(str)` | `ends-with` | Source string ends with the suffix |
| `.Matches(regex)` | `matches` | Source string matches the regular expression |
| `.MinLength(n)` | `min-length` | Source string length is at least N |

```csharp
p.When(args, x => x.Value).EndsWith("@hospital.org")
    .Then(t => t.Element("email-status").SetText("internal"))
    .Else(e => e.Element("email-status").SetText("external"));

p.When(args, x => x.Value).Matches(@"^\(\d{3}\) \d{3}-\d{4}$")
    .Then(t => t.Element("phone-status").SetText("valid"))
    .Else(e => e.Element("phone-status").SetText("invalid format"));
```

---

## Array operators

| C# Method | Plan `op` | Description |
|-----------|-----------|-------------|
| `.ArrayContains(item)` | `array-contains` | Source array contains the specified element |

The plan carries an `elementCoerceAs` field for element-level coercion.

```csharp
var allergies = p.Component<NativeCheckList>(m => m.Allergies);
p.When(allergies.Value()).ArrayContains("PEN")
    .Then(t => t.Element("penicillin-warning").Show())
    .Else(e => e.Element("penicillin-warning").Hide());
```

---

## Composition

### And (all must match)

**Direct chaining** -- start a new source for the second operand:

```csharp
p.When(args, x => x.Value).Eq("MC")
    .And(args, x => x.Age).Gte(65)
    .Then(t => t.Element("status").SetText("eligible"));
```

**With component sources:**

```csharp
var name = p.Component<NativeTextBox>(m => m.ResidentName);
var age = p.Component<FusionNumericTextBox>(m => m.Age);

p.When(name.Value()).NotEmpty()
    .And(age.Value()).Gte(18m)
    .Then(t => t.Element("status").SetText("form complete"));
```

**Lambda form** -- for parenthesized grouping:

```csharp
p.When(args, x => x.CareLevel).Eq("MC")
    .And(c => c.When(args, x => x.Age).Gte(65)
                .Or(c2 => c2.When(args, x => x.HasDiagnosis).Truthy()))
    .Then(t => /* ... */);
```

Chained `.And().And()` is flattened into a single `AllGuard` (no unnecessary nesting).

### Or (any must match)

Same two forms as `And`:

```csharp
// Direct
p.When(args, x => x.Value).Eq("VIP")
    .Or(args, x => x.Status).Eq("Legacy")
    .Then(t => t.Element("priority").SetText("high"));

// Lambda
p.When(args, x => x.Score).Gt(90)
    .Or(c => c.When(args, x => x.IsOverride).Truthy())
    .Then(t => /* ... */);
```

Chained `.Or().Or()` is flattened into a single `AnyGuard`.

### Not

Inverts any guard:

```csharp
p.When(args, x => x.Value).IsEmpty().Not()
    .Then(t => /* value is NOT empty */);
```

---

## Confirm guard

A special guard that pauses the pipeline and shows a confirmation dialog. It does not evaluate a data source.

```csharp
p.Confirm("Discharge this resident?")
    .Then(t =>
    {
        t.Post("/api/residents/discharge", g => g.IncludeAll())
         .Response(r => r.OnSuccess(s =>
             s.Component<FusionToast>().SetContent("Discharged").Success().Show()));
    });
```

`Confirm` is started from `PipelineBuilder` directly (not from `When`).

---

## Branching: Then / ElseIf / Else

Every guard terminates with `.Then()`, which starts the reaction body and returns a `BranchBuilder`.

### Simple if/else

```csharp
p.When(args, x => x.Value).Eq("active")
    .Then(t => t.Element("status").AddClass("text-green-600"))
    .Else(e => e.Element("status").AddClass("text-red-600"));
```

### Multi-branch

```csharp
p.When(args, x => x.Value).Eq("IL")
    .Then(t => t.Element("level").SetText("Independent Living"))
    .ElseIf(args, x => x.Value).Eq("AL")
    .Then(t => t.Element("level").SetText("Assisted Living"))
    .ElseIf(args, x => x.Value).Eq("MC")
    .Then(t => t.Element("level").SetText("Memory Care"))
    .Else(e => e.Element("level").SetText("Unknown"));
```

The runtime evaluates branches in order and executes the first match. If no branch matches and `Else` is present, the else branch runs.
