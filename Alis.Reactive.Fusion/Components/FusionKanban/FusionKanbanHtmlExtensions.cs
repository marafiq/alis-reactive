using System;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            this IHtmlHelper<TModel> html,
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
