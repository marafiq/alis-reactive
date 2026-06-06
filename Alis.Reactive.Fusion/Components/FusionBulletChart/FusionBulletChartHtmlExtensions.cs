using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Charts;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionBulletChartBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionBulletChartHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion BulletChart and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">The Reactive Plan that receives bullet chart event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the underlying Syncfusion bullet chart builder.</param>
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
