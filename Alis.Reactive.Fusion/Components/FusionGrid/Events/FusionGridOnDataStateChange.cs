using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the SF Grid "dataStateChange" event.
    /// Fires when the Grid needs data: on init, sort, page, or filter.
    /// </summary>
    /// <remarks>
    /// Top-level fields carry the FULL grid state on every event:
    /// skip/take for paging, sorted[] for all active sort columns (multi-sort).
    /// Always send the full state to the server so paging preserves sort order.
    /// </remarks>
    public class FusionGridDataStateChangeArgs
    {
        /// <summary>Paging offset (0-based). Always present.</summary>
        public int Skip { get; set; }

        /// <summary>Page size. Always present.</summary>
        public int Take { get; set; }

        /// <summary>Active sort columns. Empty when unsorted. Supports multi-sort.</summary>
        public List<FusionGridSortColumn>? Sorted { get; set; }

        /// <summary>Action details: requestType, columnName, direction, currentPage.</summary>
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
    /// Contains requestType plus context-specific parameters.
    /// </summary>
    public class FusionGridAction
    {
        /// <summary>SF Grid action type constants for use with When conditions.</summary>
        public const string Sorting = "sorting";
        public const string Paging = "paging";
        public const string Filtering = "filtering";
        public const string Searching = "searching";
        public const string Grouping = "grouping";
        public const string Refresh = "refresh";

        /// <summary>Gets or sets the request type (e.g. "sorting", "paging").</summary>
        public string? RequestType { get; set; }

        /// <summary>Gets or sets the column name being sorted (sorting actions only).</summary>
        public string? ColumnName { get; set; }

        /// <summary>Gets or sets the sort direction (sorting actions only).</summary>
        public string? Direction { get; set; }

        /// <summary>Gets or sets the current page number.</summary>
        public int CurrentPage { get; set; }

        /// <summary>Gets or sets the previous page number.</summary>
        public int PreviousPage { get; set; }

        /// <summary>Gets or sets the page size.</summary>
        public int PageSize { get; set; }

        public FusionGridAction() { }
    }
}
