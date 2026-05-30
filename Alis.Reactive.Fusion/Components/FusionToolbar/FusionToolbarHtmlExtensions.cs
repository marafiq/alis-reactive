using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionToolbarBuilder.
    /// Non-input component - NO InputField wrapper, NO input component registration.
    /// </summary>
    public static class FusionToolbarHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionToolbar with the given element ID.
        /// </summary>
        public static FusionToolbarBuilder<TModel> FusionToolbar<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ToolbarBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Toolbar(elementId);
            build(builder);

            return new FusionToolbarBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
