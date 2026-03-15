using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion AutoCompleteBuilder (ComboBox).
    ///
    /// Usage (in .cshtml):
    ///   Html.ComboBoxFor(plan, expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionComboBox&gt;(m => m.Physician).SetValue("Dr. Smith");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionComboBoxReactiveExtensions
    {
        private static readonly FusionComboBox Component = new FusionComboBox();

        public static AutoCompleteBuilder Reactive<TModel, TArgs>(
            this AutoCompleteBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionComboBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionComboBoxEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }
    }
}
