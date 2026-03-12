using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeCheckBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   @Html.NativeCheckBox("toggle-id")
    ///       .CssClass("h-4 w-4 rounded")
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.When(args, a => a.Checked).Truthy()
    ///            .Then(t => t.Element("panel").Show())
    ///            .Else(e => e.Element("panel").Hide());
    ///       })
    ///
    /// .Reactive() is always the last call — the builder implements IHtmlContent.
    /// </summary>
    public static class NativeCheckBoxReactiveExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        public static NativeCheckBoxBuilder<TModel> Reactive<TModel, TArgs>(
            this NativeCheckBoxBuilder<TModel> builder,
            IReactivePlan<TModel> plan,
            Func<NativeCheckBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeCheckBoxEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(builder.ElementId, descriptor.JsEvent, _component.Vendor, readExpr: _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);
            (plan as ReactivePlan<TModel>)?.RegisterBuildContexts(pb.BuildContexts);

            return builder;
        }
    }
}
