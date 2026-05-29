using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionMentionReactiveExtensions
    {
        private static readonly FusionMention Component = new FusionMention();

        public static FusionMentionBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionMentionBuilder<TModel> builder,
            Func<FusionMentionEvents, TypedEvent<TArgs>> on,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = on(FusionMentionEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);
            return builder;
        }
    }
}
