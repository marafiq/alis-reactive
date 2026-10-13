using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Charts;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion BulletChart, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionBulletChart(plan, "census-chart", b =&gt; ...)</c>, then reach it in a callback
    /// and use its members directly:
    /// <code>
    /// var chart = p.Component&lt;FusionBulletChart&gt;("census-chart");
    /// p.Element("last-position").Text.Set(chart.GetActualIndex(-1, 5));
    /// </code>
    /// </remarks>
    public sealed class FusionBulletChart : FusionComponent
    {
        /// <summary>Wraps an index into a range of items: past the last item it becomes 0, below 0 it becomes the last index.</summary>
        /// <param name="index">The zero-based index to wrap.</param>
        /// <param name="totalLength">The number of items in the range.</param>
        /// <returns>The index within the range; an index already inside it comes back unchanged.</returns>
        public TypedSource<int> GetActualIndex(Arg<int> index, Arg<int> totalLength) => Invoke<int>(index, totalLength);

        /// <summary>Raised before a tooltip renders.</summary>
        public TypedEvent<FusionBulletChartTooltipRenderArgs> TooltipRender =>
            Event<FusionBulletChartTooltipRenderArgs>();

        /// <summary>Raised when the user clicks the chart.</summary>
        public TypedEvent<FusionBulletChartMouseClickArgs> BulletChartMouseClick =>
            Event<FusionBulletChartMouseClickArgs>();
    }

    /// <summary>Payload of <see cref="FusionBulletChart.TooltipRender"/>.</summary>
    /// <remarks>Rewrite the tooltip: <c>args.Text.Set("On target")</c>.</remarks>
    public class FusionBulletChartTooltipRenderArgs : JsObject
    {
        /// <summary>Actual value of the feature bar.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Target values of the comparative bar.</summary>
        public ReadOnly<string[]> Target => ReadOnly<string[]>();

        /// <summary>The event name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Tooltip template markup when the chart renders a template.</summary>
        public ReadOnly<string> Template => ReadOnly<string>();

        /// <summary>Tooltip text; write it to replace the text before it renders.</summary>
        public Prop<string> Text => Prop<string>();
    }

    /// <summary>Payload of <see cref="FusionBulletChart.BulletChartMouseClick"/>.</summary>
    public class FusionBulletChartMouseClickArgs : JsObject
    {
        /// <summary>ID of the clicked element.</summary>
        public ReadOnly<string> Target => ReadOnly<string>();

        /// <summary>Horizontal position of the click in the chart.</summary>
        public ReadOnly<double> X => ReadOnly<double>();

        /// <summary>Vertical position of the click in the chart.</summary>
        public ReadOnly<double> Y => ReadOnly<double>();
    }
#nullable restore annotations

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionBulletChartHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion BulletChart and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving bullet chart event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionBulletChart, TModel> FusionBulletChart<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<BulletChartBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().BulletChart(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionBulletChart>(elementId, builder.Render());
        }
    }
}
