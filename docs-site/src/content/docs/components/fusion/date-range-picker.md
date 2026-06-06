---
title: FusionDateRangePicker
description: Start + end date picker. Value is a [Date, Date] array.
sidebar:
  order: 7
---

Use DateRangePicker when the input represents a span -- a resident's stay period, a compliance-audit date range, a report filter. This component is unique: the Syncfusion ej2 instance's `.value` returns `[Date, Date]` -- an array of two dates -- so your model property is `DateTime[]?`, not `DateTime`.

**Model type:** `DateTime[]?` &nbsp; **ValueMember:** `"value"` (returns `[Date, Date]`) &nbsp; **Events:** `Changed`

## How do I render one?

```csharp
// Model property is DateTime[] -- matches Syncfusion [Date, Date] value
public DateTime[]? StayPeriod { get; set; }

// View
Html.InputField(plan, m => m.StayPeriod, o => o.Required().Label("Stay Period"))
    .FusionDateRangePicker(b => b
        .Placeholder("Select date range"));
```

## How do I read just the start or end date in a condition?

`StartDate()` and `EndDate()` read explicit component members `"startDate"` and `"endDate"` -- independent of the component's registered `ValueMember`. They return individual `DateTime` values for typed condition comparison, so you can compare them separately without unpacking the array yourself.

```csharp
var stay = p.Component<FusionDateRangePicker>(m => m.StayPeriod);

p.When(stay.StartDate()).NotNull()
    .Then(t => t.Element("start-echo").SetText(stay.StartDate()));

p.When(stay.EndDate()).NotNull()
    .Then(t => t.Element("end-echo").SetText(stay.EndDate()));
```

## How does gather serialize the range?

`IncludeAll()` reads `ej2.value` -> `[Date, Date]`. Gather's `emitArray` iterates each Date and serializes via `toString()` -> ISO 8601.

- **JSON POST:** `{ "StayPeriod": ["2026-07-01T...", "2026-07-15T..."] }` -- ASP.NET binds `DateTime[]` natively.
- **FormData:** repeated key `StayPeriod=ISO&StayPeriod=ISO` -- ASP.NET binds arrays from repeated keys.
- **GET:** repeated params -- same pattern.

## Source extensions

| Extension | Returns | Component member | Use case |
|---|---|---|---|
| `StartDate()` | `TypedComponentSource<DateTime>` | `"startDate"` | Individual date for conditions |
| `EndDate()` | `TypedComponentSource<DateTime>` | `"endDate"` | Individual date for conditions |
| `Value()` | `TypedComponentSource<DateTime[]>` | `"value"` | Full array for gather/validation |

No `SetValue()` is provided -- the DateRangePicker is set by user interaction only.
