namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.PivotView
{
    /// <summary>
    /// The executive director's occupancy and care-hours report: residents at month end and care hours
    /// given, by wing and care level, for the months the report covers.
    /// </summary>
    public sealed class PivotViewModel
    {
        public string CommunityName { get; init; } = "";

        /// <summary>The months the report covers, as the page names them: <c>July to September</c>.</summary>
        public string Period { get; init; } = "";

        public IReadOnlyList<OccupancyCensusRow> Census { get; init; } = new List<OccupancyCensusRow>();
    }

    /// <summary>The report's link: <c>?community=maple-grove</c>.</summary>
    public sealed class OccupancyReportQuery
    {
        public string? Community { get; init; }
    }

    /// <summary>One wing and care level in one month.</summary>
    public sealed class OccupancyCensusRow
    {
        public string Wing { get; init; } = "";
        public string CareLevel { get; init; } = "";
        public string Month { get; init; } = "";
        public int Residents { get; init; }
        public int CareHours { get; init; }
    }

    /// <summary>The query of <c>GET Census</c>: the community whose latest three months load.</summary>
    public sealed class OccupancyCensusQuery
    {
        public string? Community { get; init; }
    }

    /// <summary>The latest three months of a community's census.</summary>
    public sealed class OccupancyCensus
    {
        public string Period { get; init; } = "";
        public IReadOnlyList<OccupancyCensusRow> Rows { get; init; } = new List<OccupancyCensusRow>();
    }

    /// <summary>The body of <c>POST Snapshots/{community}</c>: the report's layout with today's figures.</summary>
    public class OccupancySnapshotLayout
    {
        public string Layout { get; init; } = "";
    }

    /// <summary>A board snapshot as the page sends it: the community in the path, the layout in the body.</summary>
    public sealed class OccupancySnapshotRequest : OccupancySnapshotLayout
    {
        public string? Community { get; init; }
    }

    /// <summary>The path of <c>GET Snapshots/{community}</c>: the community whose snapshot opens.</summary>
    public sealed class OccupancySnapshotLookup
    {
        public string? Community { get; init; }
    }

    /// <summary>A saved board snapshot: its layout and the months its figures cover.</summary>
    public sealed class OccupancySnapshot
    {
        public string Message { get; init; } = "";
        public string Layout { get; init; } = "";
        public string Period { get; init; } = "";
    }

    /// <summary>The body of <c>POST BoardPacket/{community}/{currentView}</c>: the report's layout, without figures.</summary>
    public class BoardPacketLayout
    {
        public string Layout { get; init; } = "";
    }

    /// <summary>The board packet entry as the page sends it: the community and the view in the path, the layout in the body.</summary>
    public sealed class BoardPacketRequest : BoardPacketLayout
    {
        public string? Community { get; init; }
        public string CurrentView { get; init; } = "";
    }

    /// <summary>What the server says about a snapshot or a board packet entry.</summary>
    public sealed class BoardReply
    {
        public string Message { get; init; } = "";
    }

    /// <summary>The communities and their census: July to September, and August to October once October closes.</summary>
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

        // Memory care is the North wing's secured unit; the South wing keeps a few independent apartments.
        private static IEnumerable<OccupancyCensusRow> Month(
            string month,
            int northAssisted, int northAssistedHours,
            int memoryCare, int memoryCareHours,
            int southAssisted, int southAssistedHours,
            int independent, int independentHours) =>
            new[]
            {
                Row("North", "Assisted Living", month, northAssisted, northAssistedHours),
                Row("North", "Memory Care", month, memoryCare, memoryCareHours),
                Row("South", "Assisted Living", month, southAssisted, southAssistedHours),
                Row("South", "Independent", month, independent, independentHours)
            };

        private static OccupancyCensusRow Row(string wing, string careLevel, string month, int residents, int careHours) =>
            new() { Wing = wing, CareLevel = careLevel, Month = month, Residents = residents, CareHours = careHours };
    }
}
