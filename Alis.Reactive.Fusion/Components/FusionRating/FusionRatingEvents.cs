namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionRating"/> component.
    /// </summary>
    public sealed class FusionRatingEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionRatingEvents Instance = new FusionRatingEvents();
        private FusionRatingEvents() { }

        /// <summary>Fires when the rating value changes.</summary>
        public TypedEvent<FusionRatingValueChangedArgs> ValueChanged =>
            new TypedEvent<FusionRatingValueChangedArgs>(
                "valueChanged", new FusionRatingValueChangedArgs());
    }
}
