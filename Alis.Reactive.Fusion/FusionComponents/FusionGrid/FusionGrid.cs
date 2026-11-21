using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A grid: EJ2's Grid, rows in columns a person can page, sort, filter, search, group, select and edit.
/// </summary>
/// <remarks>
/// A member that takes or hands rows is typed by the view's row class, <c>TRow</c>, and one that takes a row's key
/// by the type of the rows' key member, <c>TKey</c>, text or a number. A member that takes a column takes a
/// <see cref="Field"/>, as <c>Field.Of((Resident r) =&gt; r.Name)</c>. An event's payload is read with <c>Read</c>,
/// and a handler writes true to its <c>Cancel</c> to stop the action.
/// </remarks>
[Mirrors("@syncfusion/ej2-grids", "Grid")]
public sealed class FusionGrid : FusionComponent
{
    /// <summary>Gets the rows the grid shows.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The rows.</returns>
    public Property<IReadOnlyList<TRow>> DataSource<TRow>() => Property<IReadOnlyList<TRow>>();

    /// <summary>Gets the page of rows the grid shows, with the count of every row the server holds.</summary>
    /// <remarks>Write the page the server answers, so the grid pages over rows it does not hold.</remarks>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The page.</returns>
    public Property<DataResult<TRow>> Page<TRow>() => Property<DataResult<TRow>>(name: "dataSource");

    /// <summary>Renders the grid again from its rows.</summary>
    public void Refresh() => Method();

    /// <summary>Moves the grid to a page.</summary>
    /// <param name="pageNo">The page's number, from 1.</param>
    public void GoToPage(Arg<int> pageNo) => Method([pageNo]);

    /// <summary>Reads the rows the grid's page shows.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The rows.</returns>
    public MethodResult<IReadOnlyList<TRow>> GetCurrentViewRecords<TRow>() => Method<IReadOnlyList<TRow>>();

    /// <summary>Sorts the grid by a column.</summary>
    /// <param name="columnName">The column's field.</param>
    /// <param name="direction">The direction of the sort.</param>
    /// <param name="isMultiSort">Whether the sort keeps the grid's other sorted columns.</param>
    public void SortColumn(Arg<Field> columnName, Arg<SortDirection> direction, Arg<bool> isMultiSort) => Method([columnName, direction, isMultiSort]);

    /// <summary>Clears every sort of the grid.</summary>
    public void ClearSorting() => Method();

    /// <summary>Filters the grid's rows by the text of a column.</summary>
    /// <param name="fieldName">The column's field, a member that holds text.</param>
    /// <param name="filterOperator">How a row's text is compared with the filter's text.</param>
    /// <param name="filterValue">The filter's text.</param>
    public void FilterByColumn(Arg<Field<string>> fieldName, Arg<FilterOperator> filterOperator, Arg<string> filterValue) => Method([fieldName, filterOperator, filterValue]);

    /// <summary>Clears every filter of the grid.</summary>
    public void ClearFiltering() => Method();

    /// <summary>Shows the rows that hold a text; empty text shows every row.</summary>
    /// <param name="searchString">The text.</param>
    public void Search(Arg<string> searchString) => Method([searchString]);

    /// <summary>Groups the grid's rows by a column.</summary>
    /// <param name="columnName">The column's field.</param>
    public void GroupColumn(Arg<Field> columnName) => Method([columnName]);

    /// <summary>Stops grouping the grid's rows by a column.</summary>
    /// <param name="columnName">The column's field.</param>
    public void UngroupColumn(Arg<Field> columnName) => Method([columnName]);

    /// <summary>Clears every grouping of the grid.</summary>
    public void ClearGrouping() => Method();

    /// <summary>Shows a hidden column, found by its field.</summary>
    /// <param name="keys">The column's field.</param>
    public void ShowColumns(Arg<Field> keys) => Method([keys, Fixed("field")]);

    /// <summary>Hides a column, found by its field.</summary>
    /// <param name="keys">The column's field.</param>
    public void HideColumns(Arg<Field> keys) => Method([keys, Fixed("field")]);

    /// <summary>Fits the width of every column to its content.</summary>
    public void AutoFitColumns() => Method();

    /// <summary>Fits the width of a column to its content.</summary>
    /// <param name="fieldNames">The column's field.</param>
    public void AutoFitColumns(Arg<Field> fieldNames) => Method([fieldNames]);

