using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionBreadcrumb"/> into the reactive plan.
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
            var descriptor = eventSelector(FusionBreadcrumbEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
