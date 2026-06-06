using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionRichTextEditor"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionRichTextEditorReactiveExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

        /// <summary>
        /// Wires a <see cref="FusionRichTextEditor"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">Event args type inferred from the event selector.</typeparam>
        /// <param name="plan">Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Changed</c>.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static RichTextEditorBuilder Reactive<TModel, TArgs>(
            this RichTextEditorBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionRichTextEditorEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionRichTextEditorEvents.Instance);

            // RTE uses model.Id (set by FusionRichTextEditorHtmlExtensions) instead
            // of HtmlAttributes["id"] because Syncfusion RTE Render() uses model.Id for the
            // textarea's id attribute, not HtmlAttributes.
            var componentId = builder.model.Id;

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
