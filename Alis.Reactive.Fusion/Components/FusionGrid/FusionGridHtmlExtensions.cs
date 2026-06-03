using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Grids;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating a <see cref="FusionGridBuilder{TModel}"/>.
    /// Non-input component: no InputField wrapper, no input component registration.
    /// </summary>
    public static class FusionGridHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionGrid with the given element ID.
        /// Non-input component: renders directly, no label/validation wrapper.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <param name="html">The HTML helper.</param>
        /// <param name="plan">The Reactive Plan that registers component behavior.</param>
        /// <param name="elementId">The DOM element ID for the grid.</param>
        /// <param name="build">Callback to configure columns, paging, sorting, etc.</param>
        /// <returns>A builder for chaining <c>.Reactive()</c>.</returns>
        public static FusionGridBuilder<TModel> FusionGrid<TModel, TRow>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<GridBuilder<TRow>> build)
            where TModel : class
            where TRow : class
        {
            // NO input component registration — this is NOT an input component

            var builder = html.EJS().Grid<TRow>(elementId);
            build(builder);

            return new FusionGridBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
