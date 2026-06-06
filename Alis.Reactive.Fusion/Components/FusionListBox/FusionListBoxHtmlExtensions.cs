using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionListBox with Syncfusion MVC builder-owned static configuration.
    /// </summary>
    public static class FusionListBoxHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ListBox and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving list box event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static FusionListBoxBuilder<TModel> FusionListBox<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ListBoxBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ListBox(elementId);
            build(builder);

            return new FusionListBoxBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
