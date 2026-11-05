using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class AgeCensusViewModel
    {
        public decimal? MinAge { get; set; }
    }

    public class AgeCensusResident
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
    }

    public class AgeCensusQuery
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool RequiresCounts { get; set; }
        public List<FusionGridSortColumn> Sorted { get; set; } = new();
        public decimal? MinAge { get; set; }

        public string Change { get; set; } = "";

        public string SortedColumn { get; set; } = "";

        public string SortedDirection { get; set; } = "";

        public int ToPage { get; set; }

        public int FromPage { get; set; }

        public int PageSize { get; set; }
    }

    public class AgeCensusPage : IFusionGridPage<AgeCensusResident>
    {
        public List<AgeCensusResident> Result { get; set; } = new();

        IReadOnlyList<AgeCensusResident> IFusionGridPage<AgeCensusResident>.Result => Result;
        public int Count { get; set; }
        public string Summary { get; set; } = "";
        public string Change { get; set; } = "";
    }
}
