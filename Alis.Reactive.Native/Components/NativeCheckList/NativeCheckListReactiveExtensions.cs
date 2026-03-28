using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeCheckListBuilder.
    ///
    /// Creates ONE entry targeting the container div's change event (bubbles from checkboxes).
    /// checklist.ts syncs checked values into container.value (string[]) and hidden.value (CSV).
    /// The ComponentEventTrigger reads container.value — getting the full array.
    ///
    /// This extension only wires the developer's custom pipeline.
    /// </summary>
    public static class NativeCheckListReactiveExtensions
    {
        private static readonly NativeCheckList _component = new NativeCheckList();

        public static NativeCheckListBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckListBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
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

            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
