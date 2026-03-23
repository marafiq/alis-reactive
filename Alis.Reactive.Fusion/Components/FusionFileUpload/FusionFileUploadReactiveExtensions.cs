using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
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
        public static UploaderBuilder Reactive<TModel, TArgs>(
            this UploaderBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionFileUploadEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            ReactiveWiringHelper.Wire<TModel, FusionFileUpload, TArgs>(
                plan, builder.model.Id, (string)attrs["name"],
                eventSelector(FusionFileUploadEvents.Instance), pipeline);
            return builder;
        }
    }
}
