using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionTab"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is called on the builder returned by
    /// <see cref="FusionTabHtmlExtensions.FusionTab{TModel}"/>:
    /// <code>
    /// @(Html.FusionTab(plan, "my-tabs", b =&gt; { /* items */ })
    ///     .Reactive(evt =&gt; evt.Selected, (args, p) =&gt; { /* commands */ }))
    /// </code>
    /// </remarks>
    public static class FusionTabReactiveExtensions
    {
        private static readonly FusionTab Component = new FusionTab();

        /// <summary>
        /// Attaches a reactive workflow to a Fusion Tab event.
        /// </summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TArgs">The event payload type inferred from the selected event.</typeparam>
        /// <param name="builder">The tab builder to attach behavior to.</param>
        /// <param name="eventSelector">Selects the event to listen for.</param>
        /// <param name="pipeline">Builds the workflow executed when the event fires.</param>
        /// <returns>The current builder.</returns>
        public static FusionTabBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionTabBuilder<TModel> builder,
            Func<FusionTabEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(FusionTabEvents.Instance);
            var scope = builder.Plan.Authoring.CreateObjectEventScope(
                builder.ElementId,
                Component.Vendor,
                null,
                null,
                reactiveEvent.EventName);
            var pb = new PipelineBuilder<TModel>(builder.Plan.Authoring, scope);
            pipeline(reactiveEvent.Payload, pb);
            builder.Plan.AddWorkflow(scope, pb);

            return builder;
        }
    }
}
