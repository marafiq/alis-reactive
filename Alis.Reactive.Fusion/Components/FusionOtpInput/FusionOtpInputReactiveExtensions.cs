using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionOtpInput"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionOtpInputReactiveExtensions
    {
        private static readonly FusionOtpInput Component = new FusionOtpInput();

        /// <summary>
        /// Wires a <see cref="FusionOtpInput"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <remarks>
        /// Select the event with <c>evt =&gt; evt.Input</c>, <c>evt =&gt; evt.ValueChanged</c>, etc.; see IntelliSense for the full event set.
        /// </remarks>
        public static OtpInputBuilder Reactive<TModel, TArgs>(
            this OtpInputBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionOtpInputEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionOtpInputEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
