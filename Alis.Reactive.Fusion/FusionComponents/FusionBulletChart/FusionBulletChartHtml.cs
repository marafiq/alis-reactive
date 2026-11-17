using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Charts;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a bullet chart.
/// </summary>
public static partial class FusionBulletChartHtml
{
    private static readonly ConditionalWeakTable<BulletChartBuilder, Action<BulletChartBuilder, ItemPaths>> NamedFields = new();

    /// <summary>Names the members of an item the chart draws as its value and its target.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Shift s) =&gt; s.Hours</c>.</remarks>
    /// <param name="build">The chart's builder.</param>
    /// <param name="value">The member drawn as an item's value.</param>
    /// <param name="target">The member drawn as an item's target.</param>
    /// <returns>The builder.</returns>
    public static BulletChartBuilder Fields<TItem, TItemValue, TTarget>(this BulletChartBuilder build, Expression<Func<TItem, TItemValue>> value, Expression<Func<TItem, TTarget>> target)
        => Named(build, (chart, paths) => chart.ValueField(paths.Of(value)).TargetField(paths.Of(target)));

    /// <summary>Names the members of an item the chart draws as its value and its target, and the category it shows it under.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Shift s) =&gt; s.Hours</c>.</remarks>
    /// <param name="build">The chart's builder.</param>
    /// <param name="value">The member drawn as an item's value.</param>
    /// <param name="target">The member drawn as an item's target.</param>
    /// <param name="category">The member shown as an item's category.</param>
    /// <returns>The builder.</returns>
    public static BulletChartBuilder Fields<TItem, TItemValue, TTarget, TCategory>(this BulletChartBuilder build, Expression<Func<TItem, TItemValue>> value, Expression<Func<TItem, TTarget>> target, Expression<Func<TItem, TCategory>> category)
        => Named(build, (chart, paths) => chart.ValueField(paths.Of(value)).TargetField(paths.Of(target)).CategoryField(paths.Of(category)));

    private static BulletChartBuilder Built<TModel>(ReactivePlan<TModel> plan, BulletChartBuilder chart, Action<BulletChartBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(chart);
        if (NamedFields.TryGetValue(chart, out var fields)) fields(chart, new PlanPaths<TModel>(plan));
        return chart;
    }

    private static BulletChartBuilder Items<TModel, TItem>(ReactivePlan<TModel> plan, BulletChartBuilder chart, IReadOnlyList<TItem> items)
        => chart.DataSource(FusionData.AsWritten(plan.Json(items ?? throw new ArgumentNullException(nameof(items)))));

    private static BulletChartBuilder Named(BulletChartBuilder build, Action<BulletChartBuilder, ItemPaths> fields)
    {
        NamedFields.Remove(build);
        NamedFields.Add(build, fields);
        return build;
    }

    private abstract class ItemPaths
    {
        internal abstract string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member);
    }

    private sealed class PlanPaths<TModel>(ReactivePlan<TModel> plan) : ItemPaths
    {
        internal override string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member) => string.Join(".", plan.JsonPath(member));
    }
}
