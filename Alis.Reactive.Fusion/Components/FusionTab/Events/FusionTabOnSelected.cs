namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for the Fusion Tab <c>selected</c> event.
    /// </summary>
    public class FusionTabSelectedArgs
    {
        /// <summary>The zero-based index of the newly selected tab.</summary>
        public int SelectedIndex { get; set; }

        /// <summary>The zero-based index of the previously selected tab.</summary>
        public int PreviousIndex { get; set; }

        /// <summary>Whether the selection was triggered by a swipe gesture.</summary>
        public bool IsSwiped { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FusionTabSelectedArgs"/> class.
        /// </summary>
        public FusionTabSelectedArgs() { }
    }
}
