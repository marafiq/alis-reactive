using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionTooltip"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is called on the builder returned by
    /// <see cref="FusionTooltipHtmlExtensions.FusionTooltip{TModel}"/>:
    /// <code>
    /// @(Html.FusionTooltip(plan, "staff-tooltip", b =&gt; { b.Position(...); })
    ///     .Reactive(evt =&gt; evt.BeforeOpen, (args, p) =&gt; { /* commands */ }))
    /// </code>
    /// </remarks>
    public static class FusionTooltipReactiveExtensions
    {
        private static readonly FusionTooltip Component = new FusionTooltip();

        public static FusionTooltipBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionTooltipBuilder<TModel> builder,
            Func<FusionTooltipEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionTooltipEvents.Instance);
            var pb = new PipelineBuilder<TModel>(builder.Plan.Context);
            pipeline(descriptor.Args, pb);

            builder.Plan.Context.WireComponentEvent(builder.ElementId, Component.Vendor, descriptor.JsEvent, pb.BuildReactions());

            return builder;
        }
    }
}
