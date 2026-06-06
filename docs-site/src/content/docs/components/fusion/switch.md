---
title: FusionSwitch
description: Toggle switch bound to a bool. ValueMember is "checked".
sidebar:
  order: 11
---

A toggle switch for boolean settings -- opt in to notifications, mark a resident as self-directed, enable a medication reminder. Same concept as NativeCheckBox but rendered as a Syncfusion Switch control. Note that the ValueMember is `"checked"` (not `"value"`), matching Syncfusion's API.

**Model type:** `bool` &nbsp; **ValueMember:** `"checked"` &nbsp; **Events:** `Changed`

## How do I render one?

```csharp
Html.InputField(plan, m => m.ReceiveNotifications, o => o.Label("Receive Notifications"))
    .FusionSwitch(b => b
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("change-value").SetText(args, x => x.Checked);
            p.When(args, x => x.Checked).Truthy()
                .Then(t => t.Element("args-condition").SetText("notifications enabled"))
                .Else(e => e.Element("args-condition").SetText("notifications disabled"));
        }));
```

## How do I set its checked state?

```csharp
p.Component<FusionSwitch>(m => m.ReceiveNotifications).SetChecked(false);
```

## Reference

| Extension | Description |
|---|---|
| `SetChecked(bool)` | Sets the checked state (coerced to boolean in the plan) |
| `Value()` | Reads the current checked state as a typed source (`TypedComponentSource<bool>`) for conditions and gather |
