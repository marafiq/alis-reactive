using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion RichTextEditorBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.RichTextEditorFor(expr)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionRichTextEditor&gt;(m => m.CarePlan).SetValue("&lt;p&gt;Updated&lt;/p&gt;");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionRichTextEditorReactiveExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

        public static RichTextEditorBuilder Reactive<TModel, TArgs>(
            this RichTextEditorBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionRichTextEditorEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionRichTextEditorEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            // RTE uses model.Id (set by FusionRichTextEditorHtmlExtensions) instead
            // of HtmlAttributes["id"] because SF RTE Render() uses model.Id for the
            // textarea's id attribute, not HtmlAttributes.
            var componentId = builder.model.Id;
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
