namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.EventRendered.
    /// Fires before each event/appointment renders on the UI.
    /// Use to apply custom styling (e.g., unassigned shifts in red).
    /// Set cancel to true to prevent the event from rendering.
    /// </summary>
    public class FusionScheduleEventRenderedArgs
    {
        /// <summary>Set to true to prevent this event from rendering.</summary>
        public bool Cancel { get; set; }

        /// <summary>The event data being rendered. Use in conditions for conditional styling.</summary>
        public FusionScheduleEventData Data { get; set; } = new FusionScheduleEventData();

        /// <summary>The render type: "event" for normal events.</summary>
        public string Type { get; set; } = "";

        public FusionScheduleEventRenderedArgs() { }
    }

    /// <summary>
    /// Event data nested inside eventRendered args. Contains the schedule event's
    /// domain fields for use in conditional styling (e.g., red for unassigned).
    /// </summary>
    public class FusionScheduleEventData
    {
        public int Id { get; set; }
        public string Subject { get; set; } = "";
        public int ShiftId { get; set; }
        public bool IsUnassigned { get; set; }
        public string? CategoryColor { get; set; }
        public string? StaffName { get; set; }
        public string? StaffRole { get; set; }

        public FusionScheduleEventData() { }
    }
}
