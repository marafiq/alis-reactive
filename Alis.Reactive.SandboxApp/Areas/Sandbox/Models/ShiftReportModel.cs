namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>ShiftReport page model for custom-event payload array operations.</summary>
    public sealed class ShiftReportModel
    {
    }

    /// <summary>
    /// Custom event payload for <c>shift-report</c>; the array DSL reads
    /// <see cref="Alerts"/> by element members in the handler.
    /// </summary>
    public sealed class ShiftReportPayload
    {
        public ResidentAlert[] Alerts { get; set; } = System.Array.Empty<ResidentAlert>();
    }

    /// <summary>HTTP response used to seed the custom event payload array.</summary>
    public sealed class AlertsResponse
    {
        public ResidentAlert[] Alerts { get; set; } = System.Array.Empty<ResidentAlert>();
    }

    /// <summary>Alert array element read by per-element predicates and selectors.</summary>
    public sealed class ResidentAlert
    {
        public string Resident { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool Acknowledged { get; set; }
    }
}
