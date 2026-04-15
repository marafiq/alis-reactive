---
title: FusionMultiColumnComboBox
description: Combo box with a multi-column dropdown. Select one from structured rows.
sidebar:
  order: 10
---

A combo box that shows multiple columns inside its dropdown. Use it when picking one record from a structured list where the user needs more than a single string to disambiguate -- name + ID + department, or medication + dose + route.

**Model type:** `string` &nbsp; **ReadExpr:** `"value"` &nbsp; **Events:** `Changed`

Shares the same data-source + cascade vocabulary as [DropDownList](./dropdown-list/) -- the difference is purely visual (multi-column layout in the popup).

## Reference

| Extension | Description |
|---|---|
| `SetValue(string?)` | Sets the selected value |
| `SetText(string)` | Sets the display text |
| `SetDataSource(source, path)` | Sets data source from event payload or response body |
| `DataBind()` | Flushes pending changes |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `ShowPopup()` / `HidePopup()` | Open or close the dropdown |
