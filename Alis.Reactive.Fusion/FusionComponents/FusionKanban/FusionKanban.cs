using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A Kanban board: EJ2's Kanban, cards in columns by the value of their key member.
/// </summary>
/// <remarks>
/// A member that takes or hands cards is typed by the view's card class, <c>TCard</c>, and one that takes or hands a
/// column's key by the type of the cards' key member, <c>TKey</c>, text or a number. An event's payload is read with
/// <c>Read</c>, and a handler writes true to its <c>Cancel</c> to stop the action.
/// </remarks>
[Mirrors("@syncfusion/ej2-kanban", "Kanban")]
public sealed class FusionKanban : FusionComponent
{
    /// <summary>Gets the cards the board shows.</summary>
    /// <remarks>Call <see cref="DataBind"/> after a write, so the board shows the cards.</remarks>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The cards.</returns>
    public Property<IReadOnlyList<TCard>> DataSource<TCard>() => Property<IReadOnlyList<TCard>>();

    /// <summary>Reads the cards of a column.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <typeparam name="TKey">The type of the cards' key member.</typeparam>
    /// <param name="columnKey">The column's key.</param>
    /// <returns>The column's cards.</returns>
    public MethodResult<IReadOnlyList<TCard>> GetColumnData<TCard, TKey>(Arg<TKey> columnKey) => Method<IReadOnlyList<TCard>>([columnKey]);

    /// <summary>Reads the cards of a swimlane.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <param name="keyField">The swimlane's key.</param>
    /// <returns>The swimlane's cards.</returns>
    public MethodResult<IReadOnlyList<TCard>> GetSwimlaneData<TCard>(Arg<string> keyField) => Method<IReadOnlyList<TCard>>([keyField]);

    /// <summary>Applies the changes written to the board's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Adds a card to the board at a position.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <param name="cardData">The card.</param>
    /// <param name="index">The position the card takes.</param>
    public void AddCard<TCard>(Arg<TCard> cardData, Arg<int> index) where TCard : class => Method([cardData, index]);

    /// <summary>Replaces a card of the board with the card of the same key.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <param name="cardData">The card.</param>
    public void UpdateCard<TCard>(Arg<TCard> cardData) where TCard : class => Method([cardData]);

    /// <summary>Deletes cards from the board.</summary>
    /// <remarks>Takes a card, a list of cards or a card's key, text or a number, which the board finds by its key member.</remarks>
    /// <typeparam name="TCard">A card's class, a list of cards, or the type of a card's key.</typeparam>
    /// <param name="cardData">The card, the cards or the key.</param>
    public void DeleteCard<TCard>(Arg<TCard> cardData) => Method([cardData]);

    /// <summary>Adds a column to the board at a position.</summary>
    /// <typeparam name="TKey">The type of the cards' key member.</typeparam>
    /// <param name="columnOptions">The column.</param>
    /// <param name="index">The position the column takes.</param>
    public void AddColumn<TKey>(Arg<ColumnsModel<TKey>> columnOptions, Arg<int> index) => Method([columnOptions, index]);

    /// <summary>Deletes the column at a position.</summary>
    /// <param name="index">The column's position.</param>
    public void DeleteColumn(Arg<int> index) => Method([index]);

    /// <summary>Shows a hidden column.</summary>
    /// <typeparam name="TKey">The type of the cards' key member.</typeparam>
    /// <param name="key">The column's key.</param>
    public void ShowColumn<TKey>(Arg<TKey> key) => Method([key]);

    /// <summary>Hides a column.</summary>
    /// <typeparam name="TKey">The type of the cards' key member.</typeparam>
    /// <param name="key">The column's key.</param>
    public void HideColumn<TKey>(Arg<TKey> key) => Method([key]);

    /// <summary>Shows the board's spinner.</summary>
    public void ShowSpinner() => Method();

    /// <summary>Hides the spinner <see cref="ShowSpinner"/> showed.</summary>
    public void HideSpinner() => Method();

    /// <summary>Opens the board's card dialog for an action on a card.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <param name="action">The action the dialog opens for.</param>
    /// <param name="data">The card the dialog shows.</param>
    public void OpenDialog<TCard>(Arg<CurrentAction> action, Arg<TCard> data) => Method([action, data]);

    /// <summary>Closes the board's card dialog.</summary>
    public void CloseDialog() => Method();

    /// <summary>Gets the event raised as each action of the board begins.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<ActionEventArgs<TCard>> ActionBegin<TCard>() => Event<ActionEventArgs<TCard>>();

    /// <summary>Gets the event raised as each action of the board completes.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<ActionEventArgs<TCard>> ActionComplete<TCard>() => Event<ActionEventArgs<TCard>>();

    /// <summary>Gets the event raised before the cards bind to the board.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<ReturnType<TCard>> DataBinding<TCard>() => Event<ReturnType<TCard>>();

    /// <summary>Gets the event raised once the cards are bound to the board.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<ReturnType<TCard>> DataBound<TCard>() => Event<ReturnType<TCard>>();

