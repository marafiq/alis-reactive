using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionMenu"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionMenuReactiveExtensions
    {
        private static readonly FusionMenu Component = new FusionMenu();

        public static FusionMenuBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionMenuBuilder<TModel> builder,
            Func<FusionMenuEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionMenuEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
