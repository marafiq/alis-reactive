using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionCheckBox"/> into the reactive plan.
    /// </summary>
    public static class FusionCheckBoxReactiveExtensions
    {
        private static readonly FusionCheckBox Component = new FusionCheckBox();

        /// <summary>
        /// Wires a FusionCheckBox event to a reactive pipeline that executes in the browser.
        /// </summary>
        public static CheckBoxBuilder Reactive<TModel, TArgs>(
            this CheckBoxBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionCheckBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionCheckBoxEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
