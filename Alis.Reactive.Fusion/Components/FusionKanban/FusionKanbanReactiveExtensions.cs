using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionKanban"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionKanbanReactiveExtensions
    {
        private static readonly FusionKanban Component = new FusionKanban();

        public static FusionKanbanBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionKanbanBuilder<TModel> builder,
            Func<FusionKanbanEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionKanbanEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);
            return builder;
        }
    }
}
