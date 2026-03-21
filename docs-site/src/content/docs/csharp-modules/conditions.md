---
title: Conditions
description: Runtime branching with When/Then/ElseIf/Else, comparison and text operators, guard composition with And/Or/Not, and confirm dialogs.
sidebar:
  order: 5
---

Conditions live inside a `.Reactive()` or `Html.On()` pipeline. They let you branch based on values that only exist in the browser — an event payload, a component's current state.

From the [Grammar Tree](/csharp-modules/mental-model/#the-grammar-tree) — the conditions subset:

```
pipeline.When(source).OPERATOR                    § start a condition
├── .Then(then => { })                            § true branch
├── .ElseIf(source).OPERATOR.Then(then => { })    § additional branches
├── .Else(else_ => { })                           § fallback branch
├── .And(source).OPERATOR                         § all must be true
├── .Or(source).OPERATOR                          § any can be true
├── .Not()                                        § invert the guard

pipeline.Confirm("message")                       § browser confirmation dialog
├── .Then(then => { })                            § user clicked OK
├── .Else(else_ => { })                           § user clicked Cancel
```

## What does a condition look like?

Inside a `.Reactive()` handler, conditions follow this shape:

```csharp
.NativeCheckBox(b => b.Reactive(plan, evt => evt.Changed, (args, pipeline) =>
{
    pipeline.When(args, a => a.Checked).Truthy()
        .Then(t => t.Element("panel").Show())
        .Else(e => e.Element("panel").Hide());
}));
```

The grammar: `pipeline.When(source).OPERATOR.Then(...).Else(...)`. `When` takes a source (where to read the value). The operator tests it. `Then` defines what happens if the test passes.

## Where does the value come from?

### From event args

Inside a `CustomEvent<TPayload>` or `.Reactive()` handler:

```csharp
Html.On(plan, t => t.CustomEvent<ScorePayload>("set-score", (args, pipeline) =>
    pipeline.When(args, x => x.Score).Gte(90)
        .Then(then => then.Element("grade").SetText("A"))
        .ElseIf(args, x => x.Score).Gte(80)
        .Then(then => then.Element("grade").SetText("B"))
        .Else(else_ => else_.Element("grade").SetText("F"))
));
```

The expression `x => x.Score` produces the dot-path `"evt.score"`. At runtime, the value is read from the event payload.

### From a component

Anywhere in a pipeline, you can read the current value of a form component:

```csharp
var careLevel = pipeline.Component<FusionDropDownList>(m => m.CareLevel);

pipeline.When(careLevel.Value()).Eq("memory-care")
    .Then(then => then.Element("memory-care-section").Show())
    .Else(else_ => else_.Element("memory-care-section").Hide());
```

`.Value()` gives you typed access to the component's live value -- the runtime reads it when evaluating the condition.

## What operators are available?

### Comparison operators

These take an operand that must match the source property type:

```csharp
pipeline.When(args, x => x.Score).Gte(90)             // Greater than or equal
pipeline.When(args, x => x.Status).NotEq("discharged") // Not equal
```

| Operator | Meaning |
|----------|---------|
| `.Eq(value)` | Equal to |
| `.NotEq(value)` | Not equal to |
| `.Gt(value)` | Greater than |
| `.Gte(value)` | Greater than or equal |
| `.Lt(value)` | Less than |
| `.Lte(value)` | Less than or equal |

### Presence operators

These take no arguments:

```csharp
pipeline.When(args, x => x.IsActive).Truthy()
pipeline.When(args, x => x.NullableScore).IsNull()
pipeline.When(args, x => x.Notes).NotEmpty()
```

| Operator | Meaning |
|----------|---------|
| `.Truthy()` | Non-null, non-zero, non-empty-string, non-false |
| `.Falsy()` | Null, zero, empty string, or false |
| `.IsNull()` | Strictly null or undefined |
| `.NotNull()` | Not null and not undefined |
| `.IsEmpty()` | Null or empty string |
| `.NotEmpty()` | Not null and not empty string |

### Membership operators

Test whether a value belongs to a set:

```csharp
pipeline.When(args, x => x.Category).In("A", "B", "C")
pipeline.When(args, x => x.Status).NotIn("discharged", "deceased")
```

| Operator | Meaning |
|----------|---------|
| `.In(v1, v2, ...)` | Value is one of the listed values |
| `.NotIn(v1, v2, ...)` | Value is none of the listed values |

### Range operator

```csharp
pipeline.When(args, x => x.Age).Between(18, 65)
```

Tests `low <= value <= high` (inclusive).

### Text operators

For string sources:

```csharp
pipeline.When(args, x => x.Name).Contains("admin")
pipeline.When(args, x => x.Email).StartsWith("admin@")
pipeline.When(args, x => x.Email).EndsWith("@corp.com")
pipeline.When(args, x => x.Email).Matches(@"^[a-z]+@[a-z]+\.[a-z]+$")
pipeline.When(args, x => x.Name).MinLength(3)
```

| Operator | Meaning |
|----------|---------|
| `.Contains("sub")` | Value contains the substring |
| `.StartsWith("pre")` | Value starts with the prefix |
| `.EndsWith("suf")` | Value ends with the suffix |
| `.Matches("regex")` | Value matches the regular expression |
| `.MinLength(n)` | Value length is at least `n` |

### Array operator

For array-typed sources (like multi-select values):

```csharp
pipeline.When(selectedServices.Value()).ArrayContains("physical-therapy")
```

### Source-vs-source comparison

Both sides can be runtime sources. Compare two component values directly:

```csharp
var startDate = pipeline.Component<FusionDatePicker>(m => m.AdmissionDate);
var endDate = pipeline.Component<FusionDatePicker>(m => m.DischargeDate);

pipeline.When(startDate.Value()).Gt(endDate.Value())
    .Then(then => then.Element("date-error").SetText("Admission cannot be after discharge"))
    .Else(else_ => else_.Element("date-error").Hide());
```

Both sides must have the same type. All six comparison operators (`Eq`, `NotEq`, `Gt`, `Gte`, `Lt`, `Lte`) support source-vs-source.

## How does branching work?

### Then

Every condition must end with `.Then()`:

```csharp
pipeline.When(args, x => x.Score).Gte(90)
    .Then(then =>
    {
        then.Element("grade").SetText("A");
        then.Element("grade").AddClass("text-green-600");
    });
```

The `then` parameter is a full pipeline builder -- you can use `Element()`, `Component()`, `Dispatch()`, HTTP requests, and even nested conditions.

### ElseIf

Chain additional conditions after `.Then()`:

```csharp
pipeline.When(args, x => x.Score).Gte(90)
    .Then(then => then.Element("grade").SetText("A"))
    .ElseIf(args, x => x.Score).Gte(80)
    .Then(then => then.Element("grade").SetText("B"))
    .ElseIf(args, x => x.Score).Gte(70)
    .Then(then => then.Element("grade").SetText("C"))
    .Else(else_ => else_.Element("grade").SetText("F"));
```

`ElseIf` accepts both event args and component sources:

```csharp
// ... continued from a When/Then chain:
.ElseIf(args, x => x.Score).Gte(80)    // from event payload
.ElseIf(careLevel.Value()).Eq("skilled") // from a component
```

### Else

The fallback branch when no preceding condition matches:

```csharp
pipeline.When(args, x => x.CareLevel).Eq("memory-care")
    .Then(then => then.Element("instructions").SetText("Memory care protocol"))
    .Else(else_ => else_.Element("instructions").SetText("Standard protocol"));
```

`Else` is terminal -- you cannot chain `ElseIf` after it. It is also optional. If omitted and no branch matches, nothing happens.

## How do I combine multiple conditions?

### And -- all must be true

Direct chaining:

```csharp
pipeline.When(args, x => x.Score).Gte(90)
    .And(args, x => x.Status).Eq("active")
    .Then(then => then.Element("result").SetText("Active High Scorer"));
```

Multiple `.And()` calls flatten into a single guard:

```csharp
pipeline.When(args, x => x.Age).Gte(65)
    .And(args, x => x.CareLevel).Eq("memory-care")
    .And(args, x => x.Status).Eq("active")
    .Then(then => then.Element("triple-match").Show());
```

And also works with component sources:

```csharp
pipeline.When(args, x => x.Score).Gte(90)
    .And(careLevel.Value()).Eq("memory-care")
    .Then(then => then.Element("result").SetText("High-scoring memory care"));
```

### Or -- any can be true

```csharp
pipeline.When(args, x => x.Role).Eq("admin")
    .Or(args, x => x.Role).Eq("superuser")
    .Then(then => then.Element("authorized").Show());
```

### Lambda composition -- mixing And/Or

When you need to mix AND and OR, use the lambda form:

```csharp
pipeline.When(args, x => x.Age).Gte(65)
    .And(cs => cs.When(args, x => x.CareLevel).Eq("memory-care")
                  .Or(args, x => x.CareLevel).Eq("skilled-nursing"))
    .Then(then => then.Element("senior-high-care").Show());
```

This reads as: "age >= 65 AND (careLevel is memory-care OR skilled-nursing)".

The `cs` parameter provides a fresh `When` entry point for the nested group.

### Not -- invert a guard

```csharp
pipeline.When(args, x => x.Status).Eq("discharged").Not()
    .Then(then => then.Element("active-indicator").Show());
```

`.Not()` wraps the preceding guard in an inversion. Combine it with `And`/`Or`:

```csharp
pipeline.When(args, x => x.Age).Gte(65)
    .And(cs => cs.When(args, x => x.Status).Eq("discharged").Not())
    .Then(then => then.Element("active-senior").Show());
```

## What is the Confirm guard?

A special condition that pauses the pipeline and shows a browser confirmation dialog:

```csharp
pipeline.Confirm("Are you sure you want to discharge this resident?")
    .Then(then =>
    {
        then.Element("status").SetText("Processing discharge...");
        then.Dispatch("discharge-confirmed");
    });
```

If the user clicks OK, the `Then` branch executes. If they click Cancel, the pipeline halts. You can add an `Else` branch to handle cancellation:

```csharp
pipeline.Confirm("Are you sure you want to proceed?")
    .Then(then => then.Element("result").SetText("Confirmed"))
    .Else(else_ => else_.Element("result").SetText("Cancelled"));
```

Confirm also works inside lambda composition:

```csharp
pipeline.When(args, x => x.HasUnsavedChanges).Truthy()
    .And(cs => cs.Confirm("You have unsaved changes. Continue anyway?"))
    .Then(then => then.Dispatch("navigate-away"));
```

## Can I guard a single command instead of wrapping it in When/Then?

Yes. Per-command guards are available on element targets after source-bound methods:

```csharp
Html.On(plan, t => t.CustomEvent<ScorePayload>("check-per-action", (args, pipeline) =>
{
    pipeline.Element("per-action-result").SetText("Always runs");
    var el = pipeline.Element("per-action-bonus");
    el.SetText("Bonus!");
    el.When(args, x => x.Score, csb => csb.Gte(90));
}));
```

The first command always executes. The second command (`SetText("Bonus!")`) is skipped at runtime if `Score < 90`. The rest of the pipeline continues either way.

The per-command `When` takes the same event args and expression as the pipeline-level `When`, plus a lambda that picks an operator:

## Can I write multiple independent conditions in one pipeline?

Yes. Each `When` block evaluates independently:

```csharp
Html.On(plan, t => t.CustomEvent<AssessmentPayload>("scored", (args, pipeline) =>
{
    // Condition 1: Grade assignment
    pipeline.When(args, x => x.Score).Gte(90)
        .Then(then => then.Element("grade").SetText("Excellent"))
        .Else(else_ => else_.Element("grade").SetText("Needs Improvement"));

    // Condition 2: Alert visibility (completely independent)
    pipeline.When(args, x => x.Score).Lt(60)
        .Then(then => then.Element("low-score-alert").Show())
        .Else(else_ => else_.Element("low-score-alert").Hide());
}));
```

Both conditions evaluate every time the event fires. Neither blocks the other. Each `When/Then/Else` block is independent -- the runtime evaluates them separately.

## What about type safety?

The source property type (`TProp`) flows through the entire chain. If `Score` is `int`, then `.Eq()` demands an `int`. If `CareLevel` is `string`, then `.Eq()` demands a `string`. The compiler rejects type mismatches.

At runtime, automatic coercion is applied based on the C# type:

| C# Type | Coercion |
|---------|----------|
| `string` | null becomes `""` |
| `int`, `long`, `decimal`, `double` | NaN becomes `0` |
| `bool` | `"false"` becomes `false` |
| `DateTime` | Compared as ISO 8601 strings |

You never specify coercion manually -- it is derived from the C# property type.

**Next:** [HTTP Pipeline](/csharp-modules/http-pipeline/) -- GET/POST/PUT/DELETE, gather, loading states, typed responses, and chained/parallel requests.
