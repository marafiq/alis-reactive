using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

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
        private static readonly NativeButton _component = new NativeButton();

        public static NativeButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this NativeButtonBuilder<TModel> builder,
            IReactivePlan<TModel> plan,
            Func<NativeButtonEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeButtonEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(builder.ElementId, descriptor.JsEvent, _component.Vendor);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);

            return builder;
        }
    }
}
