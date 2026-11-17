using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Schedule;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionScheduleHtml
{
    private static ScheduleBuilder Built<TModel>(ReactivePlan<TModel> plan, ScheduleBuilder schedule, Action<ScheduleBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(schedule);
        if (schedule.model.EventSettings is { DataSource: { } events } settings) settings.DataSource = FusionData.AsWritten(plan.Json(events));
        return schedule;
    }
}
