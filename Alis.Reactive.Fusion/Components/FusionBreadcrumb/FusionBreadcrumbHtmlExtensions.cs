using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating a typed FusionBreadcrumb slice.
    /// Non-input component — no input registration wrapper.
    /// </summary>
    public static class FusionBreadcrumbHtmlExtensions
    {
        public static FusionBreadcrumbBuilder<TModel> FusionBreadcrumb<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<BreadcrumbBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Breadcrumb(elementId);
            build(builder);

            return new FusionBreadcrumbBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
