using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionTab"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionTabReactiveExtensions
    {
        private static readonly FusionTab Component = new FusionTab();

        /// <summary>
        /// Wires a <see cref="FusionTab"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The FusionTab builder being wired.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Selected</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        public static FusionTabBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionTabBuilder<TModel> builder,
            Func<FusionTabEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionTabEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);


            return builder;
        }
    }
}
