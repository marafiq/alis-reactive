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
    /// Render it with <c>Html.FusionDialog(plan, "discharge-dialog", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionDialog&gt;("discharge-dialog")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Show)</c>.
    /// </remarks>
    public sealed class FusionDialog : FusionComponent
    {
        /// <summary>Opens the dialog.</summary>
        public ComponentMethod Show => Method("show");

        /// <summary>Closes the dialog.</summary>
        public ComponentMethod Hide => Method("hide");

        /// <summary>Recomputes the dialog position after its content changes size.</summary>
        public ComponentMethod RefreshPosition => Method("refreshPosition");

        /// <summary>Raised before the dialog opens.</summary>
        public TypedEvent<FusionDialogBeforeOpenArgs> BeforeOpen =>
            Event<FusionDialogBeforeOpenArgs>("beforeOpen");

        /// <summary>Raised before the dialog closes.</summary>
        public TypedEvent<FusionDialogBeforeCloseArgs> BeforeClose =>
            Event<FusionDialogBeforeCloseArgs>("beforeClose");

        /// <summary>Raised after the dialog opens.</summary>
        public TypedEvent<FusionDialogOpenArgs> Open =>
            Event<FusionDialogOpenArgs>("open");

        /// <summary>Raised after the dialog closes.</summary>
        public TypedEvent<FusionDialogCloseArgs> Close =>
            Event<FusionDialogCloseArgs>("close");

        /// <summary>Raised when the user clicks the modal overlay.</summary>
        public TypedEvent<FusionDialogOverlayClickArgs> OverlayClick =>
            Event<FusionDialogOverlayClickArgs>("overlayClick");
    }

    /// <summary>Payload of <see cref="FusionDialog.BeforeOpen"/>.</summary>
    public class FusionDialogBeforeOpenArgs
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <see cref="FusionDialog.BeforeClose"/>.</summary>
    public class FusionDialogBeforeCloseArgs
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>Whether user interaction started the close.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionDialog.Open"/>; carries no data.</summary>
    public class FusionDialogOpenArgs
    {
    }

    /// <summary>Payload of <see cref="FusionDialog.Close"/>; carries no data.</summary>
    public class FusionDialogCloseArgs
    {
    }

    /// <summary>Payload of <see cref="FusionDialog.OverlayClick"/>; carries no data.</summary>
    public class FusionDialogOverlayClickArgs
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
