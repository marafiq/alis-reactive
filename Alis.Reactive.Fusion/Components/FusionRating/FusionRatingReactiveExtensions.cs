using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionRating"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionRatingReactiveExtensions
    {
        private static readonly FusionRating Component = new FusionRating();

        /// <summary>
        /// Wires a <see cref="FusionRating"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">Event args type inferred from the event selector.</typeparam>
        /// <param name="plan">Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static RatingBuilder Reactive<TModel, TArgs>(
            this RatingBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionRatingEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionRatingEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
