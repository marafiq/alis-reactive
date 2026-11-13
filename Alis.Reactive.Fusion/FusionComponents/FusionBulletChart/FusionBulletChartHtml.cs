using System;
using System.Linq.Expressions;
using Syncfusion.EJ2.Charts;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a bullet chart.
/// </summary>
public static partial class FusionBulletChartHtml
{
    /// <summary>Names the members of an item the chart draws as its value and its target.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Shift s) =&gt; s.Hours</c>.</remarks>
    /// <param name="build">The chart's builder.</param>
    /// <param name="value">The member drawn as an item's value.</param>
    /// <param name="target">The member drawn as an item's target.</param>
    /// <returns>The builder.</returns>
    public static BulletChartBuilder Fields<TItem, TItemValue, TTarget>(this BulletChartBuilder build, Expression<Func<TItem, TItemValue>> value, Expression<Func<TItem, TTarget>> target)
        => throw new NotImplementedException();

    /// <summary>Names the members of an item the chart draws as its value and its target, and the category it shows it under.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Shift s) =&gt; s.Hours</c>.</remarks>
    /// <param name="build">The chart's builder.</param>
    /// <param name="value">The member drawn as an item's value.</param>
    /// <param name="target">The member drawn as an item's target.</param>
    /// <param name="category">The member shown as an item's category.</param>
    /// <returns>The builder.</returns>
    public static BulletChartBuilder Fields<TItem, TItemValue, TTarget, TCategory>(this BulletChartBuilder build, Expression<Func<TItem, TItemValue>> value, Expression<Func<TItem, TTarget>> target, Expression<Func<TItem, TCategory>> category)
        => throw new NotImplementedException();
}
