using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Charts;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionBulletChartBuilder.
    /// </summary>
    public static class FusionBulletChartHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionBulletChart with the given element ID.
        /// </summary>
        public static FusionBulletChartBuilder<TModel> FusionBulletChart<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<BulletChartBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().BulletChart(elementId);
            build(builder);

            return new FusionBulletChartBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
