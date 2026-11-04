namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.ShiftReport
{
    public sealed class ShiftAlertReviewModel
    {
    }

    public sealed class ShiftAlertsResponse
    {
        public string Shift { get; set; } = "";

        public ShiftAlert[] Alerts { get; set; } = Array.Empty<ShiftAlert>();
    }

    public sealed class ShiftAlertsLoaded
    {
        public string Shift { get; set; } = "";

        public ShiftAlert[] Alerts { get; set; } = Array.Empty<ShiftAlert>();
    }

    public sealed class ShiftAlert
    {
        public string Resident { get; set; } = "";

        public string Severity { get; set; } = "";

        public int Priority { get; set; }

        public bool Acknowledged { get; set; }
    }

    public static class AlertSeverity
    {
        public const string Critical = "critical";
        public const string Urgent = "urgent";
        public const string Stable = "stable";
    }

    public static class ShiftAlertEvents
    {
        public static readonly AppEvent<ShiftAlertsLoaded> Loaded = AppEvent.Declare<ShiftAlertsLoaded>();
    }
}
