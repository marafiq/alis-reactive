using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionSlider"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionSliderReactiveExtensions
    {
        private static readonly FusionSlider Component = new FusionSlider();

        /// <summary>
        /// Wires a <see cref="FusionSlider"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static SliderBuilder Reactive<TModel, TArgs>(
            this SliderBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionSliderEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionSliderEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
