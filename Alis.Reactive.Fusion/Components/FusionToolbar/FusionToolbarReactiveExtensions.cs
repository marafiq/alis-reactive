using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionToolbar"/> into the reactive plan.
    /// </summary>
    public static class FusionToolbarReactiveExtensions
    {
        private static readonly FusionToolbar Component = new FusionToolbar();

        public static FusionToolbarBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionToolbarBuilder<TModel> builder,
            Func<FusionToolbarEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionToolbarEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
