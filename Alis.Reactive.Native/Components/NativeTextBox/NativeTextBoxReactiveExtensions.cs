using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
#if NET48
using System.Web;
#else
using Microsoft.AspNetCore.Html;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Wires <see cref="NativeTextBox"/> DOM events into the Reactive Plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is the final builder call; native builders render directly.
    /// </remarks>
    public static class NativeTextBoxReactiveExtensions
    {
        /// <summary>
        /// Wires a <see cref="NativeTextBox"/> DOM event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">The model value type associated with the text input component.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The NativeTextBox builder being wired.</param>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects which event to listen for, such as <c>evt => evt.Changed</c>.</param>
        /// <param name="pipeline">Builds the pipeline that runs when the event fires.</param>
        public static NativeTextBoxBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeTextBoxBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeTextBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(NativeTextBoxEvents.Instance);

            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", typedEvent, pipeline);

            return builder;
        }
    }
}
