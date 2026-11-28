using System.Runtime.CompilerServices;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Elements;

internal static class GuardedWrites
{
    private static readonly ConditionalWeakTable<Statement, Expression> WrittenObjects = new();

    internal static IfStatement Writing(IfStatement guarded, Expression written)
    {
        WrittenObjects.Add(guarded, written);
        return guarded;
    }

    internal static Expression? ObjectWrittenBy(Statement statement) => WrittenObjects.TryGetValue(statement, out var written) ? written : null;
}
