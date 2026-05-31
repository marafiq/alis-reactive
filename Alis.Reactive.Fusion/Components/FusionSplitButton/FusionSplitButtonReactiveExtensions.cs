using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionSplitButton"/> into the reactive plan.
    /// </summary>
    public static class FusionSplitButtonReactiveExtensions
    {
        private static readonly FusionSplitButton Component = new FusionSplitButton();

        /// <summary>
        /// Wires a FusionSplitButton event to a reactive pipeline that executes in the browser.
        /// </summary>
        public static FusionSplitButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionSplitButtonBuilder<TModel> builder,
            Func<FusionSplitButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionSplitButtonEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);
            return builder;
        }
    }
}
