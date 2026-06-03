using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionDateRangePicker"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionDateRangePickerReactiveExtensions
    {
        private static readonly FusionDateRangePicker Component = new FusionDateRangePicker();

        /// <summary>
        /// Wires a <see cref="FusionDateRangePicker"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Syncfusion date range picker builder.</param>
        /// <param name="plan">The plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Changed</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        public static DateRangePickerBuilder Reactive<TModel, TArgs>(
            this DateRangePickerBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionDateRangePickerEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionDateRangePickerEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
