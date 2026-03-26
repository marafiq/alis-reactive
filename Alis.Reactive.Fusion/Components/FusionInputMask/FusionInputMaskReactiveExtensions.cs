using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion MaskedTextBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.MaskedTextBoxFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionInputMask&gt;(m => m.PhoneNumber).SetValue("(555) 123-4567");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionInputMaskReactiveExtensions
    {
        public static MaskedTextBoxBuilder Reactive<TModel, TArgs>(
            this MaskedTextBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionInputMaskEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionInputMask, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionInputMaskEvents.Instance), pipeline);
            return builder;
        }
    }
}
