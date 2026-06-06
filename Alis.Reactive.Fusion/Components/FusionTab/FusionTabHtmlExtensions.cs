using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionTabBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionTabHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Tab and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">The Reactive Plan that receives tab event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the underlying Syncfusion tab builder.</param>
        public static FusionTabBuilder<TModel> FusionTab<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<TabBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Tab(elementId);
            build(builder);

            return new FusionTabBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
