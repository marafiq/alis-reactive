using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion NumericTextBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.NumericTextBoxFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionNumericTextBox&gt;(m => m.Amount).SetValue(100);
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionNumericTextBoxReactiveExtensions
    {
        public static NumericTextBoxBuilder Reactive<TModel, TArgs>(
            this NumericTextBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionNumericTextBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionNumericTextBox, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionNumericTextBoxEvents.Instance), pipeline);
            return builder;
        }
    }
}
