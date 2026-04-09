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
        private static readonly NativeHiddenField _component = new NativeHiddenField();

        public static NativeHiddenFieldBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeHiddenFieldBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeHiddenFieldEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeHiddenFieldEvents.Instance);
            var pb = new PipelineBuilder<TModel>(plan.Context);
            pipeline(descriptor.Args, pb);

            plan.Context.WireComponentEvent(builder.ElementId, "native", descriptor.JsEvent, pb.BuildReactions());

            return builder;
        }
    }
}
