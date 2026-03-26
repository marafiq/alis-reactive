using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion AutoCompleteBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.AutoCompleteFor(plan, expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionAutoComplete&gt;(m => m.Physician).SetValue("Dr. Smith");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionAutoCompleteReactiveExtensions
    {
        public static AutoCompleteBuilder Reactive<TModel, TArgs>(
            this AutoCompleteBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionAutoCompleteEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionAutoComplete, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionAutoCompleteEvents.Instance), pipeline);
            return builder;
        }
    }
}
