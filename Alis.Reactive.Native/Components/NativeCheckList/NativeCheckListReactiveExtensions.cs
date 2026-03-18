using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeCheckListBuilder.
    ///
    /// Creates ONE entry targeting the hidden input's change event (not N per checkbox).
    /// checklist.ts aggregates all checked values into the hidden input and dispatches
    /// a synthetic change event on it. The ComponentEventTrigger reads el.value from
    /// the hidden input — getting the full comma-separated aggregate.
    ///
    /// Hidden input sync + event dispatch is handled by checklist.ts — not here.
    /// This extension only wires the developer's custom pipeline.
    /// </summary>
    public static class NativeCheckListReactiveExtensions
    {
        private static readonly NativeCheckList _component = new NativeCheckList();

        public static NativeCheckListBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckListBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeCheckListEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeCheckListEvents.Instance);

            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            // Single entry on the hidden input — checklist.ts dispatches change after sync
            var trigger = new ComponentEventTrigger(
                builder.ElementId, descriptor.JsEvent, _component.Vendor,
                builder.BindingPath, _component.ReadExpr);

            plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

            return builder;
        }
    }
}
