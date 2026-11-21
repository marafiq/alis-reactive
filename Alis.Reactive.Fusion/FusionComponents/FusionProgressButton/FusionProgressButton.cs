using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A progress button: EJ2's ProgressButton, a button that shows how far the work it started has gone.
/// </summary>
/// <remarks>
/// A handler of <see cref="Progress"/> reads the percent reached and writes <see cref="ProgressEventArgs.Percent"/> or
/// <see cref="ProgressEventArgs.Step"/>, which the button moves on from.
/// </remarks>
[Mirrors("@syncfusion/ej2-splitbuttons", "ProgressButton")]
public sealed class FusionProgressButton : FusionComponent
{
    /// <summary>Gets the button's text.</summary>
    public Property<string> Content => Property<string>();

    /// <summary>Gets the CSS classes of the button's element, separated by spaces.</summary>
    public Property<string> CssClass => Property<string>();

    /// <summary>Gets whether the button is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Gets whether the button fills its background as the progress moves.</summary>
    public Property<bool> EnableProgress => Property<bool>();

    /// <summary>Applies the changes written to the button's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Starts the button's progress.</summary>
    public void Start() => Method();

    /// <summary>Starts the button's progress at a percent.</summary>
    /// <param name="percent">The percent the progress starts at.</param>
    public void Start(Arg<double> percent) => Method([percent]);

    /// <summary>Stops the button's progress.</summary>
    public void Stop() => Method();

    /// <summary>Completes the button's progress.</summary>
    public void ProgressComplete() => Method();

    /// <summary>Moves the keyboard's focus to the button.</summary>
    public void FocusIn() => Method();

    /// <summary>Gets the event raised when the progress starts.</summary>
    public Event<ProgressEventArgs> Begin => Event<ProgressEventArgs>();

    /// <summary>Gets the event raised at each interval of the progress.</summary>
    public Event<ProgressEventArgs> Progress => Event<ProgressEventArgs>();

    /// <summary>Gets the event raised when the progress completes.</summary>
    public Event<ProgressEventArgs> End => Event<ProgressEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Begin"/>, <see cref="Progress"/> and <see cref="End"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-splitbuttons", "ProgressEventArgs")]
    public sealed class ProgressEventArgs : JsObject
    {
        /// <summary>Gets the percent the progress has reached; a write moves the progress to it.</summary>
        public Property<double> Percent => Property<double>();

        /// <summary>Gets how long the progress has run, in milliseconds.</summary>
        public ReadOnly<double> CurrentDuration => ReadOnly<double>();

        /// <summary>Gets the percent the progress moves at each interval; a write sets the next ones.</summary>
        public Property<double> Step => Property<double>();
    }
}
