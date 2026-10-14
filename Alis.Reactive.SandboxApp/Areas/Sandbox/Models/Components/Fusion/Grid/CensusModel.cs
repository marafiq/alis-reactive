using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The business office's census by age: the youngest age the census counts.</summary>
    public class AgeCensusViewModel
    {
        /// <summary>The census lists residents this age or older; empty lists every resident.</summary>
        public decimal? MinAge { get; set; }
    }

    /// <summary>A resident as the census lists them.</summary>
    public class AgeCensusResident
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
    }

    /// <summary>
    /// What the census grid asks for: the page, the sort, the youngest age it counts, and the change that made
    /// it ask.
    /// </summary>
    public class AgeCensusQuery
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool RequiresCounts { get; set; }
        public List<FusionGridSortColumn> Sorted { get; set; } = new();
        public decimal? MinAge { get; set; }

        /// <summary>The kind of change, such as <c>paging</c>; empty when the page first loads.</summary>
        public string Change { get; set; } = "";

        /// <summary>The column a sort changed.</summary>
        public string SortedColumn { get; set; } = "";

        /// <summary>The direction a sort changed to.</summary>
        public string SortedDirection { get; set; } = "";

        /// <summary>The page a page change moved to.</summary>
        public int ToPage { get; set; }

        /// <summary>The page a page change moved from.</summary>
        public int FromPage { get; set; }

        /// <summary>How many residents a page shows after a page change.</summary>
        public int PageSize { get; set; }
    }

    /// <summary>The residents of the page, how many the census counts, and what the last change did.</summary>
    public class AgeCensusPage
    {
        public List<AgeCensusResident> Result { get; set; } = new();
        public int Count { get; set; }
        public string Summary { get; set; } = "";
        public string Change { get; set; } = "";
    }
}
