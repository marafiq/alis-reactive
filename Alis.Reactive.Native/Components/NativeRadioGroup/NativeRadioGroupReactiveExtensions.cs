using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Adds Reactive Plan event wiring to rendered <see cref="NativeRadioGroup"/> builders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates one component event trigger per radio option so each radio button
    /// can start the same reaction graph. <c>.Reactive()</c> returns the same
    /// builder so it can stay at the end of the fluent render chain.
    /// </para>
    /// </remarks>
    public static class NativeRadioGroupReactiveExtensions
    {
        /// <summary>
        /// Adds component event triggers for the selected DOM event on each radio option.
        /// </summary>
        /// <typeparam name="TModel">Model that owns the bound component value.</typeparam>
        /// <typeparam name="TProp">Bound value type for the component.</typeparam>
        /// <typeparam name="TArgs">Payload type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="plan">Reactive Plan that receives the component event triggers.</param>
        /// <param name="eventSelector">Selects which event to listen for, such as <c>evt => evt.Changed</c>.</param>
        /// <param name="pipeline">Defines the reaction graph that runs when the event fires.</param>
        public static NativeRadioGroupBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeRadioGroupBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeRadioGroupEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(NativeRadioGroupEvents.Instance);

            for (int i = 0; i < builder.Options.Count; i++)
            {
                var radioId = $"{builder.ElementId}_r{i}";
                ComponentEventOnboarding.Wire(plan, radioId, "native", typedEvent, pipeline);
            }

            return builder;
        }
    }
}
