using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionTooltip"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionTooltipReactiveExtensions
    {
        private static readonly FusionTooltip Component = new FusionTooltip();

        /// <summary>
        /// Wires a <see cref="FusionTooltip"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">Event args type inferred from the event selector.</typeparam>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.BeforeOpen</c>.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static FusionTooltipBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionTooltipBuilder<TModel> builder,
            Func<FusionTooltipEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionTooltipEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
