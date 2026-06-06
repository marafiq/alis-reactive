namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the Syncfusion Grid "recordClick" event.
    /// </summary>
    /// <typeparam name="TRow">Grid row DTO type.</typeparam>
    public class FusionGridRecordClickArgs<TRow>
        where TRow : class
    {
        /// <summary>Row data for the clicked record.</summary>
        public TRow RowData { get; set; } = default!;

        /// <summary>Zero-based row index.</summary>
        public int RowIndex { get; set; }

        /// <summary>Zero-based cell index.</summary>
        public int CellIndex { get; set; }

        /// <summary>Syncfusion event name.</summary>
        public string? Name { get; set; }
    }
}
