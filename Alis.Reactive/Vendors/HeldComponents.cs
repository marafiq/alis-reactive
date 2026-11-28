using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Alis.Reactive.Declarations;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Vendors;

internal static class HeldComponents
{
    private static readonly ConditionalWeakTable<Expression, IReadOnlyList<PathStep>> ElementPaths = new();

    internal static Binding Recording(Binding bound, IReadOnlyList<PathStep>? elementPath)
        => elementPath is null ? bound : new Recorded(bound, elementPath);

    internal static IReadOnlyList<PathStep>? ElementPathOf(Expression expression)
        => ElementPaths.TryGetValue(expression, out var elementPath) ? elementPath : null;

    private static Expression Records(Expression target, IReadOnlyList<PathStep> elementPath)
    {
        var located = target is ObjExpression;
        if (located || ElementPaths.TryGetValue(target, out _)) return target;
        ElementPaths.Add(target, elementPath);
        return target;
    }

    private sealed class Recorded(Binding bound, IReadOnlyList<PathStep> elementPath) : Binding
    {
        internal override Expression Target => Records(bound.Target, elementPath);

        internal override Expression WriteTarget => bound.WriteTarget;

        internal override Expression Arguments => bound.Arguments;

        internal override BlockWriter Writer => bound.Writer;

        internal override PathMembers Reads(bool optional) => bound.Reads(optional);

        internal override bool IsAnEventsPayload => bound.IsAnEventsPayload;

        internal override bool MayBeAbsent => bound.MayBeAbsent;
    }
}
