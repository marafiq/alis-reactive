using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionMultiSelect"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionMultiSelectReactiveExtensions
    {
        private static readonly FusionMultiSelect Component = new FusionMultiSelect();

        /// <summary>
        /// Wires a <see cref="FusionMultiSelect"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">Event args type inferred from the event selector.</typeparam>
        /// <param name="plan">Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Changed</c>.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static MultiSelectBuilder Reactive<TModel, TArgs>(
            this MultiSelectBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionMultiSelectEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionMultiSelectEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
