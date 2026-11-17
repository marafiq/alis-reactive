using System;
using Syncfusion.EJ2.Schedule;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionScheduleHtml
{
    private static ScheduleBuilder Built(ScheduleBuilder schedule, Action<ScheduleBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(schedule);
        return schedule;
    }
}
