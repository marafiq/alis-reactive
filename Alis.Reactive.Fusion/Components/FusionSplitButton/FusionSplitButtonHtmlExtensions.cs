using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.SplitButtons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionSplitButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionSplitButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion SplitButton with a stable component id.
        /// </summary>
        public static FusionSplitButtonBuilder<TModel> FusionSplitButton<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<SplitButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().SplitButton(elementId);
            build(builder);

            return new FusionSplitButtonBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
