using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Adds Reactive Plan event wiring to rendered <see cref="NativeButton"/> builders.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> returns the same builder so it can stay at the end of the fluent render chain.
    /// </remarks>
    public static class NativeButtonReactiveExtensions
    {
        /// <summary>
        /// Adds a component event trigger for the selected DOM event.
        /// </summary>
        /// <typeparam name="TModel">Current Razor view model.</typeparam>
        /// <typeparam name="TArgs">Payload type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The NativeButton builder being wired.</param>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects which event to listen for, such as <c>evt => evt.Click</c>.</param>
        /// <param name="pipeline">Defines the reaction graph that runs when the event fires.</param>
        public static NativeButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this NativeButtonBuilder<TModel> builder,
            ReactivePlan<TModel> plan,
            Func<NativeButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(NativeButtonEvents.Instance);

            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", typedEvent, pipeline);

            return builder;
        }
    }
}
