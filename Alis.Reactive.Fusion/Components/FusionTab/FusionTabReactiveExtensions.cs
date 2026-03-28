using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionTab"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is called on the builder returned by
    /// <see cref="FusionTabHtmlExtensions.FusionTab{TModel}"/>:
    /// <code>
    /// Html.FusionTab(plan, "my-tabs", b =&gt; { /* items */ })
    ///     .Reactive(evt =&gt; evt.Selected, (args, p) =&gt; { /* commands */ })
    ///     .Render();
    /// </code>
    /// </remarks>
    public static class FusionTabReactiveExtensions
    {
        private static readonly FusionTab Component = new FusionTab();

        public static FusionTabBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionTabBuilder<TModel> builder,
            Func<FusionTabEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionTabEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(
                builder.ElementId,
                descriptor.JsEvent,
                Component.Vendor);

            foreach (var reaction in pb.BuildReactions())
                builder.Plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
