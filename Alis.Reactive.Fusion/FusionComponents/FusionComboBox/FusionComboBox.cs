using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A combo box: EJ2's ComboBox, a list a person chooses an item from or types into, which holds the value of the
/// item chosen.
/// </summary>
/// <remarks>
/// A box bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person chooses an item of the list or the value changes.
/// </remarks>
/// <typeparam name="TValue">The type of the member the box binds, which admits null, since the box holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-dropdowns", "ComboBox")]
public sealed class FusionComboBox<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the value the box holds, null until a person chooses an item.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets the text the box shows for the item chosen, null until a choice.</summary>
    public Property<string?> Text => Property<string?>();

    /// <summary>Gets the position of the item chosen in the list, null until a choice.</summary>
    public Property<int?> Index => Property<int?>();

    /// <summary>Replaces the items offered by the box with a local list.</summary>
    /// <typeparam name="TItem">The type of an item in the list.</typeparam>
    /// <returns>The property that accepts the list.</returns>
    public WriteOnly<IReadOnlyList<TItem>> DataSource<TItem>() => WriteOnly<IReadOnlyList<TItem>>();

    /// <summary>Gets the item field settings for the box.</summary>
    public FieldSettings Fields => LiveObject<FieldSettings>();

    /// <summary>Gets the DataManager configured as this box's data source.</summary>
    /// <remarks>Use this member on a box bound to a DataManager.</remarks>
    public DataManager ManagedDataSource => LiveObject<DataManager>(name: "dataSource");

    /// <summary>Gets the box's live query, which holds null until a query is configured.</summary>
    public DataQuery Query => LiveObject<DataQuery>(nullable: true);

    /// <summary>Applies the changes written to the box's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Moves the keyboard's focus out of the box, if the box holds it.</summary>
    public void FocusOut() => Method();

    /// <summary>Opens the box's list.</summary>
    public void ShowPopup() => Method();

    /// <summary>Closes the box's list.</summary>
    public void HidePopup() => Method();

    /// <summary>Shows the loading indicator on the box's dropdown arrow.</summary>
    public void ShowSpinner() => Method();

    /// <summary>Removes the box's loading indicator.</summary>
    public void HideSpinner() => Method();

    /// <summary>Clears the box's value.</summary>
    public void Clear() => Method();

    /// <summary>Gets the event raised when a person chooses an item of the list, or when the box's value changes.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    /// <summary>Gets the event raised when the box takes the keyboard's focus.</summary>
    /// <remarks>Its listener receives no member a view reads.</remarks>
    public Event Focus => Event();

    /// <summary>Gets the event raised when the keyboard's focus leaves the box.</summary>
    /// <remarks>Its listener receives no member a view reads.</remarks>
    public Event Blur => Event();

    /// <summary>Gets the event raised as a person types a search in the box.</summary>
    public Event<FilteringEventArgs> Filtering => Event<FilteringEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>The item members the box uses for its option labels and group headings.</summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "FieldSettingsModel")]
    public sealed class FieldSettings : JsObject
    {
        /// <summary>Gets the name of the member the box shows as an option's label.</summary>
        public Property<string?> Text => Property<string?>(optional: true);

        /// <summary>Gets the name of the member the box groups its options by.</summary>
        public Property<string?> GroupBy => Property<string?>(optional: true);
    }

    /// <summary>The EJ2 DataManager that supplies this box's rows.</summary>
    [Mirrors("@syncfusion/ej2-data", "DataManager")]
    public sealed class DataManager : JsObject
    {
        /// <summary>Runs a live EJ2 Query against this manager and waits for its rows.</summary>
        /// <param name="query">A source of the EJ2 Query to execute.</param>
        /// <typeparam name="TRow">The type of each row returned by the query's adaptor.</typeparam>
        /// <returns>The settled response for an adaptor that returns an array of rows.</returns>
        public MethodResult<QueryResult<TRow>> ExecuteQuery<TRow>(TypedSource<DataQuery> query)
            => MethodAsync<QueryResult<TRow>>([(Arg<DataQuery>)query]);
    }

    /// <summary>The EJ2 Query configured on this box.</summary>
    [Mirrors("@syncfusion/ej2-data", "Query")]
    public sealed class DataQuery : JsObject
    {
        /// <summary>Executes the query against its configured DataManager and waits for its rows.</summary>
        /// <typeparam name="TRow">The type of each row returned by the query's adaptor.</typeparam>
        /// <returns>The settled response for an adaptor that returns an array of rows.</returns>
        public MethodResult<QueryResult<TRow>> Execute<TRow>() => MethodAsync<QueryResult<TRow>>();
    }

    /// <summary>The result of a configured query whose adaptor returns an array of rows.</summary>
    /// <typeparam name="TRow">The type of each row.</typeparam>
    public sealed class QueryResult<TRow>
    {
        /// <summary>Gets or sets the rows the query returned.</summary>
        [JsonPropertyName("result")]
        public required IReadOnlyList<TRow> Result { get; set; }
    }

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the value the box holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>Gets the catalogue row selected by the change, read with <c>Read</c>.</summary>
        /// <typeparam name="TItem">The type of the catalogue rows.</typeparam>
        /// <returns>The selected row, or null when the selection was cleared.</returns>
        public ReadOnly<TItem?> ItemData<TItem>()
            where TItem : class
            => ReadOnly<TItem?>();

        /// <summary>Gets the catalogue row selected before the change, read with <c>Read</c>.</summary>
        /// <typeparam name="TItem">The type of the catalogue rows.</typeparam>
        /// <returns>The previous row, or null when no row was selected before the change.</returns>
        public ReadOnly<TItem?> PreviousItemData<TItem>()
            where TItem : class
            => ReadOnly<TItem?>();

        /// <summary>Gets the selected list item. Its members read nothing when the change has no list item.</summary>
        public HTMLLIElement Item => LiveObject<HTMLLIElement>(nullable: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>What a listener of <see cref="Filtering"/> receives.</summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "FilteringEventArgs")]
    public sealed class FilteringEventArgs : JsObject
    {
        /// <summary>Gets the search text entered in the box.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Gets whether the box skips its local filter. Set this before requesting matches.</summary>
        public Property<bool> PreventDefaultAction => Property<bool>();

        /// <summary>Shows the matching items in the box.</summary>
        /// <typeparam name="TItem">The type of the items in the reply.</typeparam>
        /// <param name="dataSource">The matching items.</param>
        public void UpdateData<TItem>(Arg<IReadOnlyList<TItem>> dataSource) => Method([dataSource]);
    }

    /// <summary>The selected list item a catalogue change hands its listener.</summary>
    [Mirrors("lib.dom", "HTMLLIElement")]
    public sealed class HTMLLIElement : JsObject
    {
        /// <summary>Gets the text of the selected list item.</summary>
        public ReadOnly<string> TextContent => ReadOnly<string>();
    }
}
