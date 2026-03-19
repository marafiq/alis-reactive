using System;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionDateRangePicker.Changed (SF "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.StartDate).NotNull()
    /// ExpressionPathHelper resolves x => x.StartDate to "evt.startDate".
    /// CoercionTypes infers DateTime → "date" for runtime coercion.
    ///
    /// UNIQUE: exposes StartDate AND EndDate as separate DateTime? properties,
    /// plus DaySpan (computed duration) and IsInteracted.
    /// </summary>
    public class FusionDateRangePickerChangeArgs
    {
        /// <summary>Start of the selected date range.</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>End of the selected date range.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Number of days in the selected range.</summary>
        public int DaySpan { get; set; }

        /// <summary>True if the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionDateRangePickerChangeArgs() { }
    }
}