    /// <summary>Moves a column to the place of another.</summary>
    /// <param name="fromFName">The field of the column that moves.</param>
    /// <param name="toFName">The field of the column whose place it takes.</param>
    public void ReorderColumns(Arg<Field> fromFName, Arg<Field> toFName) => Method([fromFName, toFName]);

    /// <summary>Reads a column of the grid, found by its field.</summary>
    /// <param name="field">The column's field.</param>
    /// <returns>The column.</returns>
    public Column GetColumnByField(Arg<Field> field) => ReturnedObject<Column>([field]);

    /// <summary>Renders the grid's header again, so it shows a column's written text.</summary>
    public void RefreshHeader() => Method();

    /// <summary>Opens the grid's column chooser.</summary>
    public void OpenColumnChooser() => Method();

    /// <summary>Selects a row.</summary>
    /// <param name="index">The row's position, from 0.</param>
    public void SelectRow(Arg<int> index) => Method([index]);

    /// <summary>Selects the rows from one position to another.</summary>
    /// <param name="startIndex">The first row's position, from 0.</param>
    /// <param name="endIndex">The last row's position.</param>
    public void SelectRowsByRange(Arg<int> startIndex, Arg<int> endIndex) => Method([startIndex, endIndex]);

    /// <summary>Clears the grid's selection.</summary>
    public void ClearSelection() => Method();

    /// <summary>Reads the positions of the selected rows.</summary>
    /// <returns>The positions, from 0.</returns>
    public MethodResult<IReadOnlyList<int>> GetSelectedRowIndexes() => Method<IReadOnlyList<int>>();

    /// <summary>Reads the selected rows.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The rows.</returns>
    public MethodResult<IReadOnlyList<TRow>> GetSelectedRecords<TRow>() => Method<IReadOnlyList<TRow>>();

    /// <summary>Reads the position of a row, found by its key.</summary>
    /// <typeparam name="TKey">The type of the rows' key member, text or a number.</typeparam>
    /// <param name="value">The row's key.</param>
    /// <returns>The row's position, from 0.</returns>
    public MethodResult<int> GetRowIndexByPrimaryKey<TKey>(Arg<TKey> value) => Method<int>([value]);

    /// <summary>Opens the selected row for editing.</summary>
    public void StartEdit() => Method();

    /// <summary>Saves the row or the batch being edited.</summary>
    public void EndEdit() => Method();

    /// <summary>Closes the row being edited without saving it.</summary>
    public void CloseEdit() => Method();

    /// <summary>Adds a row to the grid.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <param name="data">The row.</param>
    public void AddRecord<TRow>(Arg<TRow> data) where TRow : class => Method([data]);

    /// <summary>Adds a row to the grid at a position.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <param name="data">The row.</param>
    /// <param name="index">The position the row takes, from 0.</param>
    public void AddRecord<TRow>(Arg<TRow> data, Arg<int> index) where TRow : class => Method([data, index]);

    /// <summary>Deletes the selected row.</summary>
    public void DeleteRecord() => Method();

    /// <summary>Replaces the row at a position.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <param name="index">The row's position, from 0.</param>
    /// <param name="data">The row that replaces it.</param>
    public void UpdateRow<TRow>(Arg<int> index, Arg<TRow> data) where TRow : class => Method([index, data]);

    /// <summary>Replaces the data of a row, found by its key, without saving it.</summary>
    /// <remarks>Write both type arguments, the key's type and the row class.</remarks>
    /// <typeparam name="TKey">The type of the rows' key member, text or a number.</typeparam>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <param name="key">The row's key.</param>
    /// <param name="rowData">The row's data.</param>
    public void SetRowData<TKey, TRow>(Arg<TKey> key, Arg<TRow> rowData) where TRow : class => Method([key, rowData]);

    /// <summary>Writes a value to a cell of a row, found by its key, without saving it.</summary>
    /// <remarks>Write both type arguments, the key's type and the cell's.</remarks>
    /// <typeparam name="TKey">The type of the rows' key member, text or a number.</typeparam>
    /// <typeparam name="TValue">The type of the column's member.</typeparam>
    /// <param name="key">The row's key.</param>
    /// <param name="field">The cell's column.</param>
    /// <param name="value">The value.</param>
    public void SetCellValue<TKey, TValue>(Arg<TKey> key, Arg<Field<TValue>> field, Arg<TValue> value) => Method([key, field, value]);

    /// <summary>Opens a cell for editing, in the grid's batch edit mode.</summary>
    /// <param name="index">The row's position, from 0.</param>
    /// <param name="field">The cell's column.</param>
    public void EditCell(Arg<int> index, Arg<Field> field) => Method([index, field]);

