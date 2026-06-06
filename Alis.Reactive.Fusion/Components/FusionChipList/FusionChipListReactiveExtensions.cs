using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionChipListReactiveExtensions
    {
        private static readonly FusionChipList Component = new FusionChipList();

        public static FusionChipListBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionChipListBuilder<TModel> builder,
            Func<FusionChipListEvents, TypedEvent<TArgs>> on,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = on(FusionChipListEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);
            return builder;
        }
    }
}
