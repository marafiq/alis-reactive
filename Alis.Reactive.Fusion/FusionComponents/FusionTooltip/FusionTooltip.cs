using Alis.Reactive.Declarations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A tooltip: EJ2's Tooltip, which shows a short text over the element it targets.
/// </summary>
/// <remarks>
/// EJ2 raises <see cref="BeforeRender"/> and <see cref="BeforeOpen"/> before the tooltip shows and
/// <see cref="AfterOpen"/> once it shows, then <see cref="BeforeClose"/> before it hides and <see cref="AfterClose"/>
/// once it hides. A listener of a before event writes true to <see cref="TooltipEventArgs.Cancel"/> to stop it.
/// </remarks>
[Mirrors("@syncfusion/ej2-popups", "Tooltip")]
public sealed class FusionTooltip : FusionComponent
{
    /// <summary>Shows the tooltip.</summary>
    public void Open() => Method();

    /// <summary>Hides the tooltip.</summary>
    public void Close() => Method();

    /// <summary>Renders the tooltip's content again and places it again over its target.</summary>
    public void Refresh() => Method();

    /// <summary>Gets the event raised before the tooltip and its content are added to the page.</summary>
    public Event<TooltipEventArgs> BeforeRender => Event<TooltipEventArgs>();

    /// <summary>Gets the event raised before the tooltip shows over its target.</summary>
    public Event<TooltipEventArgs> BeforeOpen => Event<TooltipEventArgs>();

    /// <summary>Gets the event raised after the tooltip shows.</summary>
    public Event<TooltipEventArgs> AfterOpen => Event<TooltipEventArgs>();

    /// <summary>Gets the event raised before the tooltip hides.</summary>
    public Event<TooltipEventArgs> BeforeClose => Event<TooltipEventArgs>();

    /// <summary>Gets the event raised after the tooltip hides.</summary>
    public Event<TooltipEventArgs> AfterClose => Event<TooltipEventArgs>();

    /// <summary>
    /// What a listener of the tooltip's events receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-popups", "TooltipEventArgs")]
    public sealed class TooltipEventArgs : JsObject
    {
        /// <summary>Gets whether the tooltip stops what it is about to do; write true in a before event to stop it.</summary>
        public Property<bool> Cancel => Property<bool>();

        /// <summary>Gets whether a person's gesture raised the event.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }
}
