namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ShiftScheduleViewModel
    {
    }

    public class ScheduledShift
    {
        public int ShiftId { get; set; }

        public string Day { get; set; } = "";

        public string Shift { get; set; } = "";

        public string Nurse { get; set; } = "";

        public string Aide { get; set; } = "";
    }
}
