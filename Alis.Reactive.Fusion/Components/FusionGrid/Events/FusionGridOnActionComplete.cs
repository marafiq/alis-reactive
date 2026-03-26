using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the SF Grid "dataStateChange" event.
    /// Fires when the Grid needs data — on init, sort, page, filter.
    ///
    /// Top-level fields carry the FULL grid state on every event:
    ///   skip/take — paging offset and page size
    ///   sorted   — array of {name, direction} for all active sort columns (multi-sort)
    ///
    /// Always send the full state to the server — paging preserves sort, sort preserves page.
    /// </summary>
    public class FusionGridDataStateChangeArgs
    {
        /// <summary>Paging offset (0-based). Always present.</summary>
        public int Skip { get; set; }

        /// <summary>Page size. Always present.</summary>
        public int Take { get; set; }

        /// <summary>Active sort columns. Empty when unsorted. Supports multi-sort.</summary>
        public List<FusionGridSortColumn>? Sorted { get; set; }

        /// <summary>Action details — requestType, columnName, direction, currentPage.</summary>
        public FusionGridAction Action { get; set; } = new FusionGridAction();

        public FusionGridDataStateChangeArgs() { }
    }

    /// <summary>
    /// One sort column in the grid's sorted state.
    /// SF uses lowercase direction: "ascending" / "descending".
    /// </summary>
    public class FusionGridSortColumn
    {
        /// <summary>Field name (e.g., "name", "age").</summary>
        public string Name { get; set; } = "";

        /// <summary>Sort direction: "ascending" or "descending" (lowercase).</summary>
        public string Direction { get; set; } = "";

        public FusionGridSortColumn() { }
    }

    /// <summary>
    /// Action details from the dataStateChange event.
    /// Contains requestType + context-specific params.
    /// </summary>
    public class FusionGridAction
    {
        /// <summary>SF Grid action type constants — use with When conditions.</summary>
        public const string Sorting = "sorting";
        public const string Paging = "paging";
        public const string Filtering = "filtering";
        public const string Searching = "searching";
        public const string Grouping = "grouping";
        public const string Refresh = "refresh";

        public string? RequestType { get; set; }
        public string? ColumnName { get; set; }
        public string? Direction { get; set; }
        public int CurrentPage { get; set; }
        public int PreviousPage { get; set; }
        public int PageSize { get; set; }

        public FusionGridAction() { }
    }
}
