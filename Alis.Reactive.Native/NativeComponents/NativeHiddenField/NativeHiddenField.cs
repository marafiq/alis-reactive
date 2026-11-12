using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A hidden field: a hidden HTML <c>input</c> that holds a member's value.
/// </summary>
/// <remarks>
/// A hidden field bound to a member is an input like any other: a form sends its <see cref="Value"/> and a check
/// reads it. It has no person to commit it, so its <see cref="Change"/> is raised when a write changes the value,
/// and not after a write of the same text. A hidden input takes no focus.
/// </remarks>
[Mirrors("lib.dom", "HTMLInputElement")]
public sealed class NativeHiddenField : NativeComponent, IInputComponent
{
    /// <summary>Gets the text the field holds.</summary>
    public Property<string> Value => Property<string>();

    /// <summary>Gets the event raised when a write changes the field's value.</summary>
    public Event<Changed> Change => Event<Changed>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives: an event the field's own code raises, which holds no value.
    /// </summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class Changed : JsObject
    {
        /// <summary>Gets the event's name, <c>change</c>.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Gets whether the browser raised the event; the field's own event is a script's and reads false.</summary>
        public ReadOnly<bool> IsTrusted => ReadOnly<bool>();

        /// <summary>Gets the time the event was raised, in milliseconds since the page's time origin.</summary>
        public ReadOnly<double> TimeStamp => ReadOnly<double>();
    }
}
