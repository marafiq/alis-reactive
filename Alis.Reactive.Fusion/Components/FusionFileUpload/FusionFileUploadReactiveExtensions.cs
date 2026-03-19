using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion UploaderBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.InputField(plan, m => m.Documents, o => o.Label("Documents"))
    ///       .FileUpload(b => b
    ///           .Reactive(plan, evt => evt.Selected, (args, p) =>
    ///           {
    ///               p.Element("status").SetText("Files selected");
    ///           }))
    /// </summary>
    public static class FusionFileUploadReactiveExtensions
    {
        private static readonly FusionFileUpload Component = new FusionFileUpload();

        public static UploaderBuilder Reactive<TModel, TArgs>(
            this UploaderBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionFileUploadEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionFileUploadEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            // Uploader uses Uploader(id) — id is set via the constructor, stored in model.Id.
            // name is set via HtmlAttributes.
            var componentId = builder.model.Id;
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }
    }
}
