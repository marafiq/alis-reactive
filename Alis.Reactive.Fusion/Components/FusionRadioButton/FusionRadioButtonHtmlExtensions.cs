using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionRadioButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionRadioButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion RadioButton with a stable component id.
        /// </summary>
        public static FusionRadioButtonBuilder<TModel> FusionRadioButton<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<RadioButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().RadioButton(elementId);
            build(builder);

            return new FusionRadioButtonBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
