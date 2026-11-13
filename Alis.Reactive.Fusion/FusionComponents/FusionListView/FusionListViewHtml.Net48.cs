#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Lists;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a list view.
/// </summary>
public static class FusionListViewHtml
{
    /// <summary>Renders a list view under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the list's element.</param>
    /// <param name="build">What the view sets on the list.</param>
    /// <returns>The rendered list, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionListView, TModel> FusionListView<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<ListViewBuilder> build)
        => throw new NotImplementedException();
}
#endif
