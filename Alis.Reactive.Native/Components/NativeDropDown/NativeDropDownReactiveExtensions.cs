using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

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
        private static readonly NativeDropDown _component = new NativeDropDown();

        public static NativeDropDownBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeDropDownBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeDropDownEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeDropDownEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(builder.ElementId, descriptor.JsEvent, _component.Vendor, builder.BindingPath, _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }
    }
}
