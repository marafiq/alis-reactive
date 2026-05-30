using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionSidebarBuilder.
    /// Non-input component - NO InputField wrapper, NO input component registration.
    /// </summary>
    public static class FusionSidebarHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionSidebar with the given element ID.
        /// </summary>
        public static FusionSidebarBuilder<TModel> FusionSidebar<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<SidebarBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Sidebar(elementId);
            build(builder);

            return new FusionSidebarBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
