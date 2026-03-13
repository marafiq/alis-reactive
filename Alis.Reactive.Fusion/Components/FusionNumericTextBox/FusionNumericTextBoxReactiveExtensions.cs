using System;
using System.Collections.Generic;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion NumericTextBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.NumericTextBoxFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionNumericTextBox&gt;(m => m.Amount).SetValue(100);
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionNumericTextBoxReactiveExtensions
    {
        private static readonly FusionNumericTextBox _component = new FusionNumericTextBox();

        public static NumericTextBoxBuilder Reactive<TModel, TArgs>(
            this NumericTextBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionNumericTextBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionNumericTextBoxEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, _component.Vendor, bindingPath, _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }

    }
}
