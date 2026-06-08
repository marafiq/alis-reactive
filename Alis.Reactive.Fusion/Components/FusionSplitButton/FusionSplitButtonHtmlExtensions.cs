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
    /// Creates a FusionSplitButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionSplitButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion SplitButton with a stable component id.
        /// </summary>
        public static FusionSplitButtonBuilder<TModel> FusionSplitButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
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
