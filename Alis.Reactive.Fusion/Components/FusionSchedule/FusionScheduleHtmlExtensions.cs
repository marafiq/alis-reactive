using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Schedule;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionScheduleBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionScheduleHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Schedule and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan that receives schedule event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the underlying Syncfusion schedule builder.</param>
        public static FusionScheduleBuilder<TModel> FusionSchedule<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ScheduleBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Schedule(elementId);
            build(builder);

            return new FusionScheduleBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
