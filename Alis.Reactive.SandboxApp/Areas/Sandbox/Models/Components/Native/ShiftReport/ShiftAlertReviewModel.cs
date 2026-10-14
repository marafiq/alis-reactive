namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.ShiftReport
{
    /// <summary>The charge nurse's review of a shift's alerts before she signs the shift report. No inputs.</summary>
    public sealed class ShiftAlertReviewModel
    {
    }

    /// <summary>A shift's alerts as the alert log holds them.</summary>
    public sealed class ShiftAlertsResponse
    {
        /// <summary>The shift's name, as the review heads it.</summary>
        public string Shift { get; set; } = "";

        public ShiftAlert[] Alerts { get; set; } = Array.Empty<ShiftAlert>();
    }

    /// <summary>
    /// What the page announces when a shift's alerts arrive; the review reads the alerts off it by their
    /// members.
    /// </summary>
    public sealed class ShiftAlertsLoaded
    {
        public string Shift { get; set; } = "";

        public ShiftAlert[] Alerts { get; set; } = Array.Empty<ShiftAlert>();
    }

    /// <summary>One alert raised for a resident during the shift.</summary>
    public sealed class ShiftAlert
    {
        public string Resident { get; set; } = "";

        /// <summary>One of <see cref="AlertSeverity"/>.</summary>
        public string Severity { get; set; } = "";

        /// <summary>How urgent the alert is, 1 to 10.</summary>
        public int Priority { get; set; }

        /// <summary>Whether a nurse has acknowledged the alert.</summary>
        public bool Acknowledged { get; set; }
    }

    /// <summary>How severe an alert is.</summary>
    public static class AlertSeverity
    {
        public const string Critical = "critical";
        public const string Urgent = "urgent";
        public const string Stable = "stable";
    }

    /// <summary>The document events the shift alert review raises and hears.</summary>
    public static class ShiftAlertEvents
    {
        public static readonly AppEvent<ShiftAlertsLoaded> Loaded = AppEvent.Named<ShiftAlertsLoaded>("shift-alerts-loaded");
    }
}
