using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the NativeHiddenFieldBuilder.
    /// Hidden inputs rarely fire change events -- this exists for completeness
    /// (programmatic value changes can be observed via dispatched change events).
    /// </summary>
    public static class NativeHiddenFieldReactiveExtensions
    {
        public static NativeHiddenFieldBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeHiddenFieldBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeHiddenFieldEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeHiddenFieldEvents.Instance);

            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", descriptor, pipeline);

            return builder;
        }
    }
}
