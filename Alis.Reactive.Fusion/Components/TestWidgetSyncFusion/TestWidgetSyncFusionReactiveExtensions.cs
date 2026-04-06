using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static class TestWidgetSyncFusionReactiveExtensions
    {
        private static readonly TestWidgetSyncFusion _component = new TestWidgetSyncFusion();

        public static TestWidgetSyncFusionBuilder<TModel> Reactive<TModel, TArgs>(
            this TestWidgetSyncFusionBuilder<TModel> builder,
            ReactivePlan<TModel> plan,
            Func<TestWidgetSyncFusionEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(TestWidgetSyncFusionEvents.Instance);
            var pb = new PipelineBuilder<TModel>(plan.Context);
            pipeline(descriptor.Args, pb);

            plan.Context.EnsureComponent(builder.ElementId, "fusion");
            var trigger = StartsWhen.ComponentEvent(builder.ElementId, descriptor.JsEvent);
            foreach (var reaction in pb.BuildReactions())
                plan.Context.AddBehavior(Behavior.On(trigger, reaction));

            return builder;
        }
    }
}
