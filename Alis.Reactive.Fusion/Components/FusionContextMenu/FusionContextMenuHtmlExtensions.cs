using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionContextMenuBuilder.
    /// </summary>
    public static class FusionContextMenuHtmlExtensions
    {
        /// <summary>
        /// Creates a ContextMenu with the given element ID.
        /// </summary>
        public static FusionContextMenuBuilder<TModel> FusionContextMenu<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ContextMenuBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ContextMenu(elementId);
            build(builder);

            return new FusionContextMenuBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
