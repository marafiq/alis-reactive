using System;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            this IHtmlHelper<TModel> html,
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
