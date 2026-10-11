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
    /// <c>p.Component&lt;FusionKanban&gt;("care-kanban")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. Card members and events are typed by your card class:
    /// <c>.Reactive(evt =&gt; evt.CardClick&lt;CareCard&gt;(), ...)</c>,
    /// <c>.Call(c =&gt; c.AddCard&lt;CareCard&gt;(), json.Read(x =&gt; x.Card), 0)</c>.
    /// </remarks>
    public sealed class FusionKanban : FusionComponent
    {
        /// <summary>The cards; call <see cref="DataBind"/> after writing them.</summary>
        public ComponentProperty<object> DataSource => Property<object>("dataSource");

        /// <summary>The cards, typed by your card class.</summary>
        public ComponentProperty<TCard[]> Cards<TCard>() where TCard : class =>
            Property<TCard[]>("dataSource");

        /// <summary>The cards in the column with this key.</summary>
        public ComponentFunction<string, TCard[]> GetColumnData<TCard>() where TCard : class =>
            Function<string, TCard[]>("getColumnData");

        /// <summary>The cards in the swimlane with this key.</summary>
        public ComponentFunction<string, TCard[]> GetSwimlaneData<TCard>() where TCard : class =>
            Function<string, TCard[]>("getSwimlaneData");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Adds a card at a zero-based index.</summary>
        public ComponentMethod<TCard, int> AddCard<TCard>() where TCard : class =>
            Method<TCard, int>("addCard");

        /// <summary>Replaces the card with the same key.</summary>
        public ComponentMethod<object> UpdateCard => Method<object>("updateCard");

        /// <summary>Deletes a card, given the card or its key.</summary>
        public ComponentMethod<object> DeleteCard => Method<object>("deleteCard");

        /// <summary>Adds a column at a zero-based index.</summary>
        public ComponentMethod<FusionKanbanColumn, int> AddColumn => Method<FusionKanbanColumn, int>("addColumn");

        /// <summary>Deletes the column at a zero-based index.</summary>
        public ComponentMethod<int> DeleteColumn => Method<int>("deleteColumn");

        /// <summary>Shows the column with this key.</summary>
        public ComponentMethod<string> ShowColumn => Method<string>("showColumn");

        /// <summary>Hides the column with this key.</summary>
        public ComponentMethod<string> HideColumn => Method<string>("hideColumn");

        /// <summary>Shows the loading spinner.</summary>
        public ComponentMethod ShowSpinner => Method("showSpinner");

        /// <summary>Hides the loading spinner.</summary>
        public ComponentMethod HideSpinner => Method("hideSpinner");

        /// <summary>Opens the card dialog: <c>"Add"</c> or <c>"Edit"</c>, for a card.</summary>
        public ComponentMethod<string, TCard> OpenDialog<TCard>() where TCard : class =>
            Method<string, TCard>("openDialog");

        /// <summary>Closes the card dialog.</summary>
        public ComponentMethod CloseDialog => Method("closeDialog");

        /// <summary>Raised before a card action runs.</summary>
        public TypedEvent<FusionKanbanActionArgs<TCard>> ActionBegin<TCard>() where TCard : class, new() =>
            Event<FusionKanbanActionArgs<TCard>>("actionBegin");

        /// <summary>Raised after a card action completes.</summary>
        public TypedEvent<FusionKanbanActionArgs<TCard>> ActionComplete<TCard>() where TCard : class, new() =>
            Event<FusionKanbanActionArgs<TCard>>("actionComplete");

        /// <summary>Raised before the board binds its cards.</summary>
        public TypedEvent<FusionKanbanDataBindingArgs<TCard>> DataBinding<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDataBindingArgs<TCard>>("dataBinding");

        /// <summary>Raised after the board binds its cards.</summary>
        public TypedEvent<FusionKanbanEmptyArgs> DataBound =>
            Event<FusionKanbanEmptyArgs>("dataBound");

        /// <summary>Raised when the user clicks a card.</summary>
        public TypedEvent<FusionKanbanCardClickArgs<TCard>> CardClick<TCard>() where TCard : class, new() =>
            Event<FusionKanbanCardClickArgs<TCard>>("cardClick");

        /// <summary>Raised when the user double-clicks a card.</summary>
        public TypedEvent<FusionKanbanCardClickArgs<TCard>> CardDoubleClick<TCard>() where TCard : class, new() =>
            Event<FusionKanbanCardClickArgs<TCard>>("cardDoubleClick");

        /// <summary>Raised before each column header renders.</summary>
        public TypedEvent<FusionKanbanQueryCellInfoArgs> QueryCellInfo =>
            Event<FusionKanbanQueryCellInfoArgs>("queryCellInfo");

        /// <summary>Raised after each card renders.</summary>
        public TypedEvent<FusionKanbanCardRenderedArgs<TCard>> CardRendered<TCard>() where TCard : class, new() =>
            Event<FusionKanbanCardRenderedArgs<TCard>>("cardRendered");

        /// <summary>Raised when the user starts dragging a card.</summary>
        public TypedEvent<FusionKanbanDragArgs<TCard>> DragStart<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDragArgs<TCard>>("dragStart");

        /// <summary>Raised as the user drags a card.</summary>
        public TypedEvent<FusionKanbanDragArgs<TCard>> Drag<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDragArgs<TCard>>("drag");

        /// <summary>Raised when the user drops a card.</summary>
        public TypedEvent<FusionKanbanDragArgs<TCard>> DragStop<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDragArgs<TCard>>("dragStop");

        /// <summary>Raised before the card dialog opens.</summary>
        public TypedEvent<FusionKanbanDialogArgs<TCard>> DialogOpen<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDialogArgs<TCard>>("dialogOpen");

        /// <summary>Raised before the card dialog closes.</summary>
        public TypedEvent<FusionKanbanDialogArgs<TCard>> DialogClose<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDialogArgs<TCard>>("dialogClose");

        /// <summary>Raised when the user changes cards; commit them, then call <c>EndEdit</c>.</summary>
        public TypedEvent<FusionKanbanDataSourceChangedArgs<TCard>> DataSourceChanged<TCard>() where TCard : class, new() =>
            Event<FusionKanbanDataSourceChangedArgs<TCard>>("dataSourceChanged");
    }

    /// <summary>Payload of an event that carries no data.</summary>
    public class FusionKanbanEmptyArgs
    {
    }

    /// <summary>Payload of <c>ActionBegin</c> and <c>ActionComplete</c>.</summary>
    public class FusionKanbanActionArgs<TCard>
        where TCard : class, new()
    {
        /// <summary>The kind of action.</summary>
        public string RequestType { get; set; } = "";

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>Cards the action adds.</summary>
        public List<TCard> AddedRecords { get; set; } = new List<TCard>();

        /// <summary>Cards the action changes.</summary>
        public List<TCard> ChangedRecords { get; set; } = new List<TCard>();

        /// <summary>Cards the action deletes.</summary>
        public List<TCard> DeletedRecords { get; set; } = new List<TCard>();
    }

    /// <summary>Payload of <c>DataBinding</c>.</summary>
    public class FusionKanbanDataBindingArgs<TCard>
        where TCard : class, new()
    {
        /// <summary>The cards about to bind.</summary>
        public List<TCard> Result { get; set; } = new List<TCard>();

        /// <summary>The number of cards.</summary>
        public int? Count { get; set; }
    }

    /// <summary>Payload of <c>CardClick</c> and <c>CardDoubleClick</c>.</summary>
    public class FusionKanbanCardClickArgs<TCard>
        where TCard : class, new()
    {
        /// <summary>The clicked card.</summary>
        public TCard Data { get; set; } = new TCard();

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <c>CardRendered</c>.</summary>
    public class FusionKanbanCardRenderedArgs<TCard>
        where TCard : class, new()
    {
        /// <summary>The rendered card.</summary>
        public TCard Data { get; set; } = new TCard();

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <c>DialogOpen</c> and <c>DialogClose</c>.</summary>
    public class FusionKanbanDialogArgs<TCard>
        where TCard : class, new()
    {
        /// <summary>The card in the dialog.</summary>
        public TCard Data { get; set; } = new TCard();

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary><c>"Add"</c> or <c>"Edit"</c>.</summary>
        public string RequestType { get; set; } = "";
    }

    /// <summary>Payload of <c>DragStart</c>, <c>Drag</c> and <c>DragStop</c>.</summary>
    public class FusionKanbanDragArgs<TCard>
        where TCard : class, new()
    {
        /// <summary>The dragged cards.</summary>
        public List<TCard> Data { get; set; } = new List<TCard>();

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>Zero-based index where the cards drop.</summary>
        public int DropIndex { get; set; }
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
    public class FusionKanbanQueryCellInfoArgs
    {
        /// <summary>The column headers.</summary>
        public List<FusionKanbanHeaderArgs> Data { get; set; } = new List<FusionKanbanHeaderArgs>();

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>The kind of render.</summary>
        public string RequestType { get; set; } = "";
    }

    /// <summary>Payload of <c>DataSourceChanged</c>.</summary>
    /// <remarks>Commit the change, then finish it: <c>s.Payload(args).Call(a =&gt; a.EndEdit)</c>.</remarks>
    public class FusionKanbanDataSourceChangedArgs<TCard> : JsObject
        where TCard : class, new()
    {
        /// <summary>The kind of change.</summary>
        public string RequestType { get; set; } = "";

        /// <summary>Cards the user added.</summary>
        public List<TCard> AddedRecords { get; set; } = new List<TCard>();

        /// <summary>Cards the user changed.</summary>
        public List<TCard> ChangedRecords { get; set; } = new List<TCard>();

        /// <summary>Cards the user deleted.</summary>
        public List<TCard> DeletedRecords { get; set; } = new List<TCard>();

        /// <summary>Zero-based index of the change.</summary>
        public int Index { get; set; }

        /// <summary>Applies the change to the board.</summary>
        public ComponentMethod EndEdit => Method("endEdit");

        /// <summary>Reverts the change.</summary>
        public ComponentMethod CancelEdit => Method("cancelEdit");
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
