using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionComboBox"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call inside the build callback passed to
    /// <see cref="FusionComboBoxHtmlExtensions.FusionComboBox{TModel,TProp}"/>.
    /// </remarks>
    public static class FusionComboBoxReactiveExtensions
    {
        private static readonly FusionComboBox Component = new FusionComboBox();

        /// <summary>
        /// Wires a FusionComboBox event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Syncfusion ComboBox builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects which event to react to.</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static ComboBoxBuilder Reactive<TModel, TArgs>(
            this ComboBoxBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionComboBoxEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionComboBoxEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
