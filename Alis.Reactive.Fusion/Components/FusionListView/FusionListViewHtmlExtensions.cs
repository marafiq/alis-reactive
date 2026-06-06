using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Lists;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionListView with Syncfusion MVC builder-owned static configuration.
    /// </summary>
    public static class FusionListViewHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ListView and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving list view event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static FusionListViewBuilder<TModel> FusionListView<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ListViewBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ListView(elementId);
            build(builder);

            return new FusionListViewBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
