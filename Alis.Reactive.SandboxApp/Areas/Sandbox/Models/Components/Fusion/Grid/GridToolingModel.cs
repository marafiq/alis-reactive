namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The shift schedule page: the grid holds the week's shifts, so the page has no fields of its own.</summary>
    public class ShiftScheduleViewModel
    {
    }

    /// <summary>One shift of next week's schedule, one row of the schedule grid.</summary>
    public class ScheduledShift
    {
        public int ShiftId { get; set; }

        /// <summary>The day, as the schedule prints it: <c>Mon, Oct 19</c>.</summary>
        public string Day { get; set; } = "";

        /// <summary><c>Day</c>, <c>Evening</c> or <c>Night</c>.</summary>
        public string Shift { get; set; } = "";

        /// <summary>The charge nurse, or <c>Open</c> while nobody has taken the shift.</summary>
        public string Nurse { get; set; } = "";

        /// <summary>The care aide, or <c>Open</c> while nobody has taken the shift.</summary>
        public string Aide { get; set; } = "";
    }
}
