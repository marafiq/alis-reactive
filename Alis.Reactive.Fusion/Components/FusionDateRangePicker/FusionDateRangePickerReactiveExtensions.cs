using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
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
        public static DateRangePickerBuilder Reactive<TModel, TArgs>(
            this DateRangePickerBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDateRangePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionDateRangePicker, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionDateRangePickerEvents.Instance), pipeline);
            return builder;
        }
    }
}
