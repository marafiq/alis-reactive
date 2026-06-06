using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Grids;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionGridBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionGridHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Grid and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TRow">The grid row model used by the Syncfusion builder.</typeparam>
        /// <param name="plan">Reactive Plan registering Grid behavior.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Callback to configure columns, paging, sorting, etc.</param>
        /// <returns>Builder for chaining <c>.Reactive()</c>.</returns>
        public static FusionGridBuilder<TModel> FusionGrid<TModel, TRow>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<GridBuilder<TRow>> build)
            where TModel : class
            where TRow : class
        {
            var builder = html.EJS().Grid<TRow>(elementId);
            build(builder);

            return new FusionGridBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
