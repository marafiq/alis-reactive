using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A one-time code input: EJ2's OtpInput, a row of boxes that each hold one character of a code a person types.
/// </summary>
/// <remarks>
/// An input bound to a member is an input of the form: a form sends its <see cref="Value"/> and a check reads it. EJ2
/// raises <see cref="Input"/> at each character and <see cref="ValueChanged"/> when the input loses focus after its
/// code changed, which commits the code.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "OtpInput")]
public sealed class FusionOtpInput : FusionComponent, IInputComponent
{
    /// <summary>Gets the code the input holds, as text.</summary>
    public Property<string> Value => Property<string>();

    /// <summary>Applies the changes written to the input's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Moves the keyboard's focus to the input.</summary>
    public void FocusIn() => Method();

    /// <summary>Takes the keyboard's focus away from the input.</summary>
    public void FocusOut() => Method();

    /// <summary>Gets the event raised each time a box of the input changes.</summary>
    public Event<OtpInputEventArgs> Input => Event<OtpInputEventArgs>();

    /// <summary>Gets the event raised when the input loses focus after its code changed.</summary>
    public Event<OtpChangedEventArgs> ValueChanged => Event<OtpChangedEventArgs>();

    /// <summary>Gets the event raised when a box of the input gets focus.</summary>
    public Event<OtpFocusEventArgs> Focus => Event<OtpFocusEventArgs>();

    /// <summary>Gets the event raised when a box of the input loses focus.</summary>
    public Event<OtpFocusEventArgs> Blur => Event<OtpFocusEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: ValueChanged);

    /// <summary>
    /// What a listener of <see cref="Input"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "OtpInputEventArgs")]
    public sealed class OtpInputEventArgs : JsObject
    {
        /// <summary>Gets the code the input holds after the change, as text.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Gets the code the input held before the change, as text.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>Gets the position of the box that has focus, from 0.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();
    }

    /// <summary>
    /// What a listener of <see cref="ValueChanged"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "OtpChangedEventArgs")]
    public sealed class OtpChangedEventArgs : JsObject
    {
        /// <summary>Gets the code the input holds after the change, as text.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Gets the code the input held before the change, as text.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="Focus"/> and <see cref="Blur"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "OtpFocusEventArgs")]
    public sealed class OtpFocusEventArgs : JsObject
    {
        /// <summary>Gets the code the input holds, as text.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Gets the position of the box that has focus, from 0.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();

        /// <summary>Gets whether a person moved the focus.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
}
