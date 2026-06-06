---
title: FusionMultiSelect
description: Multi-select dropdown. Value is a string[].
sidebar:
  order: 9
---

A multi-select dropdown. Use it when a record can belong to several categories -- resident allergies, staff certifications, care-plan goals. Selected values are a `string[]`.

**Model type:** `string[]` &nbsp; **ValueMember:** `"value"` &nbsp; **Events:** `Changed`, `Filtering`

## How do I render one?

```csharp
Html.InputField(plan, m => m.Allergies, o => o.Label("Allergies"))
    .FusionMultiSelect(b => b
        .DataSource(allergies)
        .Fields<AllergyItem>(x => x.Text, x => x.Value));
```

## How do I read the selected values?

`Value()` returns the whole `string[]` as a typed source -- feed it into `SetText`, gather, or a condition.

```csharp
var comp = p.Component<FusionMultiSelect>(m => m.Allergies);
p.Element("value-echo").SetText(comp.Value());
```

## How do I fetch suggestions from the server?

Same pattern as AutoComplete -- the `Filtering` event fires per keystroke, cancel the built-in filter with `args.PreventDefault(p)`, then push fresh results via `args.UpdateData(...)`.

```csharp
.Reactive(plan, evt => evt.Filtering, (args, p) =>
{
    args.PreventDefault(p);
    p.Get("/api/supplies?q=", g => g.Include(m => m.Supplies))
     .Response(r => r.OnSuccess<SuppliesResponse>((json, s) =>
     {
         args.UpdateData(s, json, j => j.Items);
     }));
})
```

## Reference

| Extension | Description |
|---|---|
| `SetValue(string[]?)` | Sets the selected values |
| `SetDataSource(source, path)` | Sets the data source |
| `DataBind()` | Applies pending property changes to the rendered component |
| `ShowPopup()` / `HidePopup()` | Open or close the popup |
| `Value()` | Reads the current selected values as a typed source (`TypedComponentSource<string[]>`) for conditions and gather |
