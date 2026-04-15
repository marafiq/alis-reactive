---
title: FusionDropDownList
description: Single-select dropdown with search and cascade support.
sidebar:
  order: 8
---

A single-select dropdown with search and filtering. Use it when the candidate list is short enough to enumerate up-front -- a care level, a facility wing, a staff role -- but long enough that radio buttons would be cramped.

**Model type:** `string` &nbsp; **ReadExpr:** `"value"` &nbsp; **Events:** `Changed`, `Focus`, `Blur`

## How do I render one?

Chain `.Reactive(plan, evt => evt.Changed, ...)` inline to wire the selection handler in the same expression that declares the component.

```csharp
Html.InputField(plan, m => m.Category, o => o.Label("Category"))
    .FusionDropDownList(b => b
        .DataSource(categories)
        .Placeholder("Select a category")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("change-value").SetText(args, x => x.Value);
            p.When(args, x => x.Value).Eq("Electronics")
                .Then(t => t.Element("args-condition").SetText("electronics selected"))
                .Else(e => e.Element("args-condition").SetText("other category"));
        }));
```

## How do I bind a typed object list?

When the data source is a list of records with a display text and a value, use `.Fields<T>()` to bind the two fields.

```csharp
.FusionDropDownList(b => b
    .DataSource(facilityRecords)
    .Fields<FacilityRecord>(x => x.Name, x => x.Id)
    .Placeholder("Select facility"))
```

## How do I cascade dropdowns?

When parent selection (facility) should reload child options (wings inside that facility), fetch the new data in the parent's `Changed` handler and push it into the child with `SetDataSource(...).DataBind()`.

```csharp
.Reactive(plan, evt => evt.Changed, (args, p) =>
{
    p.Get("/api/wings?facility=", g => g.Include(m => m.FacilityId))
     .Response(r => r.OnSuccess<WingResponse>((json, s) =>
     {
         s.Component<FusionDropDownList>(m => m.WingId)
             .SetDataSource(json, j => j.Wings)
             .DataBind();
     }));
})
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(string?)` | Sets the selected value |
| `SetText(string)` | Sets the display text |
| `SetDataSource(source, path)` | Sets data source from event payload or response body |
| `DataBind()` | Flushes pending property changes |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `ShowPopup()` / `HidePopup()` | Open or close the dropdown |
