using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
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
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Syncfusion uploader builder.</param>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Selected</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        public static UploaderBuilder Reactive<TModel, TArgs>(
            this UploaderBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionFileUploadEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionFileUploadEvents.Instance);

            // Uploader uses Uploader(id) — id is set via the constructor, stored in model.Id.
            // name is set via HtmlAttributes.
            var componentId = builder.model.Id;
            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
