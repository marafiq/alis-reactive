using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionAccordion"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionAccordionReactiveExtensions
    {
        private static readonly FusionAccordion Component = new FusionAccordion();

        /// <summary>
        /// Wires a <see cref="FusionAccordion"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">Event args type inferred from the event selector.</typeparam>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Expanded</c>.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static FusionAccordionBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionAccordionBuilder<TModel> builder,
            Func<FusionAccordionEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionAccordionEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);


            return builder;
        }
    }
}
