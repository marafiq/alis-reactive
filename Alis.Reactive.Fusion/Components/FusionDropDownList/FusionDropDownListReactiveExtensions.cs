using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionDropDownList"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionDropDownListReactiveExtensions
    {
        private static readonly FusionDropDownList Component = new FusionDropDownList();

        /// <summary>
        /// Wires a <see cref="FusionDropDownList"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <remarks>
        /// Select the event with <c>evt =&gt; evt.Changed</c>, <c>evt =&gt; evt.Focus</c>, or <c>evt =&gt; evt.Blur</c>.
        /// </remarks>
        public static DropDownListBuilder Reactive<TModel, TArgs>(
            this DropDownListBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionDropDownListEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionDropDownListEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
