using System;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a Syncfusion Schedule time cell is clicked.
    /// Includes the selected slot and resource group.
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
    }
}
