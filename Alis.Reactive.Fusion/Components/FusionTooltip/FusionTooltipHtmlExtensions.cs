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
    /// Factory extension for creating FusionTooltipBuilder.
    /// Non-input component — NO InputField wrapper, NO ComponentsMap registration.
    /// </summary>
    public static class FusionTooltipHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionTooltip with the given element ID.
        /// Non-input component: renders directly, no label/validation wrapper.
        /// </summary>
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
            // NO ComponentsMap registration — this is NOT an input component

            var builder = html.EJS().Tooltip(elementId);
            build(builder);

            return new FusionTooltipBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
