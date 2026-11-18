using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Schedule;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionScheduleHtml
{
    private static ScheduleBuilder Built<TModel>(ReactivePlan<TModel> plan, ScheduleBuilder schedule, Action<ScheduleBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(schedule);
        var names = new PlanEventNames<TModel>(plan);
        if (schedule.model.EventSettings is { Fields: { } fields }) NameEach(fields, names);
        foreach (var resource in schedule.model.Resources ?? []) resource.Field = FusionScheduleNames.Named(resource.Field, names);
        if (schedule.model.EventSettings is { DataSource: { } events } settings) settings.DataSource = FusionData.AsWritten(plan.Json(events));
        return schedule;
    }

    private static void NameEach(ScheduleField fields, EventNames names)
    {
        fields.Id = FusionScheduleNames.Named(fields.Id, names);
        fields.IsBlock = FusionScheduleNames.Named(fields.IsBlock, names);
        fields.IsReadonly = FusionScheduleNames.Named(fields.IsReadonly, names);
        fields.FollowingID = FusionScheduleNames.Named(fields.FollowingID, names);
        NameOf(fields.Subject, names);
        NameOf(fields.Location, names);
        NameOf(fields.Description, names);
        NameOf(fields.StartTime, names);
        NameOf(fields.EndTime, names);
        NameOf(fields.StartTimezone, names);
        NameOf(fields.EndTimezone, names);
        NameOf(fields.IsAllDay, names);
        NameOf(fields.RecurrenceID, names);
        NameOf(fields.RecurrenceRule, names);
        NameOf(fields.RecurrenceException, names);
    }

    private static void NameOf(ScheduleFieldOptions? field, EventNames names)
    {
        if (field is null) return;
        field.Name = FusionScheduleNames.Named(field.Name, names);
    }
}
