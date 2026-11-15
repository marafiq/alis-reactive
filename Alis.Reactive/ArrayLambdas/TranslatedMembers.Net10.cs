#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ArrayLambdas;

internal static partial class TranslatedMembers
{
    static partial void AddTheHostsOwn(Dictionary<MemberKey, Translation> table, HashSet<MemberKey> spanConversions)
    {
        table.Add(Method((string text) => text.TrimStart()), Unary(Operator.TrimStart));
        table.Add(Method((string text) => text.TrimEnd()), Unary(Operator.TrimEnd));
        table.Add(Method((string text) => text.Contains('a')), Binary(Operator.Contains));
        table.Add(Method((int[] values) => values.Contains(0)), Lookup(inAnyCollection: true));
        table.Add(Method((DayOfWeek[] values) => values.Contains(DayOfWeek.Monday)), Lookup(inAnyCollection: true));
        spanConversions.Add(SpanConversionOf((int[] values) => values.Contains(0)));
    }

    private static MemberKey SpanConversionOf(Expression<Func<int[], bool>> lookup) => Key(((MethodCallExpression)((MethodCallExpression)lookup.Body).Arguments[0]).Method);
}
#endif
