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
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="html">The MVC HTML helper.</param>
        /// <param name="plan">The reactive plan that will reference this component.</param>
        /// <param name="elementId">The controlled browser id used as the runtime join key.</param>
        /// <param name="build">Configures initial Syncfusion Button options.</param>
        /// <returns>The rendered Syncfusion button content.</returns>
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
