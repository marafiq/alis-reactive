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
    /// <summary>
    /// Creates typed <see cref="FusionBulletChartBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionBulletChartHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion BulletChart and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving bullet chart event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static FusionBulletChartBuilder<TModel> FusionBulletChart<TModel>(
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

            return new FusionBulletChartBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
