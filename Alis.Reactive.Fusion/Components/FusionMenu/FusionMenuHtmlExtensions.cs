using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionMenuBuilder.
    /// </summary>
    public static class FusionMenuHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionMenu with the given element ID.
        /// </summary>
        public static FusionMenuBuilder<TModel> FusionMenu<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<MenuBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Menu(elementId);
            build(builder);

            return new FusionMenuBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
