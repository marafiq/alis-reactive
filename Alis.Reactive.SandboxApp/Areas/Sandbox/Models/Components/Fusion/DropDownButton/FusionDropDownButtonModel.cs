namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownButton
{
    /// <summary>
    /// A charge nurse at the end of the evening shift, printing the reports the night shift needs for one wing.
    /// </summary>
    public sealed class FusionDropDownButtonModel
    {
        /// <summary>The wing whose reports are printed.</summary>
        public string? Wing { get; set; }
    }

    /// <summary>A report the print menu offers.</summary>
    /// <param name="Id">The menu item's ID.</param>
    /// <param name="Name">The menu item's text.</param>
    /// <param name="AfterMidnightCount">Whether the report waits for the midnight head count.</param>
    public sealed record ShiftReport(string Id, string Name, bool AfterMidnightCount = false);

    /// <summary>The reports the print menu offers, in menu order.</summary>
    public static class ShiftReports
    {
        /// <summary>The incident summary, which is empty when no incident was logged.</summary>
        public const string IncidentSummary = "incidents";

        /// <summary>Every report, in menu order.</summary>
        public static readonly IReadOnlyList<ShiftReport> All = new[]
        {
            new ShiftReport("handoff", "Shift handoff"),
            new ShiftReport("medications", "Medication pass"),
            new ShiftReport(IncidentSummary, "Incident summary"),
            new ShiftReport("census", "Midnight census", AfterMidnightCount: true)
        };
    }

    /// <summary>A report to print for a wing.</summary>
    public sealed class ShiftReportRequest
    {
        /// <summary>The wing the report covers.</summary>
        public string Wing { get; set; } = string.Empty;

        /// <summary>The report's menu item ID.</summary>
        public string Report { get; set; } = string.Empty;
    }

    /// <summary>What the printer did with the report.</summary>
    public sealed class ShiftReportAnswer
    {
        /// <summary>Whether the report went to the printer.</summary>
        public bool Printed { get; set; }

        /// <summary>Where the report went, or why it did not print.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>The menu item IDs of the reports printed for this shift.</summary>
        public string[] PrintedReportIds { get; set; } = [];

        /// <summary>The menu texts of the reports with nothing in them this shift.</summary>
        public string[] EmptyReportNames { get; set; } = [];
    }
}
