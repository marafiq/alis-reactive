#if NET48
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Charts;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionBulletChartHtml
{
    /// <summary>Renders a bullet chart under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the chart's element.</param>
    /// <param name="build">What the view sets on the chart.</param>
    /// <returns>The rendered chart, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionBulletChart, TModel> FusionBulletChart<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<BulletChartBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Renders a bullet chart under the id the view gives it over the items the view hands it.</summary>
    /// <remarks>The items reach the chart named as the app's JSON names them, the names <see cref="Fields{TItem, TItemValue, TTarget}"/> reads.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the chart's element.</param>
    /// <param name="items">The items the chart draws.</param>
    /// <param name="build">What the view sets on the chart.</param>
    /// <returns>The rendered chart, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionBulletChart, TModel> FusionBulletChart<TModel, TItem>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, IReadOnlyList<TItem> items, Action<BulletChartBuilder> build)
        => throw new NotImplementedException();
}
#endif
