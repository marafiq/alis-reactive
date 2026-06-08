using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.PivotView;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for rendering Syncfusion PivotView with reactive event support.
    /// </summary>
    public static class FusionPivotViewHtmlExtensions
    {
        public static FusionPivotViewBuilder<TModel> FusionPivotView<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<PivotViewBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().PivotView(elementId);
            build(builder);

            return new FusionPivotViewBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
