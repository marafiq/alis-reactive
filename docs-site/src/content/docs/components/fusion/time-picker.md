---
title: FusionTimePicker
description: Time-only picker.
sidebar:
  order: 6
---

Time-only picker for things like medication administration windows, shift handover times, or recurring daily reminders. The value is serialized as `"HH:mm"` in the plan.

**Model type:** `DateTime` &nbsp; **ValueMember:** `"value"` &nbsp; **Events:** `Changed`

## How do I render one?

```csharp
Html.InputField(plan, m => m.MedicationTime, o => o.Label("Medication Time"))
    .FusionTimePicker(b => b
        .Placeholder("Select time"));
```

## How do I set its value?

Pass a full `DateTime` -- only the time portion is used.

```csharp
p.Component<FusionTimePicker>(m => m.MedicationTime)
    .SetValue(new DateTime(2026, 1, 1, 8, 30, 0));
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(DateTime)` | Sets the time |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `Value()` | Reads the current time as a typed source (`TypedComponentSource<DateTime>`) for conditions and gather |
