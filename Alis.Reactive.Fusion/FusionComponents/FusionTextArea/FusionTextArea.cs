using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A text area: EJ2's TextArea, which holds a member's text over several lines.
/// </summary>
/// <remarks>
/// A text area bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Input"/> at each keystroke and <see cref="Change"/> when the area loses focus after its text changed,
/// which commits the text.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "TextArea")]
public sealed class FusionTextArea : FusionComponent, IInputComponent
{
    /// <summary>Gets the text the area holds, null until set.</summary>
    public Property<string?> Value => Property<string?>();

    /// <summary>Applies the changes written to the area's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Moves the keyboard's focus to the area.</summary>
    public void FocusIn() => Method();

    /// <summary>Moves the keyboard's focus away from the area.</summary>
    public void FocusOut() => Method();

    /// <summary>Gets the event raised each time the area's text changes.</summary>
    public Event<InputEventArgs> Input => Event<InputEventArgs>();

    /// <summary>Gets the event raised when the area loses focus after its text changed.</summary>
    public Event<ChangedEventArgs> Change => Event<ChangedEventArgs>();

    /// <summary>Gets the event raised when the area gets focus.</summary>
    public Event<FocusInEventArgs> Focus => Event<FocusInEventArgs>();

    /// <summary>Gets the event raised when the area loses focus.</summary>
    public Event<FocusOutEventArgs> Blur => Event<FocusOutEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Input"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "InputEventArgs")]
    public sealed class InputEventArgs : JsObject
    {
        /// <summary>Gets the text the area holds after the keystroke.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);

        /// <summary>Gets the text the area held before the keystroke.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "ChangedEventArgs")]
    public sealed class ChangedEventArgs : JsObject
    {
        /// <summary>Gets the text the area holds after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);

        /// <summary>Gets the text the area held before the change.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Focus"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "FocusInEventArgs")]
    public sealed class FocusInEventArgs : JsObject
    {
        /// <summary>Gets the text the area holds.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Blur"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "FocusOutEventArgs")]
    public sealed class FocusOutEventArgs : JsObject
    {
        /// <summary>Gets the text the area holds.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);
    }
}
