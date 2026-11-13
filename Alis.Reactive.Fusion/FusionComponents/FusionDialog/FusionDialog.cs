using Alis.Reactive.Declarations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A dialog: EJ2's Dialog, a panel shown over the page.
/// </summary>
/// <remarks>
/// A handler of <see cref="BeforeOpen"/> or <see cref="BeforeClose"/> writes true to its <c>Cancel</c> to keep the
/// dialog as it is. Its templates and buttons are set on EJ2's builder.
/// </remarks>
[Mirrors("@syncfusion/ej2-popups", "Dialog")]
public sealed class FusionDialog : FusionComponent
{
    /// <summary>Shows the dialog.</summary>
    public void Show() => Method();

    /// <summary>Hides the dialog.</summary>
    public void Hide() => Method();

    /// <summary>Places the dialog again where its position settings put it.</summary>
    public void RefreshPosition() => Method();

    /// <summary>Gets the event raised before the dialog opens.</summary>
    public Event<BeforeOpenEventArgs> BeforeOpen => Event<BeforeOpenEventArgs>();

    /// <summary>Gets the event raised before the dialog closes.</summary>
    public Event<BeforeCloseEventArgs> BeforeClose => Event<BeforeCloseEventArgs>();

    /// <summary>Gets the event raised once the dialog has opened.</summary>
    public Event Open => Event();

    /// <summary>Gets the event raised once the dialog has closed.</summary>
    public Event Close => Event();

    /// <summary>Gets the event raised when a person clicks the overlay behind the dialog.</summary>
    public Event OverlayClick => Event();

    /// <summary>
    /// What a listener of <see cref="BeforeOpen"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-popups", "BeforeOpenEventArgs")]
    public sealed class BeforeOpenEventArgs : JsObject
    {
        /// <summary>Gets whether the dialog stays closed; write true to keep it closed.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="BeforeClose"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-popups", "BeforeCloseEventArgs")]
    public sealed class BeforeCloseEventArgs : JsObject
    {
        /// <summary>Gets whether the dialog stays open; write true to keep it open.</summary>
        public Property<bool> Cancel => Property<bool>();

        /// <summary>Gets whether a person closed the dialog; a call of <see cref="Hide"/> reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
}
