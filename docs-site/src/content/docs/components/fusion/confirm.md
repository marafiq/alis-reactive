---
title: FusionConfirm
description: Confirmation dialog. App-level singleton.
sidebar:
  order: 22
---

A confirmation dialog for destructive or high-stakes actions -- discharge, delete, cancel an appointment. App-level singleton: rendered once in the layout (`@Html.FusionConfirmDialog()`), then called from any plan.

**Call from any plan:** `p.Component<FusionConfirm>()...`

## How do I use it?

```csharp
p.Component<FusionConfirm>()
    .SetContent("Are you sure you want to discharge this resident?")
    .Show();
```

## Reference

| Extension | Description |
|---|---|
| `SetContent(string)` | Sets the dialog message (calls `dataBind` after setting) |
| `Show()` | Opens the dialog |
| `Hide()` | Closes the dialog |
