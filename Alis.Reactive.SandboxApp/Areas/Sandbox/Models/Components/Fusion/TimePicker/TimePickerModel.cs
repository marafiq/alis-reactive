namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    // Journey: a care coordinator sets a resident's morning medication time.
    // The page carries over the time currently on the medication record so the
    // coordinator can review it, choose a new time, apply the community's standard
    // morning round, move focus in and out of the field, then confirm and schedule.
    public class TimePickerModel
    {
        public DateTime? MedicationTime { get; set; }
    }

    public sealed class MedicationScheduleRequest
    {
        public DateTime? MedicationTime { get; set; }
    }

    public sealed class MedicationScheduleResponse
    {
        public DateTime? MedicationTime { get; set; }

        public string Confirmation { get; set; } = string.Empty;
    }
}
