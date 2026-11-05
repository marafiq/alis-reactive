using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DirectoryViewModel
    {
        public string Find { get; set; } = "";

        public bool GroupByCareLevel { get; set; }

        public bool GroupByWing { get; set; }

        public List<DirectorySurnameTab> SurnameTabs { get; set; } = new();
    }

    public sealed record DirectorySurnameTab(string Id, string Letters, int Page);

    public class DirectoryResident
    {
        public int ResidentId { get; set; }

        public string ResidentName { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public int OpenTasks { get; set; }
        public string PrimaryNurse { get; set; } = "";
    }

    public class DirectoryQuery
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool RequiresCounts { get; set; }
        public List<FusionGridSortColumn> Sorted { get; set; } = new();
        public List<string> Group { get; set; } = new();
        public List<FusionGridTextFilterCriterion> Where { get; set; } = new();
        public List<FusionGridSearchDescriptor> Search { get; set; } = new();

        public string Change { get; set; } = "";

        public string ChangedColumn { get; set; } = "";

        public string ChangedDirection { get; set; } = "";

        public int ChangedToPage { get; set; }
    }

    public class DirectoryResults : IFusionGridPage<DirectoryResident>
    {
        public List<DirectoryResident> Result { get; set; } = new();

        IReadOnlyList<DirectoryResident> IFusionGridPage<DirectoryResident>.Result => Result;
        public int Count { get; set; }
        public string Summary { get; set; } = "";
        public string Change { get; set; } = "";
    }

    public class DirectoryCardRequest
    {
        public int ResidentId { get; set; }
        public int RowIndex { get; set; }

        public bool PickedFromDirectory { get; set; }
    }

    public class DirectoryCard
    {
        public string Name { get; set; } = "";
        public string Details { get; set; } = "";
        public string Context { get; set; } = "";
    }

    public class DirectoryNextRequest
    {
        public List<int> SelectedRowIndexes { get; set; } = new();
    }

    public class DirectoryNextResident
    {
        public int RowIndex { get; set; }
    }

    public class DirectoryRoundsRequest
    {
        public List<DirectoryResident> Residents { get; set; } = new();
    }

    public class DirectoryRounds
    {
        public string Summary { get; set; } = "";
    }
}
