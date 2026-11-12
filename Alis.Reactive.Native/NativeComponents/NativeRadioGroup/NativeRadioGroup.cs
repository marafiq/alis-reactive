using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A radio group: radios that hold one member, its value the chosen radio's value.
/// </summary>
/// <remarks>
/// A radio group bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. A
/// write of <see cref="Value"/> checks the radio of that value and no other, so a value no radio holds checks
/// none, and a write raises no <see cref="Change"/>.
/// </remarks>
[Mirrors("lib.dom", "HTMLInputElement")]
public sealed class NativeRadioGroup : NativeComponent, IInputComponent
{
    /// <summary>Gets the chosen radio's value.</summary>
    public Property<string> Value => Property<string>();

    /// <summary>Gets the event raised when a person picks a radio.</summary>
    public Event<Changed> Change => Event<Changed>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>Moves the keyboard's focus to the chosen radio, else to the first.</summary>
    public void Focus() => Method();

    /// <summary>
    /// What a listener of <see cref="Change"/> receives: an event the group's own code raises, which holds no value.
    /// </summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class Changed : JsObject
    {
        /// <summary>Gets the event's name, <c>change</c>.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Gets whether the browser raised the event; the group's own event is a script's and reads false.</summary>
        public ReadOnly<bool> IsTrusted => ReadOnly<bool>();

        /// <summary>Gets the time the event was raised, in milliseconds since the page's time origin.</summary>
        public ReadOnly<double> TimeStamp => ReadOnly<double>();
    }
}
