using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionMenuBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionMenuHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Menu and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">The Reactive Plan that receives menu event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
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
