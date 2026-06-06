---
title: FusionToast
description: Transient notifications (success, warn, danger, info). App-level singleton.
sidebar:
  order: 21
---

Toast notifications for transient feedback -- "Resident saved", "Medication logged", "Payment processing". App-level singleton: rendered once in the layout (`@Html.FusionToast()`), then called from any plan without a model expression.

**Call from any plan:** `p.Component<FusionToast>()...`

## How do I show a toast?

Configure the properties, pick a type, then call `Show()`. The type methods (`Success()`, `Warning()`, `Danger()`, `Info()`) apply the corresponding CSS class -- pick the one that matches the message's meaning, not the color you want.

```csharp
p.Component<FusionToast>()
    .SetTitle("Resident Saved")
    .SetContent("Jane Doe has been admitted to Assisted Living")
    .Success()
    .Show();
```

## Available types

| Method | CSS class |
|---|---|
| `Success()` | `e-toast-success` (green) |
| `Warning()` | `e-toast-warning` (yellow) |
| `Danger()` | `e-toast-danger` (red) |
| `Info()` | `e-toast-info` (blue) |

## Reference

| Extension | Description |
|---|---|
| `SetTitle(string)` | Sets the toast title |
| `SetContent(string)` | Sets the toast body text |
| `SetTimeout(int)` | Auto-dismiss after N milliseconds |
| `ShowCloseButton()` | Shows the close button |
| `ShowProgressBar()` | Shows the auto-dismiss progress bar |
| `Success()` / `Warning()` / `Danger()` / `Info()` | Sets the toast type styling |
| `Show()` | Displays the toast (calls `dataBind` + `show` on the Syncfusion instance) |
| `Hide()` | Hides the toast |
