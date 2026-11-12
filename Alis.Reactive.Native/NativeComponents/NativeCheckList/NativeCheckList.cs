using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A check list: a list of check boxes that holds one member, its value the checked boxes' values.
/// </summary>
/// <remarks>
/// A check list bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. A write
/// of <see cref="Value"/> checks exactly the boxes whose values it holds, a write of nothing checks none, and a
/// write raises no <see cref="Change"/>.
/// </remarks>
public sealed class NativeCheckList : NativeComponent, IInputComponent
{
    /// <summary>Gets the checked boxes' values, in the boxes' order.</summary>
    public Property<string[]> Value => Property<string[]>();

    /// <summary>Gets the event raised when a person checks or unchecks a box of the list.</summary>
    public Event<Changed> Change => Event<Changed>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>Moves the keyboard's focus to the list's first box.</summary>
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
