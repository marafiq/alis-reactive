using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionSwitch"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call inside the build callback passed to
    /// <c>FusionSwitch(...)</c>:
    /// <code>
    /// Html.InputField(plan, m =&gt; m.IsActive).FusionSwitch(b =&gt;
    /// {
    ///     b.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { /* commands */ });
    /// });
    /// </code>
    /// </remarks>
    public static class FusionSwitchReactiveExtensions
    {

        /// <summary>
        /// Wires a FusionSwitch event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Fusion builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects which event to react to (e.g. <c>evt =&gt; evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static SwitchBuilder Reactive<TModel, TArgs>(
            this SwitchBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionSwitchEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(FusionSwitchEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];
            var scope = plan.Authoring.CreateObjectEventScope(
                componentId,
                FusionSwitch.Definition,
                reactiveEvent.EventName,
                reactiveEvent.ContractAuthoring);
            var pb = new PipelineBuilder<TModel>(plan.Authoring, scope);
            pipeline(default!, pb);
            plan.AddWorkflow(scope, pb);

            return builder;
        }
    }
}
