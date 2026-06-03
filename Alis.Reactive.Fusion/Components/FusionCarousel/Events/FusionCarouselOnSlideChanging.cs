using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before a Carousel slide changes.
    /// </summary>
    public class FusionCarouselSlideChangingArgs
    {
        /// <summary>Current slide index.</summary>
        public int CurrentIndex { get; set; }

        /// <summary>Next slide index.</summary>
        public int NextIndex { get; set; }

        /// <summary>Whether the slide change was triggered by swipe.</summary>
        public bool IsSwiped { get; set; }

        /// <summary>Slide direction: Previous or Next.</summary>
        public string SlideDirection { get; set; } = string.Empty;

        /// <summary>Whether the slide change should be cancelled.</summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Creates an event payload instance for descriptor wiring.
        /// </summary>
        public FusionCarouselSlideChangingArgs()
        {
        }
    }

    /// <summary>
    /// Typed mutations on the slideChanging event args for <see cref="FusionCarousel"/>.
    /// </summary>
    public static class FusionCarouselSlideChangingArgsExtensions
    {
        /// <summary>Cancels the pending slide transition.</summary>
        public static void PreventTransition(
            this FusionCarouselSlideChangingArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
