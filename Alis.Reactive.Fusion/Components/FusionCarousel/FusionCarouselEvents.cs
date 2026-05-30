namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionCarousel"/> component.
    /// </summary>
    public sealed class FusionCarouselEvents
    {
        public static readonly FusionCarouselEvents Instance = new FusionCarouselEvents();

        private FusionCarouselEvents()
        {
        }

        /// <summary>Fires before the slide changes.</summary>
        public TypedEvent<FusionCarouselSlideChangingArgs> SlideChanging =>
            new TypedEvent<FusionCarouselSlideChangingArgs>("slideChanging", new FusionCarouselSlideChangingArgs());

        /// <summary>Fires after the slide changes.</summary>
        public TypedEvent<FusionCarouselSlideChangedArgs> SlideChanged =>
            new TypedEvent<FusionCarouselSlideChangedArgs>("slideChanged", new FusionCarouselSlideChangedArgs());
    }
}
