using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires <see cref="NativeCheckBox"/> DOM events into the Reactive Plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is the final builder call; native builders render directly.
    /// </remarks>
    public static class NativeCheckBoxReactiveExtensions
    {
        /// <summary>
        /// Wires a <see cref="NativeCheckBox"/> DOM event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The checkbox builder to wire events on.</param>
        /// <param name="plan">The plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects which event to listen for (e.g. <c>evt => evt.Changed</c>).</param>
        /// <param name="pipeline">Builds the pipeline that runs when the event fires.</param>
        public static NativeCheckBoxBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckBoxBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeCheckBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeCheckBoxEvents.Instance);

            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", descriptor, pipeline);

            return builder;
        }
    }
}
