using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A sidebar: EJ2's Sidebar, a panel beside the page's content that opens and closes.
/// </summary>
/// <remarks>
/// EJ2 raises <see cref="Open"/> and <see cref="Close"/> as the sidebar opens and closes, and a handler writes true to
/// the payload's <c>Cancel</c> to keep the sidebar as it is.
/// </remarks>
[Mirrors("@syncfusion/ej2-navigations", "Sidebar")]
public sealed class FusionSidebar : FusionComponent
{
    /// <summary>Gets whether the sidebar is open.</summary>
    public Property<bool> IsOpen => Property<bool>();

    /// <summary>Opens the sidebar, when it is closed.</summary>
    public void Show() => Method();

    /// <summary>Closes the sidebar, when it is open.</summary>
    public void Hide() => Method();

    /// <summary>Opens the sidebar when it is closed, and closes it when it is open.</summary>
    public void Toggle() => Method();

    /// <summary>Gets the event raised when the sidebar opens.</summary>
    public Event<EventArgs> Open => Event<EventArgs>();

    /// <summary>Gets the event raised when the sidebar closes.</summary>
    public Event<EventArgs> Close => Event<EventArgs>();

    /// <summary>
    /// What a listener of <see cref="Open"/> or <see cref="Close"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "EventArgs")]
    public sealed class EventArgs : JsObject
    {
        /// <summary>Gets whether a person opened or closed the sidebar, false when a method did.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);

        /// <summary>Gets whether the sidebar stays as it is; write true to keep it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }
}
