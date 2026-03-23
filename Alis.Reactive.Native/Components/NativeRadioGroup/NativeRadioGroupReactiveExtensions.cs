using System;
using Alis.Reactive.Builders;

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
                ReactiveWiringHelper.Wire<TModel, NativeRadioGroup, TArgs>(
                    plan, $"{builder.ElementId}_r{i}", builder.BindingPath,
                    descriptor, pipeline);
            }
            return builder;
        }
    }
}
