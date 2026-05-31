using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.SplitButtons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionDropDownButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionDropDownButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion DropDownButton with a stable component id.
        /// </summary>
        public static FusionDropDownButtonBuilder<TModel> FusionDropDownButton<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<DropDownButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().DropDownButton(elementId);
            build(builder);

            return new FusionDropDownButtonBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
