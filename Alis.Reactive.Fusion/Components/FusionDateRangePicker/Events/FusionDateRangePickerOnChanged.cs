using System;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionDateRangePicker"/> range changes.
    /// </summary>
    /// <remarks>
    /// Exposes start and end dates individually for conditions:
    /// <c>p.When(args, x =&gt; x.StartDate).NotNull()</c>.
    /// </remarks>
    public class FusionDateRangePickerChangeArgs
    {
        /// <summary>Start of the selected date range.</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>End of the selected date range.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Number of days in the selected range.</summary>
        public int DaySpan { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
