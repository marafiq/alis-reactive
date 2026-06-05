namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionRating"/> value changes.
    /// </summary>
    public class FusionRatingValueChangedArgs
    {
        /// <summary>New rating value.</summary>
        public double Value { get; set; }

        /// <summary>Previous rating value.</summary>
        public double PreviousValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
