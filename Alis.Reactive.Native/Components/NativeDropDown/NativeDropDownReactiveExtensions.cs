using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeDropDownBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.NativeDropDownFor(m => m.Status)
    ///       .Items(statusItems)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;NativeDropDown&gt;(m => m.Status).SetValue("active");
    ///       })
    ///
    /// .Reactive() is always the last call — native builders are IHtmlContent
    /// directly (no .Render() needed).
    /// </summary>
    public static class NativeDropDownReactiveExtensions
    {
        public static NativeDropDownBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeDropDownBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeDropDownEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            ReactiveWiringHelper.Wire<TModel, NativeDropDown, TArgs>(
                plan, builder.ElementId, builder.BindingPath,
                eventSelector(NativeDropDownEvents.Instance), pipeline);
            return builder;
        }
    }
}
