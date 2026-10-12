namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class PivotViewModel
    {
        public IReadOnlyList<PivotCensusRow> InitialRows { get; init; } = PivotCensusData.InitialRows;
    }

    /// <summary>The pivot page's link: <c>?facilityId=evergreen</c>.</summary>
    public sealed class PivotViewQuery
    {
        public string? FacilityId { get; init; }
    }

    /// <summary>The query of <c>GET api/pivot/census</c>: the facility whose census loads.</summary>
    public sealed class PivotCensusQuery
    {
        public string? FacilityId { get; init; }
    }

    public sealed class PivotCensusRow
    {
        public string Wing { get; init; } = "";
        public string CareLevel { get; init; } = "";
        public string Month { get; init; } = "";
        public int Residents { get; init; }
        public decimal Revenue { get; init; }
    }

    public sealed class PivotCensusResponse
    {
        public string Message { get; init; } = "";
        public IReadOnlyList<PivotCensusRow> Rows { get; init; } = new List<PivotCensusRow>();
    }

    /// <summary>The body of <c>POST api/pivot/{currentView}/audit</c>.</summary>
    public class PivotAuditRequest
    {
        public string? FacilityId { get; init; }
        public string Layout { get; init; } = "";
    }

    /// <summary>The audit as the page sends it: the view in the path, the facility and layout in the body.</summary>
    public sealed class PivotAuditVisit : PivotAuditRequest
    {
        public string CurrentView { get; init; } = "";
    }

    public sealed class PivotAuditResponse
    {
        public string Summary { get; init; } = "";
        public int LayoutLength { get; init; }
    }

    public sealed class PivotLayoutRequest
    {
        public string Layout { get; init; } = "";
    }

    public sealed class PivotLayoutResponse
    {
        public string Message { get; init; } = "";
        public string Layout { get; init; } = "";
    }

    public static class PivotCensusData
    {
        public static IReadOnlyList<PivotCensusRow> InitialRows { get; } =
            new List<PivotCensusRow>
            {
                Row("North", "Assisted", "Jan", 18, 126000),
                Row("North", "Memory Care", "Jan", 9, 94500),
                Row("South", "Assisted", "Jan", 21, 147000),
                Row("South", "Independent", "Jan", 16, 72000),
                Row("North", "Assisted", "Feb", 20, 140000),
                Row("South", "Memory Care", "Feb", 11, 115500)
            };

        public static IReadOnlyList<PivotCensusRow> MarchRows { get; } =
            new List<PivotCensusRow>
            {
                Row("North", "Assisted", "Mar", 12, 84000),
                Row("South", "Memory Care", "Mar", 7, 73500),
                Row("East", "Independent", "Mar", 14, 63000)
            };

        private static PivotCensusRow Row(
            string wing,
            string careLevel,
            string month,
            int residents,
            decimal revenue)
        {
            return new PivotCensusRow
            {
                Wing = wing,
                CareLevel = careLevel,
                Month = month,
                Residents = residents,
                Revenue = revenue
            };
        }
    }
}
