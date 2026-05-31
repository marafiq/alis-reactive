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
    /// Wires browser events from <see cref="NativeTextBox"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>.Reactive()</c> is always the last call in the builder chain.
    /// Native builders implement the framework's HTML content type directly, so no
    /// separate <c>.Render()</c> is needed.
    /// </para>
    /// <code>
    /// .NativeTextBox(b => b
    ///     .Placeholder("Enter name")
    ///     .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///     {
    ///         p.Element("status").SetText("changed!");
    ///     }))
    /// </code>
    /// </remarks>
    public static class NativeTextBoxReactiveExtensions
    {
        /// <summary>
        /// Wires a <see cref="NativeTextBox"/> browser event into a reactive pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <typeparam name="TArgs">The event args type selected by <paramref name="eventSelector"/>.</typeparam>
        /// <param name="builder">The text box builder to wire events on.</param>
        /// <param name="plan">The plan to add the reactive entry to.</param>
        /// <param name="eventSelector">Selects which event to listen for (e.g. <c>evt => evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the reactive pipeline that runs when the event fires.</param>
        /// <returns>The builder for continued chaining.</returns>
        public static NativeTextBoxBuilder<TModel, TProp> Reactive<TModel, TProp, TArgs>(
            this NativeTextBoxBuilder<TModel, TProp> builder,
            ReactivePlan<TModel> plan,
            Func<NativeTextBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(NativeTextBoxEvents.Instance);

            ComponentEventOnboarding.Wire(plan, builder.ElementId, "native", descriptor, pipeline);

            return builder;
        }
    }
}
