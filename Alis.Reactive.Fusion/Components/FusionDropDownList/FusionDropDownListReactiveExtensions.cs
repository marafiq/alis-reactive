using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionDropDownList"/> into the reactive plan.
    /// </summary>
    public static class FusionDropDownListReactiveExtensions
    {
        private static readonly FusionDropDownList Component = new FusionDropDownList();

        /// <summary>
        /// Wires a FusionDropDownList event to a reactive pipeline that executes in the browser.
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
            var descriptor = eventSelector(FusionDropDownListEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
