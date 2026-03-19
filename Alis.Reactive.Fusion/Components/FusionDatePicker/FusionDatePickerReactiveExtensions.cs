using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion DatePickerBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.DatePickerFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionDatePicker&gt;(m => m.AdmissionDate).SetValue(new DateTime(...));
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionDatePickerReactiveExtensions
    {
        private static readonly FusionDatePicker Component = new FusionDatePicker();

        public static DatePickerBuilder Reactive<TModel, TArgs>(
            this DatePickerBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDatePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionDatePickerEvents.Instance);
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
