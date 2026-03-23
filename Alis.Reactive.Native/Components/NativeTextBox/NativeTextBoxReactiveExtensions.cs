using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeTextBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.NativeTextBoxFor(plan, m => m.Name)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Element("status").SetText("changed!");
    ///       })
    ///
    /// .Reactive() is always the last call — native builders are IHtmlContent
    /// directly (no .Render() needed).
    /// </summary>
    public static class NativeTextBoxReactiveExtensions
    {
        public static NativeTextBoxBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeTextBoxBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeTextBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            ReactiveWiringHelper.Wire<TModel, NativeTextBox, TArgs>(
                plan, builder.ElementId, builder.BindingPath,
                eventSelector(NativeTextBoxEvents.Instance), pipeline);
            return builder;
        }
    }
}
