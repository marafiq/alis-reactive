using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires Syncfusion test widget events into reactive workflows.
    /// </summary>
    public static class TestWidgetSyncFusionReactiveExtensions
    {
        private static readonly TestWidgetSyncFusion _component = new TestWidgetSyncFusion();

        /// <summary>
        /// Attaches a reactive workflow to a Syncfusion test widget event.
        /// </summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TArgs">The event payload type inferred from the selected event.</typeparam>
        /// <param name="builder">The widget builder to attach behavior to.</param>
        /// <param name="plan">The reactive plan receiving the workflow.</param>
        /// <param name="eventSelector">Selects the event to listen for.</param>
        /// <param name="pipeline">Builds the workflow executed when the event fires.</param>
        /// <returns>The current builder.</returns>
        public static TestWidgetSyncFusionBuilder<TModel> Reactive<TModel, TArgs>(
            this TestWidgetSyncFusionBuilder<TModel> builder,
            ReactivePlan<TModel> plan,
            Func<TestWidgetSyncFusionEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(TestWidgetSyncFusionEvents.Instance);
            var scope = plan.Authoring.CreateObjectEventScope(
                builder.ElementId,
                _component.Vendor,
                null,
                _component.ValueMemberPath,
                reactiveEvent.EventName);
            var pb = new PipelineBuilder<TModel>(plan.Authoring, scope);
            pipeline(reactiveEvent.Payload, pb);
            plan.AddWorkflow(scope, pb);

            return builder;
        }
    }
}
