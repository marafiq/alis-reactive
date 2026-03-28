using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionRichTextEditor"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call inside the configure callback passed to
    /// <see cref="FusionRichTextEditorHtmlExtensions.RichTextEditor{TModel, TProp}"/>:
    /// <code>
    /// Html.InputField(plan, m =&gt; m.Notes).RichTextEditor(b =&gt;
    /// {
    ///     b.Height(200);
    ///     b.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { /* commands */ });
    /// });
    /// </code>
    /// </remarks>
    public static class FusionRichTextEditorReactiveExtensions
    {
        private static readonly FusionRichTextEditor Component = new FusionRichTextEditor();

        /// <summary>
        /// Wires a RichTextEditor event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Syncfusion builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects which event to react to (e.g. <c>evt =&gt; evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static RichTextEditorBuilder Reactive<TModel, TArgs>(
            this RichTextEditorBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionRichTextEditorEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionRichTextEditorEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            // RTE uses model.Id (set by FusionRichTextEditorHtmlExtensions) instead
            // of HtmlAttributes["id"] because SF RTE Render() uses model.Id for the
            // textarea's id attribute, not HtmlAttributes.
            var componentId = builder.model.Id;
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
