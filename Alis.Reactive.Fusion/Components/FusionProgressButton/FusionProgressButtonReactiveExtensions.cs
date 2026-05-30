using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionProgressButton"/> into the reactive plan.
    /// </summary>
    public static class FusionProgressButtonReactiveExtensions
    {
        private static readonly FusionProgressButton Component = new FusionProgressButton();

        /// <summary>
        /// Wires a FusionProgressButton event to a reactive pipeline that executes in the browser.
        /// </summary>
        public static FusionProgressButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionProgressButtonBuilder<TModel> builder,
            Func<FusionProgressButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionProgressButtonEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);
            return builder;
        }
    }
}
