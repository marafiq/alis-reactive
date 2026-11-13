using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A pivot view: EJ2's PivotView, which summarizes rows of data in a table, a chart or both.
/// </summary>
/// <remarks>
/// The view it shows is <see cref="DisplayOptionModel.Primary"/> while <see cref="DisplayOptionModel.View"/> is
/// <c>Both</c>, and <see cref="DisplayOptionModel.View"/> otherwise. Write the rows to
/// <see cref="DataSourceSettingsModel.DataSource{TRow}"/>, then call <see cref="Refresh"/>, so the pivot shows them.
/// </remarks>
[Mirrors("@syncfusion/ej2-pivotview", "PivotView")]
public sealed class FusionPivotView : FusionComponent
{
    /// <summary>Gets the rows the pivot summarizes and how it summarizes them.</summary>
    public DataSourceSettingsModel DataSourceSettings => LiveObject<DataSourceSettingsModel>();

    /// <summary>Gets whether the pivot shows a table, a chart or both, and which of them shows first.</summary>
    public DisplayOptionModel DisplayOption => LiveObject<DisplayOptionModel>();

    /// <summary>Renders the pivot again from its rows and settings.</summary>
    public void Refresh() => Method();

    /// <summary>Reads the pivot's layout as JSON text, which <see cref="LoadPersistData"/> restores.</summary>
    /// <param name="isRemoveDatasource">True to leave the rows out of the text.</param>
    /// <returns>The layout as JSON text.</returns>
    public MethodResult<string> GetPersistData(Arg<bool> isRemoveDatasource) => Method<string>([isRemoveDatasource]);

    /// <summary>Restores a layout that <see cref="GetPersistData"/> read.</summary>
    /// <param name="persistData">The layout as JSON text.</param>
    public void LoadPersistData(Arg<string> persistData) => Method([persistData]);

    /// <summary>Opens the dialog in which a person sets the pivot's conditional formatting.</summary>
    public void ShowConditionalFormattingDialog() => Method();

    /// <summary>Opens the dialog in which a person creates or edits a calculated field.</summary>
    public void CreateCalculatedFieldDialog() => Method();

    /// <summary>Gets the event raised when the pivot has rendered its table.</summary>
    public Event DataBound => Event();

    /// <summary>Gets the event raised before a cell of the pivot's table is selected.</summary>
    public Event<PivotCellSelectedEventArgs> CellSelecting => Event<PivotCellSelectedEventArgs>();

    /// <summary>
    /// The pivot's data source settings: the rows it summarizes.
    /// </summary>
    [Mirrors("@syncfusion/ej2-pivotview", "DataSourceSettingsModel")]
    public sealed class DataSourceSettingsModel : JsObject
    {
        /// <summary>Gets the rows the pivot summarizes.</summary>
        /// <remarks>Call <see cref="Refresh"/> after a write, so the pivot shows the rows.</remarks>
        /// <typeparam name="TRow">The class of the rows.</typeparam>
        /// <returns>The rows.</returns>
        public Property<IReadOnlyList<TRow>> DataSource<TRow>() => Property<IReadOnlyList<TRow>>(optional: true);
    }

    /// <summary>
    /// The pivot's display options: whether it shows a table, a chart or both.
    /// </summary>
    [Mirrors("@syncfusion/ej2-pivotview", "DisplayOptionModel")]
    public sealed class DisplayOptionModel : JsObject
    {
        /// <summary>Gets what the pivot shows: <c>Table</c>, <c>Chart</c> or <c>Both</c>.</summary>
        public ReadOnly<string> View => ReadOnly<string>(optional: true);

        /// <summary>Gets what the pivot shows first while <see cref="View"/> is <c>Both</c>: <c>Table</c> or <c>Chart</c>; its toolbar switches it.</summary>
        public ReadOnly<string> Primary => ReadOnly<string>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="CellSelecting"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-pivotview", "PivotCellSelectedEventArgs")]
    public sealed class PivotCellSelectedEventArgs : JsObject
    {
        /// <summary>Gets the cell the person selects.</summary>
        public ReadOnly<IAxisSet> Data => ReadOnly<IAxisSet>(optional: true);
    }

    /// <summary>
    /// A cell of the pivot's table, as EJ2 holds it, each member named as EJ2 names it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-pivotview", "IAxisSet")]
    public sealed class IAxisSet
    {
        /// <summary>Gets or sets the axis the cell belongs to.</summary>
        [JsonPropertyName("axis")]
        public string? Axis { get; set; }

        /// <summary>Gets or sets the text the cell shows.</summary>
        [JsonPropertyName("formattedText")]
        public string? FormattedText { get; set; }

        /// <summary>Gets or sets the cell's value.</summary>
        [JsonPropertyName("value")]
        public double? Value { get; set; }

        /// <summary>Gets or sets the cell's value before the pivot formats it.</summary>
        [JsonPropertyName("actualValue")]
        public double? ActualValue { get; set; }

        /// <summary>Gets or sets the position of the cell's row, from 0.</summary>
        [JsonPropertyName("rowIndex")]
        public int? RowIndex { get; set; }

        /// <summary>Gets or sets the position of the cell's column, from 0.</summary>
        [JsonPropertyName("colIndex")]
        public int? ColIndex { get; set; }

        /// <summary>Gets or sets whether the cell holds a subtotal.</summary>
        [JsonPropertyName("isSum")]
        public bool? IsSum { get; set; }

        /// <summary>Gets or sets whether the cell holds a grand total.</summary>
        [JsonPropertyName("isGrandSum")]
        public bool? IsGrandSum { get; set; }
    }
}
