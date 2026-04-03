using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires browser events from <see cref="NativeCheckBox"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call in the builder chain.
    /// <code>
    /// .NativeCheckBox(b => b
    ///     .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///     {
    ///         p.Element("status").SetText("toggled!");
    ///     }))
    /// </code>
    /// </remarks>
    public static class NativeCheckBoxReactiveExtensions
    {
        private static readonly NativeCheckBox _component = new NativeCheckBox();

        /// <summary>
        /// Wires a <see cref="NativeCheckBox"/> browser event into a reactive pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The checkbox builder to wire events on.</param>
        /// <param name="plan">The plan to add the reactive workflow to.</param>
        /// <param name="eventSelector">Selects which event to listen for (e.g. <c>evt => evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the reactive pipeline that runs when the event fires.</param>
        /// <returns>The builder for continued chaining.</returns>
        public static NativeCheckBoxBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckBoxBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeCheckBoxEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(NativeCheckBoxEvents.Instance);
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
