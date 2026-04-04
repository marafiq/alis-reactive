using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires native test-widget events into reactive workflows.
    /// </summary>
    public static class TestWidgetNativeReactiveExtensions
    {

        /// <summary>Attaches a reactive workflow to a test-widget event.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TArgs">The event payload type inferred from the selected event.</typeparam>
        /// <param name="builder">The widget builder to attach behavior to.</param>
        /// <param name="plan">The reactive plan receiving the workflow.</param>
        /// <param name="eventSelector">Selects the event to listen for.</param>
        /// <param name="pipeline">Builds the workflow executed when the event fires.</param>
        /// <returns>The current builder.</returns>
        public static TestWidgetNativeBuilder<TModel> Reactive<TModel, TArgs>(
            this TestWidgetNativeBuilder<TModel> builder,
            ReactivePlan<TModel> plan,
            Func<TestWidgetNativeEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(TestWidgetNativeEvents.Instance);
            var scope = plan.Authoring.CreateObjectEventScope(
                builder.ElementId,
                TestWidgetNative.Definition,
                reactiveEvent.EventName,
                reactiveEvent.ContractAuthoring);
            var pb = new PipelineBuilder<TModel>(plan.Authoring, scope);
            pipeline(default!, pb);
            plan.AddWorkflow(scope, pb);

            return builder;
        }
    }
}
