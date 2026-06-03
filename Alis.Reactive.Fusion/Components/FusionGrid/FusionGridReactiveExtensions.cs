using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionGrid"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionGridReactiveExtensions
    {
        private static readonly FusionGrid Component = new FusionGrid();

        /// <summary>
        /// Wires a <see cref="FusionGrid"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The grid builder.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.DataStateChange</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
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
