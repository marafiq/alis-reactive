using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionSmartTextAreaReactiveExtensions
    {
        private static readonly FusionSmartTextArea Component = new FusionSmartTextArea();

        public static void Reactive<TModel, TArgs>(
            this ReactivePlan<TModel> plan,
            string componentId,
            Func<FusionSmartTextAreaEvents, TypedEvent<TArgs>> on,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = on(FusionSmartTextAreaEvents.Instance);
            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);
        }

        public static void Reactive<TModel, TProp, TArgs>(
            this ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> component,
            Func<FusionSmartTextAreaEvents, TypedEvent<TArgs>> on,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var componentId = IdGenerator.For(component);
            plan.Reactive(componentId, on, pipeline);
        }
    }
}
