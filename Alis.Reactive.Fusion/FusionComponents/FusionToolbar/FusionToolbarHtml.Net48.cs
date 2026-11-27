#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a toolbar.
/// </summary>
public static class FusionToolbarHtml
{
    /// <summary>Renders a toolbar under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the toolbar's element.</param>
    /// <param name="build">What the view sets on the toolbar.</param>
    /// <returns>The rendered toolbar, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionToolbar, TModel> FusionToolbar<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<ToolbarBuilder> build)
    {
        if (plan is null) throw new ArgumentNullException(nameof(plan));
        if (elementId is null) throw new ArgumentNullException(nameof(elementId));
        if (build is null) throw new ArgumentNullException(nameof(build));
        var toolbar = html.EJS().Toolbar(elementId);
        build(toolbar);
        return plan.Render<FusionToolbar>(elementId, toolbar.Render());
    }
}
#endif
