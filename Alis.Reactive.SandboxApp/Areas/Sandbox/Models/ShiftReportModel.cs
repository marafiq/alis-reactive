namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>Model for the ShiftReport sandbox: operating on a custom event payload's array.</summary>
    public sealed class ShiftReportModel
    {
    }

    /// <summary>
    /// A developer-defined custom event payload that carries an array of objects. The
    /// <c>shift-report</c> custom event delivers this; the array DSL operates on
    /// <see cref="Alerts"/> by element members in the handler.
    /// </summary>
    public sealed class ShiftReportPayload
    {
        public ResidentAlert[] Alerts { get; set; } = System.Array.Empty<ResidentAlert>();
    }

    /// <summary>HTTP response that seeds the custom event payload's array.</summary>
    public sealed class AlertsResponse
    {
        public ResidentAlert[] Alerts { get; set; } = System.Array.Empty<ResidentAlert>();
    }

    /// <summary>An alert element with members the per-element predicates/selectors read.</summary>
    public sealed class ResidentAlert
    {
        public string Resident { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool Acknowledged { get; set; }
    }
}
