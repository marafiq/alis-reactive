using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeRadioGroupBuilder.
    ///
    /// Creates N entries (one per radio option). Each radio needs its own
    /// ComponentEventTrigger so the runtime can target the specific radio,
    /// read its value, and build the typed event context.
    ///
    /// Auto-sync (hidden input update) is handled by the factory — not here.
    /// This extension only wires the developer's custom pipeline.
    /// </summary>
    public static class NativeRadioGroupReactiveExtensions
    {
        private static readonly NativeRadioGroup _component = new NativeRadioGroup();

        public static NativeRadioGroupBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeRadioGroupBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeRadioGroupEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeRadioGroupEvents.Instance);

            for (int i = 0; i < builder.Options.Count; i++)
            {
                var pb = new PipelineBuilder<TModel>();
                pipeline(descriptor.Args, pb);

                var radioId = $"{builder.ElementId}_r{i}";
                var trigger = new ComponentEventTrigger(
                    radioId, descriptor.JsEvent, _component.Vendor,
                    builder.BindingPath, _component.ReadExpr);

                foreach (var reaction in pb.BuildReactions())
                    plan.AddEntry(new Entry(trigger, reaction));
            }

            return builder;
        }
    }
}
