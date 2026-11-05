namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentChartPageModel
    {
        public string? ReviewChartMrn { get; set; }

        public string? ChartByRecordNumber { get; set; }
    }

    public sealed record ChartLink(string MRN, string Name, string Resident);

    public sealed class ChartQuery
    {
        public string? Resident { get; set; }
    }

    public sealed record ResidentChart(string Name, string? MRN);

    public sealed record FloorCharts(ResidentChart[] Charts);

    public sealed class ChartFlagged
    {
        public string? MRN { get; set; }
    }

    public static class ResidentChartEvents
    {
        public static readonly AppEvent<ChartFlagged> ChartFlagged = AppEvent.Declare<ChartFlagged>();
    }
}
