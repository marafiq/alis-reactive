using System;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionFileUpload"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionFileUploadReactiveExtensions
    {
        private static readonly FusionFileUpload Component = new FusionFileUpload();

        /// <summary>
        /// Wires a <see cref="FusionFileUpload"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TArgs">Event args type inferred from the event selector.</typeparam>
        /// <param name="plan">Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Selected</c>.</param>
        /// <param name="pipeline">Reactive Plan pipeline for the selected event.</param>
        public static UploaderBuilder Reactive<TModel, TArgs>(
            this UploaderBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionFileUploadEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var typedEvent = eventSelector(FusionFileUploadEvents.Instance);

            var componentId = builder.model.Id;

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, typedEvent, pipeline);

            return builder;
        }
    }
}
