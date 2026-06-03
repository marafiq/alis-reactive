namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed Kanban events available to Reactive Plans.
    /// </summary>
    public sealed class FusionKanbanEvents
    {
        public static readonly FusionKanbanEvents Instance = new FusionKanbanEvents();
        private FusionKanbanEvents() { }

        public TypedEvent<FusionKanbanActionArgs<TCard>> ActionBegin<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanActionArgs<TCard>>(
                "actionBegin", new FusionKanbanActionArgs<TCard>());

        public TypedEvent<FusionKanbanActionArgs<TCard>> ActionComplete<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanActionArgs<TCard>>(
                "actionComplete", new FusionKanbanActionArgs<TCard>());

        public TypedEvent<FusionKanbanDataBindingArgs<TCard>> DataBinding<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanDataBindingArgs<TCard>>(
                "dataBinding", new FusionKanbanDataBindingArgs<TCard>());

        public TypedEvent<FusionKanbanEmptyArgs> DataBound =>
            new TypedEvent<FusionKanbanEmptyArgs>(
                "dataBound", new FusionKanbanEmptyArgs());

        public TypedEvent<FusionKanbanCardClickArgs<TCard>> CardClick<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanCardClickArgs<TCard>>(
                "cardClick", new FusionKanbanCardClickArgs<TCard>());

        public TypedEvent<FusionKanbanCardClickArgs<TCard>> CardDoubleClick<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanCardClickArgs<TCard>>(
                "cardDoubleClick", new FusionKanbanCardClickArgs<TCard>());

        public TypedEvent<FusionKanbanQueryCellInfoArgs> QueryCellInfo =>
            new TypedEvent<FusionKanbanQueryCellInfoArgs>(
                "queryCellInfo", new FusionKanbanQueryCellInfoArgs());

        public TypedEvent<FusionKanbanCardRenderedArgs<TCard>> CardRendered<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanCardRenderedArgs<TCard>>(
                "cardRendered", new FusionKanbanCardRenderedArgs<TCard>());

        public TypedEvent<FusionKanbanDragArgs<TCard>> DragStart<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanDragArgs<TCard>>(
                "dragStart", new FusionKanbanDragArgs<TCard>());

        public TypedEvent<FusionKanbanDragArgs<TCard>> Drag<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanDragArgs<TCard>>(
                "drag", new FusionKanbanDragArgs<TCard>());

        public TypedEvent<FusionKanbanDragArgs<TCard>> DragStop<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanDragArgs<TCard>>(
                "dragStop", new FusionKanbanDragArgs<TCard>());

        public TypedEvent<FusionKanbanDialogArgs<TCard>> DialogOpen<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanDialogArgs<TCard>>(
                "dialogOpen", new FusionKanbanDialogArgs<TCard>());

        public TypedEvent<FusionKanbanDialogArgs<TCard>> DialogClose<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanDialogArgs<TCard>>(
                "dialogClose", new FusionKanbanDialogArgs<TCard>());

        public TypedEvent<FusionKanbanDataSourceChangedArgs<TCard>> DataSourceChanged<TCard>()
            where TCard : class, new()
            => new TypedEvent<FusionKanbanDataSourceChangedArgs<TCard>>(
                "dataSourceChanged", new FusionKanbanDataSourceChangedArgs<TCard>());
    }
}