    /// <summary>Writes a value to a cell, in the grid's batch edit mode.</summary>
    /// <typeparam name="TValue">The type of the column's member.</typeparam>
    /// <param name="rowIndex">The row's position, from 0.</param>
    /// <param name="field">The cell's column.</param>
    /// <param name="value">The value.</param>
    public void UpdateCell<TValue>(Arg<int> rowIndex, Arg<Field<TValue>> field, Arg<TValue> value) => Method([rowIndex, field, value]);

    /// <summary>Saves the cell being edited, in the grid's batch edit mode.</summary>
    public void SaveCell() => Method();

    /// <summary>Reads the rows the grid's batch edit adds, changes and deletes, before they are saved.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The changes.</returns>
    public BatchChanges<TRow> GetBatchChanges<TRow>() => ReturnedObject<BatchChanges<TRow>>();

    /// <summary>Exports the grid's rows to an Excel file, and waits until the file is written.</summary>
    public void ExcelExport() => MethodAsync();

    /// <summary>Exports the grid's rows to a CSV file, and waits until the file is written.</summary>
    public void CsvExport() => MethodAsync();

    /// <summary>Exports the grid's rows to a PDF file, and waits until the file is written.</summary>
    public void PdfExport() => MethodAsync();

    /// <summary>Prints the grid.</summary>
    public void Print() => Method();

    /// <summary>Gets the event raised when the grid pages, sorts, filters, searches or groups rows the server holds.</summary>
    public Event<DataStateChangeEventArgs> DataStateChange => Event<DataStateChangeEventArgs>();

    /// <summary>Gets the event raised when a person clicks an item of the grid's toolbar.</summary>
    public Event<ClickEventArgs> ToolbarClick => Event<ClickEventArgs>();

    /// <summary>Gets the event raised when a person clicks a row.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The event.</returns>
    public Event<RecordClickEventArgs<TRow>> RecordClick<TRow>() => Event<RecordClickEventArgs<TRow>>();

    /// <summary>Gets the event raised once a row is selected.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The event.</returns>
    public Event<RowSelectEventArgs<TRow>> RowSelected<TRow>() => Event<RowSelectEventArgs<TRow>>();

    /// <summary>Gets the event raised as each action of the grid begins, such as a sort or a save.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The event.</returns>
    public Event<ActionEventArgs<TRow>> ActionBegin<TRow>() => Event<ActionEventArgs<TRow>>();

    /// <summary>Gets the event raised as each action of the grid completes.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The event.</returns>
    public Event<ActionEventArgs<TRow>> ActionComplete<TRow>() => Event<ActionEventArgs<TRow>>();

    /// <summary>Gets the event raised before a row opens for editing.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The event.</returns>
    public Event<BeginEditArgs<TRow>> BeginEdit<TRow>() => Event<BeginEditArgs<TRow>>();

    /// <summary>Gets the event raised before an edited cell is saved, in the grid's batch edit mode.</summary>
    /// <remarks>Write both type arguments, the row class and the type of the cell's member.</remarks>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <typeparam name="TValue">The type of the cell's member.</typeparam>
    /// <returns>The event.</returns>
    public Event<CellSaveArgs<TRow, TValue>> CellSave<TRow, TValue>() => Event<CellSaveArgs<TRow, TValue>>();

    /// <summary>Gets the event raised once an edited cell is saved, in the grid's batch edit mode.</summary>
    /// <remarks>Write both type arguments, the row class and the type of the cell's member.</remarks>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <typeparam name="TValue">The type of the cell's member.</typeparam>
    /// <returns>The event.</returns>
    public Event<CellSaveArgs<TRow, TValue>> CellSaved<TRow, TValue>() => Event<CellSaveArgs<TRow, TValue>>();

    /// <summary>Gets the event raised before the grid's batch edit is saved.</summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The event.</returns>
    public Event<BeforeBatchSaveArgs<TRow>> BeforeBatchSave<TRow>() => Event<BeforeBatchSaveArgs<TRow>>();

    /// <summary>
    /// The direction of a sort, written by its name.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<SortDirection>))]
    public enum SortDirection
    {
        /// <summary>Sorts from the lowest value to the highest.</summary>
        [JsonStringEnumMemberName("Ascending")]
        Ascending,

        /// <summary>Sorts from the highest value to the lowest.</summary>
        [JsonStringEnumMemberName("Descending")]
        Descending,
    }

