using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.PivotView;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionPivotViewHtml
{
    private static PivotViewBuilder Built<TModel>(ReactivePlan<TModel> plan, PivotViewBuilder pivot, Action<PivotViewBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(pivot);
        if (pivot.model.DataSourceSettings is { DataSource: { } rows } settings) settings.DataSource = FusionData.AsWritten(plan.Json(rows));
        return pivot;
    }
}
