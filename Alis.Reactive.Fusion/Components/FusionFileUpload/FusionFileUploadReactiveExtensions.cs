using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionFileUpload"/> into the reactive plan.
    /// </summary>
    public static class FusionFileUploadReactiveExtensions
    {
        private static readonly FusionFileUpload Component = new FusionFileUpload();

        /// <summary>
        /// Wires a FusionFileUpload event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Syncfusion uploader builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.Selected</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The same builder instance.</returns>
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
