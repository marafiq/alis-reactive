using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionDialog"/> into the reactive plan.
    /// </summary>
    public static class FusionDialogReactiveExtensions
    {
        private static readonly FusionDialog Component = new FusionDialog();

        public static FusionDialogBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionDialogBuilder<TModel> builder,
            Func<FusionDialogEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionDialogEvents.Instance);
            var pb = new PipelineBuilder<TModel>(builder.Plan.Context);
            pipeline(descriptor.Args, pb);

            builder.Plan.Context.WireComponentEvent(builder.ElementId, Component.Vendor, descriptor.JsEvent, pb.BuildReactions());

            return builder;
        }
    }
}
