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
    /// Render it with <c>Html.FusionBulletChart(plan, "census-chart", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionBulletChart&gt;("census-chart")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Get(c =&gt; c.GetActualIndex, 2, 5)</c>.
    /// </remarks>
    public sealed class FusionBulletChart : FusionComponent
    {
        /// <summary>Maps a data index to its drawn position, given the number of points.</summary>
        public TypedSource<int> GetActualIndex(Arg<int> first, Arg<int> second) => Invoke<int>(first, second);

        /// <summary>Raised before a tooltip renders.</summary>
        public TypedEvent<FusionBulletChartTooltipRenderArgs> TooltipRender =>
            Event<FusionBulletChartTooltipRenderArgs>("tooltipRender");

        /// <summary>Raised when the user clicks the chart.</summary>
        public TypedEvent<FusionBulletChartMouseClickArgs> BulletChartMouseClick =>
            Event<FusionBulletChartMouseClickArgs>("bulletChartMouseClick");
    }

    /// <summary>Payload of <see cref="FusionBulletChart.TooltipRender"/>.</summary>
    /// <remarks>Rewrite the tooltip: <c>p.Payload(args).Set(a =&gt; a.Text, "On target")</c>.</remarks>
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
