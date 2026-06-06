using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionContextMenuBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionContextMenuHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ContextMenu and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan that receives context menu event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the underlying Syncfusion context menu builder.</param>
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
