using System;
using Alis.Reactive.Builders;

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
        /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">Model value type associated with the checkbox component.</typeparam>
        /// <typeparam name="TArgs">Event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="plan">Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects which event to listen for, such as <c>evt => evt.Changed</c>.</param>
        /// <param name="pipeline">Builds the pipeline that runs when the event fires.</param>
        public static NativeCheckBoxBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeCheckBoxBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeCheckBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(NativeCheckBoxEvents.Instance);

            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", typedEvent, pipeline);

            return builder;
        }
    }
}
