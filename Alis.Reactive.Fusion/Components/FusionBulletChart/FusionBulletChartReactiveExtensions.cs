using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionBulletChart"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionBulletChartReactiveExtensions
    {
        private static readonly FusionBulletChart Component = new FusionBulletChart();

        public static FusionBulletChartBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionBulletChartBuilder<TModel> builder,
            Func<FusionBulletChartEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionBulletChartEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
