using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

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
        private static readonly NativeTextBox _component = new NativeTextBox();

        public static NativeTextBoxBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeTextBoxBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeTextBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeTextBoxEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(builder.ElementId, descriptor.JsEvent, _component.Vendor, builder.BindingPath, _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }
    }
}
