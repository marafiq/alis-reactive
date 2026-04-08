using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionGrid"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is called on the builder returned by
    /// <see cref="FusionGridHtmlExtensions.FusionGrid{TModel}"/>:
    /// <code>
    /// @(Html.FusionGrid(plan, "residents-grid", b =&gt; { /* columns */ })
    ///     .Reactive(evt =&gt; evt.DataStateChange, (args, p) =&gt; { /* commands */ }))
    /// </code>
    /// </remarks>
    public static class FusionGridReactiveExtensions
    {
        /// <summary>
        /// Wires a FusionGrid event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The grid builder.</param>
        /// <param name="eventSelector">Selects which event to react to (e.g. <c>evt =&gt; evt.DataStateChange</c>).</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static FusionGridBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionGridBuilder<TModel> builder,
            Func<FusionGridEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionGridEvents.Instance);
            var pb = new PipelineBuilder<TModel>(builder.Plan.Context);
            pipeline(descriptor.Args, pb);

            builder.Plan.Context.EnsureComponent(builder.ElementId, "fusion");
            var trigger = StartsWhen.ComponentEvent(builder.ElementId, descriptor.JsEvent);

            foreach (var reaction in pb.BuildReactions())
                builder.Plan.Context.AddBehavior(Behavior.On(trigger, reaction));

            return builder;
        }
    }
}
