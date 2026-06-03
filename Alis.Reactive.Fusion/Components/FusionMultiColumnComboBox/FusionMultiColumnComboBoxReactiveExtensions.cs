using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionMultiColumnComboBox"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionMultiColumnComboBoxReactiveExtensions
    {
        private static readonly FusionMultiColumnComboBox Component = new FusionMultiColumnComboBox();

        /// <summary>
        /// Wires a <see cref="FusionMultiColumnComboBox"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <remarks>
        /// Select the event with <c>evt =&gt; evt.Changed</c>.
        /// </remarks>
        public static MultiColumnComboBoxBuilder Reactive<TModel, TArgs>(
            this MultiColumnComboBoxBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionMultiColumnComboBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionMultiColumnComboBoxEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
