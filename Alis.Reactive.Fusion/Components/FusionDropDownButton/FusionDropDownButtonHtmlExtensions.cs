using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
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
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
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
