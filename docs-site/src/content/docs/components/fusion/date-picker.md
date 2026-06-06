---
title: FusionDatePicker
description: Date-only picker with calendar popup.
sidebar:
  order: 4
---

A date-only picker with calendar popup. Use it for admission dates, birthdays, scheduled visit dates -- anything where time-of-day is not meaningful. The `DateTime` you set is serialized as `"yyyy-MM-dd"` in the plan.

**Model type:** `DateTime` &nbsp; **ValueMember:** `"value"` &nbsp; **Events:** `Changed`

## How do I render one and react to picks?

`.Reactive(plan, evt => evt.Changed, ...)` fires every time the user commits a date. `args.Value` is the typed `DateTime?` so you can compare it directly in a `When(...)` block.

```csharp
Html.InputField(plan, m => m.AdmissionDate, o => o.Label("Admission Date"))
    .FusionDatePicker(b => b
        .Placeholder("Select admission date")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("change-value").SetText(args, x => x.Value);
            p.When(args, x => x.Value).NotNull()
                .Then(t => t.Element("args-condition").SetText("date selected"))
                .Else(e => e.Element("args-condition").SetText("no date"));
        }));
```

## How do I set its value?

```csharp
p.Component<FusionDatePicker>(m => m.AdmissionDate)
    .SetValue(new DateTime(2026, 6, 15));
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(DateTime)` | Sets the date |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `Value()` | Reads the current date as a typed source (`TypedComponentSource<DateTime>`) for conditions and gather |
