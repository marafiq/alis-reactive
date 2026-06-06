using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.InPlaceEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionInPlaceEditor"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionInPlaceEditorReactiveExtensions
    {
        private static readonly FusionInPlaceEditor Component = new FusionInPlaceEditor();

        /// <summary>
        /// Wires a <see cref="FusionInPlaceEditor"/> event into a Reactive Plan pipeline.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The FusionInPlaceEditor builder being wired.</param>
        /// <param name="plan">The Reactive Plan that receives the component event trigger.</param>
        /// <param name="eventSelector">Selects the component event, for example <c>evt =&gt; evt.ActionBegin</c>.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        public static InPlaceEditorBuilder Reactive<TModel, TArgs>(
            this InPlaceEditorBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionInPlaceEditorEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionInPlaceEditorEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
