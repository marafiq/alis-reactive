using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

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
        private static readonly FusionAccordion Component = new FusionAccordion();

        public static FusionAccordionBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionAccordionBuilder<TModel> builder,
            Func<FusionAccordionEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionAccordionEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(
                builder.ElementId,
                descriptor.JsEvent,
                Component.Vendor,
                builder.ElementId);       // bindingPath = elementId for non-input

            foreach (var reaction in pb.BuildReactions())
                builder.Plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
