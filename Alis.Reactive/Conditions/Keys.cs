using System;
using System.Collections.Generic;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conditions;

internal static partial class Keys
{
    private static readonly Dictionary<Type, Operator> OfType = Written();

    private static readonly HashSet<Type> DateTimes = [typeof(DateTime), typeof(DateTime?)];

    internal static bool IsDateOrTime<TValue>() => IsDateOrTime(typeof(TValue));

    internal static bool IsDateOrTime(Type type) => OfType.ContainsKey(type);

    internal static bool KeyedAlike(Type one, Type other)
        => OfType.TryGetValue(one, out var key) && OfType.TryGetValue(other, out var otherKey) && ReferenceEquals(key, otherKey);

    internal static Expression Of<TValue>(Expression operand, HeldForm form) => Of(typeof(TValue), operand, form);

    internal static Expression Of(Type type, Expression operand, HeldForm form)
    {
        if (DateTimes.Contains(type)) return new OpExpression(form.KeyOfADateTime, [operand]);
        return OfType.TryGetValue(type, out var key) ? new OpExpression(key, [operand]) : operand;
    }

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
