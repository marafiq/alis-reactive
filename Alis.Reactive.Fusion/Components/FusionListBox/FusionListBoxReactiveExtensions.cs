using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionListBox"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionListBoxReactiveExtensions
    {
        private static readonly FusionListBox Component = new FusionListBox();

        public static FusionListBoxBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionListBoxBuilder<TModel> builder,
            Func<FusionListBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionListBoxEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);
            return builder;
        }
    }
}
