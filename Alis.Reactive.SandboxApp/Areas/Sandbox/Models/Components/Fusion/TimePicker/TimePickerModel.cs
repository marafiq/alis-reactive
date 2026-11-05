namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
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
