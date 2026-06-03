using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires <see cref="FusionDropDownButton"/> events into the Reactive Plan.
    /// </summary>
    public static class FusionDropDownButtonReactiveExtensions
    {
        private static readonly FusionDropDownButton Component = new FusionDropDownButton();

        /// <summary>
        /// Wires a <see cref="FusionDropDownButton"/> event into a Reactive Plan pipeline.
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
