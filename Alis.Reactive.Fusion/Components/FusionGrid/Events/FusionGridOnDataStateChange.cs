using System.Collections.Generic;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for Syncfusion Grid's <c>dataStateChange</c> event.
    /// </summary>
    /// <remarks>
    /// Top-level fields carry the full grid state on every event:
    /// <c>skip</c>/<c>take</c> for paging and <c>sorted</c> for all active sort columns.
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

        /// <summary>Active group fields. Empty when ungrouped. Supports nested grouping.</summary>
        public List<string>? Group { get; set; }

        /// <summary>Active text filter criteria from Grid filter UI.</summary>
        public List<FusionGridTextFilterCriterion>? Where { get; set; }

        /// <summary>Active search descriptors from Grid toolbar or public search method.</summary>
        public List<FusionGridSearchDescriptor>? Search { get; set; }

        /// <summary>Action details such as request type, column name, direction, and current page.</summary>
        public FusionGridAction Action { get; set; } = new FusionGridAction();

        public FusionGridDataStateChangeArgs() { }
    }

    /// <summary>
    /// One sort column in the grid's sorted state.
    /// Syncfusion uses lowercase direction values: <c>ascending</c> and <c>descending</c>.
    /// </summary>
    public class FusionGridSortColumn
    {
        /// <summary>Field name being sorted.</summary>
        public string Name { get; set; } = "";

        /// <summary>Sort direction: <c>ascending</c> or <c>descending</c>.</summary>
        public string Direction { get; set; } = "";

        public FusionGridSortColumn() { }
    }

    /// <summary>
    /// Text filter criterion emitted by Grid dataStateChange for filter-bar/menu text filters.
    /// </summary>
    public class FusionGridTextFilterCriterion
    {
        /// <summary>Field being filtered.</summary>
        public string? Field { get; set; }

        /// <summary>Filter operator such as <c>contains</c>, <c>equal</c>, or <c>startswith</c>.</summary>
        public string? Operator { get; set; }

        /// <summary>Text value being filtered.</summary>
        public string? Value { get; set; }

        /// <summary>Composite predicate condition such as <c>and</c> or <c>or</c>.</summary>
        public string? Condition { get; set; }

        /// <summary>Nested predicates when Syncfusion emits a composite Predicate.</summary>
        public List<FusionGridTextFilterCriterion>? Predicates { get; set; }

        /// <summary>Predicate joining this criterion to the next criterion.</summary>
        public string? Predicate { get; set; }

        /// <summary>Whether the filter ignores case.</summary>
        public bool IgnoreCase { get; set; }

        /// <summary>Whether the filter is case-sensitive.</summary>
        public bool MatchCase { get; set; }

        /// <summary>Whether the filter ignores accents.</summary>
        public bool IgnoreAccent { get; set; }

        public FusionGridTextFilterCriterion() { }
    }

    /// <summary>
    /// Search descriptor emitted by Grid dataStateChange for toolbar/search method operations.
    /// </summary>
    public class FusionGridSearchDescriptor
    {
        /// <summary>Fields included in the search.</summary>
        public List<string>? Fields { get; set; }

        /// <summary>Search text.</summary>
        public string? Key { get; set; }

        /// <summary>Search operator such as <c>contains</c>.</summary>
        public string? Operator { get; set; }

        /// <summary>Whether search is case-insensitive.</summary>
        public bool IgnoreCase { get; set; }

        /// <summary>Whether search ignores accents.</summary>
        public bool IgnoreAccent { get; set; }

        public FusionGridSearchDescriptor() { }
    }

    /// <summary>
    /// Action details from the dataStateChange event.
    /// Contains <c>requestType</c> plus context-specific parameters.
    /// </summary>
    public class FusionGridAction
    {
        /// <summary>Syncfusion Grid action type constants for use with <c>When</c> conditions.</summary>
        public const string Sorting = "sorting";
        public const string Paging = "paging";
        public const string Filtering = "filtering";
        public const string Searching = "searching";
        public const string Grouping = "grouping";
        public const string Ungrouping = "ungrouping";
        public const string Refresh = "refresh";

        /// <summary>Syncfusion request type, such as <c>sorting</c> or <c>paging</c>.</summary>
        public string? RequestType { get; set; }

        /// <summary>Column name for sorting actions.</summary>
        public string? ColumnName { get; set; }

        /// <summary>Sort direction for sorting actions.</summary>
        public string? Direction { get; set; }

        /// <summary>Current page number after the action.</summary>
        public int CurrentPage { get; set; }

        /// <summary>Previous page number before the action.</summary>
        public int PreviousPage { get; set; }

        /// <summary>Current page size.</summary>
        public int PageSize { get; set; }

        public FusionGridAction() { }
    }
}
