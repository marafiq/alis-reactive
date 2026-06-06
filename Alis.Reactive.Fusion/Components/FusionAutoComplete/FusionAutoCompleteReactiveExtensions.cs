using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionAutoComplete"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionAutoCompleteReactiveExtensions
    {
        private static readonly FusionAutoComplete Component = new FusionAutoComplete();

        /// <summary>
        /// Wires a <see cref="FusionAutoComplete"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <remarks>Select the event with <c>evt =&gt; evt.Changed</c> or <c>evt =&gt; evt.Filtering</c>.</remarks>
        public static AutoCompleteBuilder Reactive<TModel, TArgs>(
            this AutoCompleteBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionAutoCompleteEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionAutoCompleteEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
