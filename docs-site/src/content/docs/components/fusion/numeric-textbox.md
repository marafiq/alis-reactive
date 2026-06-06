---
title: FusionNumericTextBox
description: Numeric input with spin buttons, formatting, and min/max constraints.
sidebar:
  order: 3
---

A numeric input with spin buttons, formatting, and min/max constraints. Bind it to any numeric model property (`decimal`, `int`, `double`). The framework coerces the value to `"number"` in the plan so you can write typed math in conditions.

**Model type:** `decimal` &nbsp; **ReadExpr:** `"value"` &nbsp; **Events:** `Changed`, `Focus`, `Blur`

## How do I render one?

```csharp
Html.InputField(plan, m => m.Amount, o => o.Label("Amount"))
    .FusionNumericTextBox(b => b
        .Min(-100).Max(99999).Step(1));
```

## How do I set its value?

The `decimal` type flows through the plan as a JSON number; the runtime writes it straight to the Syncfusion instance's `value` property.

```csharp
p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(42m);
p.Component<FusionNumericTextBox>(m => m.Amount).SetMin(0m);
```

## How do I read its value as a source?

```csharp
var comp = p.Component<FusionNumericTextBox>(m => m.Amount);
p.Element("value-echo").SetText(comp.Value());
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(decimal)` | Sets the value (coerced to number in the plan) |
| `SetMin(decimal)` | Sets the minimum allowed value |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `Increment()` | Increases value by the step amount |
| `Decrement()` | Decreases value by the step amount |
| `Value()` | Reads the current value as a typed source (`TypedComponentSource<decimal>`) for conditions and gather |
