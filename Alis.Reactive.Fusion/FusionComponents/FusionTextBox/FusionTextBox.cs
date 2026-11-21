using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A text box: EJ2's TextBox, which holds a member's text.
/// </summary>
/// <remarks>
/// A text box bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Input"/> at each keystroke and <see cref="Change"/> when the box loses focus after its text changed,
/// which commits the text.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "TextBox")]
public sealed class FusionTextBox : FusionComponent, IInputComponent
{
    /// <summary>Gets the text the box holds, null until set.</summary>
    public Property<string?> Value => Property<string?>();

    /// <summary>Applies the changes written to the box's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Moves the keyboard's focus away from the box.</summary>
    public void FocusOut() => Method();

    /// <summary>Adds icons to one side of the box.</summary>
    /// <param name="position">The side the icons go on.</param>
    /// <param name="icons">The CSS classes of the icons, one icon to each class name, separated by commas.</param>
    public void AddIcon(Arg<IconPosition> position, Arg<string> icons) => Method([position, icons]);

    /// <summary>Gets the event raised each time the box's text changes.</summary>
    public Event<InputEventArgs> Input => Event<InputEventArgs>();

    /// <summary>Gets the event raised when the box loses focus after its text changed.</summary>
    public Event<ChangedEventArgs> Change => Event<ChangedEventArgs>();

    /// <summary>Gets the event raised when the box gets focus.</summary>
    public Event<FocusInEventArgs> Focus => Event<FocusInEventArgs>();

    /// <summary>Gets the event raised when the box loses focus.</summary>
    public Event<FocusOutEventArgs> Blur => Event<FocusOutEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// The side of the box that <see cref="AddIcon"/> puts its icons on.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<IconPosition>))]
    public enum IconPosition
    {
        /// <summary>After the box's text.</summary>
        [JsonStringEnumMemberName("append")]
        Append,

        /// <summary>Before the box's text.</summary>
        [JsonStringEnumMemberName("prepend")]
        Prepend,
    }

    /// <summary>
    /// What a listener of <see cref="Input"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "InputEventArgs")]
    public sealed class InputEventArgs : JsObject
    {
        /// <summary>Gets the text the box holds after the keystroke.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);

        /// <summary>Gets the text the box held before the keystroke.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "ChangedEventArgs")]
    public sealed class ChangedEventArgs : JsObject
    {
        /// <summary>Gets the text the box holds after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);

        /// <summary>Gets the text the box held before the change.</summary>
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
        /// <summary>Gets the text the box holds.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Blur"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "FocusOutEventArgs")]
    public sealed class FocusOutEventArgs : JsObject
    {
        /// <summary>Gets the text the box holds.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);
    }
}
