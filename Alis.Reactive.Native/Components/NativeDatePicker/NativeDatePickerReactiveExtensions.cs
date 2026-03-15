using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeDatePickerBuilder.
    ///
    /// Usage (in .cshtml):
    ///   Html.NativeDatePickerFor(plan, m => m.BirthDate)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Element("status").SetText("date changed!");
    ///       })
    ///
    /// .Reactive() is always the last call — native builders are IHtmlContent
    /// directly (no .Render() needed).
    /// </summary>
    public static class NativeDatePickerReactiveExtensions
    {
        private static readonly NativeDatePicker _component = new NativeDatePicker();

        public static NativeDatePickerBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeDatePickerBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeDatePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeDatePickerEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(builder.ElementId, descriptor.JsEvent, _component.Vendor, builder.BindingPath, _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }
    }
}
