using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A text area: an HTML <c>textarea</c> that holds a member's text.
/// </summary>
/// <remarks>
/// A text area bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it.
/// </remarks>
[Mirrors("lib.dom", "HTMLTextAreaElement")]
public sealed class NativeTextArea : NativeComponent, IInputComponent
{
    /// <summary>Gets the text the area holds, with its line breaks.</summary>
    public Property<string> Value => Property<string>();

    /// <summary>Gets the event raised when a person commits the area's text.</summary>
    public Event<Changed> Change => Event<Changed>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>Moves the keyboard's focus to the area.</summary>
    public void Focus() => Method();

    /// <summary>
    /// What a listener of <see cref="Change"/> receives: the browser's event, which holds no value.
    /// </summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class Changed : JsObject
    {
        /// <summary>Gets the event's name, <c>change</c>.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Gets whether the browser raised the event; one a script raised reads false.</summary>
        public ReadOnly<bool> IsTrusted => ReadOnly<bool>();

        /// <summary>Gets the time the event was raised, in milliseconds since the page's time origin.</summary>
        public ReadOnly<double> TimeStamp => ReadOnly<double>();
    }
}
