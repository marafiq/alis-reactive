using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Kanban;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for rendering Syncfusion Kanban with reactive event support.
    /// Static board setup stays on Syncfusion's MVC builder.
    /// </summary>
    public static class FusionKanbanHtmlExtensions
    {
        public static FusionKanbanBuilder<TModel> FusionKanban<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<KanbanBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Kanban(elementId);
            build(builder);

            return new FusionKanbanBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
