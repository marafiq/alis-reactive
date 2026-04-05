using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

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

        public static FusionTabBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionTabBuilder<TModel> builder,
            Func<FusionTabEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionTabEvents.Instance);
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
