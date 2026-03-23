using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion DropDownListBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.DropDownListFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionDropDownList&gt;(m => m.Country).SetValue("US");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionDropDownListReactiveExtensions
    {
        public static DropDownListBuilder Reactive<TModel, TArgs>(
            this DropDownListBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDropDownListEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionDropDownList, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionDropDownListEvents.Instance), pipeline);
            return builder;
        }
    }
}
