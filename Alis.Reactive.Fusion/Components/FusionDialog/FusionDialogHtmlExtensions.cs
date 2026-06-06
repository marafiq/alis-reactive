using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Popups;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionDialogBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionDialogHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Dialog and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan that receives dialog event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the underlying Syncfusion dialog builder.</param>
        public static FusionDialogBuilder<TModel> FusionDialog<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<DialogBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Dialog(elementId);
            build(builder);

            return new FusionDialogBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