    /// <summary>
    /// How <see cref="FilterByColumn"/> compares a row's text with the filter's text, written by its name.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<FilterOperator>))]
    public enum FilterOperator
    {
        /// <summary>Keeps a row whose text holds the filter's text.</summary>
        [JsonStringEnumMemberName("contains")]
        Contains,

        /// <summary>Keeps a row whose text is the filter's text.</summary>
        [JsonStringEnumMemberName("equal")]
        Equal,

        /// <summary>Keeps a row whose text is not the filter's text.</summary>
        [JsonStringEnumMemberName("notequal")]
        NotEqual,

        /// <summary>Keeps a row whose text starts with the filter's text.</summary>
        [JsonStringEnumMemberName("startswith")]
        StartsWith,

        /// <summary>Keeps a row whose text ends with the filter's text.</summary>
        [JsonStringEnumMemberName("endswith")]
        EndsWith,
    }

    /// <summary>
    /// A page of rows the server answers, each member named as EJ2 names it.
    /// </summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    [Mirrors("@syncfusion/ej2-grids", "DataResult")]
    public sealed class DataResult<TRow>
    {
        /// <summary>Gets or sets the rows of the page.</summary>
        [JsonPropertyName("result")]
        public required IReadOnlyList<TRow> Result { get; set; }

        /// <summary>Gets or sets the count of every row the server holds.</summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    /// <summary>
    /// A column of the grid, which <see cref="GetColumnByField"/> reads.
    /// </summary>
    [Mirrors("@syncfusion/ej2-grids", "Column")]
    public sealed class Column : JsObject
    {
        /// <summary>Gets the text the column's header shows.</summary>
        /// <remarks>Call <see cref="RefreshHeader"/> after a write, so the header shows the text.</remarks>
        public Property<string> HeaderText => Property<string>();

        /// <summary>Gets whether the column shows.</summary>
        public Property<bool> Visible => Property<bool>();
    }

    /// <summary>
    /// The rows the grid's batch edit adds, changes and deletes, which <see cref="GetBatchChanges{TRow}"/> reads and
    /// <see cref="BeforeBatchSaveArgs{TRow}"/> holds.
    /// </summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    [Mirrors("@syncfusion/ej2-grids", "BatchChanges")]
    public sealed class BatchChanges<TRow> : JsObject
    {
        /// <summary>Gets the rows the batch adds.</summary>
        public ReadOnly<IReadOnlyList<TRow>> AddedRecords => ReadOnly<IReadOnlyList<TRow>>(optional: true);

        /// <summary>Gets the rows the batch changes.</summary>
        public ReadOnly<IReadOnlyList<TRow>> ChangedRecords => ReadOnly<IReadOnlyList<TRow>>(optional: true);

        /// <summary>Gets the rows the batch deletes.</summary>
        public ReadOnly<IReadOnlyList<TRow>> DeletedRecords => ReadOnly<IReadOnlyList<TRow>>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="DataStateChange"/> receives: the state of the grid the server answers with a page.
    /// </summary>
    [Mirrors("@syncfusion/ej2-grids", "DataStateChangeEventArgs")]
    public sealed class DataStateChangeEventArgs : JsObject
    {
        /// <summary>Gets the event's name, <c>dataStateChange</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the count of rows the page skips.</summary>
        public ReadOnly<int> Skip => ReadOnly<int>(optional: true);

        /// <summary>Gets the count of rows the page takes.</summary>
        public ReadOnly<int> Take => ReadOnly<int>(optional: true);

        /// <summary>Gets whether the grid asks for the count of every row.</summary>
        public ReadOnly<bool> RequiresCounts => ReadOnly<bool>(optional: true);

        /// <summary>Gets the columns the rows are sorted by.</summary>
        public ReadOnly<IReadOnlyList<Sorts>> Sorted => ReadOnly<IReadOnlyList<Sorts>>(optional: true);

        /// <summary>Gets the fields the rows are grouped by.</summary>
        public ReadOnly<IReadOnlyList<string>> Group => ReadOnly<IReadOnlyList<string>>(optional: true);

        /// <summary>Gets the filters the rows meet.</summary>
        public ReadOnly<IReadOnlyList<Predicate>> Where => ReadOnly<IReadOnlyList<Predicate>>(optional: true);

        /// <summary>Gets the searches the rows meet.</summary>
        public ReadOnly<IReadOnlyList<SearchSettingsModel>> Search => ReadOnly<IReadOnlyList<SearchSettingsModel>>(optional: true);

