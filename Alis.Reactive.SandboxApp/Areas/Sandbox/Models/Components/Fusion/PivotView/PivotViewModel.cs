namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.PivotView
{
    public sealed class PivotViewModel
    {
        public string CommunityName { get; init; } = "";

        public string Period { get; init; } = "";

        public IReadOnlyList<OccupancyCensusRow> Census { get; init; } = new List<OccupancyCensusRow>();
    }

    public sealed class OccupancyReportQuery
    {
        public string? Community { get; init; }
    }

    public sealed class OccupancyCensusRow
    {
        public string Wing { get; init; } = "";
        public string CareLevel { get; init; } = "";
        public string Month { get; init; } = "";
        public int Residents { get; init; }
        public int CareHours { get; init; }
    }

    public sealed class OccupancyCensusQuery
    {
        public string? Community { get; init; }
    }

    public sealed class OccupancyCensus
    {
        public string Period { get; init; } = "";
        public IReadOnlyList<OccupancyCensusRow> Rows { get; init; } = new List<OccupancyCensusRow>();
    }

    public class OccupancySnapshotLayout
    {
        public string Layout { get; init; } = "";
    }

    public sealed class OccupancySnapshotRequest : OccupancySnapshotLayout
    {
        public string? Community { get; init; }
    }

    public sealed class OccupancySnapshotLookup
    {
        public string? Community { get; init; }
    }

    public sealed class OccupancySnapshot
    {
        public string Message { get; init; } = "";
        public string Layout { get; init; } = "";
        public string Period { get; init; } = "";
    }

    public class BoardPacketLayout
    {
        public string Layout { get; init; } = "";
    }

    public sealed class BoardPacketRequest : BoardPacketLayout
    {
        public string? Community { get; init; }
        public string CurrentView { get; init; } = "";
    }

    public sealed class BoardReply
    {
        public string Message { get; init; } = "";
    }

    public static class OccupancyCensusData
    {
        public static IReadOnlyDictionary<string, string> Communities { get; } = new Dictionary<string, string>
        {
            ["maple-grove"] = "Maple Grove",
            ["cedar-hill"] = "Cedar Hill"
        };

        public const string QuarterPeriod = "July to September";

        public const string LatestPeriod = "August to October";

        public static IReadOnlyList<OccupancyCensusRow> Quarter { get; } = Month("July", 18, 1620, 9, 1890, 21, 1980, 2, 60)
            .Concat(August).Concat(September).ToList();

        public static IReadOnlyList<OccupancyCensusRow> Latest { get; } = August.Concat(September)
            .Concat(Month("October", 20, 1800, 10, 2150, 21, 1950, 4, 110)).ToList();

        private static IEnumerable<OccupancyCensusRow> August => Month("August", 18, 1650, 9, 1930, 22, 2020, 1, 25);

        private static IEnumerable<OccupancyCensusRow> September => Month("September", 19, 1710, 10, 2140, 22, 2010, 3, 90);

        private static IEnumerable<OccupancyCensusRow> Month(
            string month,
            int northAssisted, int northAssistedHours,
            int memoryCare, int memoryCareHours,
            int southAssisted, int southAssistedHours,
            int independent, int independentHours)
            => new[]
            {
                Row("North", "Assisted Living", month, northAssisted, northAssistedHours),
                Row("North", "Memory Care", month, memoryCare, memoryCareHours),
                Row("South", "Assisted Living", month, southAssisted, southAssistedHours),
                Row("South", "Independent", month, independent, independentHours)
            };

        private static OccupancyCensusRow Row(string wing, string careLevel, string month, int residents, int careHours)
            => new() { Wing = wing, CareLevel = careLevel, Month = month, Residents = residents, CareHours = careHours };
    }
}
