using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

internal static class FusionGridNames
{
    private const string NamedWhereTheGridRenders = "named where Html.FusionGrid renders";

    private static readonly ConditionalWeakTable<string, Func<RowNames, string>> Deferred = new();

    internal static string Defer(Func<RowNames, string> named)
    {
        var text = new string(NamedWhereTheGridRenders.ToCharArray());
        Deferred.Add(text, named);
        return text;
    }

    internal static string Named(string text, RowNames names) => Deferred.TryGetValue(text, out var named) ? named(names) : text;

    internal static string Path(IReadOnlyList<string> names) => string.Join(".", names);
}

internal abstract class RowNames
{
    internal abstract IReadOnlyList<string> Of<TRow, TMember>(Expression<Func<TRow, TMember>> member);
}

internal sealed class PlanRowNames<TModel>(ReactivePlan<TModel> plan) : RowNames
{
    internal override IReadOnlyList<string> Of<TRow, TMember>(Expression<Func<TRow, TMember>> member) => plan.JsonPath(member);
}
