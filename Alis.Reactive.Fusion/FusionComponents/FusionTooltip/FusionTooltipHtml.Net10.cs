#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.Popups;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a tooltip.
/// </summary>
public static class FusionTooltipHtml
{
    /// <summary>Renders a tooltip under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the tooltip's element.</param>
    /// <param name="build">What the view sets on the tooltip, its content and its target among them.</param>
    /// <returns>The rendered tooltip, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTooltip, TModel> FusionTooltip<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<TooltipBuilder> build)
        => throw new NotImplementedException();
}
#endif