    /// <summary>Gets the event raised when a person clicks a card.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<CardClickEventArgs<TCard>> CardClick<TCard>() => Event<CardClickEventArgs<TCard>>();

    /// <summary>Gets the event raised when a person double-clicks a card.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<CardClickEventArgs<TCard>> CardDoubleClick<TCard>() => Event<CardClickEventArgs<TCard>>();

    /// <summary>Gets the event raised before each column of the board renders.</summary>
    /// <typeparam name="TKey">The type of the cards' key member.</typeparam>
    /// <returns>The event.</returns>
    public Event<QueryCellInfoEventArgs<TKey>> QueryCellInfo<TKey>() => Event<QueryCellInfoEventArgs<TKey>>();

    /// <summary>Gets the event raised before each card of the board renders.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<CardRenderedEventArgs<TCard>> CardRendered<TCard>() => Event<CardRenderedEventArgs<TCard>>();

    /// <summary>Gets the event raised when a person starts dragging a card.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<DragEventArgs<TCard>> DragStart<TCard>() => Event<DragEventArgs<TCard>>();

    /// <summary>Gets the event raised as a person drags a card over another column or swimlane.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<DragEventArgs<TCard>> Drag<TCard>() => Event<DragEventArgs<TCard>>();

    /// <summary>Gets the event raised when a person drops a card.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<DragEventArgs<TCard>> DragStop<TCard>() => Event<DragEventArgs<TCard>>();

    /// <summary>Gets the event raised before the card dialog opens.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<DialogEventArgs<TCard>> DialogOpen<TCard>() => Event<DialogEventArgs<TCard>>();

    /// <summary>Gets the event raised before the card dialog closes.</summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<DialogEventArgs<TCard>> DialogClose<TCard>() => Event<DialogEventArgs<TCard>>();

    /// <summary>Gets the event raised when a card is added, changed or deleted, before the board shows the change.</summary>
    /// <remarks>A handler calls <c>EndEdit</c> once the app keeps the change, or <c>CancelEdit</c> to drop it.</remarks>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    /// <returns>The event.</returns>
    public Event<DataSourceChangedEventArgs<TCard>> DataSourceChanged<TCard>() => Event<DataSourceChangedEventArgs<TCard>>();

    /// <summary>
    /// The action the board's card dialog opens for, written by its name.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<CurrentAction>))]
    public enum CurrentAction
    {
        /// <summary>Adds a card.</summary>
        [JsonStringEnumMemberName("Add")]
        Add,

        /// <summary>Edits a card.</summary>
        [JsonStringEnumMemberName("Edit")]
        Edit,

        /// <summary>Deletes a card.</summary>
        [JsonStringEnumMemberName("Delete")]
        Delete,
    }

    /// <summary>
    /// What a listener of <see cref="ActionBegin{TCard}"/> or <see cref="ActionComplete{TCard}"/> receives.
    /// </summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "ActionEventArgs")]
    public sealed class ActionEventArgs<TCard> : JsObject
    {
        /// <summary>Gets the name of the action.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();

        /// <summary>Gets whether the action stops; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets the cards the action adds.</summary>
        public ReadOnly<IReadOnlyList<TCard>> AddedRecords => ReadOnly<IReadOnlyList<TCard>>(optional: true);

        /// <summary>Gets the cards the action changes.</summary>
        public ReadOnly<IReadOnlyList<TCard>> ChangedRecords => ReadOnly<IReadOnlyList<TCard>>(optional: true);

        /// <summary>Gets the cards the action deletes.</summary>
        public ReadOnly<IReadOnlyList<TCard>> DeletedRecords => ReadOnly<IReadOnlyList<TCard>>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="DataBinding{TCard}"/> or <see cref="DataBound{TCard}"/> receives.
    /// </summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "ReturnType")]
    public sealed class ReturnType<TCard> : JsObject
    {
        /// <summary>Gets the cards that bind to the board.</summary>
        public ReadOnly<IReadOnlyList<TCard>> Result => ReadOnly<IReadOnlyList<TCard>>();

        /// <summary>Gets the count of the cards.</summary>
        public ReadOnly<int> Count => ReadOnly<int>();
    }

    /// <summary>
    /// What a listener of <see cref="CardClick{TCard}"/> or <see cref="CardDoubleClick{TCard}"/> receives.
    /// </summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "CardClickEventArgs")]
    public sealed class CardClickEventArgs<TCard> : JsObject
    {
        /// <summary>Gets the card the person clicked.</summary>
        public ReadOnly<TCard> Data => ReadOnly<TCard>();

        /// <summary>Gets whether the click is stopped; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="QueryCellInfo{TKey}"/> receives.
    /// </summary>
    /// <typeparam name="TKey">The type of the cards' key member.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "QueryCellInfoEventArgs")]
    public sealed class QueryCellInfoEventArgs<TKey> : JsObject
    {
        /// <summary>Gets the headers of the column that renders.</summary>
        public ReadOnly<IReadOnlyList<HeaderArgs<TKey>>> Data => ReadOnly<IReadOnlyList<HeaderArgs<TKey>>>(optional: true);

