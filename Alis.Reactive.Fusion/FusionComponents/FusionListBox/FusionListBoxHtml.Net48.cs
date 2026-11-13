#if NET48
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a list box.
/// </summary>
public static class FusionListBoxHtml
{
    /// <summary>Renders a list box under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <remarks>Name every type argument: the model and the list the values of the selected items make.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the box's element.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionListBox<TValue>, TModel> FusionListBox<TModel, TValue>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<ListBoxBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Renders a list box under the id the view gives it over the items the view hands it.</summary>
    /// <remarks>
    /// The items reach the box named as the app's JSON names them. Name every type argument: the model, the list the
    /// values of the selected items make and the items' class.
    /// </remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the box's element.</param>
    /// <param name="items">The items the box offers.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered box, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionListBox<TValue>, TModel> FusionListBox<TModel, TValue, TItem>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, IReadOnlyList<TItem> items, Action<ListBoxBuilder> build)
        => throw new NotImplementedException();
}
#endif
