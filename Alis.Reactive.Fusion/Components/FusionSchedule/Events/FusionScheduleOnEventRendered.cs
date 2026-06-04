namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before Syncfusion Schedule renders an appointment.
    /// Use <see cref="Data"/> for conditional styling. Set <see cref="Cancel"/>
    /// to prevent the appointment from rendering.
    /// </summary>
    public class FusionScheduleEventRenderedArgs
    {
        /// <summary>Set to true before the callback returns to prevent this appointment from rendering.</summary>
        public bool Cancel { get; set; }

        /// <summary>Schedule event data being rendered. Use in conditions for conditional styling.</summary>
        public FusionScheduleEventData Data { get; set; } = new FusionScheduleEventData();

        /// <summary>Syncfusion render type, for example <c>event</c>.</summary>
        public string Type { get; set; } = "";

        public FusionScheduleEventRenderedArgs() { }
    }

    /// <summary>
    /// Schedule event data projected from Syncfusion event records and <c>getEvents()</c>.
    /// </summary>
    public class FusionScheduleEventData
    {
        /// <summary>Schedule event identifier.</summary>
        public int Id { get; set; }

        /// <summary>Display title for the appointment.</summary>
        public string Subject { get; set; } = "";

        /// <summary>Resource or group identifier for the shift.</summary>
        public int ShiftId { get; set; }

        /// <summary>Whether the appointment currently has no assigned staff member.</summary>
        public bool IsUnassigned { get; set; }

        /// <summary>Optional color used by Syncfusion when rendering the appointment.</summary>
        public string? CategoryColor { get; set; }

        /// <summary>Assigned staff member name when available.</summary>
        public string? StaffName { get; set; }

        /// <summary>Assigned staff member role when available.</summary>
        public string? StaffRole { get; set; }

        public FusionScheduleEventData() { }
    }
}
