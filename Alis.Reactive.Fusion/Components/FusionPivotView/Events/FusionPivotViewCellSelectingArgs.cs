namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for PivotView cellSelecting. This event is emitted during normal
    /// late event subscription, which matches reactive runtime wiring.
    /// </summary>
    public sealed class FusionPivotViewCellSelectingArgs
    {
        public FusionPivotViewCellData Data { get; set; } = new FusionPivotViewCellData();
    }

    /// <summary>
    /// Pivot cell data exposed by Syncfusion inside cellSelecting args.
    /// </summary>
    public sealed class FusionPivotViewCellData
    {
        public string Axis { get; set; } = "";
        public string ActualText { get; set; } = "";
        public string FormattedText { get; set; } = "";
        public decimal Value { get; set; }
        public decimal ActualValue { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public string RowHeaders { get; set; } = "";
        public string ColumnHeaders { get; set; } = "";
        public bool IsSum { get; set; }
        public bool IsGrandSum { get; set; }
    }
}
