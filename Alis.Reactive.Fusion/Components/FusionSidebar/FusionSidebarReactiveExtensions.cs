using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionSidebar"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionSidebarReactiveExtensions
    {
        private static readonly FusionSidebar Component = new FusionSidebar();

        public static FusionSidebarBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionSidebarBuilder<TModel> builder,
            Func<FusionSidebarEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionSidebarEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
