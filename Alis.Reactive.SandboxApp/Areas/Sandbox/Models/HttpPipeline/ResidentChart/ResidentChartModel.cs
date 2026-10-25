namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Resident charts: the coordinator opens a resident's chart summary, and can flag the chart the
    /// server offers for the care team's review.
    /// </summary>
    public class ResidentChartPageModel
    {
        public string? ReviewChartMrn { get; set; }

        /// <summary>The resident whose chart the coordinator opens by its medical record number.</summary>
        public string? ChartByRecordNumber { get; set; }
    }

    /// <summary>
    /// A chart the coordinator can open by its medical record number: MRN is named in capitals, and the
    /// list shows it by the name the row carries ("mrn").
    /// </summary>
    public sealed record ChartLink(string MRN, string Name, string Resident);

    /// <summary>The query of <c>GET ResidentChart/Chart/{resident}</c>: the resident whose chart opens.</summary>
    public sealed class ChartQuery
    {
        public string? Resident { get; set; }
    }

    /// <summary>
    /// A resident's chart summary. MRN, the medical record number, is an all-capitals member: the server
    /// writes it as "mrn", as ASP.NET Core writes every member name.
    /// </summary>
    public sealed record ResidentChart(string Name, string? MRN);

    /// <summary>The charts of every resident on the floor.</summary>
    public sealed record FloorCharts(ResidentChart[] Charts);

    /// <summary>A chart flagged for the care team's review, named by its medical record number.</summary>
    public sealed class ChartFlagged
    {
        public string? MRN { get; set; }
    }

    /// <summary>The document events the ResidentChart page raises and hears.</summary>
    public static class ResidentChartEvents
    {
        public static readonly AppEvent<ChartFlagged> ChartFlagged = AppEvent.Declare<ChartFlagged>();
    }
}
