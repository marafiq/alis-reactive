using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionDialog"/> events into the Reactive Plan.
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
            var typedEvent = eventSelector(FusionDialogEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
