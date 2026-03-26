using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto NativeCheckBoxBuilder.
    /// Model-bound only: Reactive&lt;TModel, TProp, TArgs&gt; (TProp inferred from builder)
    /// </summary>
    public static class NativeCheckBoxReactiveExtensions
    {
        public static NativeCheckBoxBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckBoxBuilder<TModel, TProp> builder,
            IReactivePlan<TModel> plan,
            Func<NativeCheckBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            ReactiveWiringHelper.Wire<TModel, NativeCheckBox, TArgs>(
                plan, builder.ElementId, builder.BindingPath,
                eventSelector(NativeCheckBoxEvents.Instance), pipeline);
            return builder;
        }
    }
}
