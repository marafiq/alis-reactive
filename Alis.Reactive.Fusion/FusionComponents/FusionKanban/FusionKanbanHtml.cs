using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Kanban;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionKanbanHtml
{
    private static KanbanBuilder Built<TModel>(ReactivePlan<TModel> plan, KanbanBuilder board, Action<KanbanBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(board);
        if (board.model.DataSource is { } cards) board.model.DataSource = FusionData.AsWritten(plan.Json(cards));
        return board;
    }
}
