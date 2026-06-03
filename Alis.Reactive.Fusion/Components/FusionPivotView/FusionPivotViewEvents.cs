namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed PivotView events available to Reactive Plans.
    /// </summary>
    public sealed class FusionPivotViewEvents
    {
        public static readonly FusionPivotViewEvents Instance = new FusionPivotViewEvents();
        private FusionPivotViewEvents() { }

        public TypedEvent<FusionPivotViewDataBoundArgs> DataBound =>
            new TypedEvent<FusionPivotViewDataBoundArgs>(
                "dataBound", new FusionPivotViewDataBoundArgs());

        public TypedEvent<FusionPivotViewCellSelectingArgs> CellSelecting =>
            new TypedEvent<FusionPivotViewCellSelectingArgs>(
                "cellSelecting", new FusionPivotViewCellSelectingArgs());
    }
}
