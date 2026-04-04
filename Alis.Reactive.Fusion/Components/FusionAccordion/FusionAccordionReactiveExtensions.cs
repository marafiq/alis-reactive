using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionAccordion"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is called on the builder returned by
    /// <see cref="FusionAccordionHtmlExtensions.FusionAccordion{TModel}"/>:
    /// <code>
    /// @(Html.FusionAccordion(plan, "my-accordion", b =&gt; { /* items */ })
    ///     .Reactive(evt =&gt; evt.Expanded, (args, p) =&gt; { /* commands */ }))
    /// </code>
    /// </remarks>
    public static class FusionAccordionReactiveExtensions
    {

        /// <summary>
        /// Attaches a reactive workflow to a Fusion Accordion event.
        /// </summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TArgs">The event payload type inferred from the selected event.</typeparam>
        /// <param name="builder">The accordion builder to attach behavior to.</param>
        /// <param name="eventSelector">Selects the event to listen for.</param>
        /// <param name="pipeline">Builds the workflow executed when the event fires.</param>
        /// <returns>The current builder.</returns>
        public static FusionAccordionBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionAccordionBuilder<TModel> builder,
            Func<FusionAccordionEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(FusionAccordionEvents.Instance);
            var scope = builder.Plan.Authoring.CreateObjectEventScope(
                builder.ElementId,
                FusionAccordion.Definition,
                reactiveEvent.EventName,
                reactiveEvent.ContractAuthoring);
            var pb = new PipelineBuilder<TModel>(builder.Plan.Authoring, scope);
            pipeline(default!, pb);
            builder.Plan.AddWorkflow(scope, pb);

            return builder;
        }
    }
}
