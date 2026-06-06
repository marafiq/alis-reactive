using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionProgressButton"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionProgressButtonReactiveExtensions
    {
        private static readonly FusionProgressButton Component = new FusionProgressButton();

        /// <summary>
        /// Wires a <see cref="FusionProgressButton"/> event into a Reactive Plan pipeline.
        /// </summary>
        public static FusionProgressButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionProgressButtonBuilder<TModel> builder,
            Func<FusionProgressButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionProgressButtonEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);
            return builder;
        }
    }
}
