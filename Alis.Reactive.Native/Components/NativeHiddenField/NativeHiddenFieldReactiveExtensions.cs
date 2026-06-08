using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires <see cref="NativeHiddenField"/> DOM events into the Reactive Plan.
    /// </summary>
    /// <remarks>
    /// Hidden inputs do not raise user-driven change events. Programmatic value
    /// changes are observable only when the caller dispatches a DOM <c>change</c> event.
    /// </remarks>
    public static class NativeHiddenFieldReactiveExtensions
    {
        /// <summary>
        /// Wires a <see cref="NativeHiddenField"/> DOM event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">Model value type associated with the hidden input component.</typeparam>
        /// <typeparam name="TArgs">Event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="plan">Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects which event to listen for, such as <c>evt => evt.Changed</c>.</param>
        /// <param name="pipeline">Builds the pipeline that runs when the event fires.</param>
        public static NativeHiddenFieldBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeHiddenFieldBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeHiddenFieldEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(NativeHiddenFieldEvents.Instance);

            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", typedEvent, pipeline);

            return builder;
        }
    }
}
