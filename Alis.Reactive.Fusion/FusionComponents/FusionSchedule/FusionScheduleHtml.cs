using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Schedule;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionScheduleHtml
{
    private static ScheduleBuilder Built<TModel>(ReactivePlan<TModel> plan, ScheduleBuilder schedule, Action<ScheduleBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(schedule);
        var ofAnEvent = new PlanMemberNames<TModel>(plan, "the event", "an event");
        var ofAResource = new PlanMemberNames<TModel>(plan, "the resource", "a resource");
        if (schedule.model.EventSettings is { Fields: { } fields }) NameEach(fields, ofAnEvent);
        foreach (var resource in schedule.model.Resources ?? []) NameEach(resource, ofAnEvent, ofAResource, plan);
        if (schedule.model.EventSettings is { DataSource: { } events } settings) settings.DataSource = FusionData.AsWritten(plan.Json(events));
        return schedule;
    }

    private static void NameEach<TModel>(ScheduleResource resource, MemberNames ofAnEvent, MemberNames ofAResource, ReactivePlan<TModel> plan)
    {
        resource.Field = FusionScheduleNames.Named(resource.Field, ofAnEvent);
        resource.IdField = FusionScheduleNames.Named(resource.IdField, ofAResource);
        resource.TextField = FusionScheduleNames.Named(resource.TextField, ofAResource);
        resource.ColorField = FusionScheduleNames.Named(resource.ColorField, ofAResource);
        resource.GroupIDField = FusionScheduleNames.Named(resource.GroupIDField, ofAResource);
        resource.ExpandedField = FusionScheduleNames.Named(resource.ExpandedField, ofAResource);
        resource.StartHourField = FusionScheduleNames.Named(resource.StartHourField, ofAResource);
        resource.EndHourField = FusionScheduleNames.Named(resource.EndHourField, ofAResource);
        resource.WorkDaysField = FusionScheduleNames.Named(resource.WorkDaysField, ofAResource);
        resource.CssClassField = FusionScheduleNames.Named(resource.CssClassField, ofAResource);
        if (resource.DataSource is { } items) resource.DataSource = FusionData.AsWritten(plan.Json(items));
    }

    private static void NameEach(ScheduleField fields, MemberNames names)
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

    private static void NameOf(ScheduleFieldOptions? field, MemberNames names)
    {
        if (field is null) return;
        field.Name = FusionScheduleNames.Named(field.Name, names);
    }
}
