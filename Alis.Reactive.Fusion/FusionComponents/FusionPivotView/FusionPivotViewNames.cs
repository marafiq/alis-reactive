using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

internal static class FusionPivotViewNames
{
    private const string NamedWhereThePivotRenders = "named where Html.FusionPivotView renders";

    private static readonly ConditionalWeakTable<string, Func<PivotRowNames, string>> Deferred = new();

    internal static string Defer(Func<PivotRowNames, string> named)
    {
        var text = new string(NamedWhereThePivotRenders.ToCharArray());
        Deferred.Add(text, named);
        return text;
    }

    internal static string? Named(string? text, PivotRowNames names) => text is not null && Deferred.TryGetValue(text, out var named) ? named(names) : text;
}

internal abstract class PivotRowNames
{
    internal abstract string Of<TRow, TMember>(Expression<Func<TRow, TMember>> member);
}

internal sealed class PlanPivotRowNames<TModel>(ReactivePlan<TModel> plan) : PivotRowNames
{
    internal override string Of<TRow, TMember>(Expression<Func<TRow, TMember>> member)
    {
        var names = plan.JsonPath(member);
        if (names.Count != 1) throw new InvalidOperationException($"FusionPivotViewFields names '{member}', a path past one member of the row, and the pivot reads each field as one member of a row; name one member, such as row => row.Ward.");
        return names[0];
    }
}
