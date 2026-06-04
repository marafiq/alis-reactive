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
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan that receives toolbar event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and plan entries.</param>
        /// <param name="build">Configures the underlying Syncfusion toolbar builder.</param>
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
