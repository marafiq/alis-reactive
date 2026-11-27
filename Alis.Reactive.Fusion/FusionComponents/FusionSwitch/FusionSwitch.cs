using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A switch: EJ2's Switch, which a person turns on or off.
/// </summary>
/// <remarks>
/// A switch bound to a member is an input: a request container sends its <see cref="Checked"/> and a check reads it.
/// The switch raises <see cref="Change"/> after an activation changes its state.
/// </remarks>
[Mirrors("@syncfusion/ej2-buttons", "Switch")]
public sealed class FusionSwitch : FusionComponent, IInputComponent
{
    /// <summary>Gets whether the switch is on.</summary>
    public Property<bool> Checked => Property<bool>();

    /// <summary>Gets the classes on the switch wrapper.</summary>
    public Property<string> CssClass => Property<string>();

    /// <summary>Gets whether the native control is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Gets whether the switch uses a right-to-left layout.</summary>
    public Property<bool> EnableRtl => Property<bool>();

    /// <summary>Gets the text shown while the switch is off.</summary>
    public Property<string> OffLabel => Property<string>();

    /// <summary>Gets the text shown while the switch is on.</summary>
    public Property<string> OnLabel => Property<string>();

    /// <summary>Gets the native form value submitted for this switch when it is checked.</summary>
    public Property<string> Value => Property<string>();

    /// <summary>Applies the pending property changes to the switch now.</summary>
    public void DataBind() => Method();

    /// <summary>Toggles the switch without a native event.</summary>
    public void Toggle() => Method();

    /// <summary>Activates the native input through its click method.</summary>
    public void Click() => Method();

    /// <summary>Moves focus to the switch's input.</summary>
    public void FocusIn() => Method();

    /// <summary>Removes the enhanced switch and leaves its original input unchecked.</summary>
    public void Destroy() => Method();

    /// <summary>Gets the event raised before an activation changes the switch.</summary>
    public Event<BeforeChangeEventArgs> BeforeChange => Event<BeforeChangeEventArgs>();

    /// <summary>Gets the event raised after an activation changes the switch.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Checked, committedBy: Change);

    /// <summary>What a listener of <see cref="BeforeChange"/> receives.</summary>
    [Mirrors("@syncfusion/ej2-buttons", "BeforeChangeEventArgs")]
    public sealed class BeforeChangeEventArgs : JsObject
    {
        /// <summary>Gets whether the activation is canceled. Write true before the handler returns to cancel it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets the prior checked state. The switch takes the inverse of this value when the activation proceeds.</summary>
        public Property<bool> Checked => Property<bool>(optional: true);
    }

    /// <summary>What a listener of <see cref="Change"/> receives.</summary>
    [Mirrors("@syncfusion/ej2-buttons", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets whether the switch is on after the flip.</summary>
        public ReadOnly<bool> Checked => ReadOnly<bool>(optional: true);

        /// <summary>Gets the original DOM event when there was one.</summary>
        public ActivationEvent OriginalEvent => LiveObject<ActivationEvent>(name: "event", optional: true, nullable: true);
    }

    /// <summary>The part of the original browser event that identifies how the switch was activated.</summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class ActivationEvent : JsObject
    {
        /// <summary>Gets whether the browser dispatched the event as a trusted activation.</summary>
        public ReadOnly<bool> IsTrusted => ReadOnly<bool>();
    }
}