        /// <summary>Gets whether the column renders; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>();

        /// <summary>Gets the name of the part that renders.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();
    }

    /// <summary>
    /// What a listener of <see cref="CardRendered{TCard}"/> receives.
    /// </summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "CardRenderedEventArgs")]
    public sealed class CardRenderedEventArgs<TCard> : JsObject
    {
        /// <summary>Gets the card that renders.</summary>
        public ReadOnly<TCard> Data => ReadOnly<TCard>(optional: true);

        /// <summary>Gets whether the card renders; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="DragStart{TCard}"/>, <see cref="Drag{TCard}"/> or <see cref="DragStop{TCard}"/>
    /// receives.
    /// </summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "DragEventArgs")]
    public sealed class DragEventArgs<TCard> : JsObject
    {
        /// <summary>Gets the cards the person drags.</summary>
        public ReadOnly<IReadOnlyList<TCard>> Data => ReadOnly<IReadOnlyList<TCard>>();

        /// <summary>Gets whether the drag stops; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets the position the cards drop at.</summary>
        public ReadOnly<int> DropIndex => ReadOnly<int>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="DialogOpen{TCard}"/> or <see cref="DialogClose{TCard}"/> receives.
    /// </summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "DialogEventArgs")]
    public sealed class DialogEventArgs<TCard> : JsObject
    {
        /// <summary>Gets the card the dialog shows.</summary>
        public ReadOnly<TCard> Data => ReadOnly<TCard>();

        /// <summary>Gets whether the dialog stays as it is; write true to keep it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets the action the dialog is for.</summary>
        public ReadOnly<CurrentAction> RequestType => ReadOnly<CurrentAction>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="DataSourceChanged{TCard}"/> receives.
    /// </summary>
    /// <typeparam name="TCard">The class of the cards.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "DataSourceChangedEventArgs")]
    public sealed class DataSourceChangedEventArgs<TCard> : JsObject
    {
        /// <summary>Gets the name of the change.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>(optional: true);

        /// <summary>Gets the cards the change adds.</summary>
        public ReadOnly<IReadOnlyList<TCard>> AddedRecords => ReadOnly<IReadOnlyList<TCard>>(optional: true);

        /// <summary>Gets the cards the change changes.</summary>
        public ReadOnly<IReadOnlyList<TCard>> ChangedRecords => ReadOnly<IReadOnlyList<TCard>>(optional: true);

        /// <summary>Gets the cards the change deletes.</summary>
        public ReadOnly<IReadOnlyList<TCard>> DeletedRecords => ReadOnly<IReadOnlyList<TCard>>(optional: true);

        /// <summary>Gets the position of the change.</summary>
        public ReadOnly<int> Index => ReadOnly<int>(optional: true);

        /// <summary>Shows the change on the board, once the app keeps it.</summary>
        public void EndEdit() => Method(optional: true);

        /// <summary>Drops the change, so the board shows the cards as they were.</summary>
        public void CancelEdit() => Method(optional: true);
    }

    /// <summary>
    /// A column the view adds to the board, each member named as EJ2 names it.
    /// </summary>
    /// <typeparam name="TKey">The type of the cards' key member.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "ColumnsModel")]
    public sealed class ColumnsModel<TKey>
    {
        /// <summary>Gets or sets the column's key, the value of the key member of the cards it holds.</summary>
        [JsonPropertyName("keyField")]
        public required TKey KeyField { get; set; }

        /// <summary>Gets or sets the text the column's header shows.</summary>
        [JsonPropertyName("headerText")]
        public string HeaderText { get; set; } = "";

        /// <summary>Gets or sets whether a person can collapse and expand the column.</summary>
        [JsonPropertyName("allowToggle")]
        public bool AllowToggle { get; set; }

        /// <summary>Gets or sets whether the column shows expanded.</summary>
        [JsonPropertyName("isExpanded")]
        public bool IsExpanded { get; set; } = true;
    }

    /// <summary>
    /// A header of a column, which <see cref="QueryCellInfo{TKey}"/> hands, each member named as EJ2 names it.
    /// </summary>
    /// <typeparam name="TKey">The type of the cards' key member.</typeparam>
    [Mirrors("@syncfusion/ej2-kanban", "HeaderArgs")]
    public sealed class HeaderArgs<TKey>
    {
        /// <summary>Gets or sets the column's key, as the app gave it.</summary>
        [JsonPropertyName("keyField")]
        public required TKey KeyField { get; set; }

        /// <summary>Gets or sets the text the header shows.</summary>
        [JsonPropertyName("textField")]
        public string TextField { get; set; } = "";

        /// <summary>Gets or sets the count of the column's cards.</summary>
        [JsonPropertyName("count")]
        public int? Count { get; set; }
    }
}
