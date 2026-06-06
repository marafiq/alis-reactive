using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionButtonHtmlExtensions
    {
        /// <summary>
        /// Renders a non-input FusionButton with a stable component id.
        /// </summary>
        /// <param name="plan">Reactive Plan referencing this component.</param>
        /// <param name="elementId">Controlled DOM element ID used as the runtime join key.</param>
        /// <param name="build">Configures initial Syncfusion Button options.</param>
        /// <returns>Builder that renders the Syncfusion button and carries its Reactive Plan ID.</returns>
        public static FusionButtonBuilder<TModel> FusionButton<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Button(elementId);
            build(builder);

            return new FusionButtonBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
