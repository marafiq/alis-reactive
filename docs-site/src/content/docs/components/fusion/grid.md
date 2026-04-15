---
title: FusionGrid
description: Data grid with server-side sort, paging, and filter.
sidebar:
  order: 17
---

A data grid for tabular records -- residents, care incidents, medication schedules. Handles sort, paging, and filter on the server. Non-input component: no `InputField` wrapper, no model binding. You load data by pushing an HTTP response into the grid with `SetDataSource`.

**Render as:** `@(Html.FusionGrid(plan, "id", b => ...))` &nbsp; **Events:** `DataStateChange`

## How do I render a grid with columns?

Pass `GridColumn` instances to `.Columns(...)`. Each column has a `Field` (matching a property on the row type), a `HeaderText`, and a `Width`.

```csharp
@(Html.FusionGrid(plan, "residents-grid", b => b
    .AllowSorting(true)
    .AllowPaging(true)
    .PageSettings(new GridPageSettings { PageSize = 10 })
    .Columns(new List<GridColumn>
    {
        new GridColumn { Field = "name", HeaderText = "Name", Width = "180" },
        new GridColumn { Field = "age", HeaderText = "Age", Width = "80" },
        new GridColumn { Field = "careLevel", HeaderText = "Care Level", Width = "150" },
        new GridColumn { Field = "wing", HeaderText = "Wing", Width = "100" },
    })))
```

When you need compile-time safety on cell templates, use `FusionTemplateExpression` to bind a template to a typed member of the row model.

## How do I wire server-side sort, page, and filter?

The `DataStateChange` event args expose `Skip`, `Take`, `Sorted`, and a nested `Action` describing what triggered the request. Guard on `.NotEq(FusionGridAction.Refresh)` so you do not re-POST when the grid is refreshing itself after an in-place update. Then `Gather` the state, POST it to your data endpoint, and push the response back with `SetDataSource`.

```csharp
.Reactive(evt => evt.DataStateChange, (args, p) =>
{
    p.When(args, x => x.Action.RequestType).NotEq(FusionGridAction.Refresh)
        .Then(t =>
        {
            t.Post("/Sandbox/Components/Grid/Data")
             .Gather(g => g
                 .FromEvent(args, x => x.Skip, "skip")
                 .FromEvent(args, x => x.Take, "take")
                 .FromEvent(args, x => x.Sorted, "sorted")
                 .Include<FusionNumericTextBox, GridModel>(m => m.MinAge))
             .Response(r => r.OnSuccess<ResidentGridResponse>((json, s) =>
             {
                 s.Component<FusionGrid>("residents-grid")
                     .SetDataSource(json);
             }));
        });
})
```

## How do I load the first page on `DomReady`?

```csharp
@{ Html.On(plan, t => t.DomReady(p =>
{
    p.Post("/Sandbox/Components/Grid/Data")
     .Gather(g => g.Static("skip", 0).Static("take", 10))
     .Response(r => r.OnSuccess<ResidentGridResponse>((json, s) =>
     {
         s.Component<FusionGrid>("residents-grid")
             .SetDataSource(json);
     }));
})); }
```

## Reference

| Extension | Description |
|---|---|
| `SetDataSource(ResponseBody<T> source, Expression<Func<T, object?>> path)` | Replaces the grid data source with items selected from a response body |
| `SetDataSource(ResponseBody<T> source)` | Replaces the grid data source with the entire response body (custom binding `{ result, count }`) |
| `SetDataSource(TSource source, Expression<Func<TSource, object?>> path)` | Replaces the grid data source with items from an event payload |
| `Refresh()` | Triggers a grid refresh to re-render with the current data source |
