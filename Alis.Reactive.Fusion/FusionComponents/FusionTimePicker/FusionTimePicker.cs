using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A time picker: EJ2's TimePicker, which holds the time of day a person chose.
/// </summary>
/// <remarks>
/// A picker bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. A handler of
/// <see cref="Change"/> reads the committed time.
/// </remarks>
/// <typeparam name="TValue">The type of the member the picker binds, a date that admits null, since the picker holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-calendars", "TimePicker")]
public sealed class FusionTimePicker<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the time the picker holds, null until a person chooses one.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Moves the keyboard's focus to the picker.</summary>
    public void FocusIn() => Method();

    /// <summary>Takes the keyboard's focus away from the picker.</summary>
    public void FocusOut() => Method();

    /// <summary>Gets the event raised when the picker's time changes.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-calendars", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the time the picker holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }
}
