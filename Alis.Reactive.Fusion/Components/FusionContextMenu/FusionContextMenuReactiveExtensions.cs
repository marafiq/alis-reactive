using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionContextMenu"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionContextMenuReactiveExtensions
    {
        private static readonly FusionContextMenu Component = new FusionContextMenu();

        public static FusionContextMenuBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionContextMenuBuilder<TModel> builder,
            Func<FusionContextMenuEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionContextMenuEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
