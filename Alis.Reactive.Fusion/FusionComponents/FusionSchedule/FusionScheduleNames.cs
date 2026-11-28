using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

internal static class FusionScheduleNames
{
    private const string NamedWhereTheScheduleRenders = "named where Html.FusionSchedule renders";

    private static readonly ConditionalWeakTable<string, Func<MemberNames, string>> Deferred = new();

    internal static string Defer(Func<MemberNames, string> named)
    {
        var text = new string(NamedWhereTheScheduleRenders.ToCharArray());
        Deferred.Add(text, named);
        return text;
    }

    internal static string? Named(string? text, MemberNames names) => text is not null && Deferred.TryGetValue(text, out var named) ? named(names) : text;
}

internal abstract class MemberNames
{
    internal abstract string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member);
}

internal sealed class PlanMemberNames<TModel>(ReactivePlan<TModel> plan, string theHolder, string aHolder) : MemberNames
{
    internal override string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member)
    {
        var names = plan.JsonPath(member);
        if (names.Count != 1) throw new InvalidOperationException($"FusionScheduleFields names '{member}', a path past one member of {theHolder}, and the schedule reads each field as one member of {aHolder}; name one member, such as item => item.Subject.");
        return names[0];
    }
}
