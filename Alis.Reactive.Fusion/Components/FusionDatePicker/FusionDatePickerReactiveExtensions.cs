using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
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
        public static DatePickerBuilder Reactive<TModel, TArgs>(
            this DatePickerBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDatePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionDatePicker, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionDatePickerEvents.Instance), pipeline);
            return builder;
        }
    }
}
