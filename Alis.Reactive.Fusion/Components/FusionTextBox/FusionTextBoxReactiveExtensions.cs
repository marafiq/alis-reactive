using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionTextBox"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionTextBoxReactiveExtensions
    {
        private static readonly FusionTextBox Component = new FusionTextBox();

        /// <summary>
        /// Wires a <see cref="FusionTextBox"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <remarks>
        /// Select the event with <c>evt =&gt; evt.Input</c>, <c>evt =&gt; evt.Changed</c>, etc.; see IntelliSense for the full event set.
        /// </remarks>
        public static TextBoxBuilder Reactive<TModel, TArgs>(
            this TextBoxBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionTextBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionTextBoxEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
