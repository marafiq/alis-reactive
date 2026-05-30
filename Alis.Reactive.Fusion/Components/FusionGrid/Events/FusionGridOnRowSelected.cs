namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the SF Grid "rowSelected" event.
    /// </summary>
    /// <typeparam name="TRow">The row DTO type bound to the grid.</typeparam>
    public class FusionGridRowSelectedArgs<TRow>
        where TRow : class
    {
        /// <summary>Selected row data.</summary>
        public TRow Data { get; set; } = default!;

        /// <summary>Selected row index.</summary>
        public int RowIndex { get; set; }

        /// <summary>Previously selected row index.</summary>
        public int PreviousRowIndex { get; set; }

        /// <summary>Whether selection came from user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionGridRowSelectedArgs() { }
    }
}
