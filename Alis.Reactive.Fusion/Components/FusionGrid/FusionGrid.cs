using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Grids;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion Grid, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionGrid&lt;TModel, ResidentRow&gt;(plan, "roster", b =&gt; ...)</c> and target it
    /// with <c>p.Component&lt;FusionGrid&gt;("roster")</c>; every declared member works with <c>Set</c>, <c>Call</c>
    /// and <c>Get</c>. Columns are named by <see cref="Field"/>, and row members and events are typed by
    /// your row class:
    /// <code>
    /// grid.Call(c =&gt; c.SortColumn, Field.Of((ResidentRow r) =&gt; r.Name), FusionGridSortDirection.Descending, false)
    /// grid.Call(c =&gt; c.SetCellValue&lt;int&gt;(), 6001, Field.Of((ResidentRow r) =&gt; r.OpenTasks), 4)
    /// .Reactive(evt =&gt; evt.RowSelected&lt;ResidentRow&gt;(), (args, p) =&gt; ...)
    /// </code>
    /// The grid is not a form input: read its rows and selection with <c>Get</c> where a request gathers them.
    /// </remarks>
    public sealed class FusionGrid : FusionComponent
    {
        /// <summary>The rows; write an array, or a response body that is one.</summary>
        public Prop<object> DataSource => Prop<object>();

        /// <summary>The rows, typed by your row class.</summary>
        public Prop<TRow[]> Rows<TRow>() where TRow : class => Prop<TRow[]>("dataSource");

        /// <summary>Renders the rows again from the data source.</summary>
        public void Refresh() => Call();

        /// <summary>The rows on the current page, after sorting, filtering and grouping.</summary>
        public TypedSource<TRow[]> GetCurrentViewRecords<TRow>() where TRow : class => Invoke<TRow[]>();

        /// <summary>The zero-based index of the row with this primary key.</summary>
        public TypedSource<int> GetRowIndexByPrimaryKey(Arg<int> value) => Invoke<int>(value);

        /// <summary>Selects the row at a zero-based index.</summary>
        public void SelectRow(Arg<int> value) => Call(value);

        /// <summary>Selects the rows from one zero-based index to another.</summary>
        public void SelectRowsByRange(Arg<int> first, Arg<int> second) => Call(first, second);

        /// <summary>Clears the selection.</summary>
        public void ClearSelection() => Call();

        /// <summary>The zero-based indexes of the selected rows.</summary>
        public TypedSource<int[]> GetSelectedRowIndexes() => Invoke<int[]>();

        /// <summary>The selected rows, typed by your row class.</summary>
        public TypedSource<TRow[]> GetSelectedRecords<TRow>() where TRow : class => Invoke<TRow[]>();

        /// <summary>Shows a page, counted from one.</summary>
        public void GoToPage(Arg<int> value) => Call(value);

        /// <summary>Sorts by a column; pass <see langword="true"/> to keep the sorts already applied.</summary>
        public void SortColumn(Arg<Field> first, Arg<FusionGridSortDirection> second, Arg<bool> third) => Call(first, second, third);

        /// <summary>Clears every sort.</summary>
        public void ClearSorting() => Call();

        /// <summary>Shows the rows that contain this text; an empty text clears the search.</summary>
        public void Search(Arg<string> value) => Call(value);

        /// <summary>Filters a column by text.</summary>
        public void FilterByColumn(Arg<Field> first, Arg<FusionGridTextFilterOperator> second, Arg<string> third) => Call(first, second, third);

        /// <summary>Clears every filter.</summary>
        public void ClearFiltering() => Call();

        /// <summary>Groups the rows by a column.</summary>
        public void GroupColumn(Arg<Field> value) => Call(value);

        /// <summary>Removes the grouping by a column.</summary>
        public void UngroupColumn(Arg<Field> value) => Call(value);

        /// <summary>Clears every grouping.</summary>
        public void ClearGrouping() => Call();

        /// <summary>Shows a hidden column.</summary>
        public void ShowColumn(Arg<Field> value) => Call(value, Fixed("field"), name: "showColumns");

        /// <summary>Hides a column.</summary>
        public void HideColumn(Arg<Field> value) => Call(value, Fixed("field"), name: "hideColumns");

        /// <summary>Moves a column before another one.</summary>
        public void ReorderColumns(Arg<Field> first, Arg<Field> second) => Call(first, second);

        /// <summary>Fits every column to its content.</summary>
        public void AutoFitColumns() => Call();

        /// <summary>Fits one column to its content.</summary>
        public void AutoFitColumn(Arg<Field> value) => Call(value, name: "autoFitColumns");

        /// <summary>Opens the column chooser.</summary>
        public void OpenColumnChooser() => Call();

        /// <summary>Starts editing the selected row, inline or in a dialog as the edit settings say.</summary>
        public void StartEdit() => Call();

        /// <summary>Saves the edit; in batch mode, saves the pending changes.</summary>
        public void EndEdit() => Call();

        /// <summary>Cancels the edit.</summary>
        public void CloseEdit() => Call();

        /// <summary>Adds a row at the top.</summary>
        public void AddRecord<TRow>(Arg<TRow> value) where TRow : class => Call(value);

        /// <summary>Adds a row at a zero-based index.</summary>
        public void AddRecordAt<TRow>(Arg<TRow> first, Arg<int> second) where TRow : class => Call(first, second, name: "addRecord");

        /// <summary>Deletes the selected row.</summary>
        public void DeleteRecord() => Call();

        /// <summary>Replaces the row at a zero-based index.</summary>
        public void UpdateRow<TRow>(Arg<int> first, Arg<TRow> second) where TRow : class => Call(first, second);

        /// <summary>Replaces the data of the row with this primary key.</summary>
        public void SetRowData<TRow>(Arg<int> first, Arg<TRow> second) where TRow : class => Call(first, second);

        /// <summary>Sets a cell of the row with this primary key.</summary>
        public void SetCellValue<TValue>(Arg<int> first, Arg<Field<TValue>> second, Arg<TValue> third) => Call(first, second, third);

        /// <summary>Opens a cell for batch editing: the row's zero-based index, then the column.</summary>
        public void EditCell(Arg<int> first, Arg<Field> second) => Call(first, second);

        /// <summary>Saves the cell being batch edited.</summary>
        public void SaveCell() => Call();

        /// <summary>Changes a cell as a pending batch edit: the row's zero-based index, the column, the value.</summary>
        public void UpdateCell<TValue>(Arg<int> first, Arg<Field<TValue>> second, Arg<TValue> third) => Call(first, second, third);

        /// <summary>The pending batch edits, typed by your row class.</summary>
        public TypedSource<FusionGridBatchChanges<TRow>> GetBatchChanges<TRow>() where TRow : class => Invoke<FusionGridBatchChanges<TRow>>();

        /// <summary>Downloads the rows as an Excel file.</summary>
        public void ExcelExport() => Call();

        /// <summary>Downloads the rows as a CSV file.</summary>
        public void CsvExport() => Call();

        /// <summary>Downloads the rows as a PDF file.</summary>
        public void PdfExport() => Call();

        /// <summary>Prints the grid.</summary>
        public void Print() => Call();

        /// <summary>Raised when the grid needs data for a page, sort or filter, in custom binding.</summary>
        public TypedEvent<FusionGridDataStateChangeArgs> DataStateChange =>
            Event<FusionGridDataStateChangeArgs>("dataStateChange");

        /// <summary>Raised when the user clicks a toolbar item; branch on <c>args.Item.Id</c>.</summary>
        public TypedEvent<FusionGridToolbarClickArgs> ToolbarClick =>
            Event<FusionGridToolbarClickArgs>("toolbarClick");

        /// <summary>Raised when the user clicks a row.</summary>
        public TypedEvent<FusionGridRecordClickArgs<TRow>> RecordClick<TRow>() where TRow : class =>
            Event<FusionGridRecordClickArgs<TRow>>("recordClick");

        /// <summary>Raised after a row is selected.</summary>
        public TypedEvent<FusionGridRowSelectedArgs<TRow>> RowSelected<TRow>() where TRow : class =>
            Event<FusionGridRowSelectedArgs<TRow>>("rowSelected");

        /// <summary>Raised before an action such as edit, save, delete, sort, page or filter runs.</summary>
        public TypedEvent<FusionGridEditActionArgs<TRow>> ActionBegin<TRow>() where TRow : class =>
            Event<FusionGridEditActionArgs<TRow>>("actionBegin");

        /// <summary>Raised after an action such as edit, save, delete, sort, page or filter completes.</summary>
        public TypedEvent<FusionGridEditActionArgs<TRow>> ActionComplete<TRow>() where TRow : class =>
            Event<FusionGridEditActionArgs<TRow>>("actionComplete");

        /// <summary>Raised before a row enters edit mode.</summary>
        public TypedEvent<FusionGridBeginEditArgs<TRow>> BeginEdit<TRow>() where TRow : class =>
            Event<FusionGridBeginEditArgs<TRow>>("beginEdit");

        /// <summary>Raised before a batch-edited cell is saved.</summary>
        public TypedEvent<FusionGridCellSaveArgs<TRow, TValue>> CellSave<TRow, TValue>() where TRow : class =>
            Event<FusionGridCellSaveArgs<TRow, TValue>>("cellSave");

        /// <summary>Raised after a batch-edited cell is saved.</summary>
        public TypedEvent<FusionGridCellSavedArgs<TRow, TValue>> CellSaved<TRow, TValue>() where TRow : class =>
            Event<FusionGridCellSavedArgs<TRow, TValue>>("cellSaved");

        /// <summary>Raised before the pending batch edits are saved.</summary>
        public TypedEvent<FusionGridBeforeBatchSaveArgs<TRow>> BeforeBatchSave<TRow>() where TRow : class =>
            Event<FusionGridBeforeBatchSaveArgs<TRow>>("beforeBatchSave");
    }

    /// <summary>Sort direction for <see cref="FusionGrid.SortColumn"/>.</summary>
    public enum FusionGridSortDirection
    {
        /// <summary>Ascending sort order.</summary>
        Ascending,

        /// <summary>Descending sort order.</summary>
        Descending
    }

    /// <summary>Text filter operator for <see cref="FusionGrid.FilterByColumn"/>.</summary>
    public enum FusionGridTextFilterOperator
    {
        /// <summary>Value contains the text.</summary>
        [JsonStringEnumMemberName("contains")]
        Contains,

        /// <summary>Value equals the text.</summary>
        [JsonStringEnumMemberName("equal")]
        Equal,

        /// <summary>Value does not equal the text.</summary>
        [JsonStringEnumMemberName("notequal")]
        NotEqual,

        /// <summary>Value starts with the text.</summary>
        [JsonStringEnumMemberName("startswith")]
        StartsWith,

        /// <summary>Value ends with the text.</summary>
        [JsonStringEnumMemberName("endswith")]
        EndsWith
    }

    /// <summary>
    /// Event args for Syncfusion Grid's <c>dataStateChange</c> event.
    /// </summary>
    /// <remarks>
    /// Syncfusion emits a variant-specific payload for each data-state trigger.
    /// <c>skip</c>, <c>take</c>, and <c>requiresCounts</c> are common in
    /// custom-binding rows, while <c>sorted</c>, <c>group</c>, <c>where</c>,
    /// and <c>search</c> appear only on the trigger variants that emit them.
    /// Do not infer missing fields from another variant.
    /// </remarks>
    public class FusionGridDataStateChangeArgs : JsObject
    {
        /// <summary>Syncfusion event name injected by the EJ2 event observer.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Paging offset (0-based). Always present.</summary>
        public ReadOnly<int> Skip => ReadOnly<int>();

        /// <summary>Page size. Always present.</summary>
        public ReadOnly<int> Take => ReadOnly<int>();

        /// <summary>Whether the data response must include both result records and total count.</summary>
        public ReadOnly<bool> RequiresCounts => ReadOnly<bool>();

        /// <summary>Active sort columns when this trigger emits sorting state. Supports multi-sort.</summary>
        public ReadOnly<List<FusionGridSortColumn>> Sorted => ReadOnly<List<FusionGridSortColumn>>();

        /// <summary>
        /// Active group fields when this trigger emits grouping state.
        /// Method-trigger ungrouping omits this payload key rather than emitting an empty array.
        /// </summary>
        public ReadOnly<List<string>> Group => ReadOnly<List<string>>();

        /// <summary>Active text filter criteria from Grid filter UI.</summary>
        public ReadOnly<List<FusionGridTextFilterCriterion>> Where => ReadOnly<List<FusionGridTextFilterCriterion>>();

        /// <summary>Active search criteria from Grid toolbar or public search method.</summary>
        public ReadOnly<List<FusionGridSearchDescriptor>> Search => ReadOnly<List<FusionGridSearchDescriptor>>();

        /// <summary>Action details such as request type, column name, direction, and current page.</summary>
        public ReadOnly<FusionGridAction> Action => ReadOnly<FusionGridAction>();
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
    }

    /// <summary>
    /// Text filter criterion emitted by Grid dataStateChange for filter-bar/menu text filters.
    /// </summary>
    public class FusionGridTextFilterCriterion
    {
        /// <summary>Field being filtered.</summary>
        public string Field { get; set; }

        /// <summary>Filter operator such as <c>contains</c>, <c>equal</c>, or <c>startswith</c>.</summary>
        public string Operator { get; set; }

        /// <summary>Filter text supplied by this criterion.</summary>
        public string Value { get; set; }

        /// <summary>Composite predicate condition such as <c>and</c> or <c>or</c>.</summary>
        public string Condition { get; set; }

        /// <summary>Whether this predicate node contains nested predicates.</summary>
        public bool IsComplex { get; set; }

        /// <summary>Nested predicates when Syncfusion emits a composite Predicate.</summary>
        public List<FusionGridTextFilterCriterion> Predicates { get; set; }

        /// <summary>Whether the filter ignores case.</summary>
        public bool IgnoreCase { get; set; }

        /// <summary>Whether the filter ignores accents.</summary>
        public bool IgnoreAccent { get; set; }
    }

    /// <summary>
    /// Search criteria emitted by Grid dataStateChange for toolbar/search method operations.
    /// </summary>
    public class FusionGridSearchDescriptor
    {
        /// <summary>Fields included in the search.</summary>
        public List<string> Fields { get; set; }

        /// <summary>Search text.</summary>
        public string Key { get; set; }

        /// <summary>Search operator such as <c>contains</c>.</summary>
        public string Operator { get; set; }

        /// <summary>Whether search is case-insensitive.</summary>
        public bool IgnoreCase { get; set; }

        /// <summary>Whether search ignores accents.</summary>
        public bool IgnoreAccent { get; set; }
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
        public string RequestType { get; set; }

        /// <summary>Syncfusion action event name injected by the EJ2 event observer.</summary>
        public string Name { get; set; }

        /// <summary>Syncfusion action event type, such as <c>actionBegin</c>.</summary>
        public string Type { get; set; }

        /// <summary>Whether the current action can be cancelled.</summary>
        public bool Cancel { get; set; }

        /// <summary>Column name for sorting actions.</summary>
        public string ColumnName { get; set; }

        /// <summary>Sort direction for sorting actions.</summary>
        public string Direction { get; set; }

        /// <summary>Page number after the data-state action.</summary>
        public int CurrentPage { get; set; }

        /// <summary>Previous page number before the action.</summary>
        public int PreviousPage { get; set; }

        /// <summary>Page size after the data-state action.</summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Event args for the Syncfusion Grid "toolbarClick" event.
    /// Fires when any toolbar item is clicked, including custom items.
    /// Read <see cref="Item"/> to branch on which button was pressed.
    /// </summary>
    public class FusionGridToolbarClickArgs : JsObject
    {
        /// <summary>Toolbar item that raised the click event.</summary>
        public ReadOnly<FusionGridToolbarItem> Item => ReadOnly<FusionGridToolbarItem>();

        /// <summary>Toolbar action cancel flag carried by Syncfusion.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>Syncfusion event name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>
    /// Toolbar item carried by a Grid "toolbarClick" event.
    /// Custom items expose the id and text declared on the builder Toolbar.
    /// </summary>
    public class FusionGridToolbarItem
    {
        /// <summary>Toolbar item id, such as a custom command id or <c>{gridId}_excelexport</c>.</summary>
        public string Id { get; set; }

        /// <summary>Toolbar item display text.</summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Event args for the Syncfusion Grid "recordClick" event.
    /// </summary>
    /// <typeparam name="TRow">Grid row DTO type.</typeparam>
    public class FusionGridRecordClickArgs<TRow> : JsObject
        where TRow : class
    {
        /// <summary>Row data for the clicked record.</summary>
        public ReadOnly<TRow> RowData => ReadOnly<TRow>();

        /// <summary>Zero-based row index.</summary>
        public ReadOnly<int> RowIndex => ReadOnly<int>();

        /// <summary>Zero-based cell index.</summary>
        public ReadOnly<int> CellIndex => ReadOnly<int>();

        /// <summary>Syncfusion event name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>
    /// Event args for the Syncfusion Grid "rowSelected" event.
    /// </summary>
    /// <typeparam name="TRow">Grid row DTO type.</typeparam>
    public class FusionGridRowSelectedArgs<TRow> : JsObject
        where TRow : class
    {
        /// <summary>Row data carried by the selected row event.</summary>
        public ReadOnly<TRow> Data => ReadOnly<TRow>();

        /// <summary>Zero-based index of the row after selection.</summary>
        public ReadOnly<int> RowIndex => ReadOnly<int>();

        /// <summary>Zero-based index of the previously selected row, when one exists.</summary>
        public ReadOnly<int?> PreviousRowIndex => ReadOnly<int?>();

        /// <summary>Whether user interaction triggered the row selection.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();

        /// <summary>Syncfusion event name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>The pending batch edits, as <see cref="FusionGrid.GetBatchChanges{TRow}"/> returns them.</summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    public class FusionGridBatchChanges<TRow>
        where TRow : class
    {
        /// <summary>Rows the user added.</summary>
        public List<TRow> AddedRecords { get; set; } = new List<TRow>();

        /// <summary>Rows the user changed.</summary>
        public List<TRow> ChangedRecords { get; set; } = new List<TRow>();

        /// <summary>Rows the user deleted.</summary>
        public List<TRow> DeletedRecords { get; set; } = new List<TRow>();
    }

    /// <summary>Payload of <c>BeginEdit</c>; set <c>Cancel</c> to keep the row out of edit mode.</summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    public class FusionGridBeginEditArgs<TRow> : JsObject
        where TRow : class
    {
        /// <summary>The row about to be edited.</summary>
        public ReadOnly<TRow> RowData => ReadOnly<TRow>();

        /// <summary>Zero-based index of the row.</summary>
        public ReadOnly<int> RowIndex => ReadOnly<int>();

        /// <summary>The edit type the grid reports.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> to cancel the edit.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <c>ActionBegin</c> and <c>ActionComplete</c>; set <c>Cancel</c> in <c>ActionBegin</c> to stop the action.</summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    public class FusionGridEditActionArgs<TRow> : JsObject
        where TRow : class
    {
        /// <summary>The event name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>The kind of action, for example <c>"save"</c>, <c>"delete"</c> or <c>"sorting"</c>.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();

        /// <summary><c>"add"</c> or <c>"edit"</c> when a row is saved.</summary>
        public ReadOnly<string> Action => ReadOnly<string>();

        /// <summary>The edit type the grid reports.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> in <c>ActionBegin</c> to cancel the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>The row the action applies to.</summary>
        public ReadOnly<TRow> Data => ReadOnly<TRow>();

        /// <summary>The row before the edit.</summary>
        public ReadOnly<TRow> PreviousData => ReadOnly<TRow>();

        /// <summary>Zero-based index of the row, when the action has one.</summary>
        public ReadOnly<int?> RowIndex => ReadOnly<int?>();

        /// <summary>Zero-based index of the selected row.</summary>
        public ReadOnly<int> SelectedRow => ReadOnly<int>();
    }

    /// <summary>Payload of <c>CellSave</c>; set <c>Cancel</c> to keep the cell from saving.</summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    /// <typeparam name="TValue">The type of the cell value.</typeparam>
    public class FusionGridCellSaveArgs<TRow, TValue> : JsObject
        where TRow : class
    {
        /// <summary>The row of the cell.</summary>
        public ReadOnly<TRow> RowData => ReadOnly<TRow>();

        /// <summary>The column field of the cell.</summary>
        public ReadOnly<string> ColumnName => ReadOnly<string>();

        /// <summary>The value being saved.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>The value before the edit.</summary>
        public ReadOnly<TValue> PreviousValue => ReadOnly<TValue>();

        /// <summary>Set to <see langword="true"/> to cancel the save.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <c>CellSaved</c>.</summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    /// <typeparam name="TValue">The type of the cell value.</typeparam>
    public class FusionGridCellSavedArgs<TRow, TValue> : JsObject
        where TRow : class
    {
        /// <summary>The row of the cell.</summary>
        public ReadOnly<TRow> RowData => ReadOnly<TRow>();

        /// <summary>The column field of the cell.</summary>
        public ReadOnly<string> ColumnName => ReadOnly<string>();

        /// <summary>The saved value.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>The value before the edit.</summary>
        public ReadOnly<TValue> PreviousValue => ReadOnly<TValue>();
    }

    /// <summary>Payload of <c>BeforeBatchSave</c>; set <c>Cancel</c> to keep the pending edits unsaved.</summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    public class FusionGridBeforeBatchSaveArgs<TRow> : JsObject
        where TRow : class
    {
        /// <summary>The pending edits.</summary>
        public ReadOnly<FusionGridBatchChanges<TRow>> BatchChanges => ReadOnly<FusionGridBatchChanges<TRow>>();

        /// <summary>Set to <see langword="true"/> to cancel the save.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionGrid"/> against a plan.</summary>
    public static class FusionGridHtmlExtensions
    {
        /// <summary>
        /// Renders a Syncfusion Grid with the element ID the plan targets it by.
        /// </summary>
        /// <typeparam name="TModel">The view model the plan is authored for.</typeparam>
        /// <typeparam name="TRow">Your row class, which types the Syncfusion builder.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">The plan the grid's behavior is authored in.</param>
        /// <param name="elementId">The element ID, shared by the markup and <c>p.Component&lt;FusionGrid&gt;(elementId)</c>.</param>
        /// <param name="build">Configures columns, paging, sorting and the rest on the Syncfusion builder.</param>
        /// <returns>The rendered grid; wire its events with <c>.Reactive(evt =&gt; evt.RowSelected&lt;TRow&gt;(), ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionGrid, TModel> FusionGrid<TModel, TRow>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<GridBuilder<TRow>> build)
            where TModel : class
            where TRow : class
        {
            var builder = html.EJS().Grid<TRow>(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionGrid>(elementId, builder.Render());
        }
    }
}
