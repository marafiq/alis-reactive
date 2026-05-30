using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionSidebar"/> into the reactive plan.
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
