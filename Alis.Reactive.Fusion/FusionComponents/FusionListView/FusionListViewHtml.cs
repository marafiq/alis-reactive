using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Lists;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionListViewHtml
{
    private static ListViewBuilder Built<TModel>(ReactivePlan<TModel> plan, ListViewBuilder list, Action<ListViewBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(list);
        if (list.model.DataSource is { } items) list.model.DataSource = FusionData.AsWritten(plan.Json(items));
        return list;
    }
}
