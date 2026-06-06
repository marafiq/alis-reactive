using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionSplitButton"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionSplitButtonReactiveExtensions
    {
        private static readonly FusionSplitButton Component = new FusionSplitButton();

        /// <summary>
        /// Wires a <see cref="FusionSplitButton"/> event into a Reactive Plan pipeline.
        /// </summary>
        public static FusionSplitButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionSplitButtonBuilder<TModel> builder,
            Func<FusionSplitButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionSplitButtonEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);
            return builder;
        }
    }
}
