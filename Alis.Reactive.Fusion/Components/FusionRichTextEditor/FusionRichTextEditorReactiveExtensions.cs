using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
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
        public static RichTextEditorBuilder Reactive<TModel, TArgs>(
            this RichTextEditorBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionRichTextEditorEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionRichTextEditor, TArgs>(
                plan, builder.model.Id, (string)attrs["name"],
                eventSelector(FusionRichTextEditorEvents.Instance), pipeline);
            return builder;
        }
    }
}
