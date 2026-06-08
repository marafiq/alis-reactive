using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
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
        /// <param name="plan">Reactive Plan receiving dialog event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static FusionDialogBuilder<TModel> FusionDialog<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
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
