using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Popups;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionTooltipBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionTooltipHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Tooltip and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving tooltip event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static FusionTooltipBuilder<TModel> FusionTooltip<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<TooltipBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Tooltip(elementId);
            build(builder);

            return new FusionTooltipBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
