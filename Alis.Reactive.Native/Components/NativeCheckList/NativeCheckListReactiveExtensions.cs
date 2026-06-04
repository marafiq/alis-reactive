using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires <see cref="NativeCheckList"/> DOM events into the Reactive Plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is the final builder call; native builders render directly.
    /// </remarks>
    public static class NativeCheckListReactiveExtensions
    {
        /// <summary>
        /// Wires a <see cref="NativeCheckList"/> DOM event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">The model value type associated with the check list component.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The check list builder to wire events on.</param>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects which event to listen for, such as <c>evt => evt.Changed</c>.</param>
        /// <param name="pipeline">Builds the pipeline that runs when the event fires.</param>
        public static NativeCheckListBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckListBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeCheckListEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeCheckListEvents.Instance);

            // The container is the Reactive Plan-registered component; inline init syncs its value
            // before the bubbled change event reaches this trigger.
            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", descriptor, pipeline);

            return builder;
        }
    }
}
