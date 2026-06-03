using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionTextArea"/> into the reactive plan.
    /// </summary>
    public static class FusionTextAreaReactiveExtensions
    {
        private static readonly FusionTextArea Component = new FusionTextArea();

        /// <summary>
        /// Wires a FusionTextArea event to a reactive pipeline that executes in the browser.
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
