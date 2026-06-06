using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionRadioButton"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionRadioButtonReactiveExtensions
    {
        private static readonly FusionRadioButton Component = new FusionRadioButton();

        /// <summary>
        /// Wires a <see cref="FusionRadioButton"/> event into a Reactive Plan pipeline.
        /// </summary>
        public static FusionRadioButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionRadioButtonBuilder<TModel> builder,
            Func<FusionRadioButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionRadioButtonEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);
            return builder;
        }
    }
}
