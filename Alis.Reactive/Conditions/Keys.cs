using System;
using System.Collections.Generic;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Conditions;

internal static partial class Keys
{
    private static readonly Dictionary<Type, Operator> OfType = Written();

    internal static bool IsDateOrTime<TValue>() => OfType.ContainsKey(typeof(TValue));

    internal static Expression Of<TValue>(Expression operand)
        => OfType.TryGetValue(typeof(TValue), out var key) ? new OpExpression(key, [operand]) : operand;

    private static Dictionary<Type, Operator> Written()
    {
        var keys = new Dictionary<Type, Operator>
        {
            [typeof(DateTime)] = Operator.DateKey,
            [typeof(DateTime?)] = Operator.DateKey,
            [typeof(DateTimeOffset)] = Operator.InstantKey,
            [typeof(DateTimeOffset?)] = Operator.InstantKey,
            [typeof(TimeSpan)] = Operator.SpanKey,
            [typeof(TimeSpan?)] = Operator.SpanKey,
        };
        AddTheHostsOwn(keys);
        return keys;
    }

    static partial void AddTheHostsOwn(Dictionary<Type, Operator> keys);
}
