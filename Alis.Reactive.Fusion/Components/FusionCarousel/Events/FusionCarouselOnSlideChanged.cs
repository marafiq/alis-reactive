namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered after a Carousel slide changes.
    /// </summary>
    public class FusionCarouselSlideChangedArgs
    {
        /// <summary>Current slide index.</summary>
        public int CurrentIndex { get; set; }

        /// <summary>Previous slide index.</summary>
        public int PreviousIndex { get; set; }

        /// <summary>Whether the slide change was triggered by swipe.</summary>
        public bool IsSwiped { get; set; }

        /// <summary>Slide direction: Previous or Next.</summary>
        public string SlideDirection { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionCarouselSlideChangedArgs()
        {
        }
    }
}
