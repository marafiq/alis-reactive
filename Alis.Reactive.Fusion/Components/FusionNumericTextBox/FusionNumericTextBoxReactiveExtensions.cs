using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionNumericTextBox"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionNumericTextBoxReactiveExtensions
    {
        private static readonly FusionNumericTextBox Component = new FusionNumericTextBox();

        /// <summary>
        /// Wires a <see cref="FusionNumericTextBox"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <remarks>
        /// Select the event with <c>evt =&gt; evt.Changed</c>, <c>evt =&gt; evt.Focus</c>, etc.; see IntelliSense for the full event set.
        /// </remarks>
        public static NumericTextBoxBuilder Reactive<TModel, TArgs>(
            this NumericTextBoxBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionNumericTextBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionNumericTextBoxEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
