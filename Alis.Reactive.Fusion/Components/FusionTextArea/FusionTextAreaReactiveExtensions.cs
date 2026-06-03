using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionTextArea"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionTextAreaReactiveExtensions
    {
        private static readonly FusionTextArea Component = new FusionTextArea();

        /// <summary>
        /// Wires a <see cref="FusionTextArea"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <remarks>
        /// Select the event with <c>evt =&gt; evt.Input</c>, <c>evt =&gt; evt.Changed</c>, etc.; see IntelliSense for the full event set.
        /// </remarks>
        public static TextAreaBuilder Reactive<TModel, TArgs>(
            this TextAreaBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionTextAreaEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionTextAreaEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
