using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionSchedule"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionScheduleReactiveExtensions
    {
        private static readonly FusionSchedule Component = new FusionSchedule();

        /// <summary>
        /// Wires a <see cref="FusionSchedule"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">Event args type inferred from the event selector.</typeparam>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.CellClicked</c>.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static FusionScheduleBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionScheduleBuilder<TModel> builder,
            Func<FusionScheduleEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionScheduleEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
