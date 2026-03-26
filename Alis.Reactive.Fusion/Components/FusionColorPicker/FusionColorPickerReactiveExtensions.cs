using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion ColorPickerBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.InputField(plan, m => m.Color, o => o.Label("Color"))
    ///       .ColorPicker(b => b
    ///           .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///           {
    ///               p.Element("color-echo").SetText(args, x => x.CurrentValue);
    ///           }))
    /// </summary>
    public static class FusionColorPickerReactiveExtensions
    {
        private static readonly FusionColorPicker Component = new FusionColorPicker();

        public static ColorPickerBuilder Reactive<TModel, TArgs>(
            this ColorPickerBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionColorPickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionColorPickerEvents.Instance);
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
