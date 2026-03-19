using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion MultiSelectBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.MultiSelectFor(plan, expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionMultiSelect&gt;(m => m.Allergies).SetValue("peanuts");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionMultiSelectReactiveExtensions
    {
        private static readonly FusionMultiSelect Component = new FusionMultiSelect();

        public static MultiSelectBuilder Reactive<TModel, TArgs>(
            this MultiSelectBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionMultiSelectEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionMultiSelectEvents.Instance);
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
