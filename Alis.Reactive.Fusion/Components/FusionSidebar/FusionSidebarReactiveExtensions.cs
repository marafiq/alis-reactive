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

        /// <summary>
        /// Wires a <see cref="FusionSidebar"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The FusionSidebar builder being wired.</param>
        /// <param name="eventSelector">Selects the component event.</param>
        /// <param name="pipeline">Configures the reactions to run when the event fires.</param>
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
