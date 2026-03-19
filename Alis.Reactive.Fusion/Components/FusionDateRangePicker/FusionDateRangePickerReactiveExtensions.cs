using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion DateRangePickerBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.DateRangePickerFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Element("echo").SetText(args, x => x.StartDate);
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionDateRangePickerReactiveExtensions
    {
        private static readonly FusionDateRangePicker Component = new FusionDateRangePicker();

        public static DateRangePickerBuilder Reactive<TModel, TArgs>(
            this DateRangePickerBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDateRangePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionDateRangePickerEvents.Instance);
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
