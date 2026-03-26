using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion MultiSelectBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.MultiSelectFor(plan, expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionMultiSelect&gt;(m => m.Allergies).SetValue("peanuts");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionMultiSelectReactiveExtensions
    {
        public static MultiSelectBuilder Reactive<TModel, TArgs>(
            this MultiSelectBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionMultiSelectEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionMultiSelect, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionMultiSelectEvents.Instance), pipeline);
            return builder;
        }
    }
}
