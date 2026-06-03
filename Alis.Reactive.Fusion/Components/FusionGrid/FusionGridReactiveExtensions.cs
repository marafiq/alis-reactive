using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionGrid"/> into the reactive plan.
    /// </summary>
    public static class FusionGridReactiveExtensions
    {
        private static readonly FusionGrid Component = new FusionGrid();

        /// <summary>
        /// Wires a FusionGrid event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The grid builder.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.DataStateChange</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The same builder instance.</returns>
        public static FusionGridBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionGridBuilder<TModel> builder,
            Func<FusionGridEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionGridEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);


            return builder;
        }
    }
}
