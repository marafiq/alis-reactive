using System;
using Alis.Reactive.Builders;

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
        public static NativeCheckListBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckListBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeCheckListEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            // Single entry on the hidden input — checklist.ts dispatches change after sync
            ReactiveWiringHelper.Wire<TModel, NativeCheckList, TArgs>(
                plan, builder.ElementId, builder.BindingPath,
                eventSelector(NativeCheckListEvents.Instance), pipeline);
            return builder;
        }
    }
}
