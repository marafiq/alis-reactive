using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeHiddenFieldBuilder.
    /// Hidden inputs rarely fire change events — this exists for completeness
    /// (programmatic value changes can be observed via dispatched change events).
    /// </summary>
    public static class NativeHiddenFieldReactiveExtensions
    {
        public static NativeHiddenFieldBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeHiddenFieldBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeHiddenFieldEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            ReactiveWiringHelper.Wire<TModel, NativeHiddenField, TArgs>(
                plan, builder.ElementId, builder.BindingPath,
                eventSelector(NativeHiddenFieldEvents.Instance), pipeline);
            return builder;
        }
    }
}
