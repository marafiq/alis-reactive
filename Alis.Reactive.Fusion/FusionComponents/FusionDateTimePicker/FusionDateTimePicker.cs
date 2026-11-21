using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A date and time picker: EJ2's DateTimePicker, which holds the day and the time a person chose.
/// </summary>
/// <remarks>
/// A picker bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. Bound to a
/// <c>DateTime?</c> member it holds a date and a time; bound to a <c>DateTimeOffset?</c> member it holds an instant.
/// </remarks>
/// <typeparam name="TValue">The type of the member the picker binds, a date that admits null, since the picker holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-calendars", "DateTimePicker")]
public sealed class FusionDateTimePicker<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the day and time the picker holds, null until a person chooses one.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Moves the keyboard's focus to the picker.</summary>
    public void FocusIn() => Method();

    /// <summary>Takes the keyboard's focus away from the picker.</summary>
    public void FocusOut() => Method();

    /// <summary>Gets the event raised when the picker's day or time changes.</summary>
    public Event<ChangedEventArgs> Change => Event<ChangedEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-calendars", "ChangedEventArgs")]
    public sealed class ChangedEventArgs : JsObject
    {
        /// <summary>Gets the day and time the picker holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }
}
