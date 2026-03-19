namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ComponentGatherModel
    {
        public string? ResidentName { get; set; }          // NativeTextBox (existing)
        public string? CareNotes { get; set; }             // NativeTextArea
        public DateTime? MedicationTime { get; set; }       // FusionDateTimePicker
        public DateTime? StayStart { get; set; }            // FusionDateRangePicker (startDate)
        public string? PhoneNumber { get; set; }            // FusionInputMask
        public string? CarePlan { get; set; }               // FusionRichTextEditor
        public bool ReceiveNotifications { get; set; }      // FusionSwitch
    }
}
