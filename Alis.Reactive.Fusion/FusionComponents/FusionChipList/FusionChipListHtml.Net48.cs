#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a chip list.
/// </summary>
public static class FusionChipListHtml
{
    /// <summary>Renders a chip list under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the list's element.</param>
    /// <param name="build">What the view sets on the list, its chips and selection among them.</param>
    /// <returns>The rendered list, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionChipList, TModel> FusionChipList<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<ChipListBuilder> build)
        => throw new NotImplementedException();
}
#endif
