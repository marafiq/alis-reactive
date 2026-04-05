using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    public static class TestWidgetNativeReactiveExtensions
    {
        private static readonly TestWidgetNative _component = new TestWidgetNative();

        public static TestWidgetNativeBuilder<TModel> Reactive<TModel, TArgs>(
            this TestWidgetNativeBuilder<TModel> builder,
            ReactivePlan<TModel> plan,
            Func<TestWidgetNativeEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(TestWidgetNativeEvents.Instance);
            var pb = new PipelineBuilder<TModel>(plan.Context);
            pipeline(descriptor.Args, pb);

            plan.Context.EnsureComponent(builder.ElementId, "native");
            var trigger = StartsWhen.ComponentEvent(builder.ElementId, descriptor.JsEvent);
            foreach (var reaction in pb.BuildReactions())
                plan.Context.AddBehavior(Behavior.On(trigger, reaction));

            return builder;
        }
    }
}
