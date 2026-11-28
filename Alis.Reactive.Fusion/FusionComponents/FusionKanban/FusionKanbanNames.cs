using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

internal static class FusionKanbanNames
{
    private const string NamedWhereTheBoardRenders = "named where Html.FusionKanban renders";

    private static readonly ConditionalWeakTable<string, Func<CardNames, string>> Deferred = new();

    internal static string Defer(Func<CardNames, string> named)
    {
        var text = new string(NamedWhereTheBoardRenders.ToCharArray());
        Deferred.Add(text, named);
        return text;
    }

    internal static string? Named(string? text, CardNames names) => text is not null && Deferred.TryGetValue(text, out var named) ? named(names) : text;
}

internal abstract class CardNames
{
    internal abstract string Of<TCard, TMember>(Expression<Func<TCard, TMember>> member);
}

internal sealed class PlanCardNames<TModel>(ReactivePlan<TModel> plan) : CardNames
{
    internal override string Of<TCard, TMember>(Expression<Func<TCard, TMember>> member)
    {
        var names = plan.JsonPath(member);
        if (names.Count != 1) throw new InvalidOperationException($"FusionKanbanFields names '{member}', a path past one member of the card, and the board reads each field as one member of a card; name one member, such as card => card.Status.");
        return names[0];
    }
}
