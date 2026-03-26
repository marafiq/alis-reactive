namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the SF Tab "selected" event.
    /// Fires when a tab is selected.
    ///
    /// Properties are primitives — safe for echo and conditions.
    /// SF args also contain selectedItem/previousItem (DOM objects) — intentionally omitted
    /// as they would serialize as [object Object].
    /// </summary>
    public class FusionTabSelectedArgs
    {
        /// <summary>The zero-based index of the newly selected tab.</summary>
        public int SelectedIndex { get; set; }

        /// <summary>The zero-based index of the previously selected tab.</summary>
        public int PreviousIndex { get; set; }

        /// <summary>Whether the selection was triggered by a swipe gesture.</summary>
        public bool IsSwiped { get; set; }

        public FusionTabSelectedArgs() { }
    }
}
