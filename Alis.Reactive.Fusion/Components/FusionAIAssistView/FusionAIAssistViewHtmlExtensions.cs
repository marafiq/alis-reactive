using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.InteractiveChat;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for rendering Syncfusion AIAssistView with reactive event support.
    /// Static component configuration stays on Syncfusion's MVC builder.
    /// </summary>
    public static class FusionAIAssistViewHtmlExtensions
    {
        public static FusionAIAssistViewBuilder<TModel> FusionAIAssistView<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<AIAssistViewBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().AIAssistView(elementId);
            build(builder);

            return new FusionAIAssistViewBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
