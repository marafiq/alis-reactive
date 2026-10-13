using System;
using System.Collections.Generic;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Kanban;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Kanban, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionKanban(plan, "care-kanban", b =&gt; ...)</c> and target it with
    /// <c>var kanban = p.Component&lt;FusionKanban&gt;("care-kanban")</c>; each write or call is its own statement.
    /// Card members and events are typed by your card class:
    /// <code>
    /// kanban.AddCard&lt;CareCard&gt;(json.Get(x =&gt; x.Card), 0);
    /// .Reactive(evt =&gt; evt.CardClick&lt;CareCard&gt;(), (args, p) =&gt; ...)
    /// </code>
    /// </remarks>
    public sealed class FusionKanban : FusionComponent
    {
        /// <summary>The cards; call <see cref="DataBind"/> after writing them.</summary>
        public Prop<object> DataSource => Prop<object>();

        /// <summary>The cards, typed by your card class.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public Prop<TCard[]> Cards<TCard>() where TCard : class => Prop<TCard[]>("dataSource");

        /// <summary>The cards in the column with this key.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        /// <param name="columnKey">The column's key, for example <c>"Open"</c>.</param>
        public TypedSource<TCard[]> GetColumnData<TCard>(Arg<string> columnKey) where TCard : class => Invoke<TCard[]>(columnKey);

        /// <summary>The cards in the swimlane with this key.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        /// <param name="keyField">The swimlane's key, for example <c>"AL"</c>.</param>
        public TypedSource<TCard[]> GetSwimlaneData<TCard>(Arg<string> keyField) where TCard : class => Invoke<TCard[]>(keyField);

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Adds a card at a zero-based index.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        /// <param name="cardData">The card to add.</param>
        /// <param name="index">The zero-based index the card takes in its column.</param>
        public void AddCard<TCard>(Arg<TCard> cardData, Arg<int> index) where TCard : class => Call(cardData, index);

        /// <summary>Replaces the card with the same key.</summary>
        /// <param name="cardData">The changed card.</param>
        public void UpdateCard(Arg<object> cardData) => Call(cardData);

        /// <summary>Deletes a card, given the card or its key.</summary>
        /// <param name="cardData">The card to delete, or its key.</param>
        public void DeleteCard(Arg<object> cardData) => Call(cardData);

        /// <summary>Adds a column at a zero-based index.</summary>
        /// <param name="columnOptions">The column to add.</param>
        /// <param name="index">The zero-based index the column takes.</param>
        public void AddColumn(Arg<FusionKanbanColumn> columnOptions, Arg<int> index) => Call(columnOptions, index);

        /// <summary>Deletes the column at a zero-based index.</summary>
        /// <param name="index">The zero-based index of the column.</param>
        public void DeleteColumn(Arg<int> index) => Call(index);

        /// <summary>Shows the column with this key.</summary>
        /// <param name="key">The column's key, for example <c>"Done"</c>.</param>
        public void ShowColumn(Arg<string> key) => Call(key);

        /// <summary>Hides the column with this key.</summary>
        /// <param name="key">The column's key, for example <c>"Done"</c>.</param>
        public void HideColumn(Arg<string> key) => Call(key);

        /// <summary>Shows the loading spinner.</summary>
        public void ShowSpinner() => Call();

        /// <summary>Hides the loading spinner.</summary>
        public void HideSpinner() => Call();

        /// <summary>Opens the card dialog: <c>"Add"</c> or <c>"Edit"</c>, for a card.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        /// <param name="action"><c>"Add"</c> to create the card or <c>"Edit"</c> to change it.</param>
        /// <param name="data">The card the dialog shows.</param>
        public void OpenDialog<TCard>(Arg<string> action, Arg<TCard> data) where TCard : class => Call(action, data);

        /// <summary>Closes the card dialog.</summary>
        public void CloseDialog() => Call();

        /// <summary>Raised before a card action runs.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanActionArgs<TCard>> ActionBegin<TCard>() where TCard : class, new() =>
            Event<FusionKanbanActionArgs<TCard>>();

        /// <summary>Raised after a card action completes.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanActionArgs<TCard>> ActionComplete<TCard>() where TCard : class, new() =>
            Event<FusionKanbanActionArgs<TCard>>();

        /// <summary>Raised before the board binds its cards.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanDataBindingArgs<TCard>> DataBinding<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDataBindingArgs<TCard>>();

        /// <summary>Raised after the board binds its cards.</summary>
        public TypedEvent<FusionKanbanEmptyArgs> DataBound =>
            Event<FusionKanbanEmptyArgs>();

        /// <summary>Raised when the user clicks a card.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanCardClickArgs<TCard>> CardClick<TCard>() where TCard : class, new() =>
            Event<FusionKanbanCardClickArgs<TCard>>();

        /// <summary>Raised when the user double-clicks a card.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanCardClickArgs<TCard>> CardDoubleClick<TCard>() where TCard : class, new() =>
            Event<FusionKanbanCardClickArgs<TCard>>();

        /// <summary>Raised before each column header renders.</summary>
        public TypedEvent<FusionKanbanQueryCellInfoArgs> QueryCellInfo =>
            Event<FusionKanbanQueryCellInfoArgs>();

        /// <summary>Raised after each card renders.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanCardRenderedArgs<TCard>> CardRendered<TCard>() where TCard : class, new() =>
            Event<FusionKanbanCardRenderedArgs<TCard>>();

        /// <summary>Raised when the user starts dragging a card.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanDragArgs<TCard>> DragStart<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDragArgs<TCard>>();

        /// <summary>Raised as the user drags a card.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanDragArgs<TCard>> Drag<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDragArgs<TCard>>();

        /// <summary>Raised when the user drops a card.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanDragArgs<TCard>> DragStop<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDragArgs<TCard>>();

        /// <summary>Raised before the card dialog opens.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanDialogArgs<TCard>> DialogOpen<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDialogArgs<TCard>>();

        /// <summary>Raised before the card dialog closes.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanDialogArgs<TCard>> DialogClose<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDialogArgs<TCard>>();

        /// <summary>Raised when the user changes cards; commit them, then call <c>EndEdit</c>.</summary>
        /// <typeparam name="TCard">Your card class.</typeparam>
        public TypedEvent<FusionKanbanDataSourceChangedArgs<TCard>> DataSourceChanged<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDataSourceChangedArgs<TCard>>();
    }

    /// <summary>Payload of an event that carries no data.</summary>
    public class FusionKanbanEmptyArgs : JsObject
    {
    }

    /// <summary>Payload of <c>ActionBegin</c> and <c>ActionComplete</c>.</summary>
    /// <typeparam name="TCard">Your card class.</typeparam>
    public class FusionKanbanActionArgs<TCard> : JsObject
        where TCard : class, new()
    {
        /// <summary>The kind of action.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>Cards the action adds.</summary>
        public ReadOnly<List<TCard>> AddedRecords => ReadOnly<List<TCard>>();

        /// <summary>Cards the action changes.</summary>
        public ReadOnly<List<TCard>> ChangedRecords => ReadOnly<List<TCard>>();

        /// <summary>Cards the action deletes.</summary>
        public ReadOnly<List<TCard>> DeletedRecords => ReadOnly<List<TCard>>();
    }

    /// <summary>Payload of <c>DataBinding</c>.</summary>
    /// <typeparam name="TCard">Your card class.</typeparam>
    public class FusionKanbanDataBindingArgs<TCard> : JsObject
        where TCard : class, new()
    {
        /// <summary>The cards about to bind.</summary>
        public ReadOnly<List<TCard>> Result => ReadOnly<List<TCard>>();

        /// <summary>The number of cards.</summary>
        public ReadOnly<int?> Count => ReadOnly<int?>();
    }

    /// <summary>Payload of <c>CardClick</c> and <c>CardDoubleClick</c>.</summary>
    /// <typeparam name="TCard">Your card class.</typeparam>
    public class FusionKanbanCardClickArgs<TCard> : JsObject
        where TCard : class, new()
    {
        /// <summary>The clicked card.</summary>
        public ReadOnly<TCard> Data => ReadOnly<TCard>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <c>CardRendered</c>.</summary>
    /// <typeparam name="TCard">Your card class.</typeparam>
    public class FusionKanbanCardRenderedArgs<TCard> : JsObject
        where TCard : class, new()
    {
        /// <summary>The rendered card.</summary>
        public ReadOnly<TCard> Data => ReadOnly<TCard>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <c>DialogOpen</c> and <c>DialogClose</c>.</summary>
    /// <typeparam name="TCard">Your card class.</typeparam>
    public class FusionKanbanDialogArgs<TCard> : JsObject
        where TCard : class, new()
    {
        /// <summary>The card in the dialog.</summary>
        public ReadOnly<TCard> Data => ReadOnly<TCard>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary><c>"Add"</c> or <c>"Edit"</c>.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();
    }

    /// <summary>Payload of <c>DragStart</c>, <c>Drag</c> and <c>DragStop</c>.</summary>
    /// <typeparam name="TCard">Your card class.</typeparam>
    public class FusionKanbanDragArgs<TCard> : JsObject
        where TCard : class, new()
    {
        /// <summary>The dragged cards.</summary>
        public ReadOnly<List<TCard>> Data => ReadOnly<List<TCard>>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>Zero-based index where the cards drop.</summary>
        public ReadOnly<int> DropIndex => ReadOnly<int>();
    }

    /// <summary>A column header as <see cref="FusionKanbanQueryCellInfoArgs"/> reports it.</summary>
    public class FusionKanbanHeaderArgs
    {
        /// <summary>The column key.</summary>
        public string KeyField { get; set; } = "";

        /// <summary>The column text.</summary>
        public string TextField { get; set; } = "";

        /// <summary>The number of cards in the column.</summary>
        public int Count { get; set; }
    }

    /// <summary>Payload of <see cref="FusionKanban.QueryCellInfo"/>.</summary>
    public class FusionKanbanQueryCellInfoArgs : JsObject
    {
        /// <summary>The column headers.</summary>
        public ReadOnly<List<FusionKanbanHeaderArgs>> Data => ReadOnly<List<FusionKanbanHeaderArgs>>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>The kind of render.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();
    }

    /// <summary>Payload of <c>DataSourceChanged</c>.</summary>
    /// <remarks>Commit the change, then finish it: <c>args.EndEdit()</c>.</remarks>
    /// <typeparam name="TCard">Your card class.</typeparam>
    public class FusionKanbanDataSourceChangedArgs<TCard> : JsObject
        where TCard : class, new()
    {
        /// <summary>The kind of change.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();

        /// <summary>Cards the user added.</summary>
        public ReadOnly<List<TCard>> AddedRecords => ReadOnly<List<TCard>>();

        /// <summary>Cards the user changed.</summary>
        public ReadOnly<List<TCard>> ChangedRecords => ReadOnly<List<TCard>>();

        /// <summary>Cards the user deleted.</summary>
        public ReadOnly<List<TCard>> DeletedRecords => ReadOnly<List<TCard>>();

        /// <summary>Zero-based index of the change.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();

        /// <summary>Applies the change to the board.</summary>
        public void EndEdit() => Call();

        /// <summary>Reverts the change.</summary>
        public void CancelEdit() => Call();
    }

    /// <summary>A column for <see cref="FusionKanban.AddColumn"/>.</summary>
    public class FusionKanbanColumn
    {
        /// <summary>The column header.</summary>
        public string HeaderText { get; set; } = "";

        /// <summary>The column key.</summary>
        public string KeyField { get; set; } = "";

        /// <summary>Whether the user can collapse the column.</summary>
        public bool AllowToggle { get; set; }

        /// <summary>Whether the column starts expanded.</summary>
        public bool IsExpanded { get; set; } = true;
    }

    /// <summary>
    /// Factory extension for rendering Syncfusion Kanban with reactive event support.
    /// Static board setup stays on Syncfusion's MVC builder.
    /// </summary>
    public static class FusionKanbanHtmlExtensions
    {
        /// <summary>
        /// Renders a Syncfusion Kanban with the element ID the plan targets it by.
        /// </summary>
        /// <typeparam name="TModel">The view model the plan is authored for.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">The plan the board's behavior is authored in.</param>
        /// <param name="elementId">The element ID, shared by the markup and <c>p.Component&lt;FusionKanban&gt;(elementId)</c>.</param>
        /// <param name="build">Configures columns, cards, swimlanes and the rest on the Syncfusion builder.</param>
        /// <returns>The rendered board; wire its events with <c>.Reactive(evt =&gt; evt.CardClick&lt;CareCard&gt;(), ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionKanban, TModel> FusionKanban<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<KanbanBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Kanban(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionKanban>(elementId, builder.Render());
        }
    }
}
