using System;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        /// <param name="plan">The Reactive Plan that receives tooltip event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the underlying Syncfusion tooltip builder.</param>
        public static FusionTooltipBuilder<TModel> FusionTooltip<TModel>(
            this IHtmlHelper<TModel> html,
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
