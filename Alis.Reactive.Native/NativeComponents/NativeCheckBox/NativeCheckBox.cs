using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A check box: an HTML <c>input</c> that holds a member of bool.
/// </summary>
/// <remarks>
/// A check box bound to a member is an input: a form sends its <see cref="Checked"/> and a check reads it. A
/// handler of <see cref="Change"/> reads the box's state after the click from <see cref="Checked"/>.
/// </remarks>
[Mirrors("lib.dom", "HTMLInputElement")]
public sealed class NativeCheckBox : NativeComponent, IInputComponent
{
    /// <summary>Gets whether the box is checked.</summary>
    public Property<bool> Checked => Property<bool>();

    /// <summary>Gets the event raised when a person checks or unchecks the box.</summary>
    public Event<Changed> Change => Event<Changed>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Checked, committedBy: Change);

    /// <summary>Moves the keyboard's focus to the box.</summary>
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