        /// <summary>Gets the action that changes the state, such as a sort.</summary>
        public GridActionEventArgs Action => LiveObject<GridActionEventArgs>(optional: true);
    }

    /// <summary>
    /// The action <see cref="DataStateChangeEventArgs"/> holds: a page, sort, filter, search or grouping.
    /// </summary>
    [Mirrors("@syncfusion/ej2-grids", "GridActionEventArgs")]
    public sealed class GridActionEventArgs : JsObject
    {
        /// <summary>Gets the name of the event EJ2 last raised with the action.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the name of the action, such as sorting or paging.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>(optional: true);

        /// <summary>Gets the kind of the action's event.</summary>
        public ReadOnly<string> Type => ReadOnly<string>(optional: true);

        /// <summary>Gets the field of the column a sort or a grouping is for.</summary>
        public ReadOnly<string> ColumnName => ReadOnly<string>(optional: true);

        /// <summary>Gets the direction of a sort.</summary>
        public ReadOnly<SortDirection> Direction => ReadOnly<SortDirection>(optional: true);
    }

    /// <summary>
    /// A column the rows are sorted by, which <see cref="DataStateChangeEventArgs"/> hands, each member named as EJ2 names it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-grids", "Sorts")]
    public sealed class Sorts
    {
        /// <summary>Gets or sets the column's field.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>Gets or sets the direction of the sort.</summary>
        [JsonPropertyName("direction")]
        public string? Direction { get; set; }
    }

    /// <summary>
    /// A filter the rows meet, which <see cref="DataStateChangeEventArgs"/> hands, each member named as EJ2 names it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-data", "Predicate")]
    public sealed class Predicate
    {
        /// <summary>Gets or sets the field of the column the filter is for.</summary>
        [JsonPropertyName("field")]
        public string Field { get; set; } = "";

        /// <summary>Gets or sets how a row's value is compared with the filter's.</summary>
        [JsonPropertyName("operator")]
        public string Operator { get; set; } = "";

        /// <summary>Gets or sets how the filter joins the filters it holds, "and" or "or".</summary>
        [JsonPropertyName("condition")]
        public string Condition { get; set; } = "";

        /// <summary>Gets or sets the filters this filter joins.</summary>
        [JsonPropertyName("predicates")]
        public IReadOnlyList<Predicate> Predicates { get; set; } = [];
    }

    /// <summary>
    /// A search the rows meet, which <see cref="DataStateChangeEventArgs"/> hands, each member named as EJ2 names it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-grids", "SearchSettingsModel")]
    public sealed class SearchSettingsModel
    {
        /// <summary>Gets or sets the fields of the columns the search reads.</summary>
        [JsonPropertyName("fields")]
        public IReadOnlyList<string>? Fields { get; set; }

        /// <summary>Gets or sets the text the search looks for.</summary>
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        /// <summary>Gets or sets how a row's value is compared with the text.</summary>
        [JsonPropertyName("operator")]
        public string? Operator { get; set; }
    }

    /// <summary>
    /// What a listener of <see cref="ToolbarClick"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "ClickEventArgs")]
    public sealed class ClickEventArgs : JsObject
    {
        /// <summary>Gets the event's name, <c>toolbarClick</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the item the person clicked, read with <c>Read</c>.</summary>
        public ReadOnly<ItemModel> Item => ReadOnly<ItemModel>();

        /// <summary>Gets whether the grid's own action for the item stops; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }

    /// <summary>
    /// An item of the grid's toolbar, as EJ2 holds it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "ItemModel")]
    public sealed class ItemModel
    {
        /// <summary>Gets or sets the item's id.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the item's text.</summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    /// <summary>
    /// What a listener of <see cref="RecordClick{TRow}"/> receives.
    /// </summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    [Mirrors("@syncfusion/ej2-grids", "RecordClickEventArgs")]
    public sealed class RecordClickEventArgs<TRow> : JsObject
    {
        /// <summary>Gets the event's name, <c>recordClick</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the row the person clicked.</summary>
        public ReadOnly<TRow> RowData => ReadOnly<TRow>(optional: true);

        /// <summary>Gets the position of the row, from 0.</summary>
        public ReadOnly<int> RowIndex => ReadOnly<int>(optional: true);

