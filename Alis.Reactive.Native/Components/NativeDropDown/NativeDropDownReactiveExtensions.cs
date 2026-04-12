using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires browser events from <see cref="NativeDropDown"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call in the builder chain.
    /// <code>
    /// .NativeDropDown(b => b
    ///     .Items(statusItems)
    ///     .Placeholder("-- Select --")
    ///     .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///     {
    ///         p.Element("status").SetText("changed!");
    ///     }))
    /// </code>
    /// </remarks>
    public static class NativeDropDownReactiveExtensions
    {
        /// <summary>
        /// Wires a <see cref="NativeDropDown"/> browser event into a reactive pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The dropdown builder to wire events on.</param>
        /// <param name="plan">The plan to add the reactive entry to.</param>
        /// <param name="eventSelector">Selects which event to listen for (e.g. <c>evt => evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the reactive pipeline that runs when the event fires.</param>
        /// <returns>The builder for continued chaining.</returns>
        public static NativeDropDownBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeDropDownBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeDropDownEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeDropDownEvents.Instance);
            var pb = new PipelineBuilder<TModel>(plan.Context);
            pipeline(descriptor.Args, pb);

            plan.Context.WireComponentEvent(builder.ElementId, "native", descriptor.JsEvent, pb.BuildReactions());

            return builder;
        }
    }
}
