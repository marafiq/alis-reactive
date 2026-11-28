using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Grids;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionGridHtml
{
    private static GridBuilder<TRow> Built<TModel, TRow>(ReactivePlan<TModel> plan, GridBuilder<TRow> grid, Action<GridBuilder<TRow>> build)
        where TRow : class
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(grid);
        var names = new PlanRowNames<TModel>(plan);
        if (grid.model.Columns is List<GridColumn> columns) NameEach(columns, names);
        if (grid.model.EditSettings is { Template: { } template } editing) editing.Template = FusionGridNames.Named(template, names);
        return grid;
    }

    private static void NameEach(List<GridColumn> columns, RowNames names)
    {
        foreach (var column in columns)
        {
            if (column.Field is { } field) column.Field = FusionGridNames.Named(field, names);
            if (column.EditTemplate is { } editor) column.EditTemplate = FusionGridNames.Named(editor, names);
            if (column.Columns is List<GridColumn> stacked) NameEach(stacked, names);
        }
    }
}
