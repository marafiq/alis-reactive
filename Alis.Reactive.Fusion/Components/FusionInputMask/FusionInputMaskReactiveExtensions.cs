using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion MaskedTextBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.MaskedTextBoxFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionInputMask&gt;(m => m.PhoneNumber).SetValue("(555) 123-4567");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionInputMaskReactiveExtensions
    {
        private static readonly FusionInputMask Component = new FusionInputMask();

        public static MaskedTextBoxBuilder Reactive<TModel, TArgs>(
            this MaskedTextBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionInputMaskEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionInputMaskEvents.Instance);
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
