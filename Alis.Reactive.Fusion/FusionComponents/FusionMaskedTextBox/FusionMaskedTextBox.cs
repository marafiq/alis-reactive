using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A masked text box: EJ2's MaskedTextBox, a text box that holds the characters its mask admits.
/// </summary>
/// <remarks>
/// A box bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. A handler of
/// <see cref="Change"/> reads the committed value.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "MaskedTextBox")]
public sealed class FusionMaskedTextBox : FusionComponent, IInputComponent
{
    /// <summary>Gets the characters the box holds without the mask's literals and prompt characters, null until a person types.</summary>
    public Property<string?> Value => Property<string?>();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Gets the event raised when the box's value changes.</summary>
    public Event<MaskChangeEventArgs> Change => Event<MaskChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "MaskChangeEventArgs")]
    public sealed class MaskChangeEventArgs : JsObject
    {
        /// <summary>Gets the characters the box holds after the change, without the mask's literals and prompt characters.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }
}
