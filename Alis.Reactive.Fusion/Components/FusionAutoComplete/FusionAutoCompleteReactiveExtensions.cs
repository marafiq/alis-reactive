using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion AutoCompleteBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.AutoCompleteFor(plan, expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionAutoComplete&gt;(m => m.Physician).SetValue("Dr. Smith");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionAutoCompleteReactiveExtensions
    {
        private static readonly FusionAutoComplete Component = new FusionAutoComplete();

        public static AutoCompleteBuilder Reactive<TModel, TArgs>(
            this AutoCompleteBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionAutoCompleteEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionAutoCompleteEvents.Instance);
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
