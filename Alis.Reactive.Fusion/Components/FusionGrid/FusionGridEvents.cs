namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionGrid"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(evt =&gt; evt.DataStateChange, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionGridEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionGridEvents Instance = new FusionGridEvents();
        private FusionGridEvents() { }

        /// <summary>
        /// Fires when the grid needs data (sort, page, filter).
        /// SF "dataStateChange" event in custom binding mode.
        /// </summary>
        public TypedEvent<FusionGridDataStateChangeArgs> DataStateChange =>
            new TypedEvent<FusionGridDataStateChangeArgs>(
                "dataStateChange", new FusionGridDataStateChangeArgs());

        /// <summary>
        /// Fires when a data row is clicked.
        /// </summary>
        /// <typeparam name="TRow">The row DTO type bound to the grid.</typeparam>
        public TypedEvent<FusionGridRecordClickArgs<TRow>> RecordClick<TRow>()
            where TRow : class
            => new TypedEvent<FusionGridRecordClickArgs<TRow>>(
                "recordClick", new FusionGridRecordClickArgs<TRow>());

        /// <summary>
        /// Fires when a data row is selected.
        /// </summary>
        /// <typeparam name="TRow">The row DTO type bound to the grid.</typeparam>
        public TypedEvent<FusionGridRowSelectedArgs<TRow>> RowSelected<TRow>()
            where TRow : class
            => new TypedEvent<FusionGridRowSelectedArgs<TRow>>(
                "rowSelected", new FusionGridRowSelectedArgs<TRow>());

        /// <summary>
        /// Fires before grid actions such as edit, save, delete, sort, page, filter, and group.
        /// </summary>
        public TypedEvent<FusionGridEditActionArgs<TRow>> ActionBegin<TRow>()
            where TRow : class
            => new TypedEvent<FusionGridEditActionArgs<TRow>>(
                "actionBegin", new FusionGridEditActionArgs<TRow>());

        /// <summary>
        /// Fires after grid actions such as edit, save, delete, sort, page, filter, and group.
        /// </summary>
        public TypedEvent<FusionGridEditActionArgs<TRow>> ActionComplete<TRow>()
            where TRow : class
            => new TypedEvent<FusionGridEditActionArgs<TRow>>(
                "actionComplete", new FusionGridEditActionArgs<TRow>());

        /// <summary>
        /// Fires before a row enters edit mode.
        /// </summary>
        public TypedEvent<FusionGridBeginEditArgs<TRow>> BeginEdit<TRow>()
            where TRow : class
            => new TypedEvent<FusionGridBeginEditArgs<TRow>>(
                "beginEdit", new FusionGridBeginEditArgs<TRow>());

        /// <summary>
        /// Fires when a batch-edit cell is being saved.
        /// </summary>
        public TypedEvent<FusionGridCellSaveArgs<TRow, TValue>> CellSave<TRow, TValue>()
            where TRow : class
            => new TypedEvent<FusionGridCellSaveArgs<TRow, TValue>>(
                "cellSave", new FusionGridCellSaveArgs<TRow, TValue>());

        /// <summary>
        /// Fires after a batch-edit cell is saved.
        /// </summary>
        public TypedEvent<FusionGridCellSaveArgs<TRow, TValue>> CellSaved<TRow, TValue>()
            where TRow : class
            => new TypedEvent<FusionGridCellSaveArgs<TRow, TValue>>(
                "cellSaved", new FusionGridCellSaveArgs<TRow, TValue>());

        /// <summary>
        /// Fires before pending batch changes are committed.
        /// </summary>
        public TypedEvent<FusionGridBeforeBatchSaveArgs<TRow>> BeforeBatchSave<TRow>()
            where TRow : class
            => new TypedEvent<FusionGridBeforeBatchSaveArgs<TRow>>(
                "beforeBatchSave", new FusionGridBeforeBatchSaveArgs<TRow>());
    }
}
