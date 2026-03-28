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
        /// <summary>Gets or sets the start of the selected date range.</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Gets or sets the end of the selected date range.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Gets or sets the number of days in the selected range.</summary>
        public int DaySpan { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionDateRangePickerChangeArgs() { }
    }
}
