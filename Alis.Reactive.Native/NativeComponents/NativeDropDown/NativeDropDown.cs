using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A drop-down: an HTML <c>select</c> that holds a member's value.
/// </summary>
/// <remarks>
/// A drop-down bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it, as text
/// the endpoint binds to the member's type.
/// </remarks>
[Mirrors("lib.dom", "HTMLSelectElement")]
public sealed class NativeDropDown : NativeComponent, IInputComponent
{
    /// <summary>Gets the selected option's value, as text; it is empty when no option is selected.</summary>
    public Property<string> Value => Property<string>();

    /// <summary>Gets the event raised when a person selects an option.</summary>
    public Event<Changed> Change => Event<Changed>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>Moves the keyboard's focus to the drop-down.</summary>
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
