using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
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
        public static DateTimePickerBuilder Reactive<TModel, TArgs>(
            this DateTimePickerBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDateTimePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionDateTimePicker, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionDateTimePickerEvents.Instance), pipeline);
            return builder;
        }
    }
}
