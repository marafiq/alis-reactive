using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The resident directory page: its search box, its grouping boxes and its surname tabs.</summary>
    public class DirectoryViewModel
    {
        /// <summary>What the supervisor looks for: part of a name, a care level, a wing or a nurse.</summary>
        public string Find { get; set; } = "";

        public bool GroupByCareLevel { get; set; }

        public bool GroupByWing { get; set; }

        /// <summary>The surname tabs, each with the page of the directory its surnames start on.</summary>
        public List<DirectorySurnameTab> SurnameTabs { get; set; } = new();
    }

    /// <summary>A tab of surnames, such as M–R, and the page of the directory where they start.</summary>
    public sealed record DirectorySurnameTab(string Id, string Letters, int Page);

    /// <summary>A resident as the directory lists them.</summary>
    public class DirectoryResident
    {
        public int ResidentId { get; set; }

        /// <summary>Surname first, as the directory sorts: <c>Patel, Amina</c>.</summary>
        public string ResidentName { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public int OpenTasks { get; set; }
        public string PrimaryNurse { get; set; } = "";
    }

    /// <summary>The residents who share a value of a grouped column, as the grid draws a group.</summary>
    public class DirectoryGroup
    {
        public string Key { get; set; } = "";
        public int Count { get; set; }
        public string Field { get; set; } = "";

        /// <summary>The group's residents, or the groups of the next grouped column.</summary>
        public List<object> Items { get; set; } = new();
    }

    /// <summary>
    /// What the grid asks for: the page, the sort, the filters, the search and the grouping it shows, and
    /// the change that made it ask.
    /// </summary>
    public class DirectoryQuery
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool RequiresCounts { get; set; }
        public List<FusionGridSortColumn> Sorted { get; set; } = new();
        public List<string> Group { get; set; } = new();
        public List<FusionGridTextFilterCriterion> Where { get; set; } = new();
        public List<FusionGridSearchDescriptor> Search { get; set; } = new();

        /// <summary>The kind of change, such as <c>sorting</c>; empty when the page first loads.</summary>
        public string Change { get; set; } = "";

        /// <summary>The column a sort changed.</summary>
        public string ChangedColumn { get; set; } = "";

        /// <summary>The direction a sort changed to.</summary>
        public string ChangedDirection { get; set; } = "";

        /// <summary>The page a page change moved to.</summary>
        public int ChangedToPage { get; set; }
    }

    /// <summary>The residents the grid shows, how many match, and what the last change did.</summary>
    public class DirectoryResults
    {
        /// <summary>The residents of the page, or every matching resident in groups.</summary>
        public List<object> Result { get; set; } = new();
        public int Count { get; set; }
        public string Summary { get; set; } = "";
        public string Change { get; set; } = "";
    }

    public class DirectoryCardRequest
    {
        public int ResidentId { get; set; }
        public int RowIndex { get; set; }

        /// <summary>Whether the supervisor picked the resident from the directory, not Next resident.</summary>
        public bool PickedFromDirectory { get; set; }
    }

    /// <summary>The resident card: who they are, where they live, and how the card was opened.</summary>
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

    /// <summary>The row of the resident after the one selected.</summary>
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
