#if NET48
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Grids;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a grid.
/// </summary>
public static class FusionGridHtml
{
    /// <summary>Renders a grid under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <remarks>Write both type arguments, the view's model and the row class.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the grid's element.</param>
    /// <param name="build">What the view sets on the grid.</param>
    /// <returns>The rendered grid, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionGrid, TModel> FusionGrid<TModel, TRow>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<GridBuilder<TRow>> build) where TRow : class
        => throw new NotImplementedException();

    /// <summary>Renders a grid under the id the view gives it over the rows the view hands it.</summary>
    /// <remarks>The rows reach the grid named as the app's JSON names them, the names <see cref="FusionGridColumns{TRow}.Field{TValue}"/> gives a column.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the grid's element.</param>
    /// <param name="rows">The rows the grid shows.</param>
    /// <param name="build">What the view sets on the grid.</param>
    /// <returns>The rendered grid, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionGrid, TModel> FusionGrid<TModel, TRow>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, IReadOnlyList<TRow> rows, Action<GridBuilder<TRow>> build) where TRow : class
        => throw new NotImplementedException();
}
#endif
