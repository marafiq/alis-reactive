---
title: FusionDateTimePicker
description: Combined date and time picker.
sidebar:
  order: 5
---

Use DateTimePicker when the scheduled moment matters to the minute -- appointment slots, medication administration windows, shift start times. The value is serialized as `"yyyy-MM-ddTHH:mm"` in the plan.

**Model type:** `DateTime` &nbsp; **ValueMember:** `"value"` &nbsp; **Events:** `Changed`

## How do I render one?

```csharp
Html.InputField(plan, m => m.AppointmentTime, o => o.Label("Appointment Time"))
    .FusionDateTimePicker(b => b
        .Placeholder("Select date and time"));
```

## How do I set its value?

```csharp
p.Component<FusionDateTimePicker>(m => m.AppointmentTime)
    .SetValue(new DateTime(2026, 4, 1, 14, 30, 0));
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(DateTime)` | Sets the date and time |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `Value()` | Reads the current datetime as a typed source (`TypedComponentSource<DateTime>`) for conditions and gather |
