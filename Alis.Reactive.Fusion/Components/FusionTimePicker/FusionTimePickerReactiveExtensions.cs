using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionTimePicker"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionTimePickerReactiveExtensions
    {
        private static readonly FusionTimePicker Component = new FusionTimePicker();

        /// <summary>
        /// Wires a <see cref="FusionTimePicker"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Changed</c>.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static TimePickerBuilder Reactive<TModel, TArgs>(
            this TimePickerBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionTimePickerEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionTimePickerEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
