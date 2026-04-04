using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// Provides confirm-dialog mutations and page-level rendering helpers.
    /// </summary>
    public static class FusionConfirmExtensions
    {
        private static readonly CapabilityProperty ContentProperty = CapabilityProperty.Named("content");
        private static readonly CapabilityMethod DataBindMethod = CapabilityMethod.Named("dataBind");
        private static readonly CapabilityMethod ShowMethod = CapabilityMethod.Named("show");
        private static readonly CapabilityMethod HideMethod = CapabilityMethod.Named("hide");

        /// <summary>Sets the confirmation message content and flushes it to the dialog instance.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="message">The confirmation message to display.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionConfirm, TModel> SetContent<TModel>(
            this ComponentRef<FusionConfirm, TModel> self, string message)
            where TModel : class
        {
            return self.Set(ContentProperty, message)
                       .Call(DataBindMethod);
        }

        /// <summary>Shows the confirmation dialog.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionConfirm, TModel> Show<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => self.Call(ShowMethod);

        /// <summary>Hides the confirmation dialog.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<FusionConfirm, TModel> Hide<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
            => self.Call(HideMethod);

        /// <summary>Renders the page-level confirmation dialog host element.</summary>
        /// <param name="html">The HTML helper used to render the host markup.</param>
        /// <returns>The confirmation dialog host markup.</returns>
        public static IHtmlContent FusionConfirmDialog(this IHtmlHelper html)
            => new HtmlString($"<div id='{FusionConfirm.ElementId}'></div>\n");
    }
}
