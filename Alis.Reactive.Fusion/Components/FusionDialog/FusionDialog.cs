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
    /// The Syncfusion Dialog, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionDialog(plan, "discharge-dialog", b =&gt; ...)</c>, then reach it in a callback
    /// and use its members directly:
    /// <code>
    /// var dialog = p.Component&lt;FusionDialog&gt;("discharge-dialog");
    /// dialog.Show();
    /// </code>
    /// </remarks>
    public sealed class FusionDialog : FusionComponent
    {
        /// <summary>Opens the dialog.</summary>
        public void Show() => Call();

        /// <summary>Closes the dialog.</summary>
        public void Hide() => Call();

        /// <summary>Recomputes the dialog position after its content changes size.</summary>
        public void RefreshPosition() => Call();

        /// <summary>Raised before the dialog opens.</summary>
        public TypedEvent<FusionDialogBeforeOpenArgs> BeforeOpen =>
            Event<FusionDialogBeforeOpenArgs>();

        /// <summary>Raised before the dialog closes.</summary>
        public TypedEvent<FusionDialogBeforeCloseArgs> BeforeClose =>
            Event<FusionDialogBeforeCloseArgs>();

        /// <summary>Raised after the dialog opens.</summary>
        public TypedEvent<FusionDialogOpenArgs> Open =>
            Event<FusionDialogOpenArgs>();

        /// <summary>Raised after the dialog closes.</summary>
        public TypedEvent<FusionDialogCloseArgs> Close =>
            Event<FusionDialogCloseArgs>();

        /// <summary>Raised when the user clicks the modal overlay.</summary>
        public TypedEvent<FusionDialogOverlayClickArgs> OverlayClick =>
            Event<FusionDialogOverlayClickArgs>();
    }

    /// <summary>Payload of <see cref="FusionDialog.BeforeOpen"/>.</summary>
    public class FusionDialogBeforeOpenArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionDialog.BeforeClose"/>.</summary>
    public class FusionDialogBeforeCloseArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>Whether user interaction started the close.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionDialog.Open"/>; carries no data.</summary>
    public class FusionDialogOpenArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionDialog.Close"/>; carries no data.</summary>
    public class FusionDialogCloseArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionDialog.OverlayClick"/>; carries no data.</summary>
    public class FusionDialogOverlayClickArgs : JsObject
    {
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionDialogHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Dialog and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving dialog event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDialog, TModel> FusionDialog<TModel>(
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

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionDialog>(elementId, builder.Render());
        }
    }
}
