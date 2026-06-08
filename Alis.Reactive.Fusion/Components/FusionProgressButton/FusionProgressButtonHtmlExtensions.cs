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
    /// Creates a FusionProgressButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionProgressButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion ProgressButton with a stable component id.
        /// </summary>
        public static FusionProgressButtonBuilder<TModel> FusionProgressButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ProgressButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ProgressButton(elementId);
            build(builder);

            return new FusionProgressButtonBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
