using System;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.CellClicked (SF "cellClick" event).
    /// Fires when a time cell is clicked. Contains the time slot and resource group.
    /// Verified: sf-schedule-test.html — startTime/endTime are ISO dates, groupIndex is int.
    /// </summary>
    public class FusionScheduleCellClickArgs
    {
        /// <summary>Start time of the clicked cell slot.</summary>
        public DateTime StartTime { get; set; }

        /// <summary>End time of the clicked cell slot.</summary>
        public DateTime EndTime { get; set; }

        /// <summary>Zero-based index of the resource group (e.g., which shift).</summary>
        public int GroupIndex { get; set; }

        /// <summary>True if the all-day row was clicked.</summary>
        public bool IsAllDay { get; set; }

        public FusionScheduleCellClickArgs() { }
    }
}
