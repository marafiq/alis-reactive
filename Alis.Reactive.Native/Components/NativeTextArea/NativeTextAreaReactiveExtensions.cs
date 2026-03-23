using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeTextAreaBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.NativeTextAreaFor(plan, m => m.CareNotes)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Element("status").SetText("changed!");
    ///       })
    ///
    /// .Reactive() is always the last call — native builders are IHtmlContent
    /// directly (no .Render() needed).
    /// </summary>
    public static class NativeTextAreaReactiveExtensions
    {
        public static NativeTextAreaBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeTextAreaBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeTextAreaEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            ReactiveWiringHelper.Wire<TModel, NativeTextArea, TArgs>(
                plan, builder.ElementId, builder.BindingPath,
                eventSelector(NativeTextAreaEvents.Instance), pipeline);
            return builder;
        }
    }
}
