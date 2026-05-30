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
        /// <summary>Creates a FusionListBox with the given element ID.</summary>
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
