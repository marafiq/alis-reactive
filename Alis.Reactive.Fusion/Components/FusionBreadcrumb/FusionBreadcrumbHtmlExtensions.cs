using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionBreadcrumbBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionBreadcrumbHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Breadcrumb and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving breadcrumb event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
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
