using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionStepper"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionStepperReactiveExtensions
    {
        private static readonly FusionStepper Component = new FusionStepper();

        public static FusionStepperBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionStepperBuilder<TModel> builder,
            Func<FusionStepperEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionStepperEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
