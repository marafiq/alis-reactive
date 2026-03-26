using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionTab (non-input component).
    /// No InputField wrapper. No ComponentsMap registration.
    /// Uses explicit string elementId (not model-expression-derived).
    /// </summary>
    public static class FusionTabHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Tab component with reactive wiring support.
        /// Non-input component: renders directly, no label/validation wrapper.
        /// </summary>
        public static FusionTabBuilder<TModel> FusionTab<TModel>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            string elementId,
            Action<TabBuilder> configure)
            where TModel : class
        {
            // NO ComponentsMap registration — Tab is NOT an input component

            var builder = html.EJS().Tab(elementId);
            configure(builder);

            return new FusionTabBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
