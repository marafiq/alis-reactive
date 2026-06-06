---
title: FusionAutoComplete
description: Text input with server-side filtering and autocomplete suggestions.
sidebar:
  order: 1
---

Reach for AutoComplete when a free-text field has thousands of candidate values and the server is the source of truth -- selecting a physician, a medication, a room number, a diagnosis code. The user types, you query, Syncfusion shows the matches.

**Model type:** `string` &nbsp; **ValueMember:** `"value"` &nbsp; **Events:** `Changed`, `Filtering`

## How do I render one?

```csharp
Html.InputField(plan, m => m.Physician, o => o.Label("Physician"))
    .FusionAutoComplete(b => b
        .DataSource(physicians)
        .Fields<PhysicianItem>(t => t.Text, v => v.Value)
        .Placeholder("Select a physician"));
```

## How do I fetch suggestions from the server as the user types?

The `Filtering` event fires on every keystroke. Cancel Syncfusion's built-in client filter with `args.PreventDefault(p)`, then fetch fresh results and feed them back with `args.UpdateData(...)`. No `DataBind()` needed -- `updateData` handles the refresh internally.

```csharp
.Reactive(plan, evt => evt.Filtering, (args, p) =>
{
    args.PreventDefault(p);
    p.Get("/api/physicians?q=", g => g.Include(m => m.Physician))
     .Response(r => r.OnSuccess<PhysicianResponse>((json, s) =>
     {
         args.UpdateData(s, json, j => j.Physicians);
     }));
})
```

## How do I cascade from another component?

When a parent selection (department, wing) should reload this AutoComplete's suggestions, call `SetDataSource(...).DataBind()` inside the parent's `Changed` handler. You're setting a property and need to flush the change -- that's what `DataBind` does.

```csharp
p.Get("/api/physicians?dept=", g => g.Include(m => m.Department))
 .Response(r => r.OnSuccess<PhysicianResponse>((json, s) =>
 {
     s.Component<FusionAutoComplete>(m => m.Physician)
         .SetDataSource(json, j => j.Physicians)
         .DataBind();
 }));
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(string?)` | Sets the selected value |
| `SetText(string)` | Sets the display text |
| `SetDataSource(source, path)` | Sets data source from event payload or response body |
| `DataBind()` | Flushes pending property changes to the Syncfusion instance |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `ShowPopup()` / `HidePopup()` | Open or close the suggestion dropdown |
| `Enable()` / `Disable()` | Enable or disable the component |
| `Value()` | Reads the current selected value as a typed source (`TypedComponentSource<string>`) for conditions and gather |
