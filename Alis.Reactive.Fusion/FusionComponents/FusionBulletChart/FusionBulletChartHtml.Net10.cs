#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public static Rendered<FusionBulletChart, TModel> FusionBulletChart<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<BulletChartBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Renders a bullet chart under the id the view gives it over the items the view hands it.</summary>
    /// <remarks>The items reach the chart named as the app's JSON names them, the names <see cref="Fields{TItem, TItemValue, TTarget}"/> reads.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the chart's element.</param>
    /// <param name="items">The items the chart draws.</param>
    /// <param name="build">What the view sets on the chart.</param>
    /// <returns>The rendered chart, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionBulletChart, TModel> FusionBulletChart<TModel, TItem>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, IReadOnlyList<TItem> items, Action<BulletChartBuilder> build)
        => throw new NotImplementedException();
}
#endif
