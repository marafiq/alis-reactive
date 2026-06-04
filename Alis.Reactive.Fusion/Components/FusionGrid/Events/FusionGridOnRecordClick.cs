namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the Syncfusion Grid "recordClick" event.
    /// </summary>
    /// <typeparam name="TRow">The row DTO type bound to the grid.</typeparam>
    public class FusionGridRecordClickArgs<TRow>
        where TRow : class
    {
        /// <summary>Current row data.</summary>
        public TRow RowData { get; set; } = default!;

        /// <summary>Zero-based row index.</summary>
        public int RowIndex { get; set; }

        /// <summary>Zero-based cell index.</summary>
        public int CellIndex { get; set; }

        /// <summary>Syncfusion event name.</summary>
        public string? Name { get; set; }

        public FusionGridRecordClickArgs() { }
    }
}
