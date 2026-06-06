namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the Syncfusion Grid "rowSelected" event.
    /// </summary>
    /// <typeparam name="TRow">Grid row DTO type.</typeparam>
    public class FusionGridRowSelectedArgs<TRow>
        where TRow : class
    {
        /// <summary>Row data carried by the selected row event.</summary>
        public TRow Data { get; set; } = default!;

        /// <summary>Zero-based index of the row after selection.</summary>
        public int RowIndex { get; set; }

        /// <summary>Zero-based index of the previously selected row.</summary>
        public int PreviousRowIndex { get; set; }

        /// <summary>Whether user interaction triggered the row selection.</summary>
        public bool IsInteracted { get; set; }
    }
}
