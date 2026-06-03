using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionTooltip"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionTooltipReactiveExtensions
    {
        private static readonly FusionTooltip Component = new FusionTooltip();

        /// <summary>
        /// Wires a <see cref="FusionTooltip"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The tooltip builder.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.BeforeOpen</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        public static FusionTooltipBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionTooltipBuilder<TModel> builder,
            Func<FusionTooltipEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionTooltipEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
