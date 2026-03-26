using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Grids;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionGrid (non-input component).
    /// No InputField wrapper. No ComponentsMap registration.
    /// Uses explicit string elementId (not model-expression-derived).
    /// </summary>
    public static class FusionGridHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Grid component with reactive wiring support.
        /// Non-input component: renders directly, no label/validation wrapper.
        /// </summary>
        public static FusionGridBuilder<TModel> FusionGrid<TModel>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            string elementId,
            Action<GridBuilder<object>> configure)
            where TModel : class
        {
            // NO ComponentsMap registration — Grid is NOT an input component

            var builder = html.EJS().Grid<object>(elementId);
            configure(builder);

            return new FusionGridBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
