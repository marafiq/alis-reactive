using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion MultiColumnComboBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.MultiColumnComboBoxFor(plan, expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionMultiColumnComboBox&gt;(m => m.Facility).SetValue("1");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionMultiColumnComboBoxReactiveExtensions
    {
        private static readonly FusionMultiColumnComboBox Component = new FusionMultiColumnComboBox();

        public static MultiColumnComboBoxBuilder Reactive<TModel, TArgs>(
            this MultiColumnComboBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionMultiColumnComboBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionMultiColumnComboBoxEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
