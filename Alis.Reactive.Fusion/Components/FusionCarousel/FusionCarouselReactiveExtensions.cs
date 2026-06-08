using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionCarousel"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionCarouselReactiveExtensions
    {
        private static readonly FusionCarousel Component = new FusionCarousel();

        public static FusionCarouselBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionCarouselBuilder<TModel> builder,
            Func<FusionCarouselEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionCarouselEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
