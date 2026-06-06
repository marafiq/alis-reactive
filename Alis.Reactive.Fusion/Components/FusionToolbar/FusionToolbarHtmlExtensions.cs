using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionToolbarBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionToolbarHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Toolbar and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving toolbar event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
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
