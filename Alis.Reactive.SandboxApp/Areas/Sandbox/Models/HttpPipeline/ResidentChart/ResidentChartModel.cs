namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Resident charts: the coordinator opens a resident's chart summary, and can flag the chart the
    /// server offers for the care team's review.
    /// </summary>
    public class ResidentChartPageModel
    {
        public string? ReviewChartMrn { get; set; }
    }

    /// <summary>
    /// A resident's chart summary. MRN, the medical record number, is an all-capitals member: the server
    /// writes it as "mrn", as ASP.NET Core writes every member name.
    /// </summary>
    public sealed record ResidentChart(string Name, string? MRN);

    /// <summary>A chart flagged for the care team's review, named by its medical record number.</summary>
    public sealed class ChartFlagged
    {
        public string? MRN { get; set; }
    }
}
