using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionBreadcrumb"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionBreadcrumbReactiveExtensions
    {
        private static readonly FusionBreadcrumb Component = new FusionBreadcrumb();

        public static FusionBreadcrumbBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionBreadcrumbBuilder<TModel> builder,
            Func<FusionBreadcrumbEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionBreadcrumbEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
