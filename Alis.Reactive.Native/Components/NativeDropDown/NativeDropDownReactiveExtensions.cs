using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires <see cref="NativeDropDown"/> DOM events into the Reactive Plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is the final builder call; native builders render directly.
    /// </remarks>
    public static class NativeDropDownReactiveExtensions
    {
        /// <summary>
        /// Wires a <see cref="NativeDropDown"/> DOM event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">The model value type associated with the dropdown component.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The NativeDropDown builder being wired.</param>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects which event to listen for, such as <c>evt => evt.Changed</c>.</param>
        /// <param name="pipeline">Builds the pipeline that runs when the event fires.</param>
        public static NativeDropDownBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeDropDownBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeDropDownEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(NativeDropDownEvents.Instance);

            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", typedEvent, pipeline);

            return builder;
        }
    }
}
