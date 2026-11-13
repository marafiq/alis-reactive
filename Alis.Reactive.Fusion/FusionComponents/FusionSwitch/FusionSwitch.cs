using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A switch: EJ2's Switch, which a person turns on or off.
/// </summary>
/// <remarks>
/// A switch bound to a member is an input: a form sends its <see cref="Checked"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person flips the switch.
/// </remarks>
[Mirrors("@syncfusion/ej2-buttons", "Switch")]
public sealed class FusionSwitch : FusionComponent, IInputComponent
{
    /// <summary>Gets whether the switch is on.</summary>
    public Property<bool> Checked => Property<bool>();

    /// <summary>Gets the event raised when a person flips the switch.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Checked, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-buttons", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets whether the switch is on after the flip.</summary>
        public ReadOnly<bool> Checked => ReadOnly<bool>(optional: true);
    }
}
