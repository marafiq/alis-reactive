using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires hidden-field events into reactive workflows.
    /// </summary>
    public static class NativeHiddenFieldReactiveExtensions
    {
        private static readonly NativeHiddenField _component = new NativeHiddenField();

        /// <summary>Attaches a reactive workflow to a hidden-field event.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TProp">The hidden field property type.</typeparam>
        /// <typeparam name="TArgs">The event payload type inferred from the selected event.</typeparam>
        /// <param name="builder">The hidden-field builder to attach behavior to.</param>
        /// <param name="plan">The reactive plan receiving the workflow.</param>
        /// <param name="eventSelector">Selects the event to listen for.</param>
        /// <param name="pipeline">Builds the workflow executed when the event fires.</param>
        /// <returns>The current builder.</returns>
        public static NativeHiddenFieldBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeHiddenFieldBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeHiddenFieldEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(NativeHiddenFieldEvents.Instance);
            var scope = plan.Authoring.CreateObjectEventScope(
                builder.ElementId,
                _component.Vendor,
                builder.BindingPath,
                _component.ValueMemberPath,
                reactiveEvent.EventName);
            var pb = new PipelineBuilder<TModel>(plan.Authoring, scope);
            pipeline(reactiveEvent.Payload, pb);
            plan.AddWorkflow(scope, pb);

            return builder;
        }
    }
}
