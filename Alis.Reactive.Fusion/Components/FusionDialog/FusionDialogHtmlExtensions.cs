using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Popups;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionDialogBuilder.
    /// Non-input component — NO InputField wrapper, NO input component registration.
    /// </summary>
    public static class FusionDialogHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionDialog with the given element ID.
        /// Non-input component: renders directly, no label/validation wrapper.
        /// </summary>
        public static FusionDialogBuilder<TModel> FusionDialog<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<DialogBuilder> build)
            where TModel : class
        {
            // NO input component registration — this is NOT an input component

            var builder = html.EJS().Dialog(elementId);
            build(builder);

            return new FusionDialogBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
