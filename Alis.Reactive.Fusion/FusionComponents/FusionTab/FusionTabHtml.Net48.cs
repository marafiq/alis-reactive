#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a tab strip.
/// </summary>
public static class FusionTabHtml
{
    /// <summary>Renders a tab strip under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the strip's element.</param>
    /// <param name="build">What the view sets on the strip and its items.</param>
    /// <returns>The rendered strip, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionTab, TModel> FusionTab<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<TabBuilder> build)
        => throw new NotImplementedException();
}
#endif
