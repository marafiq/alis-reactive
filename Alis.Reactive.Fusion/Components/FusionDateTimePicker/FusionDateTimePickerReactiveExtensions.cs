using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionDateTimePicker"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionDateTimePickerReactiveExtensions
    {
        private static readonly FusionDateTimePicker Component = new FusionDateTimePicker();

        /// <summary>
        /// Wires a <see cref="FusionDateTimePicker"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Changed</c>.</param>
        /// <param name="pipeline">Configures the reactions to run when the event fires.</param>
        public static DateTimePickerBuilder Reactive<TModel, TArgs>(
            this DateTimePickerBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionDateTimePickerEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionDateTimePickerEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
