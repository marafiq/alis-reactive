using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A numeric text box: EJ2's NumericTextBox, which holds a number a person types or steps with its spin buttons.
/// </summary>
/// <remarks>
/// A box bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. A handler of
/// <see cref="Change"/> reads the committed number.
/// </remarks>
/// <typeparam name="TValue">The type of the member the box binds, a number that admits null, since the box holds null until set.</typeparam>
[Mirrors("@syncfusion/ej2-inputs", "NumericTextBox")]
public sealed class FusionNumericTextBox<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the number the box holds, null until set.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets the least number the box takes, null for none.</summary>
    public Property<TValue> Min => Property<TValue>();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Takes the keyboard's focus away from the box.</summary>
    public void FocusOut() => Method();

    /// <summary>Raises the box's number by one step.</summary>
    public void Increment() => Method();

    /// <summary>Lowers the box's number by one step.</summary>
    public void Decrement() => Method();

    /// <summary>
    /// Gets the event raised when the box's number changes: an edit when the box loses focus, a spin button, a scroll
    /// over the focused box or a write of <see cref="Value"/>.
    /// </summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    /// <summary>Gets the event raised when the box gets focus.</summary>
    public Event<NumericFocusEventArgs> Focus => Event<NumericFocusEventArgs>();

    /// <summary>Gets the event raised when the box loses focus.</summary>
    public Event<NumericBlurEventArgs> Blur => Event<NumericBlurEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the number the box holds after the change, null when a person cleared it.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>(optional: true);

        /// <summary>Gets the number the box held before the change, null when it was clear.</summary>
        public ReadOnly<TValue> PreviousValue => ReadOnly<TValue>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Focus"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "NumericFocusEventArgs")]
    public sealed class NumericFocusEventArgs : JsObject
    {
    }

    /// <summary>
    /// What a listener of <see cref="Blur"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "NumericBlurEventArgs")]
    public sealed class NumericBlurEventArgs : JsObject
    {
    }
}
