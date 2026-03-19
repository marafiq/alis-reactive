using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion DateTimePickerBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.DateTimePickerFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionDateTimePicker&gt;(m => m.AppointmentTime).SetValue(new DateTime(...));
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionDateTimePickerReactiveExtensions
    {
        private static readonly FusionDateTimePicker Component = new FusionDateTimePicker();

        public static DateTimePickerBuilder Reactive<TModel, TArgs>(
            this DateTimePickerBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDateTimePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionDateTimePickerEvents.Instance);
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
