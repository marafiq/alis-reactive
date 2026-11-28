#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
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
    public static Rendered<FusionTab, TModel> FusionTab<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<TabBuilder> build)
    {
        if (plan is null) throw new ArgumentNullException(nameof(plan));
        if (elementId is null) throw new ArgumentNullException(nameof(elementId));
        if (build is null) throw new ArgumentNullException(nameof(build));
        var tab = html.EJS().Tab(elementId);
        build(tab);
        return plan.Render<FusionTab>(elementId, tab.Render());
    }
}
#endif
