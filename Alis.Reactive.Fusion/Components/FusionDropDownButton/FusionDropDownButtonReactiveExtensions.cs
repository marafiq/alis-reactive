using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionDropDownButton"/> into the reactive plan.
    /// </summary>
    public static class FusionDropDownButtonReactiveExtensions
    {
        private static readonly FusionDropDownButton Component = new FusionDropDownButton();

        /// <summary>
        /// Wires a FusionDropDownButton event to a reactive pipeline that executes in the browser.
        /// </summary>
        public static FusionDropDownButtonBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionDropDownButtonBuilder<TModel> builder,
            Func<FusionDropDownButtonEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionDropDownButtonEvents.Instance);
            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);
            return builder;
        }
    }
}
