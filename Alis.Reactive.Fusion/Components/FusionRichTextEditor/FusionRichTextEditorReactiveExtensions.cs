using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionRichTextEditor"/> into the reactive plan.
    /// </summary>
    public static class FusionRichTextEditorReactiveExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

        /// <summary>
        /// Wires a FusionRichTextEditor event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Syncfusion rich text editor builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Changed</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The same builder instance.</returns>
        public static RichTextEditorBuilder Reactive<TModel, TArgs>(
            this RichTextEditorBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionRichTextEditorEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionRichTextEditorEvents.Instance);

            // RTE uses model.Id (set by FusionRichTextEditorHtmlExtensions) instead
            // of HtmlAttributes["id"] because SF RTE Render() uses model.Id for the
            // textarea's id attribute, not HtmlAttributes.
            var componentId = builder.model.Id;
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
