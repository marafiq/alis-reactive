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
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TRow">The grid row model used by the Syncfusion builder.</typeparam>
        /// <param name="html">The HTML helper.</param>
        /// <param name="plan">The Reactive Plan that registers component behavior.</param>
        /// <param name="elementId">The controlled component ID shared by markup and plan entries.</param>
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
            var builder = html.EJS().Grid<TRow>(elementId);
            build(builder);

            return new FusionGridBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
