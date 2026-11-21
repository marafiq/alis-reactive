using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A check box: EJ2's CheckBox, which holds whether a member of <see cref="bool"/> is true.
/// </summary>
/// <remarks>
/// A check box bound to a member is an input: a form sends its <see cref="Checked"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person checks or clears it.
/// </remarks>
[Mirrors("@syncfusion/ej2-buttons", "CheckBox")]
public sealed class FusionCheckBox : FusionComponent, IInputComponent
{
    /// <summary>Gets whether the box is checked.</summary>
    public Property<bool> Checked => Property<bool>();

    /// <summary>Gets whether the box shows neither checked nor cleared.</summary>
    public Property<bool> Indeterminate => Property<bool>();

    /// <summary>Gets whether the box is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Applies the changes written to the box's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Clicks the box, as a person does.</summary>
    public void Click() => Method();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Gets the event raised when a person checks or clears the box.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Checked, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-buttons", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets whether the box is checked after the change.</summary>
        public ReadOnly<bool> Checked => ReadOnly<bool>(optional: true);
    }
}
