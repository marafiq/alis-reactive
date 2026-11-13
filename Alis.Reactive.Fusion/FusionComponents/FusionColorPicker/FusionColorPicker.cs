using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A color picker: EJ2's ColorPicker, which holds a member's color as hex text.
/// </summary>
/// <remarks>
/// A color picker bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2
/// raises <see cref="Change"/> when a person picks a color, which commits it.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "ColorPicker")]
public sealed class FusionColorPicker : FusionComponent, IInputComponent
{
    /// <summary>Gets the color the picker holds, as hex text such as <c>#008000ff</c>.</summary>
    public Property<string> Value => Property<string>();

    /// <summary>Gets whether the picker is disabled, so its popup does not open.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Opens the picker's popup when it is closed, and closes it when it is open.</summary>
    public void Toggle() => Method();

    /// <summary>Gets the event raised when a person picks a color.</summary>
    public Event<ColorPickerEventArgs> Change => Event<ColorPickerEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "ColorPickerEventArgs")]
    public sealed class ColorPickerEventArgs : JsObject
    {
        /// <summary>Gets the color the picker holds after the change, as hex text.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);
    }
}
