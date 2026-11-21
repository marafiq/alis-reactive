using System;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A date range picker: EJ2's DateRangePicker, which holds the first and the last day of the range a person chose.
/// </summary>
/// <remarks>
/// A picker bound to a member is an input: a form sends its <see cref="Value"/>, the range's two days, and a check
/// reads it. A handler of <see cref="Change"/> reads the committed range.
/// </remarks>
[Mirrors("@syncfusion/ej2-calendars", "DateRangePicker")]
public sealed class FusionDateRangePicker : FusionComponent, IInputComponent
{
    /// <summary>Gets the first day of the range, null until a person chooses a range.</summary>
    public Property<DateTime?> StartDate => Property<DateTime?>();

    /// <summary>Gets the last day of the range, null until a person chooses a range.</summary>
    public Property<DateTime?> EndDate => Property<DateTime?>();

    /// <summary>Gets the range's first and last day, null until a person chooses a range.</summary>
    public Property<DateTime[]?> Value => Property<DateTime[]?>();

    /// <summary>Moves the keyboard's focus to the picker.</summary>
    public void FocusIn() => Method();

    /// <summary>Takes the keyboard's focus away from the picker.</summary>
    public void FocusOut() => Method();

    /// <summary>Gets the event raised when the picker's range changes.</summary>
    public Event<RangeEventArgs> Change => Event<RangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-calendars", "RangeEventArgs")]
    public sealed class RangeEventArgs : JsObject
    {
        /// <summary>Gets the first day of the range after the change.</summary>
        public ReadOnly<DateTime?> StartDate => ReadOnly<DateTime?>(optional: true);

        /// <summary>Gets the last day of the range after the change.</summary>
        public ReadOnly<DateTime?> EndDate => ReadOnly<DateTime?>(optional: true);

        /// <summary>Gets the number of days the range spans.</summary>
        public ReadOnly<int> DaySpan => ReadOnly<int>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }
}
