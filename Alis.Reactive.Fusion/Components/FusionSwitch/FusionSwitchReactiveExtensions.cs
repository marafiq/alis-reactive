using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionSwitch"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionSwitchReactiveExtensions
    {
        private static readonly FusionSwitch Component = new FusionSwitch();

        /// <summary>
        /// Wires a <see cref="FusionSwitch"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <remarks>
        /// Select the event with <c>evt =&gt; evt.Changed</c>.
        /// </remarks>
        public static SwitchBuilder Reactive<TModel, TArgs>(
            this SwitchBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionSwitchEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionSwitchEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
