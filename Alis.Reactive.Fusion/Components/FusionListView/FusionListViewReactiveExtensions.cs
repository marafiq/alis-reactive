using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionListView"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionListViewReactiveExtensions
    {
        private static readonly FusionListView Component = new FusionListView();

        public static FusionListViewBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionListViewBuilder<TModel> builder,
            Func<FusionListViewEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionListViewEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);
            return builder;
        }
    }
}
