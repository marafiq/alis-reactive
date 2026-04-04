using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionColorPicker"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call inside the build callback passed to
    /// <see cref="FusionColorPickerHtmlExtensions.FusionColorPicker{TModel,TProp}"/>:
    /// <code>
    /// Html.InputField(plan, m =&gt; m.ThemeColor).FusionColorPicker(b =&gt;
    /// {
    ///     b.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { /* commands */ });
    /// });
    /// </code>
    /// </remarks>
    public static class FusionColorPickerReactiveExtensions
    {

        /// <summary>
        /// Wires a FusionColorPicker event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Fusion builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects which event to react to (e.g. <c>evt =&gt; evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static ColorPickerBuilder Reactive<TModel, TArgs>(
            this ColorPickerBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionColorPickerEvents, ReactiveEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var reactiveEvent = eventSelector(FusionColorPickerEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];
            var scope = plan.Authoring.CreateObjectEventScope(
                componentId,
                FusionColorPicker.Definition,
                reactiveEvent.EventName,
                reactiveEvent.ContractAuthoring);
            var pb = new PipelineBuilder<TModel>(plan.Authoring, scope);
            pipeline(default!, pb);
            plan.AddWorkflow(scope, pb);

            return builder;
        }
    }
}
