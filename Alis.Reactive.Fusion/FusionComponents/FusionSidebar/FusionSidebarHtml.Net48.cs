#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a sidebar.
/// </summary>
public static class FusionSidebarHtml
{
    /// <summary>Renders a sidebar under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the sidebar's element.</param>
    /// <param name="build">What the view sets on the sidebar.</param>
    /// <returns>The rendered sidebar, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSidebar, TModel> FusionSidebar<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<SidebarBuilder> build)
        => throw new NotImplementedException();
}
#endif
