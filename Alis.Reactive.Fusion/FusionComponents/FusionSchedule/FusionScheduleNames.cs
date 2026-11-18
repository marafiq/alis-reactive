using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

internal static class FusionScheduleNames
{
    private const string NamedWhereTheScheduleRenders = "named where Html.FusionSchedule renders";

    private static readonly ConditionalWeakTable<string, Func<EventNames, string>> Deferred = new();

    internal static string Defer(Func<EventNames, string> named)
    {
        var text = new string(NamedWhereTheScheduleRenders.ToCharArray());
        Deferred.Add(text, named);
        return text;
    }

    internal static string? Named(string? text, EventNames names) => text is not null && Deferred.TryGetValue(text, out var named) ? named(names) : text;
}

internal abstract class EventNames
{
    internal abstract string Of<TEvent, TMember>(Expression<Func<TEvent, TMember>> member);
}

internal sealed class PlanEventNames<TModel>(ReactivePlan<TModel> plan) : EventNames
{
    internal override string Of<TEvent, TMember>(Expression<Func<TEvent, TMember>> member)
    {
        var names = plan.JsonPath(member);
        if (names.Count != 1) throw new InvalidOperationException($"FusionScheduleFields names '{member}', a path past one member of the event, and the schedule reads each field as one member of an event; name one member, such as item => item.Subject.");
        return names[0];
    }
}
