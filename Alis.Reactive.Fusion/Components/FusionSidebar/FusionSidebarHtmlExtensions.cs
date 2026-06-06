using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionSidebarBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionSidebarHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Sidebar and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving sidebar event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
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
