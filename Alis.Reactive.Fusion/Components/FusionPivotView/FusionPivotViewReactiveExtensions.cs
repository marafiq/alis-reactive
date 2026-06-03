using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionPivotView"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionPivotViewReactiveExtensions
    {
        private static readonly FusionPivotView Component = new FusionPivotView();

        public static FusionPivotViewBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionPivotViewBuilder<TModel> builder,
            Func<FusionPivotViewEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionPivotViewEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);
            return builder;
        }
    }
}
