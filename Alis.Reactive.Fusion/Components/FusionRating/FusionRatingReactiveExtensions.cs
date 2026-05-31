using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionRating"/> into the reactive plan.
    /// </summary>
    public static class FusionRatingReactiveExtensions
    {
        private static readonly FusionRating Component = new FusionRating();

        /// <summary>
        /// Wires a FusionRating event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Fusion builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects which event to react to.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static RatingBuilder Reactive<TModel, TArgs>(
            this RatingBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionRatingEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionRatingEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
