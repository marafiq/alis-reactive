using Alis.Reactive.Declarations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A radio button: EJ2's RadioButton, one choice of a group whose buttons share a name.
/// </summary>
/// <remarks>EJ2 raises <see cref="Change"/> when a person chooses the button.</remarks>
[Mirrors("@syncfusion/ej2-buttons", "RadioButton")]
public sealed class FusionRadioButton : FusionComponent
{
    /// <summary>Gets whether the button is chosen.</summary>
    public Property<bool> Checked => Property<bool>();

    /// <summary>Gets whether the button is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Applies the changes written to the button's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Reads the value of the chosen button of the button's group.</summary>
    /// <returns>The chosen button's value.</returns>
    public MethodResult<string> GetSelectedValue() => Method<string>();

    /// <summary>Clicks the button, as a person does.</summary>
    public void Click() => Method();

    /// <summary>Moves the keyboard's focus to the button.</summary>
    public void FocusIn() => Method();

    /// <summary>Gets the event raised when a person chooses the button.</summary>
    public Event<ChangeArgs> Change => Event<ChangeArgs>();

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-buttons", "ChangeArgs")]
    public sealed class ChangeArgs : JsObject
    {
        /// <summary>Gets the value of the button chosen.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);
    }
}
