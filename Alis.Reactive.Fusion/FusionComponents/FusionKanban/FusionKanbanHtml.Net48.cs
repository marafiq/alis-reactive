#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Kanban;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a Kanban board.
/// </summary>
public static class FusionKanbanHtml
{
    /// <summary>Renders a Kanban board under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the board's element.</param>
    /// <param name="build">What the view sets on the board.</param>
    /// <returns>The rendered board, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionKanban, TModel> FusionKanban<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<KanbanBuilder> build)
        => throw new NotImplementedException();
}
#endif
