using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionRadioButton"/> into the reactive plan.
    /// </summary>
    public static class FusionRadioButtonReactiveExtensions
    {
        private static readonly FusionRadioButton Component = new FusionRadioButton();

        /// <summary>
        /// Wires a FusionRadioButton event to a reactive pipeline that executes in the browser.
        /// </summary>
        public static FusionRadioButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionRadioButtonBuilder<TModel> builder,
            Func<FusionRadioButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionRadioButtonEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);
            return builder;
        }
    }
}
