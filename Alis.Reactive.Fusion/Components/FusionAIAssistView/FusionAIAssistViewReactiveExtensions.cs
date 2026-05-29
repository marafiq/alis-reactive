using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires AIAssistView events into the reactive plan.
    /// </summary>
    public static class FusionAIAssistViewReactiveExtensions
    {
        private static readonly FusionAIAssistView Component = new FusionAIAssistView();

        public static FusionAIAssistViewBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionAIAssistViewBuilder<TModel> builder,
            Func<FusionAIAssistViewEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionAIAssistViewEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);
            return builder;
        }
    }
}
