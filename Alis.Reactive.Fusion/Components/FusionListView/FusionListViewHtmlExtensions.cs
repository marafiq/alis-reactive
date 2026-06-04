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
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan that receives list view event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and plan entries.</param>
        /// <param name="build">Configures the underlying Syncfusion list view builder.</param>
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
