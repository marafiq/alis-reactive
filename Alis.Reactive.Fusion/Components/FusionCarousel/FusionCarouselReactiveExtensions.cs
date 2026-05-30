using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionCarousel"/> into the reactive plan.
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
            var descriptor = eventSelector(FusionCarouselEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
