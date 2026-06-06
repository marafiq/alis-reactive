using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

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
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The FusionAccordion builder being wired.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Expanded</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        public static FusionAccordionBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionAccordionBuilder<TModel> builder,
            Func<FusionAccordionEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionAccordionEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);


            return builder;
        }
    }
}
