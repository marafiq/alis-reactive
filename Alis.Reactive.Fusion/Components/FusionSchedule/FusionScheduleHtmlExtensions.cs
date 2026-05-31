using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Schedule;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionScheduleBuilder.
    /// Non-input component — NO InputField wrapper, NO input component registration.
    /// </summary>
    public static class FusionScheduleHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionSchedule with the given element ID.
        /// Non-input component: renders directly, no label/validation wrapper.
        /// </summary>
        public static FusionScheduleBuilder<TModel> FusionSchedule<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ScheduleBuilder> build)
            where TModel : class
        {
            // NO input component registration — this is NOT an input component

            var builder = html.EJS().Schedule(elementId);
            build(builder);

            return new FusionScheduleBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
