using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>
    /// Vertical slice for FusionConfirm — SF Dialog used as a halting confirm.
    ///
    /// Three jsEmit patterns (Fusion convention: el.ej2_instances[0]):
    ///   Prop  → var c=el.ej2_instances[0]; c.content=val; c.dataBind()
    ///   Call  → el.ej2_instances[0].show()
    ///   Read  → ref:alisConfirmDialog.visible
    ///
    /// Infrastructure: Html.FusionConfirmDialog() renders the bare element.
    /// The TS runtime (confirm.ts) creates the SF Dialog instance and owns
    /// all behavior — show/hide, queuing, button wiring. No inline JS here.
    /// </summary>
    public static class FusionConfirmExtensions
    {
        // ── Prop: set dialog content message ──

        public static ComponentRef<FusionConfirm, TModel> SetContent<TModel>(
            this ComponentRef<FusionConfirm, TModel> self, string message)
            where TModel : class
        {
            return self.Emit(
                "var c=el.ej2_instances[0]; c.content=val; c.dataBind()",
                message);
        }

        // ── Call: show / hide the dialog ──

        public static ComponentRef<FusionConfirm, TModel> Show<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
        {
            return self.Emit("el.ej2_instances[0].show()");
        }

        public static ComponentRef<FusionConfirm, TModel> Hide<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
        {
            return self.Emit("el.ej2_instances[0].hide()");
        }

        // ── Read: dialog visibility state ──

        public static string IsVisible<TModel>(
            this ComponentRef<FusionConfirm, TModel> self)
            where TModel : class
        {
            return $"ref:{self.TargetId}.visible";
        }

        // ── Infrastructure: renders the confirm dialog element ──

        /// <summary>
        /// Renders the bare element for the SF Dialog confirm.
        /// Call once in _Layout.cshtml before alis-reactive.js.
        /// The TS runtime's confirm.ts module creates the SF Dialog instance.
        /// </summary>
        public static IHtmlContent FusionConfirmDialog(this IHtmlHelper html)
        {
            return new HtmlString($"<div id=\"{FusionConfirm.ElementId}\"></div>\n");
        }
    }
}
