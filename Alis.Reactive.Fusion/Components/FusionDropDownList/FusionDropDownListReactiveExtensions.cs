using System;
using System.Collections.Generic;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion DropDownListBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.DropDownListFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionDropDownList&gt;(m => m.Country).SetValue("US");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionDropDownListReactiveExtensions
    {
        private static readonly FusionDropDownList _component = new FusionDropDownList();

        public static DropDownListBuilder Reactive<TModel, TArgs>(
            this DropDownListBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDropDownListEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionDropDownListEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, _component.Vendor, bindingPath, _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);
            (plan as ReactivePlan<TModel>)?.RegisterBuildContexts(pb.BuildContexts);
            plan.RegisterComponent(componentId, _component.Vendor, bindingPath, _component.ReadExpr);

            return builder;
        }

    }
}