        /// <summary>Gets the position of the cell the person clicked in its row, from 0.</summary>
        public ReadOnly<int> CellIndex => ReadOnly<int>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="RowSelected{TRow}"/> receives.
    /// </summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    [Mirrors("@syncfusion/ej2-grids", "RowSelectEventArgs")]
    public sealed class RowSelectEventArgs<TRow> : JsObject
    {
        /// <summary>Gets the event's name, <c>rowSelected</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the selected row.</summary>
        public ReadOnly<TRow> Data => ReadOnly<TRow>(optional: true);

        /// <summary>Gets the position of the selected row, from 0.</summary>
        public ReadOnly<int> RowIndex => ReadOnly<int>(optional: true);

        /// <summary>Gets the position of the row selected before it, which reads nothing on the first selection.</summary>
        public ReadOnly<int> PreviousRowIndex => ReadOnly<int>(optional: true);

        /// <summary>Gets whether a person selected the row, rather than a call.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="ActionBegin{TRow}"/> or <see cref="ActionComplete{TRow}"/> receives.
    /// </summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    [Mirrors("@syncfusion/ej2-grids", "ActionEventArgs")]
    public sealed class ActionEventArgs<TRow> : JsObject
    {
        /// <summary>Gets the event's name, <c>actionBegin</c> or <c>actionComplete</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the name of the action, such as sorting or save.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>(optional: true);

        /// <summary>Gets the kind of the action's event.</summary>
        public ReadOnly<string> Type => ReadOnly<string>(optional: true);

        /// <summary>Gets what an edit action does to its row, such as add or edit.</summary>
        public ReadOnly<string> Action => ReadOnly<string>(optional: true);

        /// <summary>Gets whether the action stops; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets the row the action saves.</summary>
        public ReadOnly<TRow> Data => ReadOnly<TRow>(optional: true);

        /// <summary>Gets the row as it was before the action.</summary>
        public ReadOnly<TRow> PreviousData => ReadOnly<TRow>(optional: true);

        /// <summary>Gets the position of the action's row, from 0.</summary>
        public ReadOnly<int> RowIndex => ReadOnly<int>(optional: true);

        /// <summary>Gets the position of the selected row, from 0.</summary>
        public ReadOnly<int> SelectedRow => ReadOnly<int>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="BeginEdit{TRow}"/> receives.
    /// </summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    [Mirrors("@syncfusion/ej2-grids", "BeginEditArgs")]
    public sealed class BeginEditArgs<TRow> : JsObject
    {
        /// <summary>Gets the event's name, <c>beginEdit</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the row that opens for editing.</summary>
        public ReadOnly<TRow> RowData => ReadOnly<TRow>(optional: true);

        /// <summary>Gets the position of the row, from 0.</summary>
        public ReadOnly<int> RowIndex => ReadOnly<int>(optional: true);

        /// <summary>Gets the kind of the event.</summary>
        public ReadOnly<string> Type => ReadOnly<string>(optional: true);

        /// <summary>Gets whether the row stays closed; write true to keep it closed.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="CellSave{TRow, TValue}"/> or <see cref="CellSaved{TRow, TValue}"/> receives.
    /// </summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <typeparam name="TValue">The type of the cell's member.</typeparam>
    [Mirrors("@syncfusion/ej2-grids", "CellSaveArgs")]
    public sealed class CellSaveArgs<TRow, TValue> : JsObject
    {
        /// <summary>Gets the event's name, <c>cellSave</c> or <c>cellSaved</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the row the cell belongs to.</summary>
        public ReadOnly<TRow> RowData => ReadOnly<TRow>(optional: true);

        /// <summary>Gets the field of the cell's column.</summary>
        public ReadOnly<string> ColumnName => ReadOnly<string>(optional: true);

        /// <summary>Gets the value the cell saves.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>(optional: true);

        /// <summary>Gets the value the cell held before the edit.</summary>
        public ReadOnly<TValue> PreviousValue => ReadOnly<TValue>(optional: true);

        /// <summary>Gets whether the save stops; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="BeforeBatchSave{TRow}"/> receives.
    /// </summary>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    [Mirrors("@syncfusion/ej2-grids", "BeforeBatchSaveArgs")]
    public sealed class BeforeBatchSaveArgs<TRow> : JsObject
    {
        /// <summary>Gets the event's name, <c>beforeBatchSave</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the rows the batch adds, changes and deletes.</summary>
        public BatchChanges<TRow> BatchChanges => LiveObject<BatchChanges<TRow>>(optional: true);

        /// <summary>Gets whether the save stops; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }
}
