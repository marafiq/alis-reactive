namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownButton
{
    public sealed class FusionDropDownButtonModel
    {
        public string? Wing { get; set; }
    }

    public sealed record ShiftReport(string Id, string Name, bool AfterMidnightCount = false);

    public static class ShiftReports
    {
        public const string IncidentSummary = "incidents";

        public static readonly IReadOnlyList<ShiftReport> All = new[]
        {
            new ShiftReport("handoff", "Shift handoff"),
            new ShiftReport("medications", "Medication pass"),
            new ShiftReport(IncidentSummary, "Incident summary"),
            new ShiftReport("census", "Midnight census", AfterMidnightCount: true)
        };
    }

    public sealed class ShiftReportRequest
    {
        public string Wing { get; set; } = string.Empty;

        public string Report { get; set; } = string.Empty;
    }

    public sealed class ShiftReportAnswer
    {
        public bool Printed { get; set; }

        public string Message { get; set; } = string.Empty;

        public string[] PrintedReportIds { get; set; } = [];

        public string[] EmptyReportNames { get; set; } = [];
    }
}
