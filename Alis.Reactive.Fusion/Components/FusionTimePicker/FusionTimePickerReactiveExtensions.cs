using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion TimePickerBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.TimePickerFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionTimePicker&gt;(m => m.MedicationTime).SetValue(new DateTime(...));
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionTimePickerReactiveExtensions
    {
        public static TimePickerBuilder Reactive<TModel, TArgs>(
            this TimePickerBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionTimePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionTimePicker, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionTimePickerEvents.Instance), pipeline);
            return builder;
        }
    }
}
