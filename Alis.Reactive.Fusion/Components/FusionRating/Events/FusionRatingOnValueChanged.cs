namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionRating"/> value changes.
    /// </summary>
    public class FusionRatingValueChangedArgs
    {
        /// <summary>Gets or sets the new rating value.</summary>
        public double Value { get; set; }

        /// <summary>Gets or sets the previous rating value.</summary>
        public double PreviousValue { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates an event payload instance for descriptor wiring.
        /// </summary>
        public FusionRatingValueChangedArgs() { }
    }
}
