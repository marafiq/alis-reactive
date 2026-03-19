using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
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
        private static readonly FusionSwitch Component = new FusionSwitch();

        public static SwitchBuilder Reactive<TModel, TArgs>(
            this SwitchBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionSwitchEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionSwitchEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
