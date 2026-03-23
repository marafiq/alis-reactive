using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeButtonBuilder.
    ///
    /// Usage (in .cshtml):
    ///   @Html.NativeButton("save-btn", "Save")
    ///       .CssClass("...")
    ///       .Reactive(plan, evt => evt.Click, (args, p) =>
    ///       {
    ///           p.Post("/api/save", g => g.Static("name", "John"))
    ///            .Response(r => r.OnSuccess(s => s.Element("result").SetText("Saved!")));
    ///       })
    ///
    /// .Reactive() is always the last call — the builder implements IHtmlContent.
    /// </summary>
    public static class NativeButtonReactiveExtensions
    {
        public static NativeButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this NativeButtonBuilder<TModel> builder,
            IReactivePlan<TModel> plan,
            Func<NativeButtonEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            ReactiveWiringHelper.Wire<TModel, NativeButton, TArgs>(
                plan, builder.ElementId, null,
                eventSelector(NativeButtonEvents.Instance), pipeline);
            return builder;
        }
    }
}
