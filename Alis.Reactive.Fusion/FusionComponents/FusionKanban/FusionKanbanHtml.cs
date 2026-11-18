using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Kanban;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionKanbanHtml
{
    private static KanbanBuilder Built<TModel>(ReactivePlan<TModel> plan, KanbanBuilder board, Action<KanbanBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(board);
        var names = new PlanCardNames<TModel>(plan);
        board.model.KeyField = FusionKanbanNames.Named(board.model.KeyField, names);
        if (board.model.CardSettings is { } card) NameEach(card, names);
        if (board.model.SwimlaneSettings is { } lanes) NameEach(lanes, names);
        if (board.model.SortSettings is { } sort) sort.Field = FusionKanbanNames.Named(sort.Field, names);
        if (board.model.DataSource is { } cards) board.model.DataSource = FusionData.AsWritten(plan.Json(cards));
        return board;
    }

    private static void NameEach(KanbanCardSettings card, CardNames names)
    {
        card.HeaderField = FusionKanbanNames.Named(card.HeaderField, names);
        card.ContentField = FusionKanbanNames.Named(card.ContentField, names);
        card.TagsField = FusionKanbanNames.Named(card.TagsField, names);
        card.GrabberField = FusionKanbanNames.Named(card.GrabberField, names);
        card.FooterCssField = FusionKanbanNames.Named(card.FooterCssField, names);
    }

    private static void NameEach(KanbanSwimlaneSettings lanes, CardNames names)
    {
        lanes.KeyField = FusionKanbanNames.Named(lanes.KeyField, names);
        lanes.TextField = FusionKanbanNames.Named(lanes.TextField, names);
    }
}
