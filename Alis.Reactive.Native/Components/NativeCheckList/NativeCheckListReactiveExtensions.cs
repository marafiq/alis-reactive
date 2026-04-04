using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires browser events from <see cref="NativeCheckList"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call in the builder chain.
    /// <code>
    /// .NativeCheckList(b => b
    ///     .Items(allergyItems)
    ///     .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///     {
    ///         p.Element("status").SetText("updated!");
    ///     }))
    /// </code>
    /// </remarks>
    public static class NativeCheckListReactiveExtensions
    {

        /// <summary>
        /// Wires a <see cref="NativeCheckList"/> browser event into a reactive pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The check list builder to wire events on.</param>
        /// <param name="plan">The plan to add the reactive workflow to.</param>
        /// <param name="eventSelector">Selects which event to listen for (e.g. <c>evt => evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the reactive pipeline that runs when the event fires.</param>
        /// <returns>The builder for continued chaining.</returns>
        public static NativeCheckListBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckListBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeCheckListEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(NativeCheckListEvents.Instance);
            var scope = plan.Authoring.CreateObjectEventScope(
                builder.ElementId,
                NativeCheckList.Definition,
                reactiveEvent.EventName,
                reactiveEvent.ContractAuthoring);
            var pb = new PipelineBuilder<TModel>(plan.Authoring, scope);
            pipeline(default!, pb);
            plan.AddWorkflow(scope, pb);

            return builder;
        }
    }
}
