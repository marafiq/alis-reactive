using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

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
        private static readonly NativeTextArea _component = new NativeTextArea();

        public static NativeTextAreaBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeTextAreaBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeTextAreaEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeTextAreaEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(builder.ElementId, descriptor.JsEvent, _component.Vendor, builder.BindingPath, _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }
    }
}
