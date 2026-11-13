#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.Schedule;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a schedule.
/// </summary>
public static class FusionScheduleHtml
{
    /// <summary>Renders a schedule under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the schedule's element.</param>
    /// <param name="build">What the view sets on the schedule.</param>
    /// <returns>The rendered schedule, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSchedule, TModel> FusionSchedule<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<ScheduleBuilder> build)
        => throw new NotImplementedException();
}
#endif
