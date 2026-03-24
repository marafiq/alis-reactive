using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Native.Components
{
    public static class TestWidgetNativeReactiveExtensions
    {
        private static readonly TestWidgetNative _component = new TestWidgetNative();

        public static TestWidgetNativeBuilder<TModel> Reactive<TModel, TArgs>(
            this TestWidgetNativeBuilder<TModel> builder,
            IReactivePlan<TModel> plan,
            Func<TestWidgetNativeEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(TestWidgetNativeEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(
                builder.ElementId,
                descriptor.JsEvent,
                _component.Vendor,
                readExpr: _component.ReadExpr);
            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
