using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.PivotView;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionPivotViewHtml
{
    private static PivotViewBuilder Built<TModel>(ReactivePlan<TModel> plan, PivotViewBuilder pivot, Action<PivotViewBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(pivot);
        if (pivot.model.DataSourceSettings is { } named) NameEach(named, new PlanPivotRowNames<TModel>(plan));
        if (pivot.model.DataSourceSettings is { DataSource: { } rows } settings) settings.DataSource = FusionData.AsWritten(plan.Json(rows));
        return pivot;
    }

    private static void NameEach(PivotViewDataSourceSettings settings, PivotRowNames names)
    {
        NameEach(settings.Rows, names);
        NameEach(settings.Columns, names);
        NameEach(settings.Values, names);
        NameEach(settings.Filters, names);
        NameEach(settings.FieldMapping, names);
        foreach (var format in settings.FormatSettings ?? []) format.Name = FusionPivotViewNames.Named(format.Name, names);
        foreach (var sort in settings.SortSettings ?? []) sort.Name = FusionPivotViewNames.Named(sort.Name, names);
        foreach (var drilled in settings.DrilledMembers ?? []) drilled.Name = FusionPivotViewNames.Named(drilled.Name, names);
        foreach (var group in settings.GroupSettings ?? []) group.Name = FusionPivotViewNames.Named(group.Name, names);
        foreach (var condition in settings.ConditionalFormatSettings ?? []) condition.Measure = FusionPivotViewNames.Named(condition.Measure, names);
        if (settings.ExcludeFields is { } excluded) settings.ExcludeFields = [.. excluded.Select(field => FusionPivotViewNames.Named(field, names)!)];
        foreach (var filter in settings.FilterSettings ?? [])
        {
            filter.Name = FusionPivotViewNames.Named(filter.Name, names);
            filter.Measure = FusionPivotViewNames.Named(filter.Measure, names);
            filter.SelectedField = FusionPivotViewNames.Named(filter.SelectedField, names);
        }
    }

    private static void NameEach(List<PivotViewRow>? fields, PivotRowNames names)
    {
        foreach (var field in fields ?? [])
        {
            field.Name = FusionPivotViewNames.Named(field.Name, names);
            field.BaseField = FusionPivotViewNames.Named(field.BaseField, names);
        }
    }
}
