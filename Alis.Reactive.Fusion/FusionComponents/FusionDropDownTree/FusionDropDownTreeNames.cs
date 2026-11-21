using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

internal static class FusionDropDownTreeNames
{
    private const string NamedWhereTheTreeRenders = "named where FusionDropDownTree renders";

    private static readonly ConditionalWeakTable<string, Func<NodeNames, string>> Deferred = new();

    internal static string Defer(Func<NodeNames, string> named)
    {
        var text = new string(NamedWhereTheTreeRenders.ToCharArray());
        Deferred.Add(text, named);
        return text;
    }

    internal static string? Named(string? text, NodeNames names) => text is not null && Deferred.TryGetValue(text, out var named) ? named(names) : text;
}

internal abstract class NodeNames
{
    internal abstract string Of<TNode, TMember>(Expression<Func<TNode, TMember>> member);
}

internal sealed class FieldNodeNames<TModel, TValue>(InputField<TModel, TValue> field) : NodeNames
{
    internal override string Of<TNode, TMember>(Expression<Func<TNode, TMember>> member) => field.JsonName(member);
}
