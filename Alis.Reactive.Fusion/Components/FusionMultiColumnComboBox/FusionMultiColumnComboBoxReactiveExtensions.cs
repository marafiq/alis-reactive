using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion MultiColumnComboBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.MultiColumnComboBoxFor(plan, expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionMultiColumnComboBox&gt;(m => m.Facility).SetValue("1");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionMultiColumnComboBoxReactiveExtensions
    {
        public static MultiColumnComboBoxBuilder Reactive<TModel, TArgs>(
            this MultiColumnComboBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionMultiColumnComboBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionMultiColumnComboBox, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionMultiColumnComboBoxEvents.Instance), pipeline);
            return builder;
        }
    }
}
