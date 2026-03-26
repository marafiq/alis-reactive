using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion SwitchBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.InputField(plan, m => m.ReceiveNotifications, o => o.Label("Notifications"))
    ///       .Switch(b => b
    ///           .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///           {
    ///               p.Component&lt;FusionSwitch&gt;(m => m.ReceiveNotifications).SetChecked(true);
    ///           }))
    /// </summary>
    public static class FusionSwitchReactiveExtensions
    {
        public static SwitchBuilder Reactive<TModel, TArgs>(
            this SwitchBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionSwitchEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionSwitch, TArgs>(
                plan, (string)attrs["id"], (string)attrs["name"],
                eventSelector(FusionSwitchEvents.Instance), pipeline);
            return builder;
        }
    }
}
