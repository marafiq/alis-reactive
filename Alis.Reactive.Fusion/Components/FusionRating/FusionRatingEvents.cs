namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionRating"/> component.
    /// </summary>
    public sealed class FusionRatingEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionRatingEvents Instance = new FusionRatingEvents();
        private FusionRatingEvents() { }

        /// <summary>Fires when the rating value changes (SF "valueChanged" event).</summary>
        public TypedEvent<FusionRatingValueChangedArgs> ValueChanged =>
            new TypedEvent<FusionRatingValueChangedArgs>(
                "valueChanged", new FusionRatingValueChangedArgs());
    }
}
