using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionAccordion"/> into the reactive plan.
    /// </summary>
    public static class FusionAccordionReactiveExtensions
    {
        private static readonly FusionAccordion Component = new FusionAccordion();

        /// <summary>
        /// Wires a FusionAccordion event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The accordion builder.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Expanded</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The same builder instance.</returns>
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
