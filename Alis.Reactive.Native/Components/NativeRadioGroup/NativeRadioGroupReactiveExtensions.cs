using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires <see cref="NativeRadioGroup"/> DOM events into the Reactive Plan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates one Reactive Plan entry per radio option so each radio button
    /// can trigger the pipeline independently. <c>.Reactive()</c> is the final
    /// builder call; native builders render directly.
    /// </para>
    /// </remarks>
    public static class NativeRadioGroupReactiveExtensions
    {
        /// <summary>
        /// Wires a <see cref="NativeRadioGroup"/> DOM event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The radio group builder to wire events on.</param>
        /// <param name="plan">The plan that receives the component event triggers.</param>
        /// <param name="eventSelector">Selects which event to listen for (e.g. <c>evt => evt.Changed</c>).</param>
        /// <param name="pipeline">Builds the pipeline that runs when the event fires.</param>
        public static NativeRadioGroupBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeRadioGroupBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeRadioGroupEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeRadioGroupEvents.Instance);

            for (int i = 0; i < builder.Options.Count; i++)
            {
                var radioId = $"{builder.ElementId}_r{i}";
                ComponentEventOnboarding.Wire(plan, radioId, "native", descriptor, pipeline);
            }

            return builder;
        }
    }
}
