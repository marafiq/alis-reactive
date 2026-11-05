namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class SwitchModel
    {
        public bool ReceiveCareAlerts { get; set; }

        public bool EmailReminders { get; set; }

        public bool TextMessageAlerts { get; set; }
    }

    public sealed class CareAlertPreferencesRequest
    {
        public bool ReceiveCareAlerts { get; set; }

        public bool EmailReminders { get; set; }

        public bool TextMessageAlerts { get; set; }
    }

    public sealed class CareAlertPreferencesResponse
    {
        public string Summary { get; set; } = string.Empty;
    }
}
