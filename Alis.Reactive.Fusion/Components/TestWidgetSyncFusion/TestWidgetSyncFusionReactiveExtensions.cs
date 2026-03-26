using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    public static class TestWidgetSyncFusionReactiveExtensions
    {
        public static TestWidgetSyncFusionBuilder<TModel> Reactive<TModel, TArgs>(
            this TestWidgetSyncFusionBuilder<TModel> builder,
            IReactivePlan<TModel> plan,
            Func<TestWidgetSyncFusionEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            ReactiveWiringHelper.Wire<TModel, TestWidgetSyncFusion, TArgs>(
                plan, builder.ElementId, null,
                eventSelector(TestWidgetSyncFusionEvents.Instance), pipeline);
            return builder;
        }
    }
}
